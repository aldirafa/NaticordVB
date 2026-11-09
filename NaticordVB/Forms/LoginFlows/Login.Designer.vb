<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Login
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Login))
        Me.loginBanner = New System.Windows.Forms.PictureBox()
        Me.loginDuiRenderer = New DirectUI.Net.Hosting.DirectUIRenderer()
        CType(Me.loginBanner, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'loginBanner
        '
        Me.loginBanner.Image = Global.NaticordVB.My.Resources.Resources.naticord_banner
        Me.loginBanner.Location = New System.Drawing.Point(0, 0)
        Me.loginBanner.Name = "loginBanner"
        Me.loginBanner.Size = New System.Drawing.Size(412, 100)
        Me.loginBanner.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.loginBanner.TabIndex = 1
        Me.loginBanner.TabStop = False
        '
        'loginDuiRenderer
        '
        Me.loginDuiRenderer.BackColor = System.Drawing.SystemColors.AppWorkspace
        Me.loginDuiRenderer.Location = New System.Drawing.Point(0, 100)
        Me.loginDuiRenderer.Name = "loginDuiRenderer"
        Me.loginDuiRenderer.ResId = "loginMain"
        Me.loginDuiRenderer.Size = New System.Drawing.Size(412, 178)
        Me.loginDuiRenderer.TabIndex = 0
        Me.loginDuiRenderer.XmlFile = "User Interface\Login.xml"
        '
        'Login
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(412, 278)
        Me.Controls.Add(Me.loginBanner)
        Me.Controls.Add(Me.loginDuiRenderer)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "Login"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Welcome to Naticord!"
        CType(Me.loginBanner, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents loginDuiRenderer As DirectUI.Net.Hosting.DirectUIRenderer
    Friend WithEvents loginBanner As System.Windows.Forms.PictureBox

End Class
