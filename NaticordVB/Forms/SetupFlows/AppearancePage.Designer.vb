<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AppearancePage
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
        Me.appearanceHeaderRenderer = New DirectUI.Net.Hosting.DirectUIRenderer()
        Me.cutOffPanel = New System.Windows.Forms.Panel()
        Me.layoutBox = New System.Windows.Forms.ComboBox()
        Me.layoutNoticeLabel = New System.Windows.Forms.Label()
        Me.modernRadio = New System.Windows.Forms.RadioButton()
        Me.skeuoRadio = New System.Windows.Forms.RadioButton()
        Me.serversIcon = New System.Windows.Forms.PictureBox()
        Me.dmIcon = New System.Windows.Forms.PictureBox()
        Me.githubIcon = New System.Windows.Forms.PictureBox()
        Me.settingsIcon = New System.Windows.Forms.PictureBox()
        Me.accountIcon = New System.Windows.Forms.PictureBox()
        Me.icnPreviewLabel = New System.Windows.Forms.Label()
        Me.cutOffPanel.SuspendLayout()
        CType(Me.serversIcon, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dmIcon, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.githubIcon, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.settingsIcon, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.accountIcon, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'appearanceHeaderRenderer
        '
        Me.appearanceHeaderRenderer.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.appearanceHeaderRenderer.BackColor = System.Drawing.SystemColors.AppWorkspace
        Me.appearanceHeaderRenderer.Location = New System.Drawing.Point(0, 0)
        Me.appearanceHeaderRenderer.Name = "appearanceHeaderRenderer"
        Me.appearanceHeaderRenderer.ResId = "appearanceMain"
        Me.appearanceHeaderRenderer.Size = New System.Drawing.Size(800, 75)
        Me.appearanceHeaderRenderer.TabIndex = 0
        Me.appearanceHeaderRenderer.XmlFile = "User Interface\SetupFlow\AppearanceHeader.xml"
        '
        'cutOffPanel
        '
        Me.cutOffPanel.Controls.Add(Me.layoutBox)
        Me.cutOffPanel.Controls.Add(Me.layoutNoticeLabel)
        Me.cutOffPanel.Controls.Add(Me.modernRadio)
        Me.cutOffPanel.Controls.Add(Me.skeuoRadio)
        Me.cutOffPanel.Controls.Add(Me.serversIcon)
        Me.cutOffPanel.Controls.Add(Me.dmIcon)
        Me.cutOffPanel.Controls.Add(Me.githubIcon)
        Me.cutOffPanel.Controls.Add(Me.settingsIcon)
        Me.cutOffPanel.Controls.Add(Me.accountIcon)
        Me.cutOffPanel.Controls.Add(Me.icnPreviewLabel)
        Me.cutOffPanel.Location = New System.Drawing.Point(20, 75)
        Me.cutOffPanel.Name = "cutOffPanel"
        Me.cutOffPanel.Size = New System.Drawing.Size(760, 334)
        Me.cutOffPanel.TabIndex = 1
        '
        'layoutBox
        '
        Me.layoutBox.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.layoutBox.FormattingEnabled = True
        Me.layoutBox.Items.AddRange(New Object() {"Windows 8.1 - 7", "Windows 11 - 10"})
        Me.layoutBox.Location = New System.Drawing.Point(1, 120)
        Me.layoutBox.Name = "layoutBox"
        Me.layoutBox.Size = New System.Drawing.Size(182, 21)
        Me.layoutBox.TabIndex = 9
        '
        'layoutNoticeLabel
        '
        Me.layoutNoticeLabel.AutoSize = True
        Me.layoutNoticeLabel.Font = New System.Drawing.Font("Tahoma", 8.25!)
        Me.layoutNoticeLabel.Location = New System.Drawing.Point(-2, 88)
        Me.layoutNoticeLabel.Name = "layoutNoticeLabel"
        Me.layoutNoticeLabel.Size = New System.Drawing.Size(401, 26)
        Me.layoutNoticeLabel.TabIndex = 8
        Me.layoutNoticeLabel.Text = "You might want to configure how NaticordVB handles the spacing between elements." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "This is based on your operating system, and not your system theme."
        '
        'modernRadio
        '
        Me.modernRadio.AutoSize = True
        Me.modernRadio.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.modernRadio.Location = New System.Drawing.Point(1, 64)
        Me.modernRadio.Name = "modernRadio"
        Me.modernRadio.Size = New System.Drawing.Size(95, 18)
        Me.modernRadio.TabIndex = 7
        Me.modernRadio.TabStop = True
        Me.modernRadio.Text = "Modern icons"
        Me.modernRadio.UseVisualStyleBackColor = True
        '
        'skeuoRadio
        '
        Me.skeuoRadio.AutoSize = True
        Me.skeuoRadio.Checked = True
        Me.skeuoRadio.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.skeuoRadio.Location = New System.Drawing.Point(1, 41)
        Me.skeuoRadio.Name = "skeuoRadio"
        Me.skeuoRadio.Size = New System.Drawing.Size(127, 18)
        Me.skeuoRadio.TabIndex = 6
        Me.skeuoRadio.TabStop = True
        Me.skeuoRadio.Text = "Skeuomorphic icons"
        Me.skeuoRadio.UseVisualStyleBackColor = True
        '
        'serversIcon
        '
        Me.serversIcon.Image = Global.NaticordVB.My.Resources.Resources.servers_skeuo
        Me.serversIcon.Location = New System.Drawing.Point(89, 19)
        Me.serversIcon.Name = "serversIcon"
        Me.serversIcon.Size = New System.Drawing.Size(16, 16)
        Me.serversIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.serversIcon.TabIndex = 5
        Me.serversIcon.TabStop = False
        '
        'dmIcon
        '
        Me.dmIcon.Image = Global.NaticordVB.My.Resources.Resources.dm_skeuo
        Me.dmIcon.Location = New System.Drawing.Point(67, 19)
        Me.dmIcon.Name = "dmIcon"
        Me.dmIcon.Size = New System.Drawing.Size(16, 16)
        Me.dmIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.dmIcon.TabIndex = 4
        Me.dmIcon.TabStop = False
        '
        'githubIcon
        '
        Me.githubIcon.Image = Global.NaticordVB.My.Resources.Resources.github_skeuo
        Me.githubIcon.Location = New System.Drawing.Point(45, 19)
        Me.githubIcon.Name = "githubIcon"
        Me.githubIcon.Size = New System.Drawing.Size(16, 16)
        Me.githubIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.githubIcon.TabIndex = 3
        Me.githubIcon.TabStop = False
        '
        'settingsIcon
        '
        Me.settingsIcon.Image = Global.NaticordVB.My.Resources.Resources.cog_skeuo
        Me.settingsIcon.Location = New System.Drawing.Point(23, 19)
        Me.settingsIcon.Name = "settingsIcon"
        Me.settingsIcon.Size = New System.Drawing.Size(16, 16)
        Me.settingsIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.settingsIcon.TabIndex = 2
        Me.settingsIcon.TabStop = False
        '
        'accountIcon
        '
        Me.accountIcon.Image = Global.NaticordVB.My.Resources.Resources.account_skeuo
        Me.accountIcon.Location = New System.Drawing.Point(1, 19)
        Me.accountIcon.Name = "accountIcon"
        Me.accountIcon.Size = New System.Drawing.Size(16, 16)
        Me.accountIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.accountIcon.TabIndex = 1
        Me.accountIcon.TabStop = False
        '
        'icnPreviewLabel
        '
        Me.icnPreviewLabel.AutoSize = True
        Me.icnPreviewLabel.Font = New System.Drawing.Font("Tahoma", 8.25!)
        Me.icnPreviewLabel.Location = New System.Drawing.Point(-2, 1)
        Me.icnPreviewLabel.Name = "icnPreviewLabel"
        Me.icnPreviewLabel.Size = New System.Drawing.Size(73, 13)
        Me.icnPreviewLabel.TabIndex = 0
        Me.icnPreviewLabel.Text = "Icon preview:"
        '
        'AppearancePage
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.cutOffPanel)
        Me.Controls.Add(Me.appearanceHeaderRenderer)
        Me.Name = "AppearancePage"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "AppearancePage"
        Me.cutOffPanel.ResumeLayout(False)
        Me.cutOffPanel.PerformLayout()
        CType(Me.serversIcon, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.dmIcon, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.githubIcon, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.settingsIcon, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.accountIcon, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents appearanceHeaderRenderer As DirectUI.Net.Hosting.DirectUIRenderer
    Friend WithEvents cutOffPanel As System.Windows.Forms.Panel
    Friend WithEvents accountIcon As System.Windows.Forms.PictureBox
    Friend WithEvents icnPreviewLabel As System.Windows.Forms.Label
    Friend WithEvents dmIcon As System.Windows.Forms.PictureBox
    Friend WithEvents githubIcon As System.Windows.Forms.PictureBox
    Friend WithEvents settingsIcon As System.Windows.Forms.PictureBox
    Friend WithEvents serversIcon As System.Windows.Forms.PictureBox
    Friend WithEvents modernRadio As System.Windows.Forms.RadioButton
    Friend WithEvents skeuoRadio As System.Windows.Forms.RadioButton
    Friend WithEvents layoutNoticeLabel As System.Windows.Forms.Label
    Friend WithEvents layoutBox As System.Windows.Forms.ComboBox

End Class
