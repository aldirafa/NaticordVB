Public Class AppearancePage

    Private ReadOnly _setupFlowPiece As SetupFlowPiece

    Public Sub New(setupFlowPiece As SetupFlowPiece)
        InitializeComponent()
        _setupFlowPiece = setupFlowPiece

        ' Wired here (not with Handles) so they can't fire while InitializeComponent sets the default radio state.
        AddHandler skeuoRadio.CheckedChanged, AddressOf skeuoRadio_CheckedChanged
        AddHandler modernRadio.CheckedChanged, AddressOf modernRadio_CheckedChanged
        AddHandler layoutBox.SelectedIndexChanged, AddressOf layoutBox_SelectedIndexChanged

        ' Set labels to be the correct font, and not Tahoma!
        icnPreviewLabel.Font = SystemFonts.MessageBoxFont
        layoutNoticeLabel.Font = SystemFonts.MessageBoxFont

        layoutBox.SelectedItem = My.Settings.layoutMode

        If My.Settings.layoutMode = LayoutWindows11 Then
            skeuoRadio.Checked = False
            modernRadio.Checked = True
        End If
    End Sub

    Private Sub skeuoRadio_CheckedChanged(sender As Object, e As EventArgs)
        If Not skeuoRadio.Checked Then Return

        My.Settings.iconStyle = IconStyleSkeuomorphic

        ' Change all of the picture boxes to the right style
        accountIcon.Image = My.Resources.account_skeuo
        settingsIcon.Image = My.Resources.cog_skeuo
        githubIcon.Image = My.Resources.github_skeuo
        dmIcon.Image = My.Resources.dm_skeuo
        serversIcon.Image = My.Resources.servers_skeuo

        ' Set the SetupFlowPiece button icons to match the style
        _setupFlowPiece.backButton.ButtonIcon = My.Resources.arrow_back_skeuo
        _setupFlowPiece.nextButton.ButtonIcon = My.Resources.arrow_forward_skeuo

        My.Settings.Save()
    End Sub

    Private Sub modernRadio_CheckedChanged(sender As Object, e As EventArgs)
        If Not modernRadio.Checked Then Return

        My.Settings.iconStyle = "Modern"

        ' Change all of the picture boxes to the right style
        accountIcon.Image = My.Resources.account
        settingsIcon.Image = My.Resources.cog
        githubIcon.Image = My.Resources.github
        dmIcon.Image = My.Resources.dm
        serversIcon.Image = My.Resources.servers

        ' Set the SetupFlowPiece button icons to match the style
        _setupFlowPiece.backButton.ButtonIcon = My.Resources.arrow_back
        _setupFlowPiece.nextButton.ButtonIcon = My.Resources.arrow_forward

        My.Settings.Save()
    End Sub

    Private Sub layoutBox_SelectedIndexChanged(sender As Object, e As EventArgs)
        Dim selected As String = layoutBox.SelectedItem?.ToString()

        If selected = LayoutWindows11 Then
            If My.Settings.layoutMode <> LayoutWindows11 Then
                My.Settings.layoutMode = LayoutWindows11
                _setupFlowPiece.buttonPanel.Width -= 16
                _setupFlowPiece.buttonPanel.Left += 8
                My.Settings.Save()
            End If
        ElseIf selected = LayoutWindows81 Then
            If My.Settings.layoutMode <> LayoutWindows81 Then
                My.Settings.layoutMode = LayoutWindows81
                _setupFlowPiece.buttonPanel.Width += 16
                _setupFlowPiece.buttonPanel.Left -= 8
                My.Settings.Save()
            End If
        End If
    End Sub

End Class
