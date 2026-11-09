Imports System.Diagnostics
Imports NaticordVB.Networking
Imports NaticordVB.Networking.Models

''' <summary>
''' The conversation: messages drawn by hand, with avatars, names, times, reply lines, attachments and a scrollbar.
'''
''' Text is not selectable while you read. Click a message to pick it (it gets highlighted), then Ctrl+C or right-click "Copy text"
''' copies what the author wrote. Selecting part of a message comes later, together with markdown layout.
''' </summary>
Friend NotInheritable Class MessageView
    Inherits Control

    ''' <summary>The "Load earlier messages" row was clicked.</summary>
    Friend Event LoadEarlierRequested()
    ''' <summary>The status text shown instead of messages ("Loading...", an error) was clicked; used for "try again".</summary>
    Friend Event NoticeClicked()

#Region "Items"

    Private Enum ItemKind
        Message
        System
        DaySeparator
        LoadEarlier
    End Enum

    Private NotInheritable Class Extra
        Friend Lines As List(Of String) = New List(Of String)()
        Friend Text As String = ""
        ''' <summary>Where a click goes (http/https only), or Nothing when the line is plain text.</summary>
        Friend Url As String
        Friend Bounds As Rectangle
    End Class

    Private NotInheritable Class ViewItem
        Friend Kind As ItemKind
        Friend Message As DiscordMessage
        Friend Text As String = ""
        Friend IsHead As Boolean
        Friend ReplyText As String
        Friend TimeText As String = ""
        Friend Extras As New List(Of Extra)()
        Friend Lines As List(Of String) = New List(Of String)()

        ' Layout, in device pixels. The Y values are relative to the item's own top.
        Friend Top As Integer
        Friend Height As Integer
        Friend ReplyY As Integer
        Friend HeaderY As Integer
        Friend ContentY As Integer
        Friend ContentHeight As Integer
    End Class

#End Region

    Private ReadOnly _vscroll As New VScrollBar() With {.Dock = DockStyle.Right}
    Private ReadOnly _items As New List(Of ViewItem)()
    Private _messages As New List(Of DiscordMessage)()
    Private _hasEarlier As Boolean

    Private _scrollY As Integer
    Private _contentHeight As Integer
    Private _updatingScrollBar As Boolean

    Private _selectedId As String
    Private _notice As String = ""
    Private _noticeClickable As Boolean

    Private _boldFont As Font
    Private _smallFont As Font
    Private _linkFont As Font
    Private _bodyWrapper As WordWrapper
    Private _linkWrapper As WordWrapper

    ''' <summary>Looks up a channel name by id for "#channel" mentions. May return Nothing.</summary>
    <System.ComponentModel.Browsable(False), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Friend Property ChannelNameLookup As Func(Of String, String)

    Private Const LineFlags As TextFormatFlags = TextFormatFlags.Left Or TextFormatFlags.Top Or TextFormatFlags.SingleLine Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPadding Or TextFormatFlags.NoPrefix

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or ControlStyles.Selectable, True)
        TabStop = True
        BackColor = SystemColors.Window
        Controls.Add(_vscroll)
        AddHandler _vscroll.ValueChanged, AddressOf OnScrollBarChanged
        RebuildFonts()
    End Sub

