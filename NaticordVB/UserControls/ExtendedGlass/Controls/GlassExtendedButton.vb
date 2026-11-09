Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports NaticordVB.Interop

Public Class GlassExtendedButton

    Private _buttonLabel As String
    Private _buttonIcon As Image
    Private _compositionDisabled As Boolean

    Public Event ButtonClick As EventHandler

    Public Sub New()
        SetStyle(ControlStyles.SupportsTransparentBackColor, True)
        BackColor = Color.Transparent

        InitializeComponent()

        ' Initialize underscore variables
        _buttonLabel = "Button"
        _buttonIcon = Nothing
        _compositionDisabled = False

        ' Show the default label text on the caption label
        captionLabel.Text = _buttonLabel

        Cursor = Cursors.Hand
        AddHandler Click, AddressOf AllParts_Click
        For Each ctrl As Control In Controls
            ctrl.Cursor = Cursors.Hand
            AddHandler ctrl.Click, AddressOf AllParts_Click
        Next

        AddHandler iconPicture.Paint,
            Sub(s As Object, e As PaintEventArgs)
                If iconPicture.Image IsNot Nothing AndAlso Not _compositionDisabled Then
                    DrawImageOnGlass(e.Graphics, iconPicture.Image, New Rectangle(0, 0, iconPicture.Width, iconPicture.Height))
                End If
            End Sub
        captionLabel.RebindPaintSubscriptions()
    End Sub

    ' This makes it so if you click anywhere on the button, it'll register.
    Private Sub AllParts_Click(sender As Object, e As EventArgs)
        RaiseEvent ButtonClick(Me, e)
    End Sub

    <Browsable(True), Category("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
    Public Property ButtonLabel As String
        Get
            Return _buttonLabel
        End Get
        Set(value As String)
            If _buttonLabel <> value Then
                _buttonLabel = value
                captionLabel.Text = _buttonLabel
            End If
        End Set
    End Property

    <Browsable(True), Category("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
    Public Property ButtonIcon As Image
        Get
            Return _buttonIcon
        End Get
        Set(value As Image)
            If _buttonIcon IsNot value Then
                _buttonIcon = value
                iconPicture.Image = PremultiplyAlpha(_buttonIcon)
            End If
        End Set
    End Property

    Private _reversed As Boolean = False

    <Browsable(True), Category("Appearance"), Description("Reverses the order of the elements in the control."), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
    Public Property ReverseOrder As Boolean
        Get
            Return _reversed
        End Get
        Set(value As Boolean)
            If _reversed <> value Then
                _reversed = value
                LayoutControls()
            End If
        End Set
    End Property

    Private Sub LayoutControls()
        Const gap As Integer = 5
        If _reversed Then
            captionLabel.TextAlign = HorizontalAlignment.Right
            captionLabel.Location = New Point(0, captionLabel.Top)
            iconPicture.Location = New Point(captionLabel.Right + gap, iconPicture.Top)
        Else
            captionLabel.TextAlign = HorizontalAlignment.Left
            iconPicture.Location = New Point(0, iconPicture.Top)
            captionLabel.Location = New Point(iconPicture.Right + gap, captionLabel.Top)
        End If
    End Sub

    <Browsable(True), Category("Appearance"), Description("Disables composition on the button label text."), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
    Public Property CompositionDisabled As Boolean
        Get
            Return _compositionDisabled
        End Get
        Set(value As Boolean)
            If _compositionDisabled <> value Then
                _compositionDisabled = value
                captionLabel.CompositionDisabled = _compositionDisabled
                captionLabel.Invalidate()
                iconPicture.Invalidate()
            End If
        End Set
    End Property

    Private Sub DrawImageOnGlass(outputG As Graphics, image As Image, blitBounds As Rectangle)
        If blitBounds.Width <= 0 OrElse blitBounds.Height <= 0 Then Return

        Dim outputHdc As IntPtr = outputG.GetHdc()
        Dim memDc As IntPtr = CreateCompatibleDC(outputHdc)

        Dim bmi As New BitmapInfo With {
            .biSize = Marshal.SizeOf(GetType(BitmapInfo)),
            .biWidth = blitBounds.Width,
            .biHeight = -blitBounds.Height,
            .biPlanes = 1,
            .biBitCount = 32,
            .biCompression = 0
        }

        Dim dib As IntPtr = CreateDIBSection(outputHdc, bmi, 0UI, 0, IntPtr.Zero, 0UI)
        SelectObject(memDc, dib)

        Using memG As Graphics = Graphics.FromHdc(memDc)
            memG.DrawImageUnscaled(image, 0, 0)
        End Using

        BitBlt(outputHdc, blitBounds.X, blitBounds.Y, blitBounds.Width, blitBounds.Height, memDc, 0, 0, BitBltOp.SRCCOPY)

        DeleteObject(dib)
        DeleteDC(memDc)
        outputG.ReleaseHdc(outputHdc)
    End Sub

    Private Shared Function PremultiplyAlpha(image As Image) As Bitmap
        Dim bmp As New Bitmap(image.Width, image.Height, PixelFormat.Format32bppPArgb)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.CompositingMode = CompositingMode.SourceOver
            g.DrawImage(image, New Rectangle(0, 0, bmp.Width, bmp.Height))
        End Using
        Return bmp
    End Function

End Class
