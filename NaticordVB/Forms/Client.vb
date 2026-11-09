Imports System.Net.Http
Imports System.Text.Json.Nodes
Imports NaticordVB.Glass
Imports NaticordVB.Networking

Public Class Client
    Inherits GlassForm

    ' The two pages that live in contentPanel. They are shown/hidden, never closed or disposed on the fly:
    ' StatusPage hosts DirectUI, and destroying a DirectUI host window ends the whole message loop.
    Private _statusPage As StatusPage
    Private _chatPage As ChatPage

    Private WithEvents _sidebar As SidebarController
    Private ReadOnly _dmRefreshTimer As New Timer() With {.Interval = 60000}

    ''' <summary>Puts a page inside contentPanel, filling it.</summary>
    Private Sub AddToContent(page As Form)
        page.TopLevel = False
        page.FormBorderStyle = FormBorderStyle.None
        page.Dock = DockStyle.Fill

        contentPanel.Controls.Add(page)
        page.Show()
    End Sub

    ''' <summary>Opens a conversation in the content area.</summary>
    Private Sub ShowChat(channel As Networking.Models.DiscordChannel, title As String)
        _statusPage.Hide()
        _chatPage.OpenChannel(channel, title)
        _chatPage.Show()
    End Sub

    Private Sub Sidebar_ChannelChosen(channel As Networking.Models.DiscordChannel, title As String) Handles _sidebar.ChannelChosen
        ShowChat(channel, title)
    End Sub

    Public Sub New()
        InitializeComponent()
        MinimumSize = New Size(800, 500)

        ' Apply the correct composition to the window!
        CompSet(DetectDwmStatus.IsDwmEnabled(), False, Nothing, Me)
        ' Apply the toolbar layout to the buttonPanel, like in SetupFlowPiece
        If My.Settings.layoutMode = LayoutWindows11 Then
            buttonPanel.Width -= 16
            buttonPanel.Left += 8
        End If
    End Sub

    Private Async Sub Client_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' "Hold on!" is shown until the user opens a conversation.
        _statusPage = New StatusPage()
        _chatPage = New ChatPage()
        AddToContent(_statusPage)
        AddToContent(_chatPage)
        _chatPage.Hide()

        _sidebar = New SidebarController(chatList)

        ' This is an Async Sub: anything thrown here would crash the whole app, so nothing may escape.
        Try
            Await SetUserInformation()
        Catch ex As Exception
            Debug.WriteLine("SetUserInformation failed: " & ex.ToString())
        End Try
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)

        ' (If enabled!) Disable the GitHub button on the toolbar
        If My.Settings.disableGitHubButton Then githubButton.Visible = False

        ' Configure the user interface to look nice on all different system metric sizes
        sidebarRenderer.SetCueBanner("searchBox", "Search...")
        sidebarRenderer.SetCueBannerItalic("searchBox")

        Dim screenArea As Rectangle = Screen.FromPoint(Cursor.Position).WorkingArea
        Location = New Point(
            screenArea.Left + (screenArea.Width - Width) \ 2,
            screenArea.Top + (screenArea.Height - Height) \ 2)

        buttonPanel.Top = 0
        Dim panelTop As Integer = If(DetectDwmStatus.IsDwmEnabled(), glassMargin.Top, buttonPanel.Bottom)

        ' The sidebar, top to bottom: tabs, the DirectUI search box, then the list. The last pixel column is a divider line.
        Const sidebarWidth As Integer = 250
        Dim innerWidth As Integer = LogicalToDeviceUnits(sidebarWidth) - 1
        Dim tabHeight As Integer = LogicalToDeviceUnits(30)
        Dim searchHeight As Integer = LogicalToDeviceUnits(31)

        tabPanel.SetBounds(0, panelTop, innerWidth, tabHeight)
        rbMessages.SetBounds(0, 0, innerWidth \ 2, tabHeight)
        rbServers.SetBounds(innerWidth \ 2, 0, innerWidth - innerWidth \ 2, tabHeight)
        sidebarRenderer.SetBounds(0, tabPanel.Bottom, innerWidth + 1, searchHeight)
        chatList.SetBounds(0, sidebarRenderer.Bottom, innerWidth, ClientSize.Height - sidebarRenderer.Bottom)
        sidebarDivider.SetBounds(innerWidth, panelTop, 1, ClientSize.Height - panelTop)

        contentPanel.SetBounds(innerWidth + 1, panelTop, ClientSize.Width - innerWidth - 1, ClientSize.Height - panelTop)

        ' Tab icons follow the chosen icon style (skeuomorphic or modern).
        Dim skeuo As Boolean = IconSuffix = "-skeuo"
        rbMessages.Image = If(skeuo, My.Resources.dm_skeuo, My.Resources.dm)
        rbServers.Image = If(skeuo, My.Resources.servers_skeuo, My.Resources.servers)

        ' Start on the Messages tab. Checking the box also loads the list (see the CheckedChanged handlers).
        rbMessages.Checked = True

        AddHandler _dmRefreshTimer.Tick, AddressOf OnDmRefreshTick
        _dmRefreshTimer.Start()
    End Sub

    Private Async Sub rbMessages_CheckedChanged(sender As Object, e As EventArgs) Handles rbMessages.CheckedChanged
        If Not rbMessages.Checked Then Return
        Try
            Await _sidebar.ShowDirectMessagesAsync()
        Catch ex As Exception
            Debug.WriteLine("Showing messages failed: " & ex.ToString())
        End Try
    End Sub

    Private Async Sub rbServers_CheckedChanged(sender As Object, e As EventArgs) Handles rbServers.CheckedChanged
        If Not rbServers.Checked Then Return
        Try
            Await _sidebar.ShowServersAsync()
        Catch ex As Exception
            Debug.WriteLine("Showing servers failed: " & ex.ToString())
        End Try
    End Sub

    ''' <summary>Quietly refreshes the DM order (new messages move a conversation up) while the Messages tab is showing.</summary>
    Private Async Sub OnDmRefreshTick(sender As Object, e As EventArgs)
        If _sidebar Is Nothing OrElse _sidebar.Mode <> SidebarMode.DirectMessages OrElse Not rbMessages.Checked Then Return
        If Not Visible OrElse WindowState = FormWindowState.Minimized Then Return
        Try
            Await _sidebar.ShowDirectMessagesAsync(forceReload:=True)
        Catch ex As Exception
            Debug.WriteLine("DM refresh failed: " & ex.ToString())
        End Try
    End Sub

    Private Async Function SetUserInformation() As Task
        ' Use saved details first before actually loading data from Discord
        usernameLabel.Text = My.Settings.dscUsername

        Dim result As ApiResult = Await API.Instance.RequestAsync("users/@me", HttpMethod.Get, My.Settings.dscToken)

        ' The window may have been closed while we were waiting for Discord.
        If IsDisposed Then Return

        If result.Error = ApiError.Unauthorized Then
            MessageBox.Show("Your token has expired or is invalid. NaticordVB will sign you out and you will need to sign in again.",
                            "NaticordVB", MessageBoxButtons.OK, MessageBoxIcon.Error)

            ' Wipe all Discord properties
            ClearDiscordSession()
            Application.Restart()
            Return
        End If

        If Not result.IsSuccess Then
            ' Offline, timed out, rate limited...: keep what we have saved instead of failing.
            Debug.WriteLine($"Could not refresh the user profile ({result.Error}): {result.ErrorMessage}")
            If result.IsNetworkFailure Then
                Text = "NaticordVB (offline)"
                If String.IsNullOrEmpty(usernameLabel.Text) Then usernameLabel.Text = "Offline"
            End If
            Return
        End If

        Dim parsedUser As JsonObject = TryCast(JsonNode.Parse(result.Body), JsonObject)
        If parsedUser Is Nothing Then Return

        Dim userId As String = parsedUser("id")?.GetValue(Of String)()
        Dim username As String = If(parsedUser("username")?.GetValue(Of String)(), "anonymous_user")
        Dim displayName As String = If(parsedUser("global_name")?.GetValue(Of String)(), username)

        ' Save properties to settings for use!
        My.Settings.dscUsername = displayName
        My.Settings.dscUid = userId
        My.Settings.Save()

        Text = "NaticordVB"
        usernameLabel.Text = displayName
        ' TODO: Add avatar support (parsedUser("avatar"))
    End Function

    Private Shared Sub ClearDiscordSession()
        My.Settings.dscToken = Nothing
        My.Settings.dscUsername = Nothing
        My.Settings.dscUid = Nothing
        My.Settings.Save()
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        _dmRefreshTimer.Stop()
        _dmRefreshTimer.Dispose()

        MyBase.OnFormClosed(e)
    End Sub

    Private Sub accountButton_ButtonClick(sender As Object, e As EventArgs) Handles accountButton.ButtonClick
        accountContext.Show(accountButton, New Point(0, accountButton.Height))
    End Sub

    Private Sub settingsButton_ButtonClick(sender As Object, e As EventArgs) Handles settingsButton.ButtonClick
        Using settingsPiece As New SettingsFlowPiece()
            settingsPiece.ShowDialog()
        End Using
    End Sub

    Private Sub logOutStripItem_Click(sender As Object, e As EventArgs) Handles logOutStripItem.Click
        ' Wipe all Discord properties
        ClearDiscordSession()

        Dim resetAllDlg As DialogResult = MessageBox.Show("Do you want to reset all of your NaticordVB preferences too?",
                                                          "NaticordVB", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
        If resetAllDlg = DialogResult.Yes Then
            ' Wipe all of the NaticordVB settings to their defaults
            My.Settings.renderMode = Nothing
            My.Settings.hasCompletedSetup = False
            My.Settings.iconStyle = IconStyleSkeuomorphic
            My.Settings.layoutMode = Nothing
            My.Settings.disableGitHubButton = False
            My.Settings.Save()
        End If

        Application.Restart()
    End Sub

End Class
