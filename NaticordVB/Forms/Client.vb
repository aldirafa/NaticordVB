Imports System.Net.Http
Imports System.Text.Json.Nodes
Imports NaticordVB.Glass
Imports NaticordVB.Networking

Public Class Client
    Inherits GlassForm

    Private ReadOnly dscToken As String = My.Settings.dscToken

    Private ReadOnly _friendItems As New List(Of DuiListItem)()

    Private _hDmIcon As IntPtr
    Private _hServerIcon As IntPtr

    Public Sub LoadFormIntoPanel(form As Form)
        ' Copy first: disposing a child removes it from the collection we'd be enumerating.
        For Each c As Control In contentPanel.Controls.Cast(Of Control)().ToList()
            c.Dispose()
        Next
        contentPanel.Controls.Clear()

        form.TopLevel = False
        form.FormBorderStyle = FormBorderStyle.None
        form.Dock = DockStyle.Fill

        contentPanel.Controls.Add(form)
        form.Show()
    End Sub

    Private Sub SetSidebarIcon(baseName As String, elementId As String, ByRef hCache As IntPtr)
        ReleaseHBitmap(hCache)
        hCache = LoadExtResourceHBitmap(baseName)

        sidebarRenderer.DuiWindow?.SetContentBitmap(elementId, hCache)
    End Sub

    Friend Function AddFriendItem(username As String, statusText As String, avatar As Bitmap, id As Integer, statusCode As Integer,
                                  Optional extraStatusCode As Integer = 5) As DuiListItem
        Dim duiWin = sidebarRenderer.DuiWindow
        If duiWin Is Nothing Then Return Nothing

        Dim listItem As New DuiListItem(id)

        Using duiWin.BeginDefer()
            listItem.Mount(duiWin, "friendList")
            _friendItems.Add(listItem)

            listItem.SetUsername(username)
            listItem.SetStatus(statusText)

            If avatar IsNot Nothing Then listItem.SetProfilePicture(avatar)
            listItem.SetStatusIcon(statusCode, extraStatusCode)
        End Using

        Return listItem
    End Function

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
        ' Load the StatusPage into the contentPanel
        Dim statusPage As New StatusPage()
        LoadFormIntoPanel(statusPage)

        Await SetUserInformation()
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
        If DetectDwmStatus.IsDwmEnabled() Then
            sidebarRenderer.Top = glassMargin.Top
        Else
            sidebarRenderer.Top = buttonPanel.Bottom
        End If

        Dim panelTop As Integer = sidebarRenderer.Top

        contentPanel.Top = panelTop
        sidebarRenderer.Height = ClientSize.Height - panelTop
        contentPanel.Height = ClientSize.Height - panelTop

        AddFriendItem("Placeholder username", "Listening to a song", Nothing, 0, 1, 1)

        SetSidebarIcon("dm", "dmPic", _hDmIcon)
        SetSidebarIcon("servers", "serverPic", _hServerIcon)
    End Sub

    Private Async Function SetUserInformation() As Task
        ' Use saved details first before actually loading data from Discord
        usernameLabel.Text = My.Settings.dscUsername

        Dim userInfo As String = Await API.Instance.SendAPI("users/@me", HttpMethod.Get, dscToken)
        If userInfo Is Nothing Then Return

        If userInfo.Contains("Unauthorized") Then
            MessageBox.Show("Your token has expired or is invalid. NaticordVB will sign you out and you will need to sign in again.",
                            "NaticordVB", MessageBoxButtons.OK, MessageBoxIcon.Error)

            ' Wipe all Discord properties
            ClearDiscordSession()
            Application.Restart()
        Else
            Dim parsedUser As JsonObject = JsonNode.Parse(userInfo).AsObject()

            Dim userId As String = parsedUser("id")?.GetValue(Of String)()
            Dim username As String = If(parsedUser("username")?.GetValue(Of String)(), "anonymous_user")
            Dim displayName As String = If(parsedUser("global_name")?.GetValue(Of String)(), username)

            ' Save properties to settings for use!
            My.Settings.dscUsername = displayName
            My.Settings.dscUid = userId
            My.Settings.Save()

            usernameLabel.Text = displayName
            ' TODO: Add avatar support (parsedUser("avatar"))
        End If
    End Function

    Private Shared Sub ClearDiscordSession()
        My.Settings.dscToken = Nothing
        My.Settings.dscUsername = Nothing
        My.Settings.dscUid = Nothing
        My.Settings.Save()
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        ReleaseHBitmap(_hDmIcon)
        ReleaseHBitmap(_hServerIcon)

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
