Imports System.IO
Imports NaticordVB.Interop

''' <summary>Shared appearance settings: layout mode names and the bundled status/sidebar icon loader.</summary>
Friend Module Appearance

    ' Values stored in My.Settings.layoutMode
    Friend Const LayoutWindows11 As String = "Windows 11 - 10"
    Friend Const LayoutWindows81 As String = "Windows 8.1 - 7"

    ' Values stored in My.Settings.iconStyle
    Friend Const IconStyleSkeuomorphic As String = "Skeuomorphic"

    ''' <summary>"-skeuo" when the skeuomorphic icon set is selected, otherwise empty.</summary>
    Friend ReadOnly Property IconSuffix As String
        Get
            Return If(String.Equals(My.Settings.iconStyle, IconStyleSkeuomorphic, StringComparison.OrdinalIgnoreCase), "-skeuo", "")
        End Get
    End Property

    ''' <summary>
    ''' Loads ExtResources\{baseName}{suffix}.bmp as a GDI HBITMAP. The caller owns the handle and must
    ''' free it with <c>DeleteObject</c>. Returns <see cref="IntPtr.Zero"/> when the file does not exist.
    ''' </summary>
    Friend Function LoadExtResourceHBitmap(baseName As String) As IntPtr
        Dim path As String = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExtResources", $"{baseName}{IconSuffix}.bmp")
        If Not File.Exists(path) Then Return IntPtr.Zero

        Using src As Image = Image.FromFile(path)
            Using bmp As New Bitmap(src)
                Return bmp.GetHbitmap(Color.Black)
            End Using
        End Using
    End Function

    ''' <summary>Frees a GDI handle and resets it to <see cref="IntPtr.Zero"/>.</summary>
    Friend Sub ReleaseHBitmap(ByRef hBitmap As IntPtr)
        If hBitmap <> IntPtr.Zero Then
            DeleteObject(hBitmap)
            hBitmap = IntPtr.Zero
        End If
    End Sub

End Module
