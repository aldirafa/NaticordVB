Imports System.Runtime.InteropServices

Friend NotInheritable Class DetectOSVersion

    Private Sub New()
    End Sub

    ' RtlGetVersion writes a UTF-16 string, so the struct must be Unicode for its size to be right
    ' (284 bytes). With the default ANSI layout the size is wrong and RtlGetVersion rejects the call.
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Private Structure OSVERSIONINFOEX
        Public dwOSVersionInfoSize As UInteger
        Public dwMajorVersion As UInteger
        Public dwMinorVersion As UInteger
        Public dwBuildNumber As UInteger
        Public dwPlatformId As UInteger
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=128)>
        Public szCSDVersion As String
        Public wServicePackMajor As UShort
        Public wServicePackMinor As UShort
        Public wSuiteMask As UShort
        Public wProductType As Byte
        Public wReserved As Byte
    End Structure

    <DllImport("ntdll.dll")>
    Private Shared Function RtlGetVersion(ByRef lpVersionInformation As OSVERSIONINFOEX) As Integer
    End Function

    Private Shared Function GetWindowsVersion() As Version
        Dim osInfo As New OSVERSIONINFOEX With {
            .dwOSVersionInfoSize = CUInt(Marshal.SizeOf(Of OSVERSIONINFOEX)())
        }

        If RtlGetVersion(osInfo) = 0 Then
            Return New Version(CInt(osInfo.dwMajorVersion), CInt(osInfo.dwMinorVersion), CInt(osInfo.dwBuildNumber))
        End If

        Return Environment.OSVersion.Version
    End Function

    Public Shared ReadOnly Property MajorVersion As Integer
        Get
            Return GetWindowsVersion().Major
        End Get
    End Property

    Public Shared ReadOnly Property MinorVersion As Integer
        Get
            Return GetWindowsVersion().Minor
        End Get
    End Property

    Public Shared ReadOnly Property BuildNumber As Integer
        Get
            Return GetWindowsVersion().Build
        End Get
    End Property

    Public Shared Function IsWindows7To81() As Boolean
        Return MajorVersion = 6 AndAlso MinorVersion >= 1
    End Function

    Public Shared Function IsWindows10() As Boolean
        Return MajorVersion = 10 AndAlso BuildNumber < 22000
    End Function

    Public Shared Function IsWindows11() As Boolean
        Return MajorVersion = 10 AndAlso BuildNumber >= 22000
    End Function

    Public Shared Function IsAtLeastWindows10() As Boolean
        Return MajorVersion >= 10
    End Function

    Public Shared Function IsAtLeastWindows11() As Boolean
        Return MajorVersion = 10 AndAlso BuildNumber >= 22000
    End Function

    Public Shared Function GetFriendlyWindowsVersion() As String
        If IsWindows11() Then Return "Windows 11"
        If IsWindows10() Then Return "Windows 10"
        If IsWindows7To81() Then Return "Windows 7/8/8.1"
        Return "Unknown system"
    End Function

End Class
