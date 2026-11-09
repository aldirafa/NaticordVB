Imports DirectUI.Net

Friend Module Program

    <STAThread>
    Friend Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        ' Initialize DirectUI before starting any form!
        DuiInitializer.Instance.Initialize()

        Dim firstForm As Form
        If Not String.IsNullOrEmpty(My.Settings.dscToken) Then
            If Not My.Settings.hasCompletedSetup Then
                firstForm = New SetupFlowPiece()
            Else
                firstForm = New Client()
            End If
        Else
            firstForm = New Login()
        End If

        Application.Run(New NaticordContext(firstForm))
    End Sub

End Module

''' <summary>
''' Keeps the app alive while any window is open. Login hands over to the setup flow or the client and
''' then closes itself, so no single form can be the "main form" (closing it would end the app early).
''' </summary>
Friend NotInheritable Class NaticordContext
    Inherits ApplicationContext

    Public Sub New(firstForm As Form)
        AddHandler firstForm.FormClosed, AddressOf OnFormClosed
        firstForm.Show()
    End Sub

    Private Sub OnFormClosed(sender As Object, e As FormClosedEventArgs)
        Dim closed As Form = DirectCast(sender, Form)
        RemoveHandler closed.FormClosed, AddressOf OnFormClosed

        ' Hand the "keep running" duty to whichever form is still open (the next step of the flow).
        Dim next_ As Form = Application.OpenForms.Cast(Of Form)().FirstOrDefault(Function(f) f IsNot closed AndAlso f.Visible)
        If next_ Is Nothing Then
            ExitThread()
        Else
            AddHandler next_.FormClosed, AddressOf OnFormClosed
        End If
    End Sub

End Class