#Region "Public API"

    ''' <summary>The message the user picked, or Nothing.</summary>
    Friend ReadOnly Property SelectedMessage As DiscordMessage
        Get
            If _selectedId Is Nothing Then Return Nothing
            Return _messages.FirstOrDefault(Function(m) m.Id = _selectedId)
        End Get
    End Property

    ''' <summary>Empties the view and shows a status line instead (for example "Loading messages...").</summary>
    Friend Sub ShowNotice(text As String, Optional clickable As Boolean = False)
        _messages = New List(Of DiscordMessage)()
        _hasEarlier = False
        _selectedId = Nothing
        _notice = text
        _noticeClickable = clickable
        Rebuild(keepBottom:=False)
    End Sub

    ''' <summary>Replaces everything with these messages (oldest first) and scrolls to the newest one.</summary>
    Friend Sub SetMessages(messages As IEnumerable(Of DiscordMessage), hasEarlier As Boolean)
        _messages = messages.ToList()
        _hasEarlier = hasEarlier
        _selectedId = Nothing
        _notice = If(_messages.Count = 0, "No messages yet. Say hi!", "")
        _noticeClickable = False
        Rebuild(keepBottom:=True)
    End Sub

    ''' <summary>Adds newer messages. If you were reading the latest one, the view follows; otherwise your place is kept.</summary>
    Friend Sub AppendMessages(newer As IEnumerable(Of DiscordMessage))
        Dim known As New HashSet(Of String)(_messages.Select(Function(m) m.Id))
        Dim fresh = newer.Where(Function(m) known.Add(m.Id)).ToList()
        If fresh.Count = 0 Then Return

        Dim wasAtBottom As Boolean = IsAtBottom
        _messages.AddRange(fresh)
        _notice = ""
        Rebuild(keepBottom:=wasAtBottom)
    End Sub

    ''' <summary>Adds older messages above, keeping what the user is looking at in the same place on screen.</summary>
    Friend Sub PrependMessages(older As IEnumerable(Of DiscordMessage), hasEarlier As Boolean)
        Dim known As New HashSet(Of String)(_messages.Select(Function(m) m.Id))
        Dim fresh = older.Where(Function(m) known.Add(m.Id)).ToList()
        _hasEarlier = hasEarlier

        Dim anchor As ViewItem = FirstVisibleMessageItem()
        Dim anchorId As String = anchor?.Message?.Id
        Dim offset As Integer = If(anchor Is Nothing, 0, anchor.Top - _scrollY)

        _messages.InsertRange(0, fresh)
        Rebuild(keepBottom:=False)

        If anchorId IsNot Nothing Then
            Dim moved = _items.FirstOrDefault(Function(i) i.Message IsNot Nothing AndAlso i.Message.Id = anchorId)
            If moved IsNot Nothing Then SetScroll(moved.Top - offset)
        End If
    End Sub

    ''' <summary>Copies the picked message's text to the clipboard. Returns False when nothing is picked or there is nothing to copy.</summary>
    Friend Function CopySelected() As Boolean
        Dim message = SelectedMessage
        If message Is Nothing Then Return False

        Dim text As String = message.Content
        If String.IsNullOrWhiteSpace(text) Then text = String.Join(vbCrLf, message.Attachments.Select(Function(a) a.Url))
        If String.IsNullOrWhiteSpace(text) Then Return False

        Try
            Clipboard.SetText(text)
            Return True
        Catch ex As Runtime.InteropServices.ExternalException
            Return False ' another program is holding the clipboard
        End Try
    End Function

#End Region

