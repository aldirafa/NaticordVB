<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Client
    Inherits GlassForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing Then
                If components IsNot Nothing Then
                    components.Dispose()
                End If
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Client))
        Me.buttonPanel = New System.Windows.Forms.Panel()
        Me.usernameLabel = New NaticordVB.AeroLabel()
        Me.avatarBox = New System.Windows.Forms.PictureBox()
        Me.githubButton = New NaticordVB.GlassExtendedButton()
        Me.settingsButton = New NaticordVB.GlassExtendedButton()
        Me.accountButton = New NaticordVB.GlassExtendedButton()
        Me.sidebarRenderer = New DirectUI.Net.Hosting.DirectUIRenderer()
        Me.tabPanel = New System.Windows.Forms.Panel()
        Me.rbMessages = New System.Windows.Forms.RadioButton()
        Me.rbServers = New System.Windows.Forms.RadioButton()
        Me.chatList = New NaticordVB.ChatListBox()
        Me.sidebarDivider = New System.Windows.Forms.Panel()
        Me.contentPanel = New System.Windows.Forms.Panel()
        Me.accountContext = New NaticordVB.NativeContextGenerator(Me.components)
        Me.accountStatusChanger = New System.Windows.Forms.ToolStripMenuItem()
        Me.onlineToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.busyToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.idleToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.offlineToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.statusSeparator = New System.Windows.Forms.ToolStripSeparator()
        Me.customStatusStripItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.profileStripItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.toolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.logOutStripItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.buttonPanel.SuspendLayout()
        CType(Me.avatarBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.accountContext.SuspendLayout()
        Me.SuspendLayout()
        '
        'buttonPanel
        '
        Me.buttonPanel.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.buttonPanel.BackColor = System.Drawing.Color.Transparent
        Me.buttonPanel.Controls.Add(Me.usernameLabel)
        Me.buttonPanel.Controls.Add(Me.avatarBox)
        Me.buttonPanel.Controls.Add(Me.githubButton)
        Me.buttonPanel.Controls.Add(Me.settingsButton)
        Me.buttonPanel.Controls.Add(Me.accountButton)
        Me.buttonPanel.Location = New System.Drawing.Point(0, 0)
        Me.buttonPanel.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.buttonPanel.Name = "buttonPanel"
        Me.buttonPanel.Size = New System.Drawing.Size(971, 35)
        Me.buttonPanel.TabIndex = 0
        '
        'usernameLabel
        '
        Me.usernameLabel.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.usernameLabel.Location = New System.Drawing.Point(853, 2)
        Me.usernameLabel.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.usernameLabel.Name = "usernameLabel"
        Me.usernameLabel.Size = New System.Drawing.Size(88, 23)
        Me.usernameLabel.TabIndex = 4
        Me.usernameLabel.Text = "Loading..."
        Me.usernameLabel.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.usernameLabel.TextAlignVertical = System.Windows.Forms.VisualStyles.VerticalAlignment.Center
        '
        'avatarBox
        '
        Me.avatarBox.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.avatarBox.BackColor = System.Drawing.Color.Black
        Me.avatarBox.Image = Global.NaticordVB.My.Resources.Resources.user_silhouette_skeuo
        Me.avatarBox.Location = New System.Drawing.Point(947, 2)
        Me.avatarBox.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.avatarBox.Name = "avatarBox"
        Me.avatarBox.Size = New System.Drawing.Size(23, 23)
        Me.avatarBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage
        Me.avatarBox.TabIndex = 3
        Me.avatarBox.TabStop = False
        '
        'githubButton
        '
        Me.githubButton.BackColor = System.Drawing.Color.Transparent
        Me.githubButton.ButtonIcon = Global.NaticordVB.My.Resources.Resources.github_skeuo
        Me.githubButton.ButtonLabel = "GitHub"
        Me.githubButton.CompositionDisabled = False
        Me.githubButton.Location = New System.Drawing.Point(168, 2)
        Me.githubButton.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.githubButton.Name = "githubButton"
        Me.githubButton.ReverseOrder = False
        Me.githubButton.Size = New System.Drawing.Size(76, 23)
        Me.githubButton.TabIndex = 2
        '
        'settingsButton
        '
        Me.settingsButton.BackColor = System.Drawing.Color.Transparent
        Me.settingsButton.ButtonIcon = Global.NaticordVB.My.Resources.Resources.cog_skeuo
        Me.settingsButton.ButtonLabel = "Settings"
        Me.settingsButton.CompositionDisabled = False
        Me.settingsButton.Location = New System.Drawing.Point(85, 2)
        Me.settingsButton.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.settingsButton.Name = "settingsButton"
        Me.settingsButton.ReverseOrder = False
        Me.settingsButton.Size = New System.Drawing.Size(76, 23)
        Me.settingsButton.TabIndex = 1
        '
        'accountButton
        '
        Me.accountButton.BackColor = System.Drawing.Color.Transparent
        Me.accountButton.ButtonIcon = Global.NaticordVB.My.Resources.Resources.account_skeuo
        Me.accountButton.ButtonLabel = "Account"
        Me.accountButton.CompositionDisabled = False
        Me.accountButton.Location = New System.Drawing.Point(2, 2)
        Me.accountButton.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.accountButton.Name = "accountButton"
        Me.accountButton.ReverseOrder = False
        Me.accountButton.Size = New System.Drawing.Size(76, 23)
        Me.accountButton.TabIndex = 0
        '
        'sidebarRenderer
        '
        Me.sidebarRenderer.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.sidebarRenderer.BackColor = System.Drawing.SystemColors.AppWorkspace
        Me.sidebarRenderer.Location = New System.Drawing.Point(0, 65)
        Me.sidebarRenderer.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.sidebarRenderer.Name = "sidebarRenderer"
        Me.sidebarRenderer.ResId = "sidebarPieceMain"
        Me.sidebarRenderer.Size = New System.Drawing.Size(250, 31)
        Me.sidebarRenderer.TabIndex = 1
        Me.sidebarRenderer.XmlFile = "User Interface\ClientFlow\Sidebar.xml"
        '
        'tabPanel
        '
        Me.tabPanel.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.tabPanel.Controls.Add(Me.rbMessages)
        Me.tabPanel.Controls.Add(Me.rbServers)
        Me.tabPanel.Location = New System.Drawing.Point(0, 35)
        Me.tabPanel.Name = "tabPanel"
        Me.tabPanel.Size = New System.Drawing.Size(249, 30)
        Me.tabPanel.TabIndex = 3
        '
        'rbMessages
        '
        Me.rbMessages.Appearance = System.Windows.Forms.Appearance.Button
        Me.rbMessages.FlatStyle = System.Windows.Forms.FlatStyle.Standard
        Me.rbMessages.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.rbMessages.Location = New System.Drawing.Point(0, 0)
        Me.rbMessages.Name = "rbMessages"
        Me.rbMessages.Size = New System.Drawing.Size(124, 30)
        Me.rbMessages.TabIndex = 0
        Me.rbMessages.Text = "Messages"
        Me.rbMessages.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.rbMessages.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.rbMessages.UseVisualStyleBackColor = True
        '
        'rbServers
        '
        Me.rbServers.Appearance = System.Windows.Forms.Appearance.Button
        Me.rbServers.FlatStyle = System.Windows.Forms.FlatStyle.Standard
        Me.rbServers.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.rbServers.Location = New System.Drawing.Point(124, 0)
        Me.rbServers.Name = "rbServers"
        Me.rbServers.Size = New System.Drawing.Size(125, 30)
        Me.rbServers.TabIndex = 1
        Me.rbServers.Text = "Servers"
        Me.rbServers.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.rbServers.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.rbServers.UseVisualStyleBackColor = True
        '
        'chatList
        '
        Me.chatList.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.chatList.Location = New System.Drawing.Point(0, 96)
        Me.chatList.Name = "chatList"
        Me.chatList.Size = New System.Drawing.Size(249, 442)
        Me.chatList.TabIndex = 4
        '
        'sidebarDivider
        '
        Me.sidebarDivider.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.sidebarDivider.BackColor = System.Drawing.SystemColors.ControlLight
        Me.sidebarDivider.Location = New System.Drawing.Point(249, 35)
        Me.sidebarDivider.Name = "sidebarDivider"
        Me.sidebarDivider.Size = New System.Drawing.Size(1, 503)
        Me.sidebarDivider.TabIndex = 5
        '
        'contentPanel
        '
        Me.contentPanel.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.contentPanel.Location = New System.Drawing.Point(250, 34)
        Me.contentPanel.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.contentPanel.Name = "contentPanel"
        Me.contentPanel.Size = New System.Drawing.Size(721, 504)
        Me.contentPanel.TabIndex = 2
        '
        'accountContext
        '
        Me.accountContext.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.accountStatusChanger, Me.profileStripItem, Me.toolStripSeparator1, Me.logOutStripItem})
        Me.accountContext.Name = "accountContext"
        Me.accountContext.Size = New System.Drawing.Size(181, 98)
        '
        'accountStatusChanger
        '
        Me.accountStatusChanger.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.onlineToolStripMenuItem, Me.busyToolStripMenuItem, Me.idleToolStripMenuItem, Me.offlineToolStripMenuItem, Me.statusSeparator, Me.customStatusStripItem})
        Me.accountStatusChanger.Name = "accountStatusChanger"
        Me.accountStatusChanger.Size = New System.Drawing.Size(180, 22)
        Me.accountStatusChanger.Text = "Your status"
        '
        'onlineToolStripMenuItem
        '
        Me.onlineToolStripMenuItem.Image = Global.NaticordVB.My.Resources.Resources.online_skeuo
        Me.onlineToolStripMenuItem.Name = "onlineToolStripMenuItem"
        Me.onlineToolStripMenuItem.Size = New System.Drawing.Size(180, 22)
        Me.onlineToolStripMenuItem.Text = "Online"
        '
        'busyToolStripMenuItem
        '
        Me.busyToolStripMenuItem.Image = Global.NaticordVB.My.Resources.Resources.busy_skeuo
        Me.busyToolStripMenuItem.Name = "busyToolStripMenuItem"
        Me.busyToolStripMenuItem.Size = New System.Drawing.Size(180, 22)
        Me.busyToolStripMenuItem.Text = "Busy"
        '
        'idleToolStripMenuItem
        '
        Me.idleToolStripMenuItem.Image = Global.NaticordVB.My.Resources.Resources.idle_skeuo
        Me.idleToolStripMenuItem.Name = "idleToolStripMenuItem"
        Me.idleToolStripMenuItem.Size = New System.Drawing.Size(180, 22)
        Me.idleToolStripMenuItem.Text = "Idle"
        '
        'offlineToolStripMenuItem
        '
        Me.offlineToolStripMenuItem.Image = Global.NaticordVB.My.Resources.Resources.offline_skeuo
        Me.offlineToolStripMenuItem.Name = "offlineToolStripMenuItem"
        Me.offlineToolStripMenuItem.Size = New System.Drawing.Size(180, 22)
        Me.offlineToolStripMenuItem.Text = "Offline"
        '
        'statusSeparator
        '
        Me.statusSeparator.Name = "statusSeparator"
        Me.statusSeparator.Size = New System.Drawing.Size(177, 6)
        '
        'customStatusStripItem
        '
        Me.customStatusStripItem.Name = "customStatusStripItem"
        Me.customStatusStripItem.Size = New System.Drawing.Size(180, 22)
        Me.customStatusStripItem.Text = "Set custom status..."
        'profileStripItem
        '
        Me.profileStripItem.Name = "profileStripItem"
        Me.profileStripItem.Size = New System.Drawing.Size(180, 22)
        Me.profileStripItem.Text = "Your profile"
        '
        'toolStripSeparator1
        '
        Me.toolStripSeparator1.Name = "toolStripSeparator1"
        Me.toolStripSeparator1.Size = New System.Drawing.Size(177, 6)
        '
        'logOutStripItem
        '
        Me.logOutStripItem.Name = "logOutStripItem"
        Me.logOutStripItem.Size = New System.Drawing.Size(180, 22)
        Me.logOutStripItem.Text = "Log out of NaticordVB"
        '
        'Client
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(971, 538)
        Me.Controls.Add(Me.contentPanel)
        Me.Controls.Add(Me.chatList)
        Me.Controls.Add(Me.sidebarDivider)
        Me.Controls.Add(Me.sidebarRenderer)
        Me.Controls.Add(Me.tabPanel)
        Me.Controls.Add(Me.buttonPanel)
        Me.GlassMargins = New System.Windows.Forms.Padding(0, 30, 0, 0)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.Name = "Client"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "NaticordVB"
        Me.buttonPanel.ResumeLayout(False)
        CType(Me.avatarBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.accountContext.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents buttonPanel As System.Windows.Forms.Panel
    Friend WithEvents accountButton As GlassExtendedButton
    Friend WithEvents githubButton As GlassExtendedButton
    Friend WithEvents settingsButton As GlassExtendedButton
    Friend WithEvents avatarBox As System.Windows.Forms.PictureBox
    Friend WithEvents sidebarRenderer As DirectUI.Net.Hosting.DirectUIRenderer
    Friend WithEvents tabPanel As System.Windows.Forms.Panel
    Friend WithEvents rbMessages As System.Windows.Forms.RadioButton
    Friend WithEvents rbServers As System.Windows.Forms.RadioButton
    Friend WithEvents chatList As ChatListBox
    Friend WithEvents sidebarDivider As System.Windows.Forms.Panel
    Friend WithEvents usernameLabel As AeroLabel
    Friend WithEvents contentPanel As System.Windows.Forms.Panel
    Friend WithEvents accountContext As NativeContextGenerator
    Friend WithEvents accountStatusChanger As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents onlineToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents busyToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents idleToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents offlineToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents statusSeparator As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents customStatusStripItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents profileStripItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents toolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents logOutStripItem As System.Windows.Forms.ToolStripMenuItem

End Class
