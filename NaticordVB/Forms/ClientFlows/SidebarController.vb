Imports System.Threading
Imports NaticordVB.Networking
Imports NaticordVB.Networking.Models

Friend Enum SidebarMode
    DirectMessages
    Servers
End Enum

''' <summary>
''' Fills the sidebar list: your DMs, your servers and, inside a server, its channels. It owns the loading, the ordering and the
''' "couldn't load, click to retry" states; the list control itself only draws and reports clicks.
''' </summary>
Friend NotInheritable Class SidebarController

    ''' <summary>A chat was picked. <c>title</c> is what the chat header should say.</summary>
    Friend Event ChannelChosen(channel As DiscordChannel, title As String)

    Private ReadOnly _list As ChatListBox

    Private _mode As SidebarMode = SidebarMode.DirectMessages
    Private _dms As List(Of DiscordChannel)
    Private _guilds As List(Of DiscordGuild)
    Private ReadOnly _guildChannels As New Dictionary(Of String, List(Of DiscordChannel))()
    Private _currentGuild As DiscordGuild

    ' Each load takes a number; a slow answer for something the user already left is dropped instead of drawn over the new view.
    Private _generation As Integer

    Friend Sub New(list As ChatListBox)
        _list = list
        AddHandler _list.EntryClicked, AddressOf OnEntryClicked
    End Sub

    Friend ReadOnly Property Mode As SidebarMode
        Get
            Return _mode
        End Get
    End Property

#Region "Switching tabs"

    ''' <summary>Shows the DM list. With <paramref name="forceReload"/> it asks Discord again, otherwise it uses what it already has.</summary>
    Friend Async Function ShowDirectMessagesAsync(Optional forceReload As Boolean = False) As Task
        _mode = SidebarMode.DirectMessages
        _currentGuild = Nothing
        Dim gen As Integer = Interlocked.Increment(_generation)

        If _dms IsNot Nothing Then
            Render()
        Else
            ShowNotice("Loading your messages...")
        End If

        If _dms IsNot Nothing AndAlso Not forceReload Then Return

        Dim fetched = Await DiscordService.GetDirectChannelsAsync()
        If gen <> _generation Then Return

        If fetched.IsSuccess Then
            _dms = fetched.Value
            Render()
        ElseIf _dms Is Nothing Then
            ShowNotice(fetched.FriendlyError & " Click to try again.", "retry-dms")
        End If
        ' If we already had a list, a failed refresh is ignored: keep showing what we have.
    End Function

    Friend Async Function ShowServersAsync() As Task
        _mode = SidebarMode.Servers
        _currentGuild = Nothing
        Dim gen As Integer = Interlocked.Increment(_generation)

        If _guilds IsNot Nothing Then
            Render()
            Return
        End If

        ShowNotice("Loading your servers...")
        Dim fetched = Await DiscordService.GetGuildsAsync()
        If gen <> _generation Then Return

        If fetched.IsSuccess Then
            _guilds = fetched.Value
            Render()
        Else
            ShowNotice(fetched.FriendlyError & " Click to try again.", "retry-servers")
        End If
    End Function

    Private Async Function OpenGuildAsync(guild As DiscordGuild) As Task
        _currentGuild = guild
        Dim gen As Integer = Interlocked.Increment(_generation)

        If _guildChannels.ContainsKey(guild.Id) Then
            Render()
            Return
        End If

        ShowNotice("Loading channels...", Nothing, withBack:=True)
        Dim fetched = Await DiscordService.GetGuildChannelsAsync(guild.Id)
        If gen <> _generation Then Return

        If fetched.IsSuccess Then
            _guildChannels(guild.Id) = fetched.Value
            Render()
        Else
            ShowNotice(fetched.FriendlyError & " Click to try again.", "retry-guild", withBack:=True)
        End If
    End Function

#End Region

