''' <summary>
''' A one-line label that shows every script (see <see cref="FallbackText"/>). A normal WinForms Label would show Sundanese,
''' for instance, as empty boxes. Text that is too long ends with "...".
''' </summary>
Friend Class FallbackLabel
    Inherits Control

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or ControlStyles.SupportsTransparentBackColor, True)
        SetStyle(ControlStyles.Selectable, False)
        TabStop = False
    End Sub

    Protected Overrides Sub OnTextChanged(e As EventArgs)
        MyBase.OnTextChanged(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim area As Rectangle = Rectangle.FromLTRB(Padding.Left, Padding.Top, ClientSize.Width - Padding.Right, ClientSize.Height - Padding.Bottom)
        If area.Width <= 0 OrElse area.Height <= 0 Then Return
        FallbackText.DrawText(e.Graphics, Text, Font, area, ForeColor,
                              TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
    End Sub

End Class
