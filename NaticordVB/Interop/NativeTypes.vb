' Taken from WindowsFormsAero

Imports System.Runtime.InteropServices

Namespace Interop

    Friend Enum BitBltOp As UInteger
        SRCCOPY = &HCC0020UI        ' dest = source
        SRCPAINT = &HEE0086UI       ' dest = source OR dest
        SRCAND = &H8800C6UI         ' dest = source AND dest
        SRCINVERT = &H660046UI      ' dest = source XOR dest
        SRCERASE = &H440328UI       ' dest = source AND (NOT dest)
        NOTSRCCOPY = &H330008UI     ' dest = (NOT source)
        NOTSRCERASE = &H1100A6UI    ' dest = (NOT src) AND (NOT dest)
        MERGECOPY = &HC000CAUI      ' dest = (source AND pattern)
        MERGEPAINT = &HBB0226UI     ' dest = (NOT source) OR dest
        PATCOPY = &HF00021UI        ' dest = pattern
        PATPAINT = &HFB0A09UI       ' dest = DPSnoo
        PATINVERT = &H5A0049UI      ' dest = pattern XOR dest
        DSTINVERT = &H550009UI      ' dest = (NOT dest)
        BLACKNESS = &H42UI          ' dest = BLACK
        WHITENESS = &HFF0062UI      ' dest = WHITE

        NOMIRRORBITMAP = &H80000000UI   ' Do not mirror the bitmap in this call
        CAPTUREBLT = &H40000000UI       ' Include layered windows
    End Enum

    Friend Enum ClassLong As Integer
        Icon = -14
        IconSmall = -34
    End Enum

    Friend Enum WindowLong As Integer
        WndProc = -4
        HInstance = -6
        HwndParent = -8
        Style = -16
        ExStyle = -20
        UserData = -21
        Id = -12
    End Enum

    <Flags>
    Friend Enum DttOptsFlags As Integer
        DTT_TEXTCOLOR = 1
        DTT_BORDERCOLOR = 2
        DTT_SHADOWCOLOR = 4
        DTT_SHADOWTYPE = 8
        DTT_SHADOWOFFSET = 16
        DTT_BORDERSIZE = 32
        DTT_CALCRECT = 512
        DTT_APPLYOVERLAY = 1024
        DTT_GLOWSIZE = 2048
        DTT_COMPOSITED = 8192
    End Enum

    Friend Enum HitTest As Integer
        HTERROR = -2
        HTTRANSPARENT = -1
        HTNOWHERE = 0
        HTCLIENT = 1
        HTCAPTION = 2
        HTSYSMENU = 3
        HTGROWBOX = 4
        HTSIZE = HTGROWBOX
        HTMENU = 5
        HTHSCROLL = 6
        HTVSCROLL = 7
        HTMINBUTTON = 8
        HTMAXBUTTON = 9
        HTLEFT = 10
        HTRIGHT = 11
        HTTOP = 12
        HTTOPLEFT = 13
        HTTOPRIGHT = 14
        HTBOTTOM = 15
        HTBOTTOMLEFT = 16
        HTBOTTOMRIGHT = 17
        HTBORDER = 18
        HTREDUCE = HTMINBUTTON
        HTZOOM = HTMAXBUTTON
        HTSIZEFIRST = HTLEFT
        HTSIZELAST = HTBOTTOMRIGHT
        HTOBJECT = 19
        HTCLOSE = 20
        HTHELP = 21
    End Enum

    ' Named Native* so they never clash with System.Drawing.Point / Rectangle.
    <StructLayout(LayoutKind.Sequential)>
    Friend Structure NativePoint
        Public X As Integer
        Public Y As Integer

        Public Sub New(x As Integer, y As Integer)
            Me.X = x
            Me.Y = y
        End Sub

        Public Sub New(p As System.Drawing.Point)
            X = p.X
            Y = p.Y
        End Sub

        Public Sub New(p As System.Drawing.PointF)
            X = CInt(p.X)
            Y = CInt(p.Y)
        End Sub
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Friend Structure NativeRect
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer

        Public Sub New(left As Integer, top As Integer, right As Integer, bottom As Integer)
            Me.Left = left
            Me.Top = top
            Me.Right = right
            Me.Bottom = bottom
        End Sub

        Public Sub New(rect As System.Drawing.Rectangle)
            Left = rect.X
            Top = rect.Y
            Right = rect.Right
            Bottom = rect.Bottom
        End Sub

        Public Property Width As Integer
            Get
                Return Right - Left
            End Get
            Set(value As Integer)
                Right = Left + value
            End Set
        End Property

        Public Property Height As Integer
            Get
                Return Bottom - Top
            End Get
            Set(value As Integer)
                Bottom = Top + value
            End Set
        End Property

        Public Function ToRectangle() As System.Drawing.Rectangle
            Return New System.Drawing.Rectangle(Left, Top, Right - Left, Bottom - Top)
        End Function
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Friend Structure BitmapInfo
        Public biSize As Integer
        Public biWidth As Integer
        Public biHeight As Integer
        Public biPlanes As Short
        Public biBitCount As Short
        Public biCompression As Integer
        Public biSizeImage As Integer
        Public biXPelsPerMeter As Integer
        Public biYPelsPerMeter As Integer
        Public biClrUsed As Integer
        Public biClrImportant As Integer
        Public bmiColors_rgbBlue As Byte
        Public bmiColors_rgbGreen As Byte
        Public bmiColors_rgbRed As Byte
        Public bmiColors_rgbReserved As Byte
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Friend Structure DttOpts
        Public dwSize As Integer
        Public dwFlags As DttOptsFlags
        Public crText As Integer
        Public crBorder As Integer
        Public crShadow As Integer
        Public iTextShadowType As Integer
        Public ptShadowOffset As NativePoint
        Public iBorderSize As Integer
        Public iFontPropId As Integer
        Public iColorPropId As Integer
        Public iStateId As Integer
        Public fApplyOverlay As Boolean
        Public iGlowSize As Integer
        Public pfnDrawTextCallback As Integer
        Public lParam As IntPtr
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Friend Structure BlendFunction
        Public BlendOp As Byte
        Public BlendFlags As Byte
        Public SourceConstantAlpha As Byte
        Public AlphaFormat As Byte
    End Structure

End Namespace
