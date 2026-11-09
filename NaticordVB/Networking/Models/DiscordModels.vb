Imports System.Runtime.CompilerServices
Imports System.Text.Json

Namespace Networking.Models

    ''' <summary>Channel types from the Docs ("Channels > Channel Types").</summary>
    Friend Enum ChannelType As Integer
        GuildText = 0
        Dm = 1
        GuildVoice = 2
        GroupDm = 3
        GuildCategory = 4
        GuildAnnouncement = 5
        AnnouncementThread = 10
        PublicThread = 11
        PrivateThread = 12
        GuildStageVoice = 13
        GuildDirectory = 14
        GuildForum = 15
        GuildMedia = 16
    End Enum

    ''' <summary>Tolerant JSON readers: a missing, null or wrongly typed field gives a default instead of throwing.</summary>
    Friend Module JsonRead

        <Extension>
        Friend Function GetStr(el As JsonElement, name As String) As String
            Dim v As JsonElement
            If el.ValueKind = JsonValueKind.Object AndAlso el.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.String Then Return v.GetString()
            Return Nothing
        End Function

        <Extension>
        Friend Function GetInt(el As JsonElement, name As String, Optional fallback As Integer = 0) As Integer
            Dim v As JsonElement
            Dim n As Integer
            If el.ValueKind = JsonValueKind.Object AndAlso el.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.Number AndAlso v.TryGetInt32(n) Then Return n
            Return fallback
        End Function

        <Extension>
        Friend Function GetBool(el As JsonElement, name As String) As Boolean
            Dim v As JsonElement
            Return el.ValueKind = JsonValueKind.Object AndAlso el.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.True
        End Function

        ''' <summary>The items of an array property, or an empty sequence when it is missing or not an array.</summary>
        <Extension>
        Friend Function GetItems(el As JsonElement, name As String) As IEnumerable(Of JsonElement)
            Dim v As JsonElement
            If el.ValueKind = JsonValueKind.Object AndAlso el.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.Array Then Return v.EnumerateArray().ToList()
            Return Enumerable.Empty(Of JsonElement)()
        End Function

    End Module

    Friend NotInheritable Class DiscordUser
        Public Property Id As String = ""
        Public Property Username As String = ""
        Public Property GlobalName As String
        Public Property Avatar As String
        Public Property Discriminator As String = "0"
        Public Property IsBot As Boolean

        ''' <summary>What Discord shows: the display name, falling back to the username.</summary>
        Public ReadOnly Property DisplayName As String
            Get
                If Not String.IsNullOrEmpty(GlobalName) Then Return GlobalName
                If Not String.IsNullOrEmpty(Username) Then Return Username
                Return "Unknown user"
            End Get
        End Property

        Friend Shared Function FromJson(el As JsonElement) As DiscordUser
            Return New DiscordUser With {
                .Id = If(el.GetStr("id"), ""),
                .Username = If(el.GetStr("username"), ""),
                .GlobalName = el.GetStr("global_name"),
                .Avatar = el.GetStr("avatar"),
                .Discriminator = If(el.GetStr("discriminator"), "0"),
                .IsBot = el.GetBool("bot")
            }
        End Function
    End Class

    Friend NotInheritable Class DiscordChannel
        Public Property Id As String = ""
        Public Property Type As ChannelType
        Public Property GuildId As String
        Public Property Name As String
        Public Property ParentId As String
        Public Property Position As Integer
        Public Property LastMessageId As String
        Public Property Topic As String
        Public Property Icon As String
        Public Property Recipients As New List(Of DiscordUser)()

        Public ReadOnly Property IsDirect As Boolean
            Get
                Return Type = ChannelType.Dm OrElse Type = ChannelType.GroupDm
            End Get
        End Property

        ''' <summary>A channel you can read and write messages in (a DM, a group DM, a text or an announcement channel).</summary>
        Public ReadOnly Property IsChat As Boolean
            Get
                Return IsDirect OrElse Type = ChannelType.GuildText OrElse Type = ChannelType.GuildAnnouncement
            End Get
        End Property

        ''' <summary>Snowflake of the newest message as a number, for sorting. 0 when the channel has no messages.</summary>
        Public ReadOnly Property LastMessageValue As ULong
            Get
                Dim v As ULong
                Return If(Snowflake.TryParse(LastMessageId, v), v, 0UL)
            End Get
        End Property

        ''' <summary>The name to show in lists: the other person for a DM, the member names for an unnamed group, "name" for a guild channel.</summary>
        Public ReadOnly Property DisplayName As String
            Get
                If Type = ChannelType.Dm Then
                    Return If(Recipients.Count > 0, Recipients(0).DisplayName, "Unknown user")
                End If
                If Type = ChannelType.GroupDm Then
                    If Not String.IsNullOrWhiteSpace(Name) Then Return Name
                    Dim names = Recipients.Select(Function(r) r.DisplayName).ToList()
                    If names.Count = 0 Then Return "Group chat"
                    Return String.Join(", ", names.Take(3)) & If(names.Count > 3, ", ...", "")
                End If
                Return If(Name, "unnamed")
            End Get
        End Property

        Friend Shared Function FromJson(el As JsonElement) As DiscordChannel
            Return New DiscordChannel With {
                .Id = If(el.GetStr("id"), ""),
                .Type = CType(el.GetInt("type", 0), ChannelType),
                .GuildId = el.GetStr("guild_id"),
                .Name = el.GetStr("name"),
                .ParentId = el.GetStr("parent_id"),
                .Position = el.GetInt("position", 0),
                .LastMessageId = el.GetStr("last_message_id"),
                .Topic = el.GetStr("topic"),
                .Icon = el.GetStr("icon"),
                .Recipients = el.GetItems("recipients").Select(AddressOf DiscordUser.FromJson).ToList()
            }
        End Function
    End Class

    Friend NotInheritable Class DiscordGuild
        Public Property Id As String = ""
        Public Property Name As String = ""
        Public Property Icon As String
        Public Property IsOwner As Boolean
        Public Property Permissions As ULong

        Friend Shared Function FromJson(el As JsonElement) As DiscordGuild
            Dim perms As ULong
            ULong.TryParse(el.GetStr("permissions"), perms)
            Return New DiscordGuild With {
                .Id = If(el.GetStr("id"), ""),
                .Name = If(el.GetStr("name"), "Unnamed server"),
                .Icon = el.GetStr("icon"),
                .IsOwner = el.GetBool("owner"),
                .Permissions = perms
            }
        End Function
    End Class

    Friend NotInheritable Class DiscordAttachment
        Public Property Filename As String = "file"
        Public Property Url As String = ""
        Public Property Size As Long

        Friend Shared Function FromJson(el As JsonElement) As DiscordAttachment
            Dim size As Long
            Dim v As JsonElement
            If el.TryGetProperty("size", v) AndAlso v.ValueKind = JsonValueKind.Number Then v.TryGetInt64(size)
            Return New DiscordAttachment With {
                .Filename = If(el.GetStr("filename"), "file"),
                .Url = If(el.GetStr("url"), ""),
                .Size = size
            }
        End Function
    End Class

    ''' <summary>Only what is needed to show an embed as a link line: its title and address.</summary>
    Friend NotInheritable Class DiscordEmbed
        Public Property Title As String
        Public Property Url As String

        Friend Shared Function FromJson(el As JsonElement) As DiscordEmbed
            Return New DiscordEmbed With {.Title = el.GetStr("title"), .Url = el.GetStr("url")}
        End Function
    End Class

    ''' <summary>Message types from the Docs ("Message > Message Types") that need special treatment.</summary>
    Friend Enum MessageType As Integer
        [Default] = 0
        RecipientAdd = 1
        RecipientRemove = 2
        CallStarted = 3
        ChannelNameChange = 4
        ChannelIconChange = 5
        ChannelPinnedMessage = 6
        UserJoin = 7
        Reply = 19
        ChatInputCommand = 20
        ThreadStarterMessage = 21
        ContextMenuCommand = 23
    End Enum

    Friend NotInheritable Class DiscordMessage
        Public Property Id As String = ""
        Public Property ChannelId As String = ""
        Public Property Type As Integer
        Public Property Content As String = ""
        Public Property Timestamp As DateTimeOffset?
        Public Property EditedTimestamp As DateTimeOffset?
        Public Property Author As New DiscordUser()
        Public Property Mentions As New List(Of DiscordUser)()
        Public Property Attachments As New List(Of DiscordAttachment)()
        Public Property Embeds As New List(Of DiscordEmbed)()
        Public Property StickerNames As New List(Of String)()
        ''' <summary>The message this one replies to, when Discord included it. Nothing for ordinary messages, or when it was deleted.</summary>
        Public Property ReferencedMessage As DiscordMessage
        ''' <summary>The id of the replied-to message, even when its content is not available.</summary>
        Public Property ReferencedMessageId As String

        ''' <summary>A normal chat message (as opposed to "X joined", "X pinned a message", call notices...).</summary>
        Public ReadOnly Property IsRegular As Boolean
            Get
                Select Case Type
                    Case MessageType.Default, MessageType.Reply, MessageType.ChatInputCommand, MessageType.ThreadStarterMessage, MessageType.ContextMenuCommand
                        Return True
                    Case Else
                        Return False
                End Select
            End Get
        End Property

        Private Shared Function ReadTime(el As JsonElement, name As String) As DateTimeOffset?
            Dim text As String = el.GetStr(name)
            Dim parsed As DateTimeOffset
            If text IsNot Nothing AndAlso DateTimeOffset.TryParse(text, Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, parsed) Then Return parsed
            Return Nothing
        End Function

        ''' <summary>Reads a message. <c>depth</c> stops us from following reply chains more than one step.</summary>
        Friend Shared Function FromJson(el As JsonElement, Optional depth As Integer = 0) As DiscordMessage
            Dim author As JsonElement
            Dim message As New DiscordMessage With {
                .Id = If(el.GetStr("id"), ""),
                .ChannelId = If(el.GetStr("channel_id"), ""),
                .Type = el.GetInt("type", 0),
                .Content = If(el.GetStr("content"), ""),
                .Timestamp = ReadTime(el, "timestamp"),
                .EditedTimestamp = ReadTime(el, "edited_timestamp"),
                .Mentions = el.GetItems("mentions").Select(AddressOf DiscordUser.FromJson).ToList(),
                .Attachments = el.GetItems("attachments").Select(AddressOf DiscordAttachment.FromJson).ToList(),
                .Embeds = el.GetItems("embeds").Select(AddressOf DiscordEmbed.FromJson).ToList(),
                .StickerNames = el.GetItems("sticker_items").Select(Function(s) If(s.GetStr("name"), "sticker")).ToList()
            }

            If el.TryGetProperty("author", author) AndAlso author.ValueKind = JsonValueKind.Object Then message.Author = DiscordUser.FromJson(author)

            Dim reference As JsonElement
            If el.TryGetProperty("message_reference", reference) AndAlso reference.ValueKind = JsonValueKind.Object Then
                message.ReferencedMessageId = reference.GetStr("message_id")
            End If

            Dim referenced As JsonElement
            If depth = 0 AndAlso el.TryGetProperty("referenced_message", referenced) AndAlso referenced.ValueKind = JsonValueKind.Object Then
                message.ReferencedMessage = FromJson(referenced, depth + 1)
            End If

            Return message
        End Function
    End Class


    ''' <summary>The custom status line ("Studying", with an emoji) that shows under your name.</summary>
    Friend NotInheritable Class CustomStatus
        Public Property Text As String
        ''' <summary>A Unicode emoji such as a smiley face. (Server emoji are not supported yet.)</summary>
        Public Property EmojiName As String
        ''' <summary>When it clears itself, or Nothing if it stays until you remove it.</summary>
        Public Property ExpiresAt As DateTimeOffset?

        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return String.IsNullOrWhiteSpace(Text) AndAlso String.IsNullOrWhiteSpace(EmojiName)
            End Get
        End Property

        ''' <summary>"emoji text", as shown in menus.</summary>
        Public Overrides Function ToString() As String
            Return (If(EmojiName, "") & " " & If(Text, "")).Trim()
        End Function

        Friend Shared Function FromJson(el As JsonElement) As CustomStatus
            If el.ValueKind <> JsonValueKind.Object Then Return Nothing
            Dim expires As DateTimeOffset
            Dim text As String = el.GetStr("expires_at")
            Dim status As New CustomStatus With {.Text = el.GetStr("text"), .EmojiName = el.GetStr("emoji_name")}
            If text IsNot Nothing AndAlso DateTimeOffset.TryParse(text, Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, expires) Then status.ExpiresAt = expires
            Return If(status.IsEmpty, Nothing, status)
        End Function
    End Class

    ''' <summary>Your presence: "online", "idle", "dnd" (do not disturb) or "invisible", plus the custom status.</summary>
    Friend NotInheritable Class PresenceSettings
        Public Property Status As String = "online"
        Public Property Custom As CustomStatus
    End Class

End Namespace
