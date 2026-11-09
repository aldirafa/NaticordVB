Imports System.Runtime.InteropServices
Imports NaticordVB.Interop

''' <summary>
''' Draws and measures single lines of text with a font fallback for scripts the main font lacks.
'''
''' Windows' GDI text already falls back for many scripts (Chinese, Arabic, emoji...) through its "font link" list, but not for the
''' ones only the "Sans Serif Collection" font covers, such as Sundanese: those show up as empty boxes. So characters that the main
''' font has no glyph for, but a fallback font has, are drawn with that fallback font; everything else takes the normal path, so
''' ordinary text looks exactly as before.
''' </summary>
Friend Module FallbackText

    ''' <summary>Fonts tried, in order, for a character the main font cannot show.</summary>
    Private ReadOnly FallbackFamilies As String() = {"Sans Serif Collection"}

    ''' <summary>Below this code point every character is Latin-ish and the main font has it, so no lookup is needed.</summary>
    Private Const FirstCheckedCodePoint As Integer = &H250

    Private ReadOnly Coverage As New Dictionary(Of String, Boolean)()
    Private ReadOnly Fallbacks As New Dictionary(Of String, Font)()
    Private ReadOnly Gate As New Object()

    <DllImport("gdi32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
    Private Function GetGlyphIndicesW(hdc As IntPtr, text As String, count As Integer, <Out> indices As UShort(), flags As UInteger) As UInteger
    End Function

    Private Const GGI_MARK_NONEXISTING_GLYPHS As UInteger = 1UI

    Private Function FontKey(font As Font) As String
        Return font.FontFamily.Name & "|" & font.SizeInPoints.ToString(Globalization.CultureInfo.InvariantCulture) & "|" & CInt(font.Style)
    End Function

    ''' <summary>Does this font have a glyph for the character? (Characters above U+FFFF are assumed to be handled by Windows itself.)</summary>
    Private Function HasGlyph(font As Font, codePoint As Integer) As Boolean
        If codePoint > &HFFFF Then Return True

        Dim key As String = FontKey(font) & "#" & codePoint
        SyncLock Gate
            Dim known As Boolean
            If Coverage.TryGetValue(key, known) Then Return known
        End SyncLock

        Dim found As Boolean = True
        Using g As Graphics = Graphics.FromHwnd(IntPtr.Zero)
            Dim hdc As IntPtr = g.GetHdc()
            Dim hFont As IntPtr = font.ToHfont()
            Dim previous As IntPtr = SelectObject(hdc, hFont)
            Try
                Dim indices(0) As UShort
                Dim result As UInteger = GetGlyphIndicesW(hdc, ChrW(codePoint), 1, indices, GGI_MARK_NONEXISTING_GLYPHS)
                found = result <> &HFFFFFFFFUI AndAlso indices(0) <> &HFFFFUS
            Finally
                SelectObject(hdc, previous)
                DeleteObject(hFont)
                g.ReleaseHdc(hdc)
            End Try
        End Using

        SyncLock Gate
            Coverage(key) = found
        End SyncLock
        Return found
    End Function

    ''' <summary>The same size and style as <paramref name="baseFont"/>, in a fallback family. Nothing if that family isn't installed.</summary>
    Private Function FallbackFont(family As String, baseFont As Font) As Font
        Dim key As String = family & "|" & baseFont.SizeInPoints.ToString(Globalization.CultureInfo.InvariantCulture) & "|" & CInt(baseFont.Style)
        SyncLock Gate
            Dim cached As Font = Nothing
            If Fallbacks.TryGetValue(key, cached) Then Return cached

            Dim created As Font = Nothing
            Try
                Using testFamily As New FontFamily(family)
                    If testFamily.IsStyleAvailable(baseFont.Style) Then
                        created = New Font(testFamily, baseFont.SizeInPoints, baseFont.Style, GraphicsUnit.Point)
                    ElseIf testFamily.IsStyleAvailable(FontStyle.Regular) Then
                        created = New Font(testFamily, baseFont.SizeInPoints, FontStyle.Regular, GraphicsUnit.Point)
                    End If
                End Using
            Catch ex As ArgumentException
                created = Nothing ' not installed on this machine
            End Try
            Fallbacks(key) = created
            Return created
        End SyncLock
    End Function

    ''' <summary>The font to draw this character with: the main font, or a fallback font that has the glyph.</summary>
    Private Function PickFont(baseFont As Font, codePoint As Integer) As Font
        If codePoint < FirstCheckedCodePoint OrElse HasGlyph(baseFont, codePoint) Then Return baseFont

        For Each family In FallbackFamilies
            Dim candidate As Font = FallbackFont(family, baseFont)
            If candidate IsNot Nothing AndAlso HasGlyph(candidate, codePoint) Then Return candidate
        Next
        Return baseFont ' nobody has it: leave it to GDI's own fallback
    End Function

    ''' <summary>How many pixels lower (positive) or higher a piece in <paramref name="pieceFont"/> must be drawn so its baseline lines up with the main font's.</summary>
    Friend Function BaselineShift(baseFont As Font, pieceFont As Font) As Integer
        If Object.ReferenceEquals(baseFont, pieceFont) Then Return 0
        Return CInt(Math.Round(AscentPixels(baseFont) - AscentPixels(pieceFont)))
    End Function

    Private ReadOnly Property ScreenDpi As Single
        Get
            Using g As Graphics = Graphics.FromHwnd(IntPtr.Zero)
                Return g.DpiY
            End Using
        End Get
    End Property

    Private Function AscentPixels(font As Font) As Single
        Dim family As FontFamily = font.FontFamily
        Dim emHeight As Integer = family.GetEmHeight(font.Style)
        If emHeight <= 0 Then Return 0
        Return family.GetCellAscent(font.Style) / CSng(emHeight) * font.SizeInPoints * ScreenDpi / 72.0F
    End Function

    Friend NotInheritable Class Run
        Friend Text As String = ""
        Friend RunFont As Font
        Friend Width As Integer
    End Class

    ''' <summary>True when some character of the text needs a fallback font.</summary>
    Friend Function NeedsFallback(text As String, baseFont As Font) As Boolean
        If String.IsNullOrEmpty(text) Then Return False
        Dim i As Integer = 0
        Do While i < text.Length
            Dim isPair As Boolean = Char.IsHighSurrogate(text(i)) AndAlso i + 1 < text.Length AndAlso Char.IsLowSurrogate(text(i + 1))
            Dim cp As Integer = If(isPair, Char.ConvertToUtf32(text(i), text(i + 1)), AscW(text(i)))
            If cp >= FirstCheckedCodePoint AndAlso Not Object.ReferenceEquals(PickFont(baseFont, cp), baseFont) Then Return True
            i += If(isPair, 2, 1)
        Loop
        Return False
    End Function

    ''' <summary>Splits the text into runs that each use one font.</summary>
    Friend Function Split(text As String, baseFont As Font) As List(Of Run)
        Dim runs As New List(Of Run)()
        Dim current As Run = Nothing
        Dim builder As New Text.StringBuilder()

        Dim i As Integer = 0
        Do While i < text.Length
            Dim width As Integer = If(Char.IsHighSurrogate(text(i)) AndAlso i + 1 < text.Length AndAlso Char.IsLowSurrogate(text(i + 1)), 2, 1)
            Dim cp As Integer = If(width = 2, Char.ConvertToUtf32(text(i), text(i + 1)), AscW(text(i)))
            Dim chosen As Font = PickFont(baseFont, cp)

            If current Is Nothing OrElse Not Object.ReferenceEquals(current.RunFont, chosen) Then
                If current IsNot Nothing Then current.Text = builder.ToString()
                builder.Clear()
                current = New Run With {.RunFont = chosen}
                runs.Add(current)
            End If
            builder.Append(text, i, width)
            i += width
        Loop
        If current IsNot Nothing Then current.Text = builder.ToString()

        For Each r In runs
            r.Width = TextRenderer.MeasureText(r.Text, r.RunFont, New Size(Integer.MaxValue, Integer.MaxValue), MeasureFlags).Width
        Next
        Return runs
    End Function

    Private Const MeasureFlags As TextFormatFlags = TextFormatFlags.NoPadding Or TextFormatFlags.NoPrefix Or TextFormatFlags.SingleLine Or TextFormatFlags.Left

    ''' <summary>The width in pixels of one line of text, counting fallback fonts.</summary>
    Friend Function MeasureWidth(text As String, baseFont As Font) As Integer
        If String.IsNullOrEmpty(text) Then Return 0
        If Not NeedsFallback(text, baseFont) Then
            Return TextRenderer.MeasureText(text, baseFont, New Size(Integer.MaxValue, Integer.MaxValue), MeasureFlags).Width
        End If
        Return Split(text, baseFont).Sum(Function(r) r.Width)
    End Function

    ''' <summary>Shortens the text with "..." so it fits the width.</summary>
    Private Function FitWithEllipsis(text As String, baseFont As Font, maxWidth As Integer) As String
        If MeasureWidth(text, baseFont) <= maxWidth Then Return text

        Dim low As Integer = 0
        Dim high As Integer = text.Length
        Do While low < high
            Dim mid As Integer = (low + high + 1) \ 2
            ' Don't cut between the two halves of a surrogate pair.
            Dim cut As Integer = If(mid > 0 AndAlso Char.IsHighSurrogate(text(mid - 1)), mid - 1, mid)
            If MeasureWidth(text.Substring(0, cut) & ChrW(&H2026), baseFont) <= maxWidth Then low = mid Else high = mid - 1
        Loop
        Dim keep As Integer = If(low > 0 AndAlso Char.IsHighSurrogate(text(low - 1)), low - 1, low)
        Return text.Substring(0, Math.Max(0, keep)) & ChrW(&H2026)
    End Function

    ''' <summary>
    ''' A drop-in for <c>TextRenderer.DrawText</c> for one line of text. Text that needs no fallback goes straight to
    ''' <c>TextRenderer</c>, exactly as before. Understands left/centre/right, top/centre/bottom and EndEllipsis.
    ''' </summary>
    Friend Sub DrawText(g As Graphics, text As String, baseFont As Font, bounds As Rectangle, color As Color, flags As TextFormatFlags)
        If String.IsNullOrEmpty(text) Then Return
        If Not NeedsFallback(text, baseFont) Then
            TextRenderer.DrawText(g, text, baseFont, bounds, color, flags)
            Return
        End If

        Dim line As String = If((flags And TextFormatFlags.EndEllipsis) <> 0, FitWithEllipsis(text, baseFont, bounds.Width), text)
        Dim runs = Split(line, baseFont)
        Dim total As Integer = runs.Sum(Function(r) r.Width)

        Dim x As Integer = bounds.Left
        If (flags And TextFormatFlags.HorizontalCenter) <> 0 Then
            x = bounds.Left + (bounds.Width - total) \ 2
        ElseIf (flags And TextFormatFlags.Right) <> 0 Then
            x = bounds.Right - total
        End If

        Dim y As Integer = bounds.Top
        If (flags And TextFormatFlags.VerticalCenter) <> 0 Then
            y = bounds.Top + (bounds.Height - baseFont.Height) \ 2
        ElseIf (flags And TextFormatFlags.Bottom) <> 0 Then
            y = bounds.Bottom - baseFont.Height
        End If

        Const runFlags As TextFormatFlags = TextFormatFlags.Left Or TextFormatFlags.Top Or TextFormatFlags.NoPadding Or TextFormatFlags.NoPrefix Or TextFormatFlags.SingleLine
        For Each r In runs
            TextRenderer.DrawText(g, r.Text, r.RunFont, New Point(x, y + BaselineShift(baseFont, r.RunFont)), color, runFlags)
            x += r.Width
        Next
    End Sub

End Module
