Imports DirectUI.Net.Hosting

Public Class IntroductionPage

    Private ReadOnly _introHost As DuiFormAttachment

    Public Sub New()
        InitializeComponent()
        _introHost = New DuiFormAttachment(Me)
    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        _introHost.LoadXmlFile("User Interface\SetupFlow\Introduction.xml", resId:="introMain")
    End Sub

End Class
