Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms.VisualStyles
Imports NaticordVB.Networking

Friend Enum EntryKind As Integer
    ''' <summary>A direct message or group DM.</summary>
    Dm
    ''' <summary>A server (guild).</summary>
    Guild
    ''' <summary>A text channel inside a server.</summary>
    Channel
    ''' <summary>A non-clickable heading ("TEXT CHANNELS").</summary>
    Category
    ''' <summary>"&lt; Servers": goes back to the server list.</summary>
    Back
    ''' <summary>A status line such as "Loading..." or an error. Clickable so it can offer a retry.</summary>
    Notice
End Enum

''' <summary>One row of the sidebar list.</summary>
Friend NotInheritable Class SidebarEntry
    Public Property Kind As EntryKind
    ''' <summary>Stable identity (channel/guild id, or a command such as "retry"); used to keep the selection across refreshes.</summary>
    Public Property Key As String = ""
    Public Property Title As String = ""
    Public Property Subtitle As String = ""
    ''' <summary>CDN image for the avatar/icon, or Nothing for the placeholder.</summary>
    Public Property IconUrl As String
    ''' <summary>The model object behind the row (a channel or a guild).</summary>
    Public Property Tag As Object

    Public ReadOnly Property IsSelectable As Boolean
        Get
            Return Kind <> EntryKind.Category
        End Get
    End Property
End Class

