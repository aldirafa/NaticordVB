Imports System.Net.Http
Imports DirectUI.Net.Events
Imports NaticordVB.Networking

Public Class Login

    Public Sub New()
        InitializeComponent()

        ' Input the event subscribers right now!
        AddHandler loginDuiRenderer.ButtonClicked, AddressOf OnDuiClick
    End Sub

    Private Async Function SignInSequence() As Task
        Dim dscToken As String = loginDuiRenderer.Element("editToken").GetText()
        If String.IsNullOrEmpty(dscToken) Then
            MessageBox.Show("You have not entered a Discord token, put one in and try again.", "NaticordVB", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' Verify that the token actually functions correctly
        Dim result As ApiResult = Await API.Instance.RequestAsync("users/@me", HttpMethod.Get, dscToken)

        If result.Error = ApiError.Unauthorized Then
            ' Discord rejected the token
            Debug.WriteLine("Token is not correct and hasn't been saved to local settings.")
            Dim forceBox As DialogResult = MessageBox.Show(
                "Your token is incorrect, please verify that you have entered it correctly and try again. If you are sure this is the right token, click 'OK' to force it to save.",
                "NaticordVB",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Error)

            If forceBox = DialogResult.OK Then SaveToken(dscToken)
        ElseIf Not result.IsSuccess Then
            ' We couldn't tell: offline, timed out, rate limited, Discord having a bad day...
            Debug.WriteLine($"Token could not be verified ({result.Error}): {result.ErrorMessage}")
            Dim saveAnyway As DialogResult = MessageBox.Show(
                "NaticordVB couldn't reach Discord to check your token (" & result.ErrorMessage & "). Click 'OK' to save it anyway, or 'Cancel' to try again later.",
                "NaticordVB",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning)

            If saveAnyway = DialogResult.OK Then SaveToken(dscToken)
        Else
            Debug.WriteLine("Token has been saved to local settings.")
            SaveToken(dscToken)
        End If
    End Function

    Private Async Sub OnDuiClick(s As Object, e As DuiClickEventArgs)
        If e.ElementId = "btnSignIn" Then
            loginDuiRenderer.Element("btnSignIn").SetEnabled(False)

            ' Attempt to sign into Discord with a token
            Try
                Await SignInSequence()
            Finally
                loginDuiRenderer.Element("btnSignIn").SetEnabled(True)
            End Try
        ElseIf e.ElementId = "btnQrLaunch" Then
            loginDuiRenderer.Element("btnQrLaunch").SetEnabled(False)

            ' Open the QR login dialog
            Using qr As New QRLogin(Me)
                qr.ShowDialog()
            End Using
            loginDuiRenderer.Element("btnQrLaunch").SetEnabled(True)
        End If
    End Sub

    Private Sub SaveToken(dscToken As String)
        My.Settings.dscToken = dscToken
        My.Settings.Save()

        ContinueAfterLogin()
    End Sub

    ''' <summary>
    ''' Opens the setup flow (first run) or the client, then hides this form. It can't be closed: destroying a
    ''' window that hosts DirectUI ends the message loop and would take the whole app down with it.
    ''' </summary>
    Friend Sub ContinueAfterLogin()
        Dim nextForm As Form = If(My.Settings.hasCompletedSetup, DirectCast(New Client(), Form), New SetupFlowPiece())

        NaticordContext.Track(nextForm)
        nextForm.Show()

        Hide()
    End Sub

End Class
