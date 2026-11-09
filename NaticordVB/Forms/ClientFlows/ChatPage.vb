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

    Public Sub New()
        InitializeComponent()
        titleLabel.Font = New Font(SystemFonts.MessageBoxFont.FontFamily, SystemFonts.MessageBoxFont.Size + 3.0F, FontStyle.Regular)
        messageView.Font = SystemFonts.MessageBoxFont
        AddHandler _pollTimer.Tick, AddressOf OnPollTick
        AddHandler Disposed, Sub(sender, e) _pollTimer.Dispose()
    End Sub

    ''' <summary>Lets "#channel" mentions in messages show the channel's name.</summary>
    Friend Sub SetChannelNameLookup(lookup As Func(Of String, String))
        messageView.ChannelNameLookup = lookup
    End Sub

    ''' <summary>Switches the page to another conversation and loads its latest messages.</summary>
    Friend Async Function OpenChannelAsync(newChannel As DiscordChannel, title As String) As Task
        titleLabel.Text = title

        ' Clicking the chat that is already open must not reload it.
        If Channel IsNot Nothing AndAlso Channel.Id = newChannel.Id AndAlso _loaded Then Return

        Channel = newChannel
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
    End Function

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
            End If
            ' A failed poll is simply tried again on the next tick.
        Catch ex As Exception
            Debug.WriteLine("Polling failed: " & ex.ToString())
        Finally
            _polling = False
        End Try
    End Sub

End Class
