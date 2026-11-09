<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class StatusDialog
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.textLabel = New System.Windows.Forms.Label()
        Me.textBox = New System.Windows.Forms.TextBox()
        Me.emojiLabel = New System.Windows.Forms.Label()
        Me.emojiBox = New System.Windows.Forms.TextBox()
        Me.emojiHint = New System.Windows.Forms.Label()
        Me.clearAfterLabel = New System.Windows.Forms.Label()
        Me.clearAfterBox = New System.Windows.Forms.ComboBox()
        Me.problemLabel = New System.Windows.Forms.Label()
        Me.saveButton = New System.Windows.Forms.Button()
        Me.removeButton = New System.Windows.Forms.Button()
        Me.dismissButton = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'textLabel
        '
        Me.textLabel.AutoSize = True
        Me.textLabel.Location = New System.Drawing.Point(14, 14)
        Me.textLabel.Name = "textLabel"
        Me.textLabel.Size = New System.Drawing.Size(122, 15)
        Me.textLabel.TabIndex = 0
        Me.textLabel.Text = "What's happening?"
        '
        'textBox
        '
        Me.textBox.Location = New System.Drawing.Point(14, 34)
        Me.textBox.Name = "textBox"
        Me.textBox.Size = New System.Drawing.Size(372, 23)
        Me.textBox.TabIndex = 1
        '
        'emojiLabel
        '
        Me.emojiLabel.AutoSize = True
        Me.emojiLabel.Location = New System.Drawing.Point(14, 70)
        Me.emojiLabel.Name = "emojiLabel"
        Me.emojiLabel.Size = New System.Drawing.Size(80, 15)
        Me.emojiLabel.TabIndex = 2
        Me.emojiLabel.Text = "Emoji (optional)"
        '
        'emojiBox
        '
        Me.emojiBox.Location = New System.Drawing.Point(14, 90)
        Me.emojiBox.Name = "emojiBox"
        Me.emojiBox.Size = New System.Drawing.Size(60, 23)
        Me.emojiBox.TabIndex = 3
        '
        'emojiHint
        '
        Me.emojiHint.AutoSize = True
        Me.emojiHint.ForeColor = System.Drawing.SystemColors.GrayText
        Me.emojiHint.Location = New System.Drawing.Point(84, 94)
        Me.emojiHint.Name = "emojiHint"
        Me.emojiHint.Size = New System.Drawing.Size(180, 15)
        Me.emojiHint.TabIndex = 4
        Me.emojiHint.Text = "Press Windows key + . to pick one"
        '
        'clearAfterLabel
        '
        Me.clearAfterLabel.AutoSize = True
        Me.clearAfterLabel.Location = New System.Drawing.Point(14, 128)
        Me.clearAfterLabel.Name = "clearAfterLabel"
        Me.clearAfterLabel.Size = New System.Drawing.Size(66, 15)
        Me.clearAfterLabel.TabIndex = 5
        Me.clearAfterLabel.Text = "Clear after"
        '
        'clearAfterBox
        '
        Me.clearAfterBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.clearAfterBox.FormattingEnabled = True
        Me.clearAfterBox.Location = New System.Drawing.Point(14, 148)
        Me.clearAfterBox.Name = "clearAfterBox"
        Me.clearAfterBox.Size = New System.Drawing.Size(180, 23)
        Me.clearAfterBox.TabIndex = 6
        '
        'problemLabel
        '
        Me.problemLabel.ForeColor = System.Drawing.Color.Firebrick
        Me.problemLabel.Location = New System.Drawing.Point(14, 180)
        Me.problemLabel.Name = "problemLabel"
        Me.problemLabel.Size = New System.Drawing.Size(372, 18)
        Me.problemLabel.TabIndex = 7
        '
        'saveButton
        '
        Me.saveButton.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.saveButton.Location = New System.Drawing.Point(222, 208)
        Me.saveButton.Name = "saveButton"
        Me.saveButton.Size = New System.Drawing.Size(80, 27)
        Me.saveButton.TabIndex = 8
        Me.saveButton.Text = "Save"
        Me.saveButton.UseVisualStyleBackColor = True
        '
        'removeButton
        '
        Me.removeButton.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.removeButton.Location = New System.Drawing.Point(14, 208)
        Me.removeButton.Name = "removeButton"
        Me.removeButton.Size = New System.Drawing.Size(110, 27)
        Me.removeButton.TabIndex = 10
        Me.removeButton.Text = "Remove status"
        Me.removeButton.UseVisualStyleBackColor = True
        '
        'dismissButton
        '
        Me.dismissButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.dismissButton.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.dismissButton.Location = New System.Drawing.Point(306, 208)
        Me.dismissButton.Name = "cancelButton"
        Me.dismissButton.Size = New System.Drawing.Size(80, 27)
        Me.dismissButton.TabIndex = 9
        Me.dismissButton.Text = "Cancel"
        Me.dismissButton.UseVisualStyleBackColor = True
        '
        'StatusDialog
        '
        Me.AcceptButton = Me.saveButton
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.dismissButton
        Me.ClientSize = New System.Drawing.Size(400, 248)
        Me.Controls.Add(Me.textLabel)
        Me.Controls.Add(Me.textBox)
        Me.Controls.Add(Me.emojiLabel)
        Me.Controls.Add(Me.emojiBox)
        Me.Controls.Add(Me.emojiHint)
        Me.Controls.Add(Me.clearAfterLabel)
        Me.Controls.Add(Me.clearAfterBox)
        Me.Controls.Add(Me.problemLabel)
        Me.Controls.Add(Me.saveButton)
        Me.Controls.Add(Me.removeButton)
        Me.Controls.Add(Me.dismissButton)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "StatusDialog"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Set custom status"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents textLabel As System.Windows.Forms.Label
    Friend WithEvents textBox As System.Windows.Forms.TextBox
    Friend WithEvents emojiLabel As System.Windows.Forms.Label
    Friend WithEvents emojiBox As System.Windows.Forms.TextBox
    Friend WithEvents emojiHint As System.Windows.Forms.Label
    Friend WithEvents clearAfterLabel As System.Windows.Forms.Label
    Friend WithEvents clearAfterBox As System.Windows.Forms.ComboBox
    Friend WithEvents problemLabel As System.Windows.Forms.Label
    Friend WithEvents saveButton As System.Windows.Forms.Button
    Friend WithEvents removeButton As System.Windows.Forms.Button
    Friend WithEvents dismissButton As System.Windows.Forms.Button

End Class
