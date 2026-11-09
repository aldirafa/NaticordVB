' Taken from WindowsFormsAero

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

Namespace Interop

    Friend Module NativeMethods

#Region "Window manager"

        <DllImport("uxtheme.dll", CharSet:=CharSet.Unicode)>
        Friend Function SetWindowTheme(hWnd As IntPtr, pszSubAppName As String, pszSubIdList As String) As Integer
        End Function

        <DllImport("uxtheme.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
        Friend Function DrawThemeTextEx(hTheme As IntPtr, hdc As IntPtr,
                                        iPartId As Integer, iStateId As Integer, text As String,
                                        iCharCount As Integer, dwFlags As Integer,
                                        ByRef pRect As NativeRect, ByRef pOptions As DttOpts) As Integer
        End Function

        ''' <summary>Returns the active window on the current thread.</summary>
        <DllImport("user32.dll")>
        Friend Function GetActiveWindow() As IntPtr
        End Function

        <DllImport("user32.dll")>
        Friend Function GetWindowRect(hwnd As IntPtr, <Out> ByRef rect As NativeRect) As Boolean
        End Function

        <DllImport("user32.dll")>
        Friend Function SendMessage(hWnd As IntPtr, Msg As UInteger, wParam As IntPtr, lParam As IntPtr) As IntPtr
        End Function

        <DllImport("user32.dll")>
        Friend Function SendMessage(hWnd As IntPtr, Msg As UInteger, wParam As Integer, lParam As Integer) As IntPtr
        End Function

        <DllImport("user32.dll")>
        Friend Function SendMessage(hWnd As IntPtr, Msg As UInteger, wParam As UInteger, lParam As UInteger) As IntPtr
        End Function

        <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
        Friend Function SendMessage(hWnd As IntPtr, Msg As UInteger, wParam As Integer, lParam As String) As IntPtr
        End Function

        <DllImport("user32.dll")>
        Friend Function PostMessage(hWnd As IntPtr, Msg As UInteger, wParam As IntPtr, lParam As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        <DllImport("user32.dll", EntryPoint:="GetWindowLong")>
        Private Function GetWindowLong32(hWnd As IntPtr, nIndex As Integer) As Integer
        End Function

        <DllImport("user32.dll", EntryPoint:="GetWindowLongPtr")>
        Private Function GetWindowLongPtr64(hWnd As IntPtr, nIndex As Integer) As IntPtr
        End Function

        <DllImport("user32.dll", EntryPoint:="GetClassLongPtrW")>
        Private Function GetClassLong64(hWnd As IntPtr, nIndex As Integer) As IntPtr
        End Function

        <DllImport("user32.dll", EntryPoint:="GetClassLongW")>
        Private Function GetClassLong32(hWnd As IntPtr, nIndex As Integer) As Integer
        End Function

        <Extension>
        Friend Function GetWindowLong(hWnd As IntPtr, i As WindowLong) As IntPtr
            If IntPtr.Size = 8 Then
                Return GetWindowLongPtr64(hWnd, i)
            End If
            Return New IntPtr(GetWindowLong32(hWnd, i))
        End Function

        <Extension>
        Friend Function GetClassLongPtr(hWnd As IntPtr, i As ClassLong) As IntPtr
            If IntPtr.Size = 8 Then
                Return GetClassLong64(hWnd, i)
            End If
            Return New IntPtr(GetClassLong32(hWnd, i))
        End Function

#End Region

#Region "GDI, DC and Blitting"

        <DllImport("gdi32.dll")>
        Friend Function CreateCompatibleDC(hDC As IntPtr) As IntPtr
        End Function

        <DllImport("gdi32.dll", SetLastError:=True)>
        Friend Function DeleteDC(hdc As IntPtr) As Boolean
        End Function

        <DllImport("gdi32.dll")>
        Friend Function BitBlt(hdc As IntPtr, nXDest As Integer, nYDest As Integer, nWidth As Integer, nHeight As Integer,
                               hdcSrc As IntPtr, nXSrc As Integer, nYSrc As Integer, dwRop As BitBltOp) As Boolean
        End Function

        <DllImport("user32.dll")>
        Friend Function FillRect(hDC As IntPtr, <[In]> ByRef lprc As NativeRect, hbr As IntPtr) As Integer
        End Function

        <DllImport("gdi32.dll")>
        Friend Function CreateSolidBrush(crColor As Integer) As IntPtr
        End Function

        <DllImport("gdi32.dll")>
        Friend Function CreateDIBSection(hdc As IntPtr, ByRef pbmi As BitmapInfo,
                                         iUsage As UInteger, ppvBits As Integer, hSection As IntPtr, dwOffset As UInteger) As IntPtr
        End Function

        <DllImport("gdi32.dll", ExactSpelling:=True)>
        Friend Function SelectObject(hDC As IntPtr, hObject As IntPtr) As IntPtr
        End Function

        <DllImport("gdi32.dll", ExactSpelling:=True)>
        Friend Function DeleteObject(hObject As IntPtr) As Boolean
        End Function

        Friend Const AC_SRC_OVER As Byte = &H0
        Friend Const AC_SRC_ALPHA As Byte = &H1

        <DllImport("msimg32.dll")>
        Friend Function AlphaBlend(hdcDest As IntPtr, xoriginDest As Integer, yoriginDest As Integer,
                                   wDest As Integer, hDest As Integer, hdcSrc As IntPtr, xoriginSrc As Integer, yoriginSrc As Integer,
                                   wSrc As Integer, hSrc As Integer, pfn As BlendFunction) As Boolean
        End Function

#End Region

    End Module

End Namespace
