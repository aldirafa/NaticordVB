Imports System.ComponentModel
Imports NaticordVB.Interop
Imports VS = System.Windows.Forms.VisualStyles

<DefaultProperty("Text")>
Public Class AeroLabel
    Inherits Control

    Private _themeText As ThemedText
    Private _lastParent As Control
    Private _lastGrandparent As Control
    Private ReadOnly _subscribedSiblings As New List(Of Control)()

    Private _compositionDisabled As Boolean
    Private _restoringBackground As Boolean
    Private _fixClippingBug As Boolean

    <Description("Skips drawing overflow on transparent parents to fix double glow issues in certain containers."),
     Category("Appearance"),
     DefaultValue(False)>
    Public Property FixClippingBug As Boolean
        Get
            Return _fixClippingBug
        End Get
        Set(value As Boolean)
            _fixClippingBug = value
            Invalidate()
        End Set
    End Property

    <Description("Disables composition and forces text color on non-transparent backgrounds."),
     Category("Appearance"),
     DefaultValue(False)>
    Public Property CompositionDisabled As Boolean
        Get
            Return _compositionDisabled
        End Get
        Set(value As Boolean)
            _compositionDisabled = value
            Invalidate()
        End Set
    End Property

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        _themeText = New ThemedText()
        SyncThemeText()
    End Sub

    Protected Overrides Sub OnHandleDestroyed(e As EventArgs)
        UnsubscribeAll()
        _themeText?.Dispose()
        _themeText = Nothing
        MyBase.OnHandleDestroyed(e)
    End Sub

    Protected Overrides Sub OnParentChanged(e As EventArgs)
        RebindSubscriptions()
        MyBase.OnParentChanged(e)
    End Sub

    Public Sub RebindPaintSubscriptions()
        RebindSubscriptions()
    End Sub

    Private Sub RebindSubscriptions()
        UnsubscribeAll()

        _lastParent = Parent
        If Parent Is Nothing Then Return

        AddHandler Parent.Paint, AddressOf OnParentPaint
        AddHandler Parent.ControlAdded, AddressOf OnSiblingsChanged
        AddHandler Parent.ControlRemoved, AddressOf OnSiblingsChanged
        AddHandler Parent.ParentChanged, AddressOf OnParentParentChanged

        _lastGrandparent = Parent.Parent
        If _lastGrandparent IsNot Nothing Then
            AddHandler _lastGrandparent.Paint, AddressOf OnGrandparentPaint
        End If

        SubscribeSiblings()
    End Sub

    Private Sub UnsubscribeAll()
        UnsubscribeSiblings()

        If _lastGrandparent IsNot Nothing Then
            RemoveHandler _lastGrandparent.Paint, AddressOf OnGrandparentPaint
            _lastGrandparent = Nothing
        End If

        If _lastParent IsNot Nothing Then
            RemoveHandler _lastParent.Paint, AddressOf OnParentPaint
            RemoveHandler _lastParent.ControlAdded, AddressOf OnSiblingsChanged
            RemoveHandler _lastParent.ControlRemoved, AddressOf OnSiblingsChanged
            RemoveHandler _lastParent.ParentChanged, AddressOf OnParentParentChanged
            _lastParent = Nothing
        End If
    End Sub

    Private Sub UnsubscribeSiblings()
        For Each s As Control In _subscribedSiblings
            RemoveHandler s.Paint, AddressOf OnSiblingPaint
        Next
        _subscribedSiblings.Clear()
    End Sub

    Private Sub SubscribeSiblings()
        If Parent Is Nothing Then Return
        For Each sibling As Control In Parent.Controls
            If sibling Is Me Then Continue For
            AddHandler sibling.Paint, AddressOf OnSiblingPaint
            _subscribedSiblings.Add(sibling)
        Next
    End Sub

    Private Sub OnSiblingsChanged(sender As Object, e As ControlEventArgs)
        UnsubscribeSiblings()
        SubscribeSiblings()
    End Sub

    Private Sub OnParentParentChanged(sender As Object, e As EventArgs)
        If _lastGrandparent IsNot Nothing Then
            RemoveHandler _lastGrandparent.Paint, AddressOf OnGrandparentPaint
        End If
        _lastGrandparent = Parent?.Parent
        If _lastGrandparent IsNot Nothing Then
            AddHandler _lastGrandparent.Paint, AddressOf OnGrandparentPaint
        End If
    End Sub

    Private ReadOnly Property GlowDrawable As Boolean
        Get
            Return Visible AndAlso _glowEnabled AndAlso Not _compositionDisabled AndAlso _themeText IsNot Nothing
        End Get
    End Property

    Private Shared Function IsFullyTransparent(c As Control) As Boolean
        Return c.BackColor.A = 0 AndAlso c.BackgroundImage Is Nothing
    End Function

    Private Sub OnParentPaint(sender As Object, e As PaintEventArgs)
        If GlowDrawable AndAlso Not _restoringBackground Then
            If _fixClippingBug AndAlso Parent.Parent IsNot Nothing AndAlso IsFullyTransparent(Parent) Then Return
            _themeText.DrawOverflow(e.Graphics, Left, Top, Width, Height)
        End If
    End Sub

    Private Sub OnGrandparentPaint(sender As Object, e As PaintEventArgs)
        If GlowDrawable AndAlso Parent IsNot Nothing AndAlso Not _restoringBackground Then
            _themeText.BlendOverflow(e.Graphics, Parent.Left + Left, Parent.Top + Top, Width, Height)
        End If
    End Sub

    Private Sub OnSiblingPaint(sender As Object, e As PaintEventArgs)
        If Not GlowDrawable OrElse _restoringBackground Then Return
        Dim sibling As Control = DirectCast(sender, Control)
        _themeText.BlendOverflow(e.Graphics, Left - sibling.Left, Top - sibling.Top, Width, Height)
    End Sub

    Public Overrides Property Text As String
        Get
            Return MyBase.Text
        End Get
        Set(value As String)
            MyBase.Text = value
            SyncThemeText()
        End Set
    End Property

    Public Overrides Property Font As Font
        Get
            Return MyBase.Font
        End Get
        Set(value As Font)
            MyBase.Font = value
            SyncThemeText()
        End Set
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
    Public Shadows Property Padding As Padding
        Get
            Return MyBase.Padding
        End Get
        Set(value As Padding)
            MyBase.Padding = value
            SyncThemeText()
        End Set
    End Property

    Public Overrides Property ForeColor As Color
        Get
            Return MyBase.ForeColor
        End Get
        Set(value As Color)
            MyBase.ForeColor = value
            SyncThemeText()
        End Set
    End Property

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Shadows Property BackColor As Color
        Get
            Return MyBase.BackColor
        End Get
        Set(value As Color)
            ' Ignored on purpose: the label always paints over its parent.
        End Set
    End Property

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Shadows Property BackgroundImage As Image
        Get
            Return MyBase.BackgroundImage
        End Get
        Set(value As Image)
            ' Ignored on purpose.
        End Set
    End Property

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Shadows Property BackgroundImageLayout As ImageLayout
        Get
            Return MyBase.BackgroundImageLayout
        End Get
        Set(value As ImageLayout)
            ' Ignored on purpose.
        End Set
    End Property

    Private _glowSize As Integer = ThemedText.DefaultGlowSize

    <Description("Size of the glow effect around the text."),
     Category("Appearance"),
     DefaultValue(ThemedText.DefaultGlowSize)>
    Public Property GlowSize As Integer
        Get
            Return _glowSize
        End Get
        Set(value As Integer)
            _glowSize = value
            SyncThemeText()
        End Set
    End Property

    Private _glowEnabled As Boolean = True

    <Description("Enables or disables the glow effect around the text."),
     Category("Appearance"),
     DefaultValue(True)>
    Public Property GlowEnabled As Boolean
        Get
            Return _glowEnabled
        End Get
        Set(value As Boolean)
            _glowEnabled = value
            SyncThemeText()
        End Set
    End Property

    Private _horizontal As HorizontalAlignment = HorizontalAlignment.Left

    <Description("Horizontal text alignment."),
     Category("Appearance"),
     DefaultValue(GetType(HorizontalAlignment), "Left")>
    Public Property TextAlign As HorizontalAlignment
        Get
            Return _horizontal
        End Get
        Set(value As HorizontalAlignment)
            _horizontal = value
            SyncThemeText()
        End Set
    End Property

    Private _vertical As VS.VerticalAlignment = VS.VerticalAlignment.Top

    <Description("Vertical text alignment."),
     Category("Appearance"),
     DefaultValue(GetType(VS.VerticalAlignment), "Top")>
    Public Property TextAlignVertical As VS.VerticalAlignment
        Get
            Return _vertical
        End Get
        Set(value As VS.VerticalAlignment)
            _vertical = value
            SyncThemeText()
        End Set
    End Property

    Private _singleLine As Boolean = True

    <Description("Lay out text on a single line."),
     Category("Appearance"),
     DefaultValue(True)>
    Public Property SingleLine As Boolean
        Get
            Return _singleLine
        End Get
        Set(value As Boolean)
            _singleLine = value
            SyncThemeText()
        End Set
    End Property

    Private _endEllipsis As Boolean

    <Description("Trim overflowing text with an ellipsis."),
     Category("Appearance"),
     DefaultValue(False)>
    Public Property EndEllipsis As Boolean
        Get
            Return _endEllipsis
        End Get
        Set(value As Boolean)
            _endEllipsis = value
            SyncThemeText()
        End Set
    End Property

    Private _wordBreak As Boolean

    <Description("Break text only at word boundaries."),
     Category("Appearance"),
     DefaultValue(False)>
    Public Property WordBreak As Boolean
        Get
            Return _wordBreak
        End Get
        Set(value As Boolean)
            _wordBreak = value
            SyncThemeText()
        End Set
    End Property

    Private _wordEllipsis As Boolean

    <Description("Trim to the nearest word and append an ellipsis."),
     Category("Appearance"),
     DefaultValue(False)>
    Public Property WordEllipsis As Boolean
        Get
            Return _wordEllipsis
        End Get
        Set(value As Boolean)
            _wordEllipsis = value
            SyncThemeText()
        End Set
    End Property

    Private Sub SyncThemeText()
        If _themeText Is Nothing Then Return

        _themeText.Text = Text
        _themeText.Font = Font
        _themeText.Padding = Padding
        _themeText.Color = ForeColor
        _themeText.FormatFlags = BuildFormatFlags()
        _themeText.GlowSize = _glowSize
        _themeText.GlowEnabled = _glowEnabled

        Invalidate()
    End Sub

    Private Function BuildFormatFlags() As TextFormatFlags
        Dim flags As TextFormatFlags = TextFormatFlags.Default

        Select Case _horizontal
            Case HorizontalAlignment.Left : flags = flags Or TextFormatFlags.Left
            Case HorizontalAlignment.Center : flags = flags Or TextFormatFlags.HorizontalCenter
            Case HorizontalAlignment.Right : flags = flags Or TextFormatFlags.Right
        End Select

        Select Case _vertical
            Case VS.VerticalAlignment.Top : flags = flags Or TextFormatFlags.Top
            Case VS.VerticalAlignment.Center : flags = flags Or TextFormatFlags.VerticalCenter
            Case VS.VerticalAlignment.Bottom : flags = flags Or TextFormatFlags.Bottom
        End Select

        If _singleLine Then flags = flags Or TextFormatFlags.SingleLine
        If _endEllipsis Then flags = flags Or TextFormatFlags.EndEllipsis
        If _wordBreak Then flags = flags Or TextFormatFlags.WordBreak
        If _wordEllipsis Then flags = flags Or TextFormatFlags.WordEllipsis

        If RightToLeft = RightToLeft.Yes Then
            flags = flags Or TextFormatFlags.RightToLeft
        End If

        Return flags
    End Function

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnInvalidated(e As InvalidateEventArgs)
        MyBase.OnInvalidated(e)
        If Parent Is Nothing Then Return

        Dim inflate As Integer = If(_glowEnabled, _glowSize, 0)
        Dim inflatedInParent As New Rectangle(
            Left - inflate, Top - inflate,
            Width + 2 * inflate, Height + 2 * inflate)

        Parent.Invalidate(inflatedInParent, False)

        If Parent.Parent IsNot Nothing Then
            Parent.Parent.Invalidate(New Rectangle(
                Parent.Left + Left - inflate, Parent.Top + Top - inflate,
                Width + 2 * inflate, Height + 2 * inflate), False)
        End If

        For Each sibling As Control In Parent.Controls
            If sibling Is Me Then Continue For
            Dim siblingBounds As New Rectangle(sibling.Left, sibling.Top, sibling.Width, sibling.Height)
            Dim overlap As Rectangle = Rectangle.Intersect(inflatedInParent, siblingBounds)
            If overlap.IsEmpty Then Continue For

            sibling.Invalidate(New Rectangle(
                overlap.X - sibling.Left, overlap.Y - sibling.Top,
                overlap.Width, overlap.Height), False)
        Next
    End Sub

    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
        If Parent Is Nothing OrElse DesignMode Then Return

        _restoringBackground = True
        Dim state As Drawing2D.GraphicsState = e.Graphics.Save()
        Try
            e.Graphics.TranslateTransform(-Left, -Top)
            Using pe As New PaintEventArgs(e.Graphics, New Rectangle(Left, Top, Width, Height))
                InvokePaintBackground(Parent, pe)
            End Using
        Finally
            e.Graphics.Restore(state)
            _restoringBackground = False
        End Try
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        If DesignMode OrElse (_compositionDisabled AndAlso BackColor.A <> 0) Then
            Using sf As StringFormat = BuildStringFormat()
                Using brush As New SolidBrush(ForeColor)
                    e.Graphics.DrawString(Text, Font, brush,
                        New RectangleF(Padding.Left, Padding.Top,
                                       Width - Padding.Horizontal, Height - Padding.Vertical), sf)
                End Using
            End Using
            Return
        End If

        If Not Visible Then Return

        If _glowEnabled Then
            _themeText.DrawInBounds(e.Graphics, Width, Height)
        Else
            _themeText.Draw(e.Graphics, 0, 0, Width, Height)
        End If
    End Sub

    Private Function BuildStringFormat() As StringFormat
        Dim sf As New StringFormat()

        Select Case _horizontal
            Case HorizontalAlignment.Left : sf.Alignment = StringAlignment.Near
            Case HorizontalAlignment.Center : sf.Alignment = StringAlignment.Center
            Case HorizontalAlignment.Right : sf.Alignment = StringAlignment.Far
        End Select

        Select Case _vertical
            Case VS.VerticalAlignment.Top : sf.LineAlignment = StringAlignment.Near
            Case VS.VerticalAlignment.Center : sf.LineAlignment = StringAlignment.Center
            Case VS.VerticalAlignment.Bottom : sf.LineAlignment = StringAlignment.Far
        End Select

        Return sf
    End Function

    Protected Overrides Sub WndProc(ByRef m As Message)
        MyBase.WndProc(m)

        If m.Msg = CInt(WindowMessage.WM_NCHITTEST) AndAlso Not DesignMode Then
            m.Result = New IntPtr(CInt(HitTest.HTTRANSPARENT))
        End If
    End Sub

End Class
