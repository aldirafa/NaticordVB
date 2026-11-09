Imports DirectUI.Net.Hosting

Public Class StatusPage

    Private ReadOnly _statusHost As DuiFormAttachment

    Public Sub New()
        InitializeComponent()
        _statusHost = New DuiFormAttachment(Me)
    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        _statusHost.LoadXmlFile("User Interface\ClientFlow\StatusPage.xml", resId:="statusMain")
    End Sub

End Class