#Region "Building the rows"

    Private Sub Render()
        Dim entries As New List(Of SidebarEntry)

        If _mode = SidebarMode.DirectMessages Then
            For Each channel In _dms
                entries.Add(New SidebarEntry With {
                    .Kind = EntryKind.Dm,
                    .Key = channel.Id,
                    .Title = channel.DisplayName,
                    .Subtitle = DescribeDirect(channel),
                    .IconUrl = DiscordCdn.ChannelIconUrl(channel, 64),
                    .Tag = channel})
            Next
            If entries.Count = 0 Then entries.Add(NoticeEntry("No conversations yet."))

        ElseIf _currentGuild Is Nothing Then
            For Each guild In _guilds
                entries.Add(New SidebarEntry With {
                    .Kind = EntryKind.Guild,
                    .Key = guild.Id,
                    .Title = guild.Name,
                    .Subtitle = If(guild.IsOwner, "Owner", ""),
                    .IconUrl = DiscordCdn.GuildIconUrl(guild, 64),
                    .Tag = guild})
            Next
            If entries.Count = 0 Then entries.Add(NoticeEntry("You're not in any servers."))

        Else
            entries.Add(BackEntry())
            entries.AddRange(BuildChannelEntries(_guildChannels(_currentGuild.Id)))
        End If

        _list.SetEntries(entries)
        PrefetchIcons(entries)
    End Sub

    ''' <summary>Channels without a category first, then each category with its channels, all in the order the server set.</summary>
    Private Shared Function BuildChannelEntries(channels As List(Of DiscordChannel)) As List(Of SidebarEntry)
        Dim result As New List(Of SidebarEntry)
        Dim chats = channels.Where(Function(c) c.IsChat).ToList()

        For Each channel In chats.Where(Function(c) String.IsNullOrEmpty(c.ParentId)).OrderBy(Function(c) c.Position)
            result.Add(ChannelEntry(channel))
        Next

        For Each category In channels.Where(Function(c) c.Type = ChannelType.GuildCategory).OrderBy(Function(c) c.Position)
            Dim children = chats.Where(Function(c) c.ParentId = category.Id).OrderBy(Function(c) c.Position).ToList()
            If children.Count = 0 Then Continue For
            result.Add(New SidebarEntry With {.Kind = EntryKind.Category, .Key = "cat-" & category.Id, .Title = If(category.Name, "")})
            For Each channel In children
                result.Add(ChannelEntry(channel))
            Next
        Next

        If result.Count = 0 Then result.Add(NoticeEntry("There are no text channels here."))
        Return result
    End Function

    Private Shared Function ChannelEntry(channel As DiscordChannel) As SidebarEntry
        Return New SidebarEntry With {.Kind = EntryKind.Channel, .Key = channel.Id, .Title = If(channel.Name, "unnamed"), .Tag = channel}
    End Function

    Private Function BackEntry() As SidebarEntry
        Return New SidebarEntry With {.Kind = EntryKind.Back, .Key = "back", .Title = If(_currentGuild?.Name, "Servers")}
    End Function

    Private Shared Function NoticeEntry(text As String, Optional key As String = "notice") As SidebarEntry
        Return New SidebarEntry With {.Kind = EntryKind.Notice, .Key = key, .Title = text}
    End Function

    Private Sub ShowNotice(text As String, Optional key As String = Nothing, Optional withBack As Boolean = False)
        Dim entries As New List(Of SidebarEntry)
        If withBack Then entries.Add(BackEntry())
        entries.Add(NoticeEntry(text, If(key, "notice")))
        _list.SetEntries(entries)
    End Sub

    ''' <summary>"14:05", "Yesterday", "Tuesday" or "3 Nov" for when the last message was sent; "N members" for a group without messages.</summary>
    Private Shared Function DescribeDirect(channel As DiscordChannel) As String
        Dim whenSent As DateTime? = Snowflake.ToDateTime(channel.LastMessageId)
        If whenSent.HasValue Then Return FormatRecent(whenSent.Value.ToLocalTime())
        If channel.Type = ChannelType.GroupDm Then Return (channel.Recipients.Count + 1).ToString() & " members"
        Return ""
    End Function

    Private Shared Function FormatRecent(localTime As DateTime) As String
        Dim days As Integer = CInt((DateTime.Now.Date - localTime.Date).TotalDays)
        If days <= 0 Then Return localTime.ToString("t")
        If days = 1 Then Return "Yesterday"
        If days < 7 Then Return localTime.ToString("dddd")
        Return localTime.ToString("d MMM")
    End Function

    ''' <summary>Starts downloading the pictures of the rows; each one redraws the list when it arrives.</summary>
    Private Sub PrefetchIcons(entries As List(Of SidebarEntry))
        Dim urls = entries.Where(Function(e) Not String.IsNullOrEmpty(e.IconUrl) AndAlso AvatarCache.TryGet(e.IconUrl) Is Nothing).
                           Select(Function(e) e.IconUrl).Distinct().ToList()
        If urls.Count = 0 Then Return
        Dim ignored As Task = FetchIconsAsync(urls)
    End Sub

    Private Async Function FetchIconsAsync(urls As List(Of String)) As Task
        Dim downloads = urls.Select(Function(u) AvatarCache.GetAsync(u)).ToList()
        For Each download In downloads
            Dim picture As Bitmap = Await download
            If picture IsNot Nothing AndAlso Not _list.IsDisposed Then _list.Invalidate()
        Next
    End Function

#End Region

#Region "Clicks"

    Private Async Sub OnEntryClicked(entry As SidebarEntry)
        Try
            Select Case entry.Kind
                Case EntryKind.Dm
                    Dim channel = DirectCast(entry.Tag, DiscordChannel)
                    RaiseEvent ChannelChosen(channel, channel.DisplayName)

                Case EntryKind.Channel
                    Dim channel = DirectCast(entry.Tag, DiscordChannel)
                    RaiseEvent ChannelChosen(channel, "# " & If(channel.Name, "") & If(_currentGuild IsNot Nothing, "  -  " & _currentGuild.Name, ""))

                Case EntryKind.Guild
                    Await OpenGuildAsync(DirectCast(entry.Tag, DiscordGuild))

                Case EntryKind.Back
                    _currentGuild = Nothing
                    Interlocked.Increment(_generation)
                    Render()

                Case EntryKind.Notice
                    Select Case entry.Key
                        Case "retry-dms" : Await ShowDirectMessagesAsync(forceReload:=True)
                        Case "retry-servers" : Await ShowServersAsync()
                        Case "retry-guild" : If _currentGuild IsNot Nothing Then Await OpenGuildAsync(_currentGuild)
                    End Select
            End Select
        Catch ex As Exception
            ' An Async Sub must never let an exception escape: it would take the whole app down.
            Debug.WriteLine("Sidebar click failed: " & ex.ToString())
        End Try
    End Sub

#End Region

End Class