''' <summary>
''' The Messages / Servers list. A WinForms owner-drawn list because DirectUI could neither scroll nor tell which row was clicked.
''' Hover and selection use the same "Explorer" list-view look the old DirectUI rows had.
''' </summary>
Friend NotInheritable Class ChatListBox
    Inherits ListBox

    ''' <summary>Raised when the user clicks (or presses Enter on) a selectable row.</summary>
    Public Event EntryClicked As Action(Of SidebarEntry)

    Private _hot As Integer = -1
    Private _lastSelectable As Integer = -1

    Public Sub New()
        DrawMode = DrawMode.OwnerDrawVariable
        IntegralHeight = False
        BorderStyle = BorderStyle.None
        SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
    End Sub

    ''' <summary>Replaces all rows, keeping the scroll position and (if the row still exists) the selection.</summary>
    Public Sub SetEntries(entries As IEnumerable(Of SidebarEntry))
        Dim list = entries.ToList()
        Dim top As Integer = TopIndex
        Dim selectedKey As String = TryCast(SelectedItem, SidebarEntry)?.Key

        BeginUpdate()
        Try
            Items.Clear()
            _lastSelectable = -1
            _hot = -1
            Items.AddRange(list.ToArray())

            If selectedKey IsNot Nothing Then
                Dim idx = list.FindIndex(Function(e) e.Key = selectedKey AndAlso e.IsSelectable)
                If idx >= 0 Then SelectedIndex = idx
            End If
            If Items.Count > 0 Then TopIndex = Math.Min(top, Items.Count - 1)
        Finally
            EndUpdate()
        End Try
    End Sub

    ''' <summary>Selects the row with this key (highlighting the chat that is open). Pass Nothing to clear.</summary>
    Public Sub SelectKey(key As String)
        If key Is Nothing Then
            SelectedIndex = -1
            Return
        End If
        For i = 0 To Items.Count - 1
            Dim e = TryCast(Items(i), SidebarEntry)
            If e IsNot Nothing AndAlso e.Key = key AndAlso e.IsSelectable Then
                SelectedIndex = i
                Return
            End If
        Next
    End Sub

#Region "Measuring"

    Private Function RowHeight(kind As EntryKind) As Integer
        Select Case kind
            Case EntryKind.Dm, EntryKind.Guild : Return LogicalToDeviceUnits(48)
            Case EntryKind.Channel : Return LogicalToDeviceUnits(30)
            Case EntryKind.Category : Return LogicalToDeviceUnits(30)
            Case EntryKind.Back : Return LogicalToDeviceUnits(36)
            Case Else : Return LogicalToDeviceUnits(56)
        End Select
    End Function

    Protected Overrides Sub OnMeasureItem(e As MeasureItemEventArgs)
        Dim entry = TryCast(Items(e.Index), SidebarEntry)
        e.ItemHeight = If(entry Is Nothing, LogicalToDeviceUnits(30), RowHeight(entry.Kind))
    End Sub

#End Region

#Region "Drawing"

    Protected Overrides Sub OnDrawItem(e As DrawItemEventArgs)
        If e.Index < 0 Then Return
        Dim entry = TryCast(Items(e.Index), SidebarEntry)
        If entry Is Nothing Then Return

        Dim g = e.Graphics
        Dim bounds = e.Bounds
        g.FillRectangle(SystemBrushes.Window, bounds)

        Dim selected As Boolean = (e.State And DrawItemState.Selected) <> 0 AndAlso entry.IsSelectable
        Dim hot As Boolean = e.Index = _hot AndAlso entry.IsSelectable
        DrawRowBackground(g, bounds, hot, selected)

        Select Case entry.Kind
            Case EntryKind.Dm, EntryKind.Guild : DrawPersonRow(g, bounds, entry)
            Case EntryKind.Channel : DrawChannelRow(g, bounds, entry)
            Case EntryKind.Category : DrawCategoryRow(g, bounds, entry)
            Case EntryKind.Back : DrawBackRow(g, bounds, entry)
            Case Else : DrawNoticeRow(g, bounds, entry)
        End Select
    End Sub

    Private Shared Sub DrawRowBackground(g As Graphics, bounds As Rectangle, hot As Boolean, selected As Boolean)
        If Not hot AndAlso Not selected Then Return

        ' Explorer list-view parts: 2 = hot, 3 = selected, 6 = selected and hot.
        Dim state As Integer = If(selected AndAlso hot, 6, If(selected, 3, 2))
        Try
            Dim element = VisualStyleElement.CreateElement("Explorer::ListView", 1, state)
            If VisualStyleRenderer.IsSupported AndAlso VisualStyleRenderer.IsElementDefined(element) Then
                Dim inset = Rectangle.Inflate(bounds, -2, -1)
                Dim themed As New VisualStyleRenderer(element)
                themed.DrawBackground(g, inset)
                Return
            End If
        Catch ex As Exception
            ' fall through to the plain colours
        End Try

        g.FillRectangle(If(selected, SystemBrushes.ControlLight, SystemBrushes.ControlLightLight), bounds)
    End Sub

    Private Const TextFlags As TextFormatFlags = TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix

    Private Sub DrawPersonRow(g As Graphics, bounds As Rectangle, entry As SidebarEntry)
        Dim pad As Integer = LogicalToDeviceUnits(10)
        Dim size As Integer = LogicalToDeviceUnits(32)
        Dim avatar As New Rectangle(bounds.Left + pad, bounds.Top + (bounds.Height - size) \ 2, size, size)
        DrawAvatar(g, avatar, entry)

        Dim textLeft As Integer = avatar.Right + LogicalToDeviceUnits(10)
        Dim textWidth As Integer = bounds.Right - textLeft - pad

        If String.IsNullOrEmpty(entry.Subtitle) Then
            Dim rect As New Rectangle(textLeft, bounds.Top, textWidth, bounds.Height)
            TextRenderer.DrawText(g, entry.Title, Font, rect, SystemColors.WindowText, TextFlags)
        Else
            Dim half As Integer = bounds.Height \ 2
            Dim titleRect As New Rectangle(textLeft, bounds.Top + LogicalToDeviceUnits(4), textWidth, half - LogicalToDeviceUnits(4))
            Dim subRect As New Rectangle(textLeft, bounds.Top + half, textWidth, half - LogicalToDeviceUnits(4))
            TextRenderer.DrawText(g, entry.Title, Font, titleRect, SystemColors.WindowText, TextFlags)
            Using small As New Font(Font.FontFamily, Math.Max(7.0F, Font.Size - 1.0F))
                TextRenderer.DrawText(g, entry.Subtitle, small, subRect, SystemColors.GrayText, TextFlags)
            End Using
        End If
    End Sub

    Private Sub DrawChannelRow(g As Graphics, bounds As Rectangle, entry As SidebarEntry)
        Dim left As Integer = bounds.Left + LogicalToDeviceUnits(14)
        Dim hash As New Rectangle(left, bounds.Top, LogicalToDeviceUnits(16), bounds.Height)
        TextRenderer.DrawText(g, "#", Font, hash, SystemColors.GrayText, TextFlags)

        Dim rect As New Rectangle(hash.Right + LogicalToDeviceUnits(2), bounds.Top, bounds.Right - hash.Right - LogicalToDeviceUnits(12), bounds.Height)
        TextRenderer.DrawText(g, entry.Title, Font, rect, SystemColors.WindowText, TextFlags)
    End Sub

    Private Sub DrawCategoryRow(g As Graphics, bounds As Rectangle, entry As SidebarEntry)
        Dim rect As New Rectangle(bounds.Left + LogicalToDeviceUnits(12), bounds.Top + LogicalToDeviceUnits(6), bounds.Width - LogicalToDeviceUnits(20), bounds.Height - LogicalToDeviceUnits(6))
        Using small As New Font(Font.FontFamily, Math.Max(7.0F, Font.Size - 1.0F), FontStyle.Bold)
            TextRenderer.DrawText(g, entry.Title.ToUpperInvariant(), small, rect, SystemColors.GrayText, TextFlags)
        End Using
    End Sub

    Private Sub DrawBackRow(g As Graphics, bounds As Rectangle, entry As SidebarEntry)
        Dim rect As New Rectangle(bounds.Left + LogicalToDeviceUnits(12), bounds.Top, bounds.Width - LogicalToDeviceUnits(20), bounds.Height)
        Using bold As New Font(Font, FontStyle.Bold)
            TextRenderer.DrawText(g, ChrW(&H2039) & "  " & entry.Title, bold, rect, SystemColors.WindowText, TextFlags)
        End Using
    End Sub

    Private Sub DrawNoticeRow(g As Graphics, bounds As Rectangle, entry As SidebarEntry)
        Dim rect = Rectangle.Inflate(bounds, -LogicalToDeviceUnits(14), -LogicalToDeviceUnits(6))
        TextRenderer.DrawText(g, entry.Title, Font, rect, SystemColors.GrayText,
                              TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.WordBreak Or TextFormatFlags.NoPrefix)
    End Sub

    ''' <summary>A round avatar: the downloaded picture, or a grey circle with the initial while it is missing.</summary>
    Private Sub DrawAvatar(g As Graphics, rect As Rectangle, entry As SidebarEntry)
        Dim picture As Bitmap = AvatarCache.TryGet(entry.IconUrl)
        Dim oldMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias

        Using path As New GraphicsPath()
            path.AddEllipse(rect)
            If picture IsNot Nothing Then
                Dim oldInterp = g.InterpolationMode
                g.InterpolationMode = InterpolationMode.HighQualityBicubic
                Dim state = g.Save()
                g.SetClip(path, CombineMode.Replace)
                g.DrawImage(picture, rect)
                g.Restore(state)
                g.InterpolationMode = oldInterp
            Else
                g.FillPath(SystemBrushes.ControlDark, path)
                Dim initial As String = If(String.IsNullOrEmpty(entry.Title), "?", entry.Title.Substring(0, 1).ToUpperInvariant())
                TextRenderer.DrawText(g, initial, Font, rect, Color.White, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix)
            End If
        End Using

        g.SmoothingMode = oldMode
    End Sub