#Region "Building and measuring"

    Private ReadOnly Property IsAtBottom As Boolean
        Get
            Return _scrollY >= MaxScroll - LogicalToDeviceUnits(4)
        End Get
    End Property

    Private ReadOnly Property MaxScroll As Integer
        Get
            Return Math.Max(0, _contentHeight - ClientSize.Height)
        End Get
    End Property

    Private Sub RebuildFonts()
        _boldFont?.Dispose()
        _smallFont?.Dispose()
        _linkFont?.Dispose()
        _boldFont = New Font(Font, FontStyle.Bold)
        _smallFont = New Font(Font.FontFamily, Math.Max(7.0F, Font.Size - 1.0F))
        _linkFont = New Font(Font, FontStyle.Underline)
        _bodyWrapper = New WordWrapper(Font)
        _linkWrapper = New WordWrapper(_linkFont)
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        RebuildFonts()
        Rebuild(keepBottom:=IsAtBottom)
    End Sub

    Protected Overrides Sub OnSizeChanged(e As EventArgs)
        MyBase.OnSizeChanged(e)
        If Not IsHandleCreated Then Return
        Dim anchor As ViewItem = FirstVisibleMessageItem()
        Dim anchorId As String = anchor?.Message?.Id
        Dim offset As Integer = If(anchor Is Nothing, 0, anchor.Top - _scrollY)
        Dim wasAtBottom As Boolean = IsAtBottom

        Measure()
        If wasAtBottom Then
            SetScroll(MaxScroll)
        ElseIf anchorId IsNot Nothing Then
            Dim moved = _items.FirstOrDefault(Function(i) i.Message IsNot Nothing AndAlso i.Message.Id = anchorId)
            If moved IsNot Nothing Then SetScroll(moved.Top - offset)
        End If
        UpdateScrollBar()
    End Sub

    Private Sub Rebuild(keepBottom As Boolean)
        BuildItems()
        Measure()
        If keepBottom Then SetScroll(MaxScroll) Else SetScroll(_scrollY)
        UpdateScrollBar()
        PrefetchAvatars()
        Invalidate()
    End Sub

    ''' <summary>Turns the message list into rows: day separators, grouped messages, system lines.</summary>
    Private Sub BuildItems()
        _items.Clear()
        If _hasEarlier Then _items.Add(New ViewItem With {.Kind = ItemKind.LoadEarlier, .Text = "Load earlier messages"})

        Dim previousRegular As DiscordMessage = Nothing
        Dim previousDay As Date? = Nothing
        Dim lastItemWasRegular As Boolean = False

        For Each message In _messages
            Dim localTime As DateTime? = message.Timestamp?.LocalDateTime
            Dim newDay As Boolean = False
            If localTime.HasValue AndAlso (Not previousDay.HasValue OrElse previousDay.Value <> localTime.Value.Date) Then
                _items.Add(New ViewItem With {.Kind = ItemKind.DaySeparator, .Text = localTime.Value.ToString("D")})
                previousDay = localTime.Value.Date
                newDay = True
            End If

            If Not message.IsRegular Then
                _items.Add(New ViewItem With {.Kind = ItemKind.System, .Message = message, .Text = message.Author.DisplayName & " " & MessageFormatter.SystemText(message)})
                lastItemWasRegular = False
                Continue For
            End If

            Dim isReply As Boolean = message.Type = MessageType.Reply
            Dim isHead As Boolean = newDay OrElse Not lastItemWasRegular OrElse previousRegular Is Nothing OrElse isReply OrElse
                                    previousRegular.Author.Id <> message.Author.Id OrElse
                                    Not SameGroupInTime(previousRegular, message)

            Dim item As New ViewItem With {
                .Kind = ItemKind.Message,
                .Message = message,
                .IsHead = isHead,
                .Text = MessageFormatter.ToDisplayText(message, ChannelNameLookup) & If(message.EditedTimestamp.HasValue, " (edited)", ""),
                .TimeText = If(localTime.HasValue, FormatTime(localTime.Value), "")
            }
            If isReply Then item.ReplyText = DescribeReply(message)
            BuildExtras(item)

            _items.Add(item)
            previousRegular = message
            lastItemWasRegular = True
        Next
    End Sub

    Private Shared Function SameGroupInTime(a As DiscordMessage, b As DiscordMessage) As Boolean
        If Not a.Timestamp.HasValue OrElse Not b.Timestamp.HasValue Then Return False
        Return (b.Timestamp.Value - a.Timestamp.Value).Duration() <= TimeSpan.FromMinutes(7)
    End Function

    Private Shared Function FormatTime(local As DateTime) As String
        Dim days As Integer = CInt((DateTime.Now.Date - local.Date).TotalDays)
        If days = 0 Then Return "Today at " & local.ToString("t")
        If days = 1 Then Return "Yesterday at " & local.ToString("t")
        Return local.ToString("d") & " " & local.ToString("t")
    End Function

    ''' <summary>"name: first words of the message" for a reply, or a note when the original is gone.</summary>
    Private Function DescribeReply(message As DiscordMessage) As String
        Dim original = message.ReferencedMessage
        If original Is Nothing Then Return ChrW(&H21AA) & " Original message was deleted or can't be shown"

        Dim snippet As String = MessageFormatter.ToDisplayText(original, ChannelNameLookup).Replace(vbCr, " ").Replace(vbLf, " ").Trim()
        If snippet.Length = 0 Then snippet = If(original.Attachments.Count > 0, "Click to see attachment", "(no text)")
        Return ChrW(&H21AA) & " " & original.Author.DisplayName & ": " & snippet
    End Function

    ''' <summary>Attachments, link previews and stickers become extra lines under the text.</summary>
    Private Shared Sub BuildExtras(item As ViewItem)
        Dim message = item.Message

        For Each attachment In message.Attachments
            item.Extras.Add(New Extra With {.Text = "Attachment: " & MessageFormatter.DescribeAttachment(attachment), .Url = SafeUrl(attachment.Url)})
        Next

        For Each embed In message.Embeds
            Dim url As String = SafeUrl(embed.Url)
            ' A pasted link already appears in the text; don't repeat it as a preview line.
            If url IsNot Nothing AndAlso message.Content.Contains(embed.Url) AndAlso String.IsNullOrEmpty(embed.Title) Then Continue For
            Dim label As String = If(Not String.IsNullOrEmpty(embed.Title), embed.Title, embed.Url)
            If String.IsNullOrEmpty(label) Then Continue For
            item.Extras.Add(New Extra With {.Text = "Link: " & label, .Url = url})
        Next

        For Each sticker In message.StickerNames
            item.Extras.Add(New Extra With {.Text = "Sticker: " & sticker})
        Next
    End Sub

    ''' <summary>Only web addresses are ever opened from a click.</summary>
    Private Shared Function SafeUrl(url As String) As String
        Dim uri As Uri = Nothing
        If Not String.IsNullOrEmpty(url) AndAlso Uri.TryCreate(url, UriKind.Absolute, uri) AndAlso (uri.Scheme = Uri.UriSchemeHttp OrElse uri.Scheme = Uri.UriSchemeHttps) Then Return uri.AbsoluteUri
        Return Nothing
    End Function

    Private ReadOnly Property ViewportWidth As Integer
        Get
            Return Math.Max(0, ClientSize.Width - _vscroll.Width)
        End Get
    End Property

    Private Function TextLeft() As Integer
        Return LogicalToDeviceUnits(16 + 40 + 12)
    End Function

    Private Function ContentWidth() As Integer
        Return Math.Max(LogicalToDeviceUnits(60), ViewportWidth - TextLeft() - LogicalToDeviceUnits(20))
    End Function

    ''' <summary>Works out the lines, height and inner positions of every row for the current width.</summary>
    Private Sub Measure()
        Dim width As Integer = ContentWidth()
        Dim y As Integer = 0

        For Each item In _items
            item.Top = y

            Select Case item.Kind
                Case ItemKind.DaySeparator, ItemKind.LoadEarlier
                    item.Height = LogicalToDeviceUnits(38)

                Case ItemKind.System
                    item.Lines = _bodyWrapper.Wrap(item.Text, width)
                    item.Height = Math.Max(LogicalToDeviceUnits(26), item.Lines.Count * Font.Height + LogicalToDeviceUnits(10))

                Case Else
                    Dim cursor As Integer = If(item.IsHead, LogicalToDeviceUnits(14), LogicalToDeviceUnits(1))
                    If item.ReplyText IsNot Nothing Then
                        item.ReplyY = cursor
                        cursor += _smallFont.Height + LogicalToDeviceUnits(3)
                    End If
                    If item.IsHead Then
                        item.HeaderY = cursor
                        cursor += _boldFont.Height + LogicalToDeviceUnits(1)
                    End If
                    item.ContentY = cursor
                    item.Lines = _bodyWrapper.Wrap(item.Text, width)
                    item.ContentHeight = item.Lines.Count * Font.Height
                    cursor += item.ContentHeight

                    For Each extra In item.Extras
                        extra.Lines = _linkWrapper.Wrap(extra.Text, width)
                        Dim h As Integer = extra.Lines.Count * _linkFont.Height
                        extra.Bounds = New Rectangle(TextLeft(), cursor + LogicalToDeviceUnits(2), width, h)
                        cursor += h + LogicalToDeviceUnits(2)
                    Next

                    item.Height = cursor + LogicalToDeviceUnits(2)
                    If item.IsHead Then item.Height = Math.Max(item.Height, item.HeaderY + LogicalToDeviceUnits(40) + LogicalToDeviceUnits(4))
            End Select

            y += item.Height
        Next

        _contentHeight = y
    End Sub

    ''' <summary>Starts downloading the authors' avatars; the view repaints as each one arrives.</summary>
    Private Sub PrefetchAvatars()
        Dim urls = _messages.Select(Function(m) DiscordCdn.UserAvatarUrl(m.Author, 64)).
                             Where(Function(u) AvatarCache.TryGet(u) Is Nothing).Distinct().ToList()
        If urls.Count = 0 Then Return
        Dim ignored As Task = FetchAvatarsAsync(urls)
    End Sub

    Private Async Function FetchAvatarsAsync(urls As List(Of String)) As Task
        Dim downloads = urls.Select(Function(u) AvatarCache.GetAsync(u)).ToList()
        For Each download In downloads
            Dim picture As Bitmap = Await download
            If picture IsNot Nothing AndAlso Not IsDisposed Then Invalidate()
        Next
    End Function

