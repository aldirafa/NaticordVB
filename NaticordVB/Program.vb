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
''' Runs the app until the last visible window is closed.
''' Login and the setup flow hand over to the next window with Hide() (closing a window that hosts DirectUI
''' ends the whole message loop), so no single form can be the "main form" and hidden forms must not keep
''' the process alive.
''' </summary>
Friend NotInheritable Class NaticordContext
    Inherits ApplicationContext

    Public Sub New(firstForm As Form)
        Track(firstForm)
        firstForm.Show()
    End Sub

    ''' <summary>Watches a top-level window so the app can exit once none are left on screen.</summary>
    Friend Shared Sub Track(form As Form)
        AddHandler form.FormClosed, AddressOf OnFormClosed
    End Sub

    Private Shared Sub OnFormClosed(sender As Object, e As FormClosedEventArgs)
        Dim closed As Form = DirectCast(sender, Form)

        ' Only top-level forms count: pages hosted inside another form (TopLevel = False) also sit in OpenForms.
        Dim stillOpen As Boolean = Application.OpenForms.Cast(Of Form)().Any(
            Function(f) f IsNot closed AndAlso f.TopLevel AndAlso f.Visible AndAlso Not f.IsDisposed)

        If Not stillOpen Then Application.ExitThread()
    End Sub

End Class
