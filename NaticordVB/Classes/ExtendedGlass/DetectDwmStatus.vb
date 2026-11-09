Imports System.Runtime.InteropServices

Namespace Glass

    Friend NotInheritable Class DetectDwmStatus

        Private Sub New()
        End Sub

        <DllImport("dwmapi.dll", PreserveSig:=False)>
        Private Shared Sub DwmIsCompositionEnabled(<MarshalAs(UnmanagedType.Bool)> ByRef pfEnabled As Boolean)
        End Sub

        Public Shared Function IsDwmEnabled() As Boolean
            Try
                Dim isEnabled As Boolean
                DwmIsCompositionEnabled(isEnabled)
                Return isEnabled
            Catch ex As Exception
                Return False
            End Try
        End Function

    End Class

End Namespace
