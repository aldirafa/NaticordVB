Imports DirectUI.Net.Hosting

Public Class FinishedPage

    Private ReadOnly _finishedHost As DuiFormAttachment

    Public Sub New()
        InitializeComponent()
        _finishedHost = New DuiFormAttachment(Me)
    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        _finishedHost.LoadXmlFile("User Interface\SetupFlow\Finished.xml", resId:="finishMain")
    End Sub

End Class