#End Region

#Region "Mouse and keyboard"

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Dim idx As Integer = IndexFromPoint(e.Location)
        If idx = _hot Then Return
        Dim previous = _hot
        _hot = idx
        If previous >= 0 AndAlso previous < Items.Count Then Invalidate(GetItemRectangle(previous))
        If _hot >= 0 Then Invalidate(GetItemRectangle(_hot))
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        If _hot >= 0 AndAlso _hot < Items.Count Then Invalidate(GetItemRectangle(_hot))
        _hot = -1
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        If e.Button <> MouseButtons.Left Then Return
        Dim idx As Integer = IndexFromPoint(e.Location)
        If idx < 0 Then Return
        Activate(TryCast(Items(idx), SidebarEntry))
    End Sub

    ''' <summary>Headings can't be selected: if a click or arrow key lands on one, go back to the last real row.</summary>
    Protected Overrides Sub OnSelectedIndexChanged(e As EventArgs)
        Dim entry = TryCast(SelectedItem, SidebarEntry)
        If entry IsNot Nothing AndAlso Not entry.IsSelectable Then
            SelectedIndex = _lastSelectable
            Return
        End If
        _lastSelectable = SelectedIndex
        MyBase.OnSelectedIndexChanged(e)
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        Select Case e.KeyCode
            Case Keys.Down : MoveSelection(1) : e.Handled = True
            Case Keys.Up : MoveSelection(-1) : e.Handled = True
            Case Keys.Enter : Activate(TryCast(SelectedItem, SidebarEntry)) : e.Handled = True
        End Select
        If Not e.Handled Then MyBase.OnKeyDown(e)
    End Sub

    Private Sub MoveSelection(direction As Integer)
        Dim i As Integer = SelectedIndex + direction
        Do While i >= 0 AndAlso i < Items.Count
            Dim entry = TryCast(Items(i), SidebarEntry)
            If entry IsNot Nothing AndAlso entry.IsSelectable Then
                SelectedIndex = i
                Return
            End If
            i += direction
        Loop
    End Sub

    Private Sub Activate(entry As SidebarEntry)
        If entry Is Nothing OrElse Not entry.IsSelectable Then Return
        RaiseEvent EntryClicked(entry)
    End Sub

#End Region

End Class
