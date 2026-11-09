Imports NaticordVB.Glass

Public Class SetupFlowPiece
    Inherits GlassForm

    Private _pages As (Page As Form, Title As String)()
    Private _currentPage As Integer = 0

    Public Sub New()
        DecideOSSettings()
        InitializeComponent()
        MinimumSize = Size

        ' Apply the correct composition to the window!
        CompSet(DetectDwmStatus.IsDwmEnabled(), True, Me)
        ' Apply the toolbar padding to SetupFlowPiece!
        ' No, there isn't a better way to do this... It works though!
        If My.Settings.layoutMode = LayoutWindows11 Then
            buttonPanel.Width -= 16
            buttonPanel.Left += 8
        End If

        _pages = {
            (New IntroductionPage(), "Introduction"),
            (New AppearancePage(Me), "Appearance"),
            (New FinishedPage(), "You're all set!")
        }

        For Each entry In _pages
            entry.Page.TopLevel = False
            entry.Page.FormBorderStyle = FormBorderStyle.None
            entry.Page.Dock = DockStyle.Fill

            pagePanel.Controls.Add(entry.Page)

            entry.Page.Show()
            entry.Page.Hide()
        Next

        NavigateTo(0)
    End Sub

    Private Shared Sub DecideOSSettings()
        ' Decide the rendering mode that Naticord should use on the titlebar, based on OS and theme settings
        If DetectDwmStatus.IsDwmEnabled() Then
            If DetectOSVersion.IsWindows7To81() Then
                My.Settings.renderMode = "Aero"
            ElseIf DetectOSVersion.IsWindows10() Then
                My.Settings.renderMode = "Acrylic"
            ElseIf DetectOSVersion.IsWindows11() Then
                My.Settings.renderMode = "Mica"
            End If
        Else
            My.Settings.renderMode = "Composition disabled"
        End If

        ' Decide the toolbar mode that Naticord should use, based on OS (only set default on first run)
        If DetectOSVersion.IsWindows7To81() Then
            My.Settings.layoutMode = LayoutWindows81
        ElseIf DetectOSVersion.IsWindows10() OrElse DetectOSVersion.IsWindows11() Then
            My.Settings.layoutMode = LayoutWindows11
        End If

        My.Settings.Save()
    End Sub

    Private Sub OnNextClicked(sender As Object, e As EventArgs) Handles nextButton.ButtonClick
        If _currentPage < _pages.Length - 1 Then
            NavigateTo(_currentPage + 1)
        Else
            My.Settings.hasCompletedSetup = True
            My.Settings.Save()

            ' Continue to the client
            Dim clientForm As New Client()
            NaticordContext.Track(clientForm)
            clientForm.Show()

            ' Hide, don't Close: destroying a DirectUI host window ends the message loop (see Login.ContinueAfterLogin).
            Hide()
        End If
    End Sub

    Private Sub OnBackClicked(sender As Object, e As EventArgs) Handles backButton.ButtonClick
        If _currentPage > 0 Then NavigateTo(_currentPage - 1)
    End Sub

    Private Sub NavigateTo(index As Integer)
        _pages(_currentPage).Page.Hide()
        _currentPage = index

        _pages(_currentPage).Page.Show()
        backButton.Visible = index > 0

        nextButton.ButtonLabel = If(index = _pages.Length - 1, "Finish up!", "Next page")
        SetPagePieceHeader()
    End Sub

    Private Sub SetPagePieceHeader()
        If pageCountRenderer.DuiWindow Is Nothing Then Return

        Dim text As String = $"{_pages(_currentPage).Title} - Page {_currentPage + 1} out of {_pages.Length}"
        pageCountRenderer.DuiWindow.SetText("pageCountText", text)
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)

        ' CenterScreen doesn't position the window correctly here, so centre it by hand on the active screen.
        Dim screenArea As Rectangle = Screen.FromPoint(Cursor.Position).WorkingArea
        Location = New Point(
            screenArea.Left + (screenArea.Width - Width) \ 2,
            screenArea.Top + (screenArea.Height - Height) \ 2)

        buttonPanel.Top = 0
        If DetectDwmStatus.IsDwmEnabled() Then
            pageCountRenderer.Top = glassMargin.Top
        Else
            buttonPanel.BackColor = SystemColors.ButtonFace
            pageCountRenderer.Top = buttonPanel.Bottom
        End If

        Dim panelTop As Integer = pageCountRenderer.Bottom

        pagePanel.Top = panelTop
        pagePanel.Height = ClientSize.Height - panelTop

        SetPagePieceHeader()
    End Sub

End Class
