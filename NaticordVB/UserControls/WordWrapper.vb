''' <summary>
''' Breaks text into lines that fit a width. Windows' own wrapping only breaks at spaces, so a long link would run off the edge;
''' here a word that is too long is cut across lines instead. Word widths are remembered, which keeps re-wrapping while the
''' window is resized cheap.
''' </summary>
Friend NotInheritable Class WordWrapper

    Private Const MeasureFlags As TextFormatFlags = TextFormatFlags.NoPadding Or TextFormatFlags.NoPrefix Or TextFormatFlags.SingleLine Or TextFormatFlags.Left

    Private ReadOnly _font As Font
    Private ReadOnly _widths As New Dictionary(Of String, Integer)()
    Private ReadOnly _spaceWidth As Integer

    Friend Sub New(font As Font)
        _font = font
        _spaceWidth = Measure(" ")
    End Sub

    Private Function Measure(text As String) As Integer
        If text.Length = 0 Then Return 0
        Return FallbackText.MeasureWidth(text, _font)
    End Function

    ''' <summary>The width of a word (cached; very long strings are not cached).</summary>
    Private Function WordWidth(word As String) As Integer
        If word.Length > 60 Then Return Measure(word)
        Dim width As Integer
        If Not _widths.TryGetValue(word, width) Then
            width = Measure(word)
            _widths(word) = width
        End If
        Return width
    End Function

    ''' <summary>The width of a whole string in this font, in pixels.</summary>
    Friend Function TextWidth(text As String) As Integer
        Return Measure(text)
    End Function

    ''' <summary>
    ''' The lines of <paramref name="text"/> when drawn at most <paramref name="maxWidth"/> pixels wide. New lines in the text are kept;
    ''' an empty text gives no lines.
    ''' </summary>
    Friend Function Wrap(text As String, maxWidth As Integer) As List(Of String)
        Dim lines As New List(Of String)()
        If String.IsNullOrEmpty(text) Then Return lines

        Dim normalized As String = text.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Replace(vbTab, "    ")
        For Each paragraph In normalized.Split(vbLf(0))
            WrapParagraph(paragraph, maxWidth, lines)
        Next
        Return lines
    End Function

    Private Sub WrapParagraph(paragraph As String, maxWidth As Integer, lines As List(Of String))
        If paragraph.Length = 0 Then
            lines.Add("")
            Return
        End If

        Dim current As New Text.StringBuilder()
        Dim currentWidth As Integer = 0

        For Each word In paragraph.Split(" "c)
            Dim wordWidth As Integer = Me.WordWidth(word)
            Dim needed As Integer = If(current.Length = 0, wordWidth, currentWidth + _spaceWidth + wordWidth)

            If needed <= maxWidth Then
                If current.Length > 0 Then current.Append(" "c)
                current.Append(word)
                currentWidth = needed
                Continue For
            End If

            ' The word doesn't fit on this line: finish the line and start a new one.
            If current.Length > 0 Then
                lines.Add(current.ToString())
                current.Clear()
                currentWidth = 0
            End If

            If wordWidth <= maxWidth Then
                current.Append(word)
                currentWidth = wordWidth
            Else
                ' Longer than a whole line (a link, say): cut it up, keeping the last piece to continue the paragraph.
                Dim rest As String = word
                Do While Measure(rest) > maxWidth
                    Dim cut As Integer = FitLength(rest, maxWidth)
                    lines.Add(rest.Substring(0, cut))
                    rest = rest.Substring(cut)
                Loop
                current.Append(rest)
                currentWidth = Measure(rest)
            End If
        Next

        lines.Add(current.ToString())
    End Sub

    ''' <summary>How many leading characters of <paramref name="text"/> fit in the width (at least one, so the loop always advances).</summary>
    Private Function FitLength(text As String, maxWidth As Integer) As Integer
        Dim low As Integer = 1
        Dim high As Integer = text.Length
        Do While low < high
            Dim mid As Integer = (low + high + 1) \ 2
            If Measure(text.Substring(0, mid)) <= maxWidth Then low = mid Else high = mid - 1
        Loop
        Return low
    End Function

End Class
