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
            MessageBox.Show("You have not entered a Discord token, put one in and try again.", "Naticord", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' Verify that the token actually functions correctly
        Dim corrToken As String = Await API.Instance.SendAPI("/users/@me", HttpMethod.Get, dscToken)

        ' Check for the "Unauthorized" in the request, which means the token isn't correct
        If corrToken.Contains("Unauthorized") Then
            Debug.WriteLine("Token is not correct and hasn't been saved to local settings.")
            Dim forceBox As DialogResult = MessageBox.Show(
                "Your token is incorrect, please verify that you have entered it correctly and try again. If you are sure this is the right token, click 'OK' to force it to save.",
                "Naticord",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Error)

            If forceBox = DialogResult.OK Then SaveToken(dscToken)
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

    ''' <summary>Opens the setup flow (first run) or the client, and hides this form (it is the host form, so it can't be closed).</summary>
    Friend Sub ContinueAfterLogin()
        If Not My.Settings.hasCompletedSetup Then
            ' Continue to the setup flow
            Dim setupFlowPiece As New SetupFlowPiece()
            setupFlowPiece.Show()
        Else
            ' Continue to the client
            Dim clientForm As New Client()
            clientForm.Show()
        End If

        Hide()
    End Sub

End Class
