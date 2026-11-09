<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class GlassExtendedButton
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
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
        Me.iconPicture = New System.Windows.Forms.PictureBox()
        Me.captionLabel = New NaticordVB.AeroLabel()
        CType(Me.iconPicture, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'iconPicture
        '
        Me.iconPicture.Image = Global.NaticordVB.My.Resources.Resources.light_bulb_skeuo
        Me.iconPicture.Location = New System.Drawing.Point(0, 2)
        Me.iconPicture.Name = "iconPicture"
        Me.iconPicture.Size = New System.Drawing.Size(16, 16)
        Me.iconPicture.TabIndex = 0
        Me.iconPicture.TabStop = False
        '
        'captionLabel
        '
        Me.captionLabel.FixClippingBug = True
        Me.captionLabel.Location = New System.Drawing.Point(21, 0)
        Me.captionLabel.Name = "captionLabel"
        Me.captionLabel.Size = New System.Drawing.Size(126, 20)
        Me.captionLabel.TabIndex = 1
        Me.captionLabel.Text = "Example button"
        Me.captionLabel.TextAlignVertical = System.Windows.Forms.VisualStyles.VerticalAlignment.Center
        '
        'GlassExtendedButton
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Transparent
        Me.Controls.Add(Me.captionLabel)
        Me.Controls.Add(Me.iconPicture)
        Me.Name = "GlassExtendedButton"
        Me.Size = New System.Drawing.Size(150, 20)
        CType(Me.iconPicture, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents iconPicture As System.Windows.Forms.PictureBox
    Friend WithEvents captionLabel As AeroLabel

End Class