#End Region

#Region "Scrolling"

    Private Sub SetScroll(value As Integer)
        _scrollY = Math.Max(0, Math.Min(value, MaxScroll))
        UpdateScrollBar()
        Invalidate()
    End Sub

    Private Sub UpdateScrollBar()
        _updatingScrollBar = True
        Try
            Dim viewHeight As Integer = Math.Max(1, ClientSize.Height)
            _vscroll.Minimum = 0
            _vscroll.LargeChange = viewHeight
            _vscroll.SmallChange = Math.Max(1, Font.Height * 2)
            _vscroll.Maximum = Math.Max(viewHeight, _contentHeight)
            _vscroll.Enabled = MaxScroll > 0
            _vscroll.Value = Math.Max(0, Math.Min(_scrollY, Math.Max(0, _vscroll.Maximum - _vscroll.LargeChange + 1)))
        Finally
            _updatingScrollBar = False
        End Try
    End Sub

    Private Sub OnScrollBarChanged(sender As Object, e As EventArgs)
        If _updatingScrollBar Then Return
        _scrollY = _vscroll.Value
        Invalidate()
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        Dim lines As Integer = Math.Max(1, SystemInformation.MouseWheelScrollLines)
        SetScroll(_scrollY - CInt(e.Delta / 120.0 * lines * Font.Height))
    End Sub

    Protected Overrides Function IsInputKey(keyData As Keys) As Boolean
        Select Case keyData And Keys.KeyCode
            Case Keys.Up, Keys.Down, Keys.PageUp, Keys.PageDown, Keys.Home, Keys.End : Return True
        End Select
        Return MyBase.IsInputKey(keyData)
    End Function

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Dim page As Integer = Math.Max(Font.Height, ClientSize.Height - Font.Height * 2)

        Select Case e.KeyCode
            Case Keys.Up : SetScroll(_scrollY - Font.Height * 3)
            Case Keys.Down : SetScroll(_scrollY + Font.Height * 3)
            Case Keys.PageUp : SetScroll(_scrollY - page)
            Case Keys.PageDown : SetScroll(_scrollY + page)
            Case Keys.Home : SetScroll(0)
            Case Keys.End : SetScroll(MaxScroll)
            Case Keys.C
                If e.Control AndAlso CopySelected() Then e.Handled = True
            Case Keys.Escape
                If _selectedId IsNot Nothing Then
                    _selectedId = Nothing
                    Invalidate()
                End If
        End Select
    End Sub

