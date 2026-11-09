Imports NaticordVB.Networking.Models

''' <summary>The panel that shows one conversation. For now only the header; the message history and the reply box come next.</summary>
Public Class ChatPage

    <System.ComponentModel.Browsable(False), System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Friend Property Channel As DiscordChannel

    Public Sub New()
        InitializeComponent()
        titleLabel.Font = New Font(SystemFonts.MessageBoxFont.FontFamily, SystemFonts.MessageBoxFont.Size + 3.0F, FontStyle.Regular)
        bodyLabel.Font = SystemFonts.MessageBoxFont
    End Sub

    ''' <summary>Switches the page to another conversation.</summary>
    Friend Sub OpenChannel(newChannel As DiscordChannel, title As String)
        Channel = newChannel
        titleLabel.Text = title
        bodyLabel.Text = "Messages will appear here."
    End Sub

End Class
