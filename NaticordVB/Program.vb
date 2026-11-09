Imports DirectUI.Net

Friend Module Program

    <STAThread>
    Friend Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        ' Initialize DirectUI before starting any form!
        DuiInitializer.Instance.Initialize()

        If Not String.IsNullOrEmpty(My.Settings.dscToken) Then
            If Not My.Settings.hasCompletedSetup Then
                Application.Run(New SetupFlowPiece())
            Else
                Application.Run(New Client())
            End If
        Else
            Application.Run(New Login())
        End If
    End Sub

End Module
