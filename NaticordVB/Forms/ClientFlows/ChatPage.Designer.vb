<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ChatPage
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
        Me.titleLabel = New NaticordVB.FallbackLabel()
        Me.messageView = New NaticordVB.MessageView()
        Me.composerPanel = New System.Windows.Forms.Panel()
        Me.replyBar = New System.Windows.Forms.Panel()
        Me.replyLabel = New NaticordVB.FallbackLabel()
        Me.cancelReplyButton = New System.Windows.Forms.Button()
        Me.errorLabel = New NaticordVB.FallbackLabel()
        Me.inputBox = New System.Windows.Forms.TextBox()
        Me.sendButton = New System.Windows.Forms.Button()
        Me.composerPanel.SuspendLayout()
        Me.replyBar.SuspendLayout()
        Me.SuspendLayout()
        '
        'titleLabel
        '
        Me.titleLabel.Dock = System.Windows.Forms.DockStyle.Top
        Me.titleLabel.Location = New System.Drawing.Point(0, 0)
        Me.titleLabel.Name = "titleLabel"
        Me.titleLabel.Padding = New System.Windows.Forms.Padding(14, 0, 14, 0)
        Me.titleLabel.Size = New System.Drawing.Size(800, 44)
        Me.titleLabel.TabIndex = 0
        Me.titleLabel.Text = "Title"
        '
        'messageView
        '
        Me.messageView.Dock = System.Windows.Forms.DockStyle.Fill
        Me.messageView.Location = New System.Drawing.Point(0, 44)
        Me.messageView.Name = "messageView"
        Me.messageView.Size = New System.Drawing.Size(800, 366)
        Me.messageView.TabIndex = 1
        '
        'composerPanel
        '
        Me.composerPanel.Controls.Add(Me.replyBar)
        Me.composerPanel.Controls.Add(Me.errorLabel)
        Me.composerPanel.Controls.Add(Me.inputBox)
        Me.composerPanel.Controls.Add(Me.sendButton)
        Me.composerPanel.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.composerPanel.Location = New System.Drawing.Point(0, 410)
        Me.composerPanel.Name = "composerPanel"
        Me.composerPanel.Size = New System.Drawing.Size(800, 40)
        Me.composerPanel.TabIndex = 2
        '
        'replyBar
        '
        Me.replyBar.Controls.Add(Me.replyLabel)
        Me.replyBar.Controls.Add(Me.cancelReplyButton)
        Me.replyBar.Location = New System.Drawing.Point(10, 6)
        Me.replyBar.Name = "replyBar"
        Me.replyBar.Size = New System.Drawing.Size(780, 26)
        Me.replyBar.TabIndex = 0
        Me.replyBar.Visible = False
        '
        'replyLabel
        '
        Me.replyLabel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.replyLabel.ForeColor = System.Drawing.SystemColors.GrayText
        Me.replyLabel.Location = New System.Drawing.Point(0, 0)
        Me.replyLabel.Name = "replyLabel"
        Me.replyLabel.Size = New System.Drawing.Size(700, 26)
        Me.replyLabel.TabIndex = 0
        '
        'cancelReplyButton
        '
        Me.cancelReplyButton.Dock = System.Windows.Forms.DockStyle.Right
        Me.cancelReplyButton.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cancelReplyButton.Location = New System.Drawing.Point(700, 0)
        Me.cancelReplyButton.Name = "cancelReplyButton"
        Me.cancelReplyButton.Size = New System.Drawing.Size(80, 26)
        Me.cancelReplyButton.TabIndex = 1
        Me.cancelReplyButton.Text = "Cancel"
        Me.cancelReplyButton.UseVisualStyleBackColor = True
        '
        'errorLabel
        '
        Me.errorLabel.ForeColor = System.Drawing.Color.Firebrick
        Me.errorLabel.Location = New System.Drawing.Point(10, 6)
        Me.errorLabel.Name = "errorLabel"
        Me.errorLabel.Size = New System.Drawing.Size(780, 20)
        Me.errorLabel.TabIndex = 1
        Me.errorLabel.Visible = False
        '
        'inputBox
        '
        Me.inputBox.AcceptsReturn = True
        Me.inputBox.Location = New System.Drawing.Point(10, 6)
        Me.inputBox.Multiline = True
        Me.inputBox.Name = "inputBox"
        Me.inputBox.Size = New System.Drawing.Size(700, 24)
        Me.inputBox.TabIndex = 2
        '
        'sendButton
        '
        Me.sendButton.Enabled = False
        Me.sendButton.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.sendButton.Location = New System.Drawing.Point(718, 6)
        Me.sendButton.Name = "sendButton"
        Me.sendButton.Size = New System.Drawing.Size(72, 28)
        Me.sendButton.TabIndex = 3
        Me.sendButton.Text = "Send"
        Me.sendButton.UseVisualStyleBackColor = True
        '
        'ChatPage
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.messageView)
        Me.Controls.Add(Me.composerPanel)
        Me.Controls.Add(Me.titleLabel)
        Me.Name = "ChatPage"
        Me.Text = "ChatPage"
        Me.composerPanel.ResumeLayout(False)
        Me.composerPanel.PerformLayout()
        Me.replyBar.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents titleLabel As FallbackLabel
    Friend WithEvents messageView As MessageView
    Friend WithEvents composerPanel As System.Windows.Forms.Panel
    Friend WithEvents replyBar As System.Windows.Forms.Panel
    Friend WithEvents replyLabel As FallbackLabel
    Friend WithEvents cancelReplyButton As System.Windows.Forms.Button
    Friend WithEvents errorLabel As FallbackLabel
    Friend WithEvents inputBox As System.Windows.Forms.TextBox
    Friend WithEvents sendButton As System.Windows.Forms.Button

End Class
