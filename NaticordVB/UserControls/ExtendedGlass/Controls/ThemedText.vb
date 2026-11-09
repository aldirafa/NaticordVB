Imports System.Runtime.InteropServices
Imports NaticordVB.Interop
Imports VS = System.Windows.Forms.VisualStyles

Friend NotInheritable Class ThemedText
    Implements IDisposable

    Public Const DefaultGlowSize As Integer = 10
    Private Shared _renderer As VS.VisualStyleRenderer

    Private Shared Function GetRenderer() As VS.VisualStyleRenderer
        If _renderer IsNot Nothing Then Return _renderer

        If VS.VisualStyleRenderer.IsSupported AndAlso
           VS.VisualStyleRenderer.IsElementDefined(VS.VisualStyleElement.Window.Caption.Active) Then
            _renderer = New VS.VisualStyleRenderer(VS.VisualStyleElement.Window.Caption.Active)
        End If

        Return _renderer
    End Function

    Private _invalidated As Boolean = True

    Private _text As String = String.Empty
    Private _font As Font = SystemFonts.CaptionFont
    Private _formatFlags As TextFormatFlags = TextFormatFlags.Default

    Private _padding As Padding = Padding.Empty

    Private _win32Color As Integer = ColorTranslator.ToWin32(Color.Black)

    Private _glowSize As Integer = DefaultGlowSize
    Private _glowEnabled As Boolean = True

    Private _textHdc As IntPtr = IntPtr.Zero
    Private _dibSection As IntPtr = IntPtr.Zero

    Private _cachedWidth As Integer = -1
    Private _cachedHeight As Integer = -1

    Public Property Text As String
        Get
            Return _text
        End Get
        Set(value As String)
            If _text <> value Then
                _text = value
                _invalidated = True
            End If
        End Set
    End Property

    Public Property Font As Font
        Get
            Return _font
        End Get
        Set(value As Font)
            If _font IsNot value Then
                _font = value
                _invalidated = True
            End If
        End Set
    End Property

    Public Property Padding As Padding
        Get
            Return _padding
        End Get
        Set(value As Padding)
            If _padding <> value Then
                _padding = value
                _invalidated = True
            End If
        End Set
    End Property

    Public Property Color As Color
        Get
            Return ColorTranslator.FromWin32(_win32Color)
        End Get
        Set(value As Color)
            _win32Color = ColorTranslator.ToWin32(value)
            _invalidated = True
        End Set
    End Property

    Public Property FormatFlags As TextFormatFlags
        Get
            Return _formatFlags
        End Get
        Set(value As TextFormatFlags)
            If _formatFlags <> value Then
                _formatFlags = value
                _invalidated = True
            End If
        End Set
    End Property

    Public Property GlowSize As Integer
        Get
            Return _glowSize
        End Get
        Set(value As Integer)
            If _glowSize <> value Then
                _glowSize = value
                _invalidated = True
            End If
        End Set
    End Property

    Public Property GlowEnabled As Boolean
        Get
            Return _glowEnabled
        End Get
        Set(value As Boolean)
            If _glowEnabled <> value Then
                _glowEnabled = value
                _invalidated = True
            End If
        End Set
    End Property

    Public Sub Dispose() Implements IDisposable.Dispose
        FreeDib()
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Try
            FreeDib()
        Finally
            MyBase.Finalize()
        End Try
    End Sub

    Private Sub FreeDib()
        If _dibSection <> IntPtr.Zero Then
            DeleteObject(_dibSection)
            _dibSection = IntPtr.Zero
        End If
        If _textHdc <> IntPtr.Zero Then
            DeleteDC(_textHdc)
            _textHdc = IntPtr.Zero
        End If
    End Sub

    Private Shared ReadOnly PremultipliedBlend As New BlendFunction With {
        .BlendOp = AC_SRC_OVER,
        .BlendFlags = 0,
        .SourceConstantAlpha = &HFF,
        .AlphaFormat = AC_SRC_ALPHA
    }

    Public Sub Draw(g As Graphics, location As Point, size As Size)
        Draw(g, location.X, location.Y, size.Width, size.Height)
    End Sub

    Public Sub Draw(g As Graphics, rect As Rectangle)
        Draw(g, rect.X, rect.Y, rect.Width, rect.Height)
    End Sub

    Public Sub Draw(g As Graphics, x As Integer, y As Integer, width As Integer, height As Integer)
        If VS.VisualStyleRenderer.IsSupported Then
            Dim hdc As IntPtr = g.GetHdc()
            Try
                Dim src As IntPtr = EnsureHdc(hdc, width, height, 0)
                AlphaBlend(hdc, x, y, width, height, src, 0, 0, width, height, PremultipliedBlend)
            Finally
                g.ReleaseHdc(hdc)
            End Try
        Else
            Dim bounds As New Rectangle(
                x + _padding.Left, y + _padding.Top,
                width - _padding.Horizontal, height - _padding.Vertical)
            TextRenderer.DrawText(g, _text, _font, bounds, ColorTranslator.FromWin32(_win32Color), _formatFlags)
        End If
    End Sub

    Public Sub DrawInBounds(g As Graphics, width As Integer, height As Integer)
        Dim inflate As Integer = If(_glowEnabled, _glowSize, 0)

        If Not VS.VisualStyleRenderer.IsSupported OrElse inflate = 0 Then
            Draw(g, 0, 0, width, height)
            Return
        End If

        Dim dibW As Integer = width + 2 * inflate
        Dim dibH As Integer = height + 2 * inflate

        Dim hdc As IntPtr = g.GetHdc()
        Try
            Dim src As IntPtr = EnsureHdc(hdc, dibW, dibH, inflate)
            AlphaBlend(hdc, 0, 0, width, height, src, inflate, inflate, width, height, PremultipliedBlend)
        Finally
            g.ReleaseHdc(hdc)
        End Try
    End Sub

    Public Sub DrawOverflow(parentG As Graphics, controlLeft As Integer, controlTop As Integer, width As Integer, height As Integer)
        Dim inflate As Integer = If(_glowEnabled, _glowSize, 0)
        If Not VS.VisualStyleRenderer.IsSupported OrElse inflate = 0 Then Return

        Dim dibW As Integer = width + 2 * inflate
        Dim dibH As Integer = height + 2 * inflate

        Dim hdc As IntPtr = parentG.GetHdc()
        Try
            Dim src As IntPtr = EnsureHdc(hdc, dibW, dibH, inflate)
            AlphaBlend(hdc, controlLeft - inflate, controlTop - inflate,
                       dibW, dibH, src, 0, 0, dibW, dibH, PremultipliedBlend)
        Finally
            parentG.ReleaseHdc(hdc)
        End Try
    End Sub

    Public Sub BlendOverflow(dstG As Graphics, dstX As Integer, dstY As Integer, width As Integer, height As Integer)
        Dim inflate As Integer = If(_glowEnabled, _glowSize, 0)
        If Not VS.VisualStyleRenderer.IsSupported OrElse inflate = 0 Then Return

        Dim dibW As Integer = width + 2 * inflate
        Dim dibH As Integer = height + 2 * inflate

        Dim hdc As IntPtr = dstG.GetHdc()
        Try
            Dim src As IntPtr = EnsureHdc(hdc, dibW, dibH, inflate)
            AlphaBlend(hdc, dstX - inflate, dstY - inflate,
                       dibW, dibH, src, 0, 0, dibW, dibH, PremultipliedBlend)
        Finally
            dstG.ReleaseHdc(hdc)
        End Try
    End Sub

    Private Function EnsureHdc(outputHdc As IntPtr, width As Integer, height As Integer, offsetForGlow As Integer) As IntPtr
        If width = _cachedWidth AndAlso height = _cachedHeight AndAlso Not _invalidated Then
            Return _textHdc
        End If

        FreeDib()

        _cachedWidth = width
        _cachedHeight = height

        _textHdc = CreateCompatibleDC(outputHdc)

        Dim info As New BitmapInfo With {
            .biSize = Marshal.SizeOf(GetType(BitmapInfo)),
            .biWidth = width,
            .biHeight = -height,
            .biPlanes = 1,
            .biBitCount = 32,
            .biCompression = 0
        }
        _dibSection = CreateDIBSection(outputHdc, info, 0UI, 0, IntPtr.Zero, 0UI)
        SelectObject(_textHdc, _dibSection)

        Dim hFont As IntPtr = _font.ToHfont()
        SelectObject(_textHdc, hFont)

        Dim dttOpts As New DttOpts With {
            .dwSize = Marshal.SizeOf(GetType(DttOpts)),
            .dwFlags = DttOptsFlags.DTT_COMPOSITED Or DttOptsFlags.DTT_TEXTCOLOR,
            .crText = _win32Color
        }
        If _glowEnabled Then
            dttOpts.dwFlags = dttOpts.dwFlags Or DttOptsFlags.DTT_GLOWSIZE
            dttOpts.iGlowSize = _glowSize
        End If

        Dim paddedBounds As New NativeRect(
            _padding.Left + offsetForGlow,
            _padding.Top + offsetForGlow,
            width - _padding.Right - offsetForGlow,
            height - _padding.Bottom - offsetForGlow)

        Dim renderer As VS.VisualStyleRenderer = GetRenderer()
        If renderer IsNot Nothing Then
            If FallbackText.NeedsFallback(_text, _font) Then
                ' Some characters need another font (Sundanese, for one): draw the text in pieces, each in the right font.
                DrawRunsThemed(renderer.Handle, paddedBounds, dttOpts)
            Else
                Dim hr As Integer = DrawThemeTextEx(renderer.Handle, _textHdc, 0, 0,
                                                    _text, -1, CInt(_formatFlags), paddedBounds, dttOpts)
                If hr <> 0 Then
                    Marshal.ThrowExceptionForHR(hr)
                End If
            End If
        Else
            Using g As Graphics = Graphics.FromHdc(_textHdc)
                TextRenderer.DrawText(g, _text, _font,
                                      Rectangle.FromLTRB(paddedBounds.Left, paddedBounds.Top, paddedBounds.Right, paddedBounds.Bottom),
                                      ColorTranslator.FromWin32(_win32Color), _formatFlags)
            End Using
        End If

        DeleteObject(hFont)
        _invalidated = False

        Return _textHdc
    End Function

    ''' <summary>Draws the text piece by piece when different characters need different fonts, keeping the label's alignment.</summary>
    Private Sub DrawRunsThemed(hTheme As IntPtr, bounds As NativeRect, options As DttOpts)
        Dim runs = FallbackText.Split(_text, _font)
        Dim total As Integer = runs.Sum(Function(r) r.Width)

        Dim x As Integer = bounds.Left
        If (_formatFlags And TextFormatFlags.HorizontalCenter) <> 0 Then
            x = bounds.Left + (bounds.Width - total) \ 2
        ElseIf (_formatFlags And TextFormatFlags.Right) <> 0 Then
            x = bounds.Right - total
        End If

        Dim y As Integer = bounds.Top
        If (_formatFlags And TextFormatFlags.VerticalCenter) <> 0 Then
            y = bounds.Top + (bounds.Height - _font.Height) \ 2
        ElseIf (_formatFlags And TextFormatFlags.Bottom) <> 0 Then
            y = bounds.Bottom - _font.Height
        End If

        Const pieceFlags As TextFormatFlags = TextFormatFlags.Left Or TextFormatFlags.Top Or TextFormatFlags.SingleLine Or TextFormatFlags.NoPrefix Or TextFormatFlags.NoPadding
        For Each piece In runs
            Dim hPieceFont As IntPtr = piece.RunFont.ToHfont()
            Dim previous As IntPtr = SelectObject(_textHdc, hPieceFont)
            Dim pieceY As Integer = y + FallbackText.BaselineShift(_font, piece.RunFont)
            Dim rect As New NativeRect(x, pieceY, x + piece.Width + 4, pieceY + piece.RunFont.Height + 2)
            Dim hr As Integer = DrawThemeTextEx(hTheme, _textHdc, 0, 0, piece.Text, -1, CInt(pieceFlags), rect, options)
            SelectObject(_textHdc, previous)
            DeleteObject(hPieceFont)
            If hr <> 0 Then Marshal.ThrowExceptionForHR(hr)
            x += piece.Width
        Next
    End Sub

End Class
