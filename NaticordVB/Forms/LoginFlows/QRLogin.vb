Imports DirectUI.Net.Events
Imports DirectUI.Net.Hosting
Imports NaticordVB.Networking.WebSockets

Public Class QRLogin

    Private ReadOnly _qrHost As DuiFormAttachment
    Private _authSocket As AuthSocket
    Private ReadOnly _loginForm As Login

    Public Sub New(loginForm As Login)
        InitializeComponent()

        _loginForm = loginForm
        _qrHost = New DuiFormAttachment(Me)
    End Sub

    Private Async Sub QRLogin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _qrHost.LoadXmlFile("User Interface\QR.xml", resId:="qrMain")
        AddHandler _qrHost.ButtonClicked, AddressOf OnDuiClick

        _authSocket = New AuthSocket()
        AddHandler _authSocket.qrCodeReady, AddressOf OnQrCodeReady
        AddHandler _authSocket.tokenReceived, AddressOf OnTokenReceived

        Try
            Await _authSocket.StartSocket()
        Catch ex As Exception
            MessageBox.Show("Could not connect to the gateway! " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Close()
        End Try
    End Sub

    Private Sub OnQrCodeReady(url As String)
        Dim writer As New ZXing.Windows.Compatibility.BarcodeWriter With {
            .Format = ZXing.BarcodeFormat.QR_CODE,
            .Options = New ZXing.QrCode.QrCodeEncodingOptions With {
                .Width = qrPictureBox.Width,
                .Height = qrPictureBox.Height,
                .Margin = 1
            }
        }
        qrPictureBox.Image = writer.Write(url)
    End Sub

    Private Sub OnTokenReceived(token As String)
        My.Settings.dscToken = token
        My.Settings.Save()

        Close()
        _loginForm.ContinueAfterLogin()
    End Sub

    Private Sub OnDuiClick(s As Object, e As DuiClickEventArgs)
        If e.ElementId = "btnCancelQr" Then Close()
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        _authSocket?.StopSocket()
        _authSocket?.Dispose()
        MyBase.OnFormClosed(e)
    End Sub

End Class
