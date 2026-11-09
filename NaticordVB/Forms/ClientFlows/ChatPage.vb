Imports System.ComponentModel

Imports NaticordVB.Networking
Imports NaticordVB.Networking.Models

''' <summary>
''' The panel that shows one conversation: the chat name on top and the messages below.
''' New messages are picked up by asking Discord every few seconds while the window is in front.
''' </summary>
Public Class ChatPage

    Private Const PageSize As Integer = 50

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property Channel As DiscordChannel

    Private ReadOnly _pollTimer As New System.Windows.Forms.Timer() With {.Interval = 4000}

    ' Opening another chat bumps this number, so answers for the chat we just left are thrown away.
    Private _generation As Integer
    Private _loaded As Boolean
    Private _polling As Boolean
    Private _loadingEarlier As Boolean
    Private _oldestId As String
    Private _newestId As String

    Private Const MaxMessageLength As Integer = 2000
    Private Const MaxComposerLines As Integer = 6

    Private _replyTo As DiscordMessage
    Private _sending As Boolean
    ' Text typed but not sent, per chat, so switching chats never sends it to the wrong place.
    Private ReadOnly _drafts As New Dictionary(Of String, String)()

    ''' <summary>The chat was displayed up to its newest message (so it counts as read now).</summary>
    Friend Event ChannelRead(channelId As String)

    Public Sub New()
        InitializeComponent()
        titleLabel.Font = New Font(SystemFonts.MessageBoxFont.FontFamily, SystemFonts.MessageBoxFont.Size + 3.0F, FontStyle.Regular)
        messageView.Font = SystemFonts.MessageBoxFont
        inputBox.Font = SystemFonts.MessageBoxFont
        replyLabel.Font = SystemFonts.MessageBoxFont
        errorLabel.Font = SystemFonts.MessageBoxFont
        AddHandler _pollTimer.Tick, AddressOf OnPollTick
        AddHandler Disposed, Sub(sender, e) _pollTimer.Dispose()
        AddHandler composerPanel.SizeChanged, Sub(sender, e) LayoutComposer()
        LayoutComposer()
    End Sub

#Region "Composer"

    ''' <summary>Places the reply bar, error line, text box and Send button, and sizes the panel to fit (the text box grows with the text).</summary>
    Private Sub LayoutComposer()
        Dim pad As Integer = composerPanel.LogicalToDeviceUnits(10)
        Dim width As Integer = Math.Max(0, composerPanel.ClientSize.Width - 2 * pad)
        Dim y As Integer = composerPanel.LogicalToDeviceUnits(6)

        If replyBar.Visible Then
            replyBar.SetBounds(pad, y, width, composerPanel.LogicalToDeviceUnits(26))
            y += replyBar.Height + composerPanel.LogicalToDeviceUnits(4)
        End If
        If errorLabel.Visible Then
            errorLabel.SetBounds(pad, y, width, composerPanel.LogicalToDeviceUnits(20))
            y += errorLabel.Height + composerPanel.LogicalToDeviceUnits(4)
        End If

        Dim lines As Integer = 1
        If inputBox.TextLength > 0 Then lines = Math.Clamp(inputBox.GetLineFromCharIndex(inputBox.TextLength) + 1, 1, MaxComposerLines)
        Dim inputHeight As Integer = lines * inputBox.Font.Height + composerPanel.LogicalToDeviceUnits(10)
        Dim buttonWidth As Integer = composerPanel.LogicalToDeviceUnits(76)
        Dim buttonHeight As Integer = composerPanel.LogicalToDeviceUnits(28)
        inputHeight = Math.Max(inputHeight, buttonHeight)

        inputBox.SetBounds(pad, y, Math.Max(0, width - buttonWidth - composerPanel.LogicalToDeviceUnits(8)), inputHeight)
        sendButton.SetBounds(pad + width - buttonWidth, y + inputHeight - buttonHeight, buttonWidth, buttonHeight)
        inputBox.ScrollBars = If(inputBox.GetLineFromCharIndex(inputBox.TextLength) + 1 > MaxComposerLines, ScrollBars.Vertical, ScrollBars.None)

        composerPanel.Height = y + inputHeight + pad
    End Sub

    Private Sub ShowError(text As String)
        errorLabel.Text = text
        errorLabel.Visible = True
        LayoutComposer()
    End Sub

    Private Sub ClearError()
        If Not errorLabel.Visible Then Return
        errorLabel.Visible = False
        LayoutComposer()
    End Sub

    Private Sub UpdateSendButton()
        sendButton.Enabled = Not _sending AndAlso Channel IsNot Nothing AndAlso inputBox.Text.Trim().Length > 0
    End Sub

    Private Sub inputBox_TextChanged(sender As Object, e As EventArgs) Handles inputBox.TextChanged
        UpdateSendButton()
        ClearError()
        LayoutComposer()
    End Sub

    ''' <summary>Enter sends, Shift+Enter starts a new line, Esc cancels a reply.</summary>
    Private Async Sub inputBox_KeyDown(sender As Object, e As KeyEventArgs) Handles inputBox.KeyDown
        If e.KeyCode = Keys.Enter AndAlso Not e.Shift Then
            e.SuppressKeyPress = True
            e.Handled = True
            Try
                Await SendAsync()
            Catch ex As Exception
                Debug.WriteLine("Sending failed: " & ex.ToString())
            End Try
        ElseIf e.KeyCode = Keys.Escape AndAlso _replyTo IsNot Nothing Then
            e.SuppressKeyPress = True
            e.Handled = True
            CancelReply()
        End If
    End Sub

    Private Async Sub sendButton_Click(sender As Object, e As EventArgs) Handles sendButton.Click
        Try
            Await SendAsync()
        Catch ex As Exception
            Debug.WriteLine("Sending failed: " & ex.ToString())
        End Try
    End Sub

    Private Sub cancelReplyButton_Click(sender As Object, e As EventArgs) Handles cancelReplyButton.Click
        CancelReply()
        inputBox.Focus()
    End Sub

    Private Sub MessageView_ReplyRequested(message As DiscordMessage) Handles messageView.ReplyRequested
        BeginReply(message)
    End Sub

    Private Sub BeginReply(message As DiscordMessage)
        _replyTo = message
        Dim snippet As String = MessageFormatter.ToDisplayText(message, messageView.ChannelNameLookup).Replace(vbCr, " ").Replace(vbLf, " ").Trim()
        If snippet.Length > 60 Then snippet = snippet.Substring(0, 60) & ChrW(&H2026)
        replyLabel.Text = "Replying to " & message.Author.DisplayName & If(snippet.Length > 0, ": " & snippet, "")
        replyBar.Visible = True
        LayoutComposer()
        inputBox.Focus()
    End Sub

    Private Sub CancelReply()
        _replyTo = Nothing
        If replyBar.Visible Then
            replyBar.Visible = False
            LayoutComposer()
        End If
    End Sub

    Private Async Function SendAsync() As Task
        Dim target As DiscordChannel = Channel
        If _sending OrElse target Is Nothing Then Return

        ' A TextBox uses Windows line breaks; Discord uses a plain line feed.
        Dim text As String = inputBox.Text.Replace(vbCrLf, vbLf).Trim()
        If text.Length = 0 Then Return
        If text.Length > MaxMessageLength Then
            ShowError($"Messages can be at most {MaxMessageLength} characters; yours has {text.Length}.")
            Return
        End If

        _sending = True
        UpdateSendButton()
        ClearError()
        Dim replyId As String = _replyTo?.Id

        Try
            Dim sent = Await DiscordService.SendMessageAsync(target.Id, text, replyId, target.GuildId)
            If IsDisposed Then Return

            If Not sent.IsSuccess Then
                ' Show the problem only if this chat is still the one on screen; the text stays so nothing is lost.
                If target Is Channel Then ShowError(DescribeSendFailure(sent))
                Return
            End If

            ReadTracker.MarkRead(target.Id, sent.Value.Id)
            If target Is Channel Then
                inputBox.Clear()
                CancelReply()
                If Snowflake.Compare(sent.Value.Id, _newestId) > 0 Then _newestId = sent.Value.Id
                messageView.AppendMessages({sent.Value})
                messageView.ScrollToBottom()
                RaiseEvent ChannelRead(target.Id)
            Else
                _drafts.Remove(target.Id) ' it was sent after the user moved on: the draft is no longer a draft
                RaiseEvent ChannelRead(target.Id)
            End If
        Finally
            _sending = False
            UpdateSendButton()
            If Not IsDisposed AndAlso inputBox.CanFocus Then inputBox.Focus()
        End Try
    End Function

    Private Shared Function DescribeSendFailure(sent As Fetched(Of DiscordMessage)) As String
        Select Case sent.Result.Error
            Case ApiError.Forbidden : Return "You don't have permission to send messages here."
            Case ApiError.RateLimited : Return "You're sending messages too fast. Try again in " & Math.Ceiling(Math.Max(1.0, sent.Result.RetryAfterSeconds)).ToString() & " s."
            Case ApiError.Timeout : Return "Discord didn't answer, so it's unclear whether the message was sent. Check the chat before sending it again."
            Case ApiError.Offline : Return "Can't reach Discord. Your message is still here; try again."
            Case ApiError.Unauthorized : Return "Your session has expired. Sign in again."
            Case Else : Return "Couldn't send the message: " & sent.Result.ErrorMessage
        End Select
    End Function

#End Region

    ''' <summary>Lets "#channel" mentions in messages show the channel's name.</summary>
    Friend Sub SetChannelNameLookup(lookup As Func(Of String, String))
        messageView.ChannelNameLookup = lookup
    End Sub

    ''' <summary>Switches the page to another conversation and loads its latest messages.</summary>
    Friend Async Function OpenChannelAsync(newChannel As DiscordChannel, title As String) As Task
        titleLabel.Text = title

        ' Clicking the chat that is already open must not reload it.
        If Channel IsNot Nothing AndAlso Channel.Id = newChannel.Id AndAlso _loaded Then Return

        ' Keep what was typed for the chat we are leaving, and bring back the draft of the one we are opening.
        If Channel IsNot Nothing Then
            If inputBox.Text.Length > 0 Then _drafts(Channel.Id) = inputBox.Text Else _drafts.Remove(Channel.Id)
        End If
        CancelReply()
        ClearError()
        Dim draft As String = Nothing
        inputBox.Text = If(_drafts.TryGetValue(newChannel.Id, draft), draft, "")

        Channel = newChannel
        UpdateSendButton()
        _pollTimer.Stop()
        _loaded = False
        _oldestId = Nothing
        _newestId = Nothing
        Dim generation As Integer = System.Threading.Interlocked.Increment(_generation)

        messageView.ShowNotice("Loading messages...")
        Dim fetched = Await DiscordService.GetMessagesAsync(newChannel.Id, PageSize)
        If generation <> _generation OrElse IsDisposed Then Return

        If Not fetched.IsSuccess Then
            messageView.ShowNotice(fetched.FriendlyError & " Click to try again.", clickable:=True)
            Return
        End If

        Dim messages As List(Of DiscordMessage) = fetched.Value
        If messages.Count > 0 Then
            _oldestId = messages.First().Id
            _newestId = messages.Last().Id
        End If
        messageView.SetMessages(messages, hasEarlier:=messages.Count >= PageSize)
        _loaded = True
        _pollTimer.Start()
        MarkViewedAsRead()
    End Function

    ''' <summary>Everything up to the newest message we hold is on screen, so remember that and let the sidebar drop its "unread" mark.</summary>
    Private Sub MarkViewedAsRead()
        If Channel Is Nothing OrElse _newestId Is Nothing Then Return
        ReadTracker.MarkRead(Channel.Id, _newestId)
        RaiseEvent ChannelRead(Channel.Id)
    End Sub

    Private Async Sub MessageView_NoticeClicked() Handles messageView.NoticeClicked
        If Channel Is Nothing Then Return
        Try
            Await OpenChannelAsync(Channel, titleLabel.Text)
        Catch ex As Exception
            Debug.WriteLine("Retry failed: " & ex.ToString())
        End Try
    End Sub

    Private Async Sub MessageView_LoadEarlierRequested() Handles messageView.LoadEarlierRequested
        If _loadingEarlier OrElse Channel Is Nothing OrElse _oldestId Is Nothing Then Return
        _loadingEarlier = True
        Dim generation As Integer = _generation

        Try
            Dim fetched = Await DiscordService.GetMessagesAsync(Channel.Id, PageSize, before:=_oldestId)
            If generation <> _generation OrElse IsDisposed Then Return

            ' If it failed the "Load earlier messages" row stays, so the user can simply click it again.
            If fetched.IsSuccess Then
                If fetched.Value.Count > 0 Then _oldestId = fetched.Value.First().Id
                messageView.PrependMessages(fetched.Value, hasEarlier:=fetched.Value.Count >= PageSize)
            End If
        Catch ex As Exception
            Debug.WriteLine("Loading earlier messages failed: " & ex.ToString())
        Finally
            _loadingEarlier = False
        End Try
    End Sub

    ''' <summary>Picks up messages sent since the last one we have. Does nothing while the app is in the background.</summary>
    Private Async Sub OnPollTick(sender As Object, e As EventArgs)
        If Not _loaded OrElse _polling OrElse Channel Is Nothing OrElse Not Visible Then Return
        If Form.ActiveForm Is Nothing Then Return ' another program is in front: don't poll for nothing

        _polling = True
        Dim generation As Integer = _generation
        Try
            Dim fetched = Await DiscordService.GetMessagesAsync(Channel.Id, PageSize, after:=_newestId)
            If generation <> _generation OrElse IsDisposed Then Return

            If fetched.IsSuccess AndAlso fetched.Value.Count > 0 Then
                _newestId = fetched.Value.Last().Id
                messageView.AppendMessages(fetched.Value)
                MarkViewedAsRead()
            End If
            ' A failed poll is simply tried again on the next tick.
        Catch ex As Exception
            Debug.WriteLine("Polling failed: " & ex.ToString())
        Finally
            _polling = False
        End Try
    End Sub

End Class
