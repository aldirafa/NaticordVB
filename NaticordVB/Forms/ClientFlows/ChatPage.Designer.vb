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
        Me.titleLabel = New System.Windows.Forms.Label()
        Me.bodyLabel = New System.Windows.Forms.Label()
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
        Me.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'bodyLabel
        '
        Me.bodyLabel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.bodyLabel.ForeColor = System.Drawing.SystemColors.GrayText
        Me.bodyLabel.Location = New System.Drawing.Point(0, 44)
        Me.bodyLabel.Name = "bodyLabel"
        Me.bodyLabel.Size = New System.Drawing.Size(800, 406)
        Me.bodyLabel.TabIndex = 1
        Me.bodyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'ChatPage
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.bodyLabel)
        Me.Controls.Add(Me.titleLabel)
        Me.Name = "ChatPage"
        Me.Text = "ChatPage"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents titleLabel As System.Windows.Forms.Label
    Friend WithEvents bodyLabel As System.Windows.Forms.Label

End Class