#End Region

#Region "Painting"

    Private Function FirstVisibleMessageItem() As ViewItem
        Dim index As Integer = ItemIndexAt(_scrollY)
        For i = Math.Max(0, index) To _items.Count - 1
            If _items(i).Message IsNot Nothing Then Return _items(i)
        Next
        Return Nothing
    End Function

    ''' <summary>The row containing this y (in scrolled content coordinates), or -1.</summary>
    Private Function ItemIndexAt(contentY As Integer) As Integer
        If _items.Count = 0 OrElse contentY < 0 Then Return If(_items.Count = 0, -1, 0)

        Dim low As Integer = 0
        Dim high As Integer = _items.Count - 1
        Do While low < high
            Dim mid As Integer = (low + high + 1) \ 2
            If _items(mid).Top <= contentY Then low = mid Else high = mid - 1
        Loop
        Return If(contentY < _contentHeight, low, -1)
    End Function

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g = e.Graphics
        g.Clear(BackColor)

        If _items.Count = 0 Then
            DrawNotice(g)
            Return
        End If

        Dim viewHeight As Integer = ClientSize.Height
        Dim first As Integer = Math.Max(0, ItemIndexAt(_scrollY))

        For i = first To _items.Count - 1
            Dim item = _items(i)
            Dim top As Integer = item.Top - _scrollY
            If top >= viewHeight Then Exit For
            If top + item.Height < 0 Then Continue For
            DrawItem(g, item, top)
        Next
    End Sub

    Private Sub DrawNotice(g As Graphics)
        If String.IsNullOrEmpty(_notice) Then Return
        Dim rect As New Rectangle(0, 0, ViewportWidth, ClientSize.Height)
        Dim textColor As Color = If(_noticeClickable, SystemColors.HotTrack, SystemColors.GrayText)
        TextRenderer.DrawText(g, _notice, If(_noticeClickable, _linkFont, Font), rect, textColor,
                              TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.WordBreak Or TextFormatFlags.NoPrefix)
    End Sub

    Private Shared Function SelectionColor() As Color
        ' A very light tint of the system highlight colour.
        Dim h = SystemColors.Highlight
        Return Color.FromArgb(CInt(h.R * 0.1 + 255 * 0.9), CInt(h.G * 0.1 + 255 * 0.9), CInt(h.B * 0.1 + 255 * 0.9))
    End Function

    Private Sub DrawItem(g As Graphics, item As ViewItem, top As Integer)
        Dim width As Integer = ViewportWidth

        Select Case item.Kind
            Case ItemKind.DaySeparator : DrawDaySeparator(g, item, top, width)
            Case ItemKind.LoadEarlier
                Dim rect As New Rectangle(0, top, width, item.Height)
                TextRenderer.DrawText(g, item.Text, _linkFont, rect, SystemColors.HotTrack,
                                      TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix)
            Case ItemKind.System
                DrawLines(g, item.Lines, Font, TextLeft(), top + LogicalToDeviceUnits(5), ContentWidth(), SystemColors.GrayText)
            Case Else : DrawMessage(g, item, top, width)
        End Select
    End Sub

    Private Sub DrawDaySeparator(g As Graphics, item As ViewItem, top As Integer, width As Integer)
        Dim middle As Integer = top + item.Height \ 2
        Dim textSize As Size = TextRenderer.MeasureText(item.Text, _smallFont, New Size(width, item.Height), LineFlags)
        Dim textRect As New Rectangle((width - textSize.Width) \ 2, middle - textSize.Height \ 2, textSize.Width, textSize.Height)
        Dim margin As Integer = LogicalToDeviceUnits(16)
        Dim gap As Integer = LogicalToDeviceUnits(8)

        Using pen As New Pen(SystemColors.ControlLight)
            g.DrawLine(pen, margin, middle, Math.Max(margin, textRect.Left - gap), middle)
            g.DrawLine(pen, Math.Min(width - margin, textRect.Right + gap), middle, width - margin, middle)
        End Using
        TextRenderer.DrawText(g, item.Text, _smallFont, textRect, SystemColors.GrayText, LineFlags)
    End Sub

    Private Sub DrawMessage(g As Graphics, item As ViewItem, top As Integer, width As Integer)
        Dim message = item.Message
        If message.Id = _selectedId Then
            Using brush As New SolidBrush(SelectionColor())
                g.FillRectangle(brush, 0, top, width, item.Height)
            End Using
        End If

        Dim left As Integer = TextLeft()
        Dim contentW As Integer = ContentWidth()

        If item.ReplyText IsNot Nothing Then
            Dim replyRect As New Rectangle(left, top + item.ReplyY, contentW, _smallFont.Height)
            TextRenderer.DrawText(g, item.ReplyText, _smallFont, replyRect, SystemColors.GrayText, LineFlags)
        End If

        If item.IsHead Then
            Dim avatarSize As Integer = LogicalToDeviceUnits(40)
            Dim avatarRect As New Rectangle(LogicalToDeviceUnits(16), top + item.HeaderY, avatarSize, avatarSize)
            AvatarPainter.Draw(g, avatarRect, DiscordCdn.UserAvatarUrl(message.Author, 64), message.Author.DisplayName, Font)

            Dim nameSize As Size = TextRenderer.MeasureText(message.Author.DisplayName, _boldFont, New Size(contentW, _boldFont.Height), LineFlags)
            Dim nameRect As New Rectangle(left, top + item.HeaderY, Math.Min(nameSize.Width, contentW), _boldFont.Height)
            TextRenderer.DrawText(g, message.Author.DisplayName, _boldFont, nameRect, SystemColors.WindowText, LineFlags)

            Dim timeLeft As Integer = nameRect.Right + LogicalToDeviceUnits(8)
            If timeLeft < left + contentW Then
                Dim timeRect As New Rectangle(timeLeft, top + item.HeaderY + (_boldFont.Height - _smallFont.Height) \ 2, left + contentW - timeLeft, _smallFont.Height)
                TextRenderer.DrawText(g, item.TimeText, _smallFont, timeRect, SystemColors.GrayText, LineFlags)
            End If
        End If

        DrawLines(g, item.Lines, Font, left, top + item.ContentY, contentW, SystemColors.WindowText)

        For Each extra In item.Extras
            Dim bounds As Rectangle = extra.Bounds
            bounds.Offset(0, top)
            Dim linkColor As Color = If(extra.Url IsNot Nothing, SystemColors.HotTrack, SystemColors.GrayText)
            DrawLines(g, extra.Lines, If(extra.Url IsNot Nothing, _linkFont, Font), bounds.Left, bounds.Top, bounds.Width, linkColor)
        Next
    End Sub

    ''' <summary>Draws pre-wrapped lines one under another, starting at the given top-left corner.</summary>
    Private Sub DrawLines(g As Graphics, lines As List(Of String), lineFont As Font, left As Integer, top As Integer, width As Integer, color As Color)
        Dim y As Integer = top
        For Each line In lines
            If line.Length > 0 Then
                ' A few pixels of slack so rounding never clips the last letter.
                TextRenderer.DrawText(g, line, lineFont, New Rectangle(left, y, width + 8, lineFont.Height), color,
                                      TextFormatFlags.Left Or TextFormatFlags.Top Or TextFormatFlags.SingleLine Or TextFormatFlags.NoPadding Or TextFormatFlags.NoPrefix)
            End If
            y += lineFont.Height
        Next
    End Sub

