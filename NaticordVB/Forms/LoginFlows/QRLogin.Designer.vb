<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class QRLogin
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(QRLogin))
        Me.qrPictureBox = New System.Windows.Forms.PictureBox()
        CType(Me.qrPictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'qrPictureBox
        '
        Me.qrPictureBox.BackColor = System.Drawing.Color.White
        Me.qrPictureBox.Image = Global.NaticordVB.My.Resources.Resources.qr_loading
        Me.qrPictureBox.Location = New System.Drawing.Point(63, 84)
        Me.qrPictureBox.Name = "qrPictureBox"
        Me.qrPictureBox.Size = New System.Drawing.Size(180, 180)
        Me.qrPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.qrPictureBox.TabIndex = 0
        Me.qrPictureBox.TabStop = False
        '
        'QRLogin
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(302, 316)
        Me.Controls.Add(Me.qrPictureBox)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "QRLogin"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Sign into NaticordVB..."
        CType(Me.qrPictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents qrPictureBox As System.Windows.Forms.PictureBox

End Class
