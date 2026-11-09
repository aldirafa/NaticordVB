Imports System.Drawing.Drawing2D
Imports NaticordVB.Networking

''' <summary>Draws round avatars: the downloaded picture, or a grey circle with the initial while it is missing.</summary>
Friend Module AvatarPainter

    ''' <param name="iconUrl">The CDN address of the picture (only read from the cache; nothing is downloaded here).</param>
    ''' <param name="title">Whose avatar it is; its first letter is shown when there is no picture.</param>
    Friend Sub Draw(g As Graphics, rect As Rectangle, iconUrl As String, title As String, font As Font)
        Dim picture As Bitmap = AvatarCache.TryGet(iconUrl)
        Dim oldMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias

        Using path As New GraphicsPath()
            path.AddEllipse(rect)
            If picture IsNot Nothing Then
                Dim oldInterpolation = g.InterpolationMode
                g.InterpolationMode = InterpolationMode.HighQualityBicubic
                Dim state = g.Save()
                g.SetClip(path, CombineMode.Replace)
                g.DrawImage(picture, rect)
                g.Restore(state)
                g.InterpolationMode = oldInterpolation
            Else
                g.FillPath(SystemBrushes.ControlDark, path)
                ' First letter as a whole "text element": keeps surrogate pairs and letters with combining marks together.
                Dim initial As String = If(String.IsNullOrEmpty(title), "?", Globalization.StringInfo.GetNextTextElement(title).ToUpperInvariant())
                FallbackText.DrawText(g, initial, font, rect, Color.White,
                                      TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix)
            End If
        End Using

        g.SmoothingMode = oldMode
    End Sub

End Module
