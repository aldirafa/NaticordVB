Imports NaticordVB.Networking.Models

Namespace Networking

    ''' <summary>Image URLs on Discord's CDN (Docs: "API Reference > Image Formatting"). Always PNG, so animated images arrive as a still frame.</summary>
    Friend Module DiscordCdn

        Private Const Base As String = "https://cdn.discordapp.com/"

        ''' <summary>The user's avatar, or their default avatar when they have not set one.</summary>
        Friend Function UserAvatarUrl(user As DiscordUser, size As Integer) As String
            If Not String.IsNullOrEmpty(user.Avatar) Then
                Return $"{Base}avatars/{user.Id}/{user.Avatar}.png?size={size}"
            End If

            ' Default avatar: (id >> 22) mod 6 for the new username system, discriminator mod 5 for legacy "name#1234" accounts.
            Dim index As Integer = 0
            Dim legacy As Integer
            If Integer.TryParse(user.Discriminator, legacy) AndAlso legacy > 0 Then
                index = legacy Mod 5
            Else
                Dim id As ULong
                If Snowflake.TryParse(user.Id, id) Then index = CInt((id >> 22) Mod 6UL)
            End If
            Return $"{Base}embed/avatars/{index}.png"
        End Function

        Friend Function GuildIconUrl(guild As DiscordGuild, size As Integer) As String
            If String.IsNullOrEmpty(guild.Icon) Then Return Nothing
            Return $"{Base}icons/{guild.Id}/{guild.Icon}.png?size={size}"
        End Function

        ''' <summary>The picture for a DM row: the other person's avatar, or the group's icon (Nothing for a group without one).</summary>
        Friend Function ChannelIconUrl(channel As DiscordChannel, size As Integer) As String
            If channel.Type = ChannelType.Dm AndAlso channel.Recipients.Count > 0 Then
                Return UserAvatarUrl(channel.Recipients(0), size)
            End If
            If channel.Type = ChannelType.GroupDm AndAlso Not String.IsNullOrEmpty(channel.Icon) Then
                Return $"{Base}channel-icons/{channel.Id}/{channel.Icon}.png?size={size}"
            End If
            Return Nothing
        End Function

    End Module

End Namespace