#End Region

#Region "Mouse"

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Focus()
        If e.Button <> MouseButtons.Left AndAlso e.Button <> MouseButtons.Right Then Return

        If _items.Count = 0 Then
            If _noticeClickable AndAlso e.Button = MouseButtons.Left Then RaiseEvent NoticeClicked()
            Return
        End If

        Dim contentY As Integer = e.Y + _scrollY
        Dim index As Integer = ItemIndexAt(contentY)
        If index < 0 Then Return
        Dim item = _items(index)

        Select Case item.Kind
            Case ItemKind.LoadEarlier
                If e.Button = MouseButtons.Left Then RaiseEvent LoadEarlierRequested()

            Case ItemKind.Message
                If e.Button = MouseButtons.Left Then
                    For Each extra In item.Extras
                        Dim bounds As Rectangle = extra.Bounds
                        bounds.Offset(0, item.Top)
                        If extra.Url IsNot Nothing AndAlso bounds.Contains(e.X, contentY) Then
                            OpenLink(extra.Url)
                            Return
                        End If
                    Next
                End If

                _selectedId = item.Message.Id
                Invalidate()
                If e.Button = MouseButtons.Right Then ShowContextMenu(e.Location)

            Case Else
                If _selectedId IsNot Nothing Then
                    _selectedId = Nothing
                    Invalidate()
                End If
        End Select
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Cursor = If(IsOverLink(e.Location), Cursors.Hand, Cursors.Default)
    End Sub

    Private Function IsOverLink(point As Point) As Boolean
        If _items.Count = 0 Then Return _noticeClickable
        Dim contentY As Integer = point.Y + _scrollY
        Dim index As Integer = ItemIndexAt(contentY)
        If index < 0 Then Return False
        Dim item = _items(index)

        If item.Kind = ItemKind.LoadEarlier Then Return True
        For Each extra In item.Extras
            Dim bounds As Rectangle = extra.Bounds
            bounds.Offset(0, item.Top)
            If extra.Url IsNot Nothing AndAlso bounds.Contains(point.X, contentY) Then Return True
        Next
        Return False
    End Function

    Private Shared Sub OpenLink(url As String)
        Try
            Process.Start(New ProcessStartInfo(url) With {.UseShellExecute = True})
        Catch ex As Exception
            Debug.WriteLine("Could not open " & url & ": " & ex.Message)
        End Try
    End Sub

    Private Sub ShowContextMenu(location As Point)
        Using menu As New NativeContextGenerator()
            Dim copy As New ToolStripMenuItem("Copy text")
            copy.Enabled = SelectedMessage IsNot Nothing AndAlso (Not String.IsNullOrWhiteSpace(SelectedMessage.Content) OrElse SelectedMessage.Attachments.Count > 0)
            AddHandler copy.Click, Sub(s, a) CopySelected()
            menu.Items.Add(copy)
            menu.Show(Me, location)
        End Using
    End Sub

#End Region

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            _boldFont?.Dispose()
            _smallFont?.Dispose()
            _linkFont?.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

End Class
