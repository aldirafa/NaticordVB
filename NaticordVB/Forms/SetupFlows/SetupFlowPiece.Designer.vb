<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SetupFlowPiece
    Inherits GlassForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing Then
                If components IsNot Nothing Then
                    components.Dispose()
                End If

                If _pages IsNot Nothing Then
                    For Each entry In _pages
                        entry.Page.Dispose()
                    Next
                    _pages = Nothing
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SetupFlowPiece))
        Me.pageCountRenderer = New DirectUI.Net.Hosting.DirectUIRenderer()
        Me.pagePanel = New System.Windows.Forms.Panel()
        Me.buttonPanel = New System.Windows.Forms.Panel()
        Me.nextButton = New NaticordVB.GlassExtendedButton()
        Me.backButton = New NaticordVB.GlassExtendedButton()
        Me.buttonPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        'pageCountRenderer
        '
        Me.pageCountRenderer.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pageCountRenderer.BackColor = System.Drawing.SystemColors.AppWorkspace
        Me.pageCountRenderer.Location = New System.Drawing.Point(0, 35)
        Me.pageCountRenderer.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.pageCountRenderer.Name = "pageCountRenderer"
        Me.pageCountRenderer.ResId = "pageCountPieceMain"
        Me.pageCountRenderer.Size = New System.Drawing.Size(656, 40)
        Me.pageCountRenderer.TabIndex = 0
        Me.pageCountRenderer.XmlFile = "User Interface\SetupFlow\PageCountPiece.xml"
        '
        'pagePanel
        '
        Me.pagePanel.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pagePanel.BackColor = System.Drawing.Color.White
        Me.pagePanel.Location = New System.Drawing.Point(0, 75)
        Me.pagePanel.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.pagePanel.Name = "pagePanel"
        Me.pagePanel.Size = New System.Drawing.Size(656, 497)
        Me.pagePanel.TabIndex = 1
        '
        'buttonPanel
        '
        Me.buttonPanel.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.buttonPanel.BackColor = System.Drawing.Color.Transparent
        Me.buttonPanel.Controls.Add(Me.nextButton)
        Me.buttonPanel.Controls.Add(Me.backButton)
        Me.buttonPanel.Location = New System.Drawing.Point(0, 0)
        Me.buttonPanel.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.buttonPanel.Name = "buttonPanel"
        Me.buttonPanel.Size = New System.Drawing.Size(656, 35)
        Me.buttonPanel.TabIndex = 2
        '
        'nextButton
        '
        Me.nextButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.nextButton.BackColor = System.Drawing.Color.Transparent
        Me.nextButton.ButtonIcon = Global.NaticordVB.My.Resources.Resources.arrow_forward_skeuo
        Me.nextButton.ButtonLabel = "Next page"
        Me.nextButton.CompositionDisabled = False
        Me.nextButton.Location = New System.Drawing.Point(482, 2)
        Me.nextButton.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.nextButton.Name = "nextButton"
        Me.nextButton.ReverseOrder = True
        Me.nextButton.Size = New System.Drawing.Size(175, 23)
        Me.nextButton.TabIndex = 1
        '
        'backButton
        '
        Me.backButton.BackColor = System.Drawing.Color.Transparent
        Me.backButton.ButtonIcon = Global.NaticordVB.My.Resources.Resources.arrow_back_skeuo
        Me.backButton.ButtonLabel = "Go back"
        Me.backButton.CompositionDisabled = False
        Me.backButton.Location = New System.Drawing.Point(2, 2)
        Me.backButton.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.backButton.Name = "backButton"
        Me.backButton.ReverseOrder = False
        Me.backButton.Size = New System.Drawing.Size(175, 23)
        Me.backButton.TabIndex = 0
        '
        'SetupFlowPiece
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.ClientSize = New System.Drawing.Size(656, 572)
        Me.Controls.Add(Me.buttonPanel)
        Me.Controls.Add(Me.pageCountRenderer)
        Me.Controls.Add(Me.pagePanel)
        Me.GlassMargins = New System.Windows.Forms.Padding(0, 30, 0, 0)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.Name = "SetupFlowPiece"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Setup NaticordVB"
        Me.buttonPanel.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents pageCountRenderer As DirectUI.Net.Hosting.DirectUIRenderer
    Friend WithEvents pagePanel As System.Windows.Forms.Panel
    Friend WithEvents buttonPanel As System.Windows.Forms.Panel
    Friend WithEvents nextButton As GlassExtendedButton
    Friend WithEvents backButton As GlassExtendedButton

End Class
