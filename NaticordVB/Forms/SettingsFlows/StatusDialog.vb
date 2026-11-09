Imports System.Globalization
Imports NaticordVB.Networking.Models

''' <summary>Edit the custom status: some text, an optional emoji and when it should clear itself.</summary>
Public Class StatusDialog

    Private Const MaxTextLength As Integer = 128

    ''' <summary>When the dialog was closed with OK: the status to set. Nothing means "remove my custom status".</summary>
    <System.ComponentModel.Browsable(False), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Friend Property Result As CustomStatus

    Private Enum ClearAfter
        Never
        ThirtyMinutes
        OneHour
        FourHours
        Today
    End Enum

    Friend Sub New(current As CustomStatus)
        InitializeComponent()
        Font = SystemFonts.MessageBoxFont

        textBox.MaxLength = MaxTextLength
        clearAfterBox.Items.AddRange({"Don't clear", "In 30 minutes", "In 1 hour", "In 4 hours", "Today"})
        clearAfterBox.SelectedIndex = ClearAfter.Never

        If current IsNot Nothing Then
            textBox.Text = If(current.Text, "")
            emojiBox.Text = If(current.EmojiName, "")
        End If
        removeButton.Enabled = current IsNot Nothing
    End Sub

    ''' <summary>When the status should vanish, for the "clear after" choice (local time zone; "Today" means until midnight).</summary>
    Private Shared Function ExpiryFor(choice As ClearAfter, now As DateTimeOffset) As DateTimeOffset?
        Select Case choice
            Case ClearAfter.ThirtyMinutes : Return now.AddMinutes(30)
            Case ClearAfter.OneHour : Return now.AddHours(1)
            Case ClearAfter.FourHours : Return now.AddHours(4)
            Case ClearAfter.Today
                Dim midnight As DateTime = now.LocalDateTime.Date.AddDays(1)
                Return New DateTimeOffset(midnight, TimeZoneInfo.Local.GetUtcOffset(midnight))
            Case Else : Return Nothing
        End Select
    End Function

    Private Sub saveButton_Click(sender As Object, e As EventArgs) Handles saveButton.Click
        Dim text As String = textBox.Text.Trim()
        Dim emoji As String = emojiBox.Text.Trim()

        If text.Length = 0 AndAlso emoji.Length = 0 Then
            problemLabel.Text = "Type some text or an emoji, or use ""Remove status""."
            Return
        End If
        If emoji.Length > 0 AndAlso StringInfo.ParseCombiningCharacters(emoji).Length > 1 Then
            problemLabel.Text = "The emoji box takes a single emoji."
            emojiBox.Focus()
            Return
        End If

        Result = New CustomStatus With {
            .Text = If(text.Length = 0, Nothing, text),
            .EmojiName = If(emoji.Length = 0, Nothing, emoji),
            .ExpiresAt = ExpiryFor(CType(clearAfterBox.SelectedIndex, ClearAfter), DateTimeOffset.Now)
        }
        DialogResult = DialogResult.OK
    End Sub

    Private Sub removeButton_Click(sender As Object, e As EventArgs) Handles removeButton.Click
        Result = Nothing
        DialogResult = DialogResult.OK
    End Sub

    Private Sub input_TextChanged(sender As Object, e As EventArgs) Handles textBox.TextChanged, emojiBox.TextChanged
        problemLabel.Text = ""
    End Sub

End Class
