Imports System.Text.RegularExpressions
Imports NaticordVB.Networking.Models

''' <summary>
''' Turns what Discord sends in a message into text people can read: <c>&lt;@123&gt;</c> becomes "@name", custom emoji become ":name:",
''' <c>&lt;t:1700000000&gt;</c> becomes a local time, and so on. (Docs: "API Reference > Message Formatting".)
''' Markdown such as **bold** is not handled here.
''' </summary>
Friend Module MessageFormatter

    Private ReadOnly UserMention As New Regex("<@!?(\d+)>", RegexOptions.Compiled)
    Private ReadOnly RoleMention As New Regex("<@&(\d+)>", RegexOptions.Compiled)
    Private ReadOnly ChannelMention As New Regex("<#(\d+)>", RegexOptions.Compiled)
    Private ReadOnly CustomEmoji As New Regex("<a?:(\w+):\d+>", RegexOptions.Compiled)
    Private ReadOnly TimestampTag As New Regex("<t:(-?\d+)(?::([tTdDfFsSR]))?>", RegexOptions.Compiled)
    Private ReadOnly SlashCommand As New Regex("</([\w -]+):\d+>", RegexOptions.Compiled)

    ''' <summary>The message text with mentions, emoji and timestamps made readable.</summary>
    ''' <param name="channelName">Looks up a channel's name by id; may return Nothing when it is unknown.</param>
    Friend Function ToDisplayText(message As DiscordMessage, channelName As Func(Of String, String)) As String
        Dim text As String = If(message.Content, "")
        If text.Length = 0 Then Return text

        text = UserMention.Replace(text, Function(m)
                                             Dim id As String = m.Groups(1).Value
                                             Dim user = message.Mentions.FirstOrDefault(Function(u) u.Id = id)
                                             Return "@" & If(user IsNot Nothing, user.DisplayName, "unknown-user")
                                         End Function)
        text = RoleMention.Replace(text, "@role")
        text = ChannelMention.Replace(text, Function(m)
                                                Dim name As String = channelName?.Invoke(m.Groups(1).Value)
                                                Return "#" & If(String.IsNullOrEmpty(name), "unknown-channel", name)
                                            End Function)
        text = CustomEmoji.Replace(text, ":$1:")
        text = SlashCommand.Replace(text, "/$1")
        text = TimestampTag.Replace(text, Function(m)
                                              Dim seconds As Long
                                              If Not Long.TryParse(m.Groups(1).Value, seconds) Then Return m.Value
                                              Return FormatTimestamp(seconds, If(m.Groups(2).Success, m.Groups(2).Value, "f"))
                                          End Function)
        Return text
    End Function

    ''' <summary>The one-line text for a message that isn't chat: "X joined the server.", "X pinned a message..." (without the author's name).</summary>
    Friend Function SystemText(message As DiscordMessage) As String
        Select Case message.Type
            Case MessageType.RecipientAdd
                Dim added = message.Mentions.FirstOrDefault()
                Return "added " & If(added IsNot Nothing, added.DisplayName, "someone") & " to the group."
            Case MessageType.RecipientRemove : Return "left the group."
            Case MessageType.CallStarted : Return "started a call."
            Case MessageType.ChannelNameChange : Return "changed the channel name: " & message.Content
            Case MessageType.ChannelIconChange : Return "changed the group icon."
            Case MessageType.ChannelPinnedMessage : Return "pinned a message to this channel."
            Case MessageType.UserJoin : Return "joined the server."
            Case 8, 9, 10, 11 : Return "boosted the server."
            Case 18 : Return "started a thread: " & message.Content
            Case Else : Return "sent a system message."
        End Select
    End Function

    ''' <summary>"report.pdf (1.4 MB)"</summary>
    Friend Function DescribeAttachment(attachment As DiscordAttachment) As String
        Return attachment.Filename & " (" & FormatSize(attachment.Size) & ")"
    End Function

    Friend Function FormatSize(bytes As Long) As String
        If bytes < 1024 Then Return bytes & " B"
        If bytes < 1024L * 1024 Then Return (bytes / 1024.0).ToString("0.#") & " KB"
        If bytes < 1024L * 1024 * 1024 Then Return (bytes / (1024.0 * 1024)).ToString("0.#") & " MB"
        Return (bytes / (1024.0 * 1024 * 1024)).ToString("0.##") & " GB"
    End Function

    ''' <summary>Discord's &lt;t:seconds:style&gt; in local time. Styles: t T d D f F s S R.</summary>
    Private Function FormatTimestamp(seconds As Long, style As String) As String
        Dim moment As DateTime
        Try
            moment = DateTimeOffset.FromUnixTimeSeconds(seconds).LocalDateTime
        Catch ex As ArgumentOutOfRangeException
            Return "(invalid time)"
        End Try

        Select Case style
            Case "t" : Return moment.ToString("t")
            Case "T" : Return moment.ToString("T")
            Case "d" : Return moment.ToString("d")
            Case "D" : Return moment.ToString("D")
            Case "F" : Return moment.ToString("D") & " " & moment.ToString("t")
            Case "s" : Return moment.ToString("d") & " " & moment.ToString("t")
            Case "S" : Return moment.ToString("d") & " " & moment.ToString("T")
            Case "R" : Return Relative(moment)
            Case Else : Return moment.ToString("D") & " " & moment.ToString("t") ' "f", the default
        End Select
    End Function

    Private Function Relative(moment As DateTime) As String
        Dim diff As TimeSpan = DateTime.Now - moment
        Dim future As Boolean = diff < TimeSpan.Zero
        Dim span As TimeSpan = diff.Duration()

        Dim amount As String
        If span.TotalSeconds < 45 Then
            amount = "a few seconds"
        ElseIf span.TotalMinutes < 90 Then
            amount = Plural(Math.Max(1, CInt(Math.Round(span.TotalMinutes))), "minute")
        ElseIf span.TotalHours < 36 Then
            amount = Plural(CInt(Math.Round(span.TotalHours)), "hour")
        ElseIf span.TotalDays < 30 Then
            amount = Plural(CInt(Math.Round(span.TotalDays)), "day")
        ElseIf span.TotalDays < 365 Then
            amount = Plural(CInt(Math.Round(span.TotalDays / 30)), "month")
        Else
            amount = Plural(CInt(Math.Round(span.TotalDays / 365)), "year")
        End If

        Return If(future, "in " & amount, amount & " ago")
    End Function

    Private Function Plural(n As Integer, unit As String) As String
        Return n & " " & unit & If(n = 1, "", "s")
    End Function

End Module
