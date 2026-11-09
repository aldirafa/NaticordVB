' This has been taken from the 'aerocord' project found here:
' https://github.com/jukfiuune/aerocord
' Credit goes to them for this

Imports NaticordVB.Glass
Imports WindowsFormsAero

Public Class GlassForm
    Inherits AeroForm

    Public AutoColorMode As Boolean = False
    Public RenderMode As String = My.Settings.renderMode

    Public glassMargin As New Padding(0, 30, 0, 0)

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        ' DWMSBT values: 0 = auto, 1 = none, 2 = Mica, 3 = Acrylic, 4 = Mica Alt
        Select Case RenderMode
            Case "Aero"
                SetBackdrop(1)
            Case "Mica"
                SetBackdrop(2)
            Case "Acrylic"
                SetBackdrop(3)
            Case "Mica (Alt)"
                SetBackdrop(4)
            Case "Composition disabled"
                SetBackdrop(0)
                glassMargin = New Padding(0, 0, 0, 0)
        End Select
    End Sub

    Private Sub SetBackdrop(backdropType As Integer)
        GlassInvoke.SetWindowAttribute(Handle, GlassInvoke.DwmWindowAttribute.DWMWA_SYSTEMBACKDROP_TYPE, backdropType)
    End Sub

    Public Sub CompSet(compActive As Boolean, isSetup As Boolean, Optional setupFlow As SetupFlowPiece = Nothing, Optional clientForm As Client = Nothing)
        ' When composition is active there is nothing to do: the designer already has everything set to be composite.
        If compActive Then Return

        If isSetup Then
            ' Set the button panel to be the page count color
            setupFlow.buttonPanel.BackColor = SystemColors.ButtonFace

            ' Set the two buttons to have composition disabled
            setupFlow.backButton.CompositionDisabled = True
            setupFlow.nextButton.CompositionDisabled = True
        Else
            ' Set the button panel to be the button face color
            clientForm.buttonPanel.BackColor = SystemColors.ButtonFace
            ' Set the profile picture box to be the button face color too
            clientForm.avatarBox.BackColor = SystemColors.ButtonFace

            ' Set the three buttons to have composition disabled
            clientForm.accountButton.CompositionDisabled = True
            clientForm.settingsButton.CompositionDisabled = True
            clientForm.githubButton.CompositionDisabled = True
            ' And this text label...
            clientForm.usernameLabel.CompositionDisabled = True

            ' We need to offset them so they look nice on systems with no composition
            ' This doesn't matter on composite systems because the extended glass will help ease it out
            clientForm.accountButton.Top += 3
            clientForm.settingsButton.Top += 3
            clientForm.githubButton.Top += 3

            ' Offset the user details too!
            clientForm.avatarBox.Top += 3
            clientForm.usernameLabel.Top += 3
        End If
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        GlassMargins = glassMargin
    End Sub

End Class
