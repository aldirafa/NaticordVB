' This has been taken from the 'aerocord' project found here:
' https://github.com/jukfiuune/aerocord
' Credit goes to them for this

Imports System.Runtime.InteropServices

Namespace Glass

    Friend NotInheritable Class GlassInvoke

        Private Sub New()
        End Sub

        Public Enum DwmWindowAttribute
            DWMWA_SYSTEMBACKDROP_TYPE = 38
        End Enum

        <DllImport("dwmapi.dll")>
        Private Shared Function DwmSetWindowAttribute(hwnd As IntPtr, dwAttribute As DwmWindowAttribute,
                                                      ByRef pvAttribute As Integer, cbAttribute As Integer) As Integer
        End Function

        Public Shared Function SetWindowAttribute(hwnd As IntPtr, attribute As DwmWindowAttribute, parameter As Integer) As Integer
            Return DwmSetWindowAttribute(hwnd, attribute, parameter, 4) ' sizeof(Integer)
        End Function

    End Class

End Namespace
