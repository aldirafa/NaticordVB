Imports System.Runtime.CompilerServices
Imports System.Text.Json

Namespace Networking.Models

    ''' <summary>Channel types from the Docs ("Channels > Channel Types").</summary>
    Friend Enum ChannelType As Integer
        GuildText = 0
        Dm = 1
        GuildVoice = 2
        GroupDm = 3
        GuildCategory = 4
        GuildAnnouncement = 5
        AnnouncementThread = 10
        PublicThread = 11
        PrivateThread = 12
        GuildStageVoice = 13
        GuildDirectory = 14
        GuildForum = 15
        GuildMedia = 16
    End Enum

    ''' <summary>Tolerant JSON readers: a missing, null or wrongly typed field gives a default instead of throwing.</summary>
    Friend Module JsonRead

        <Extension>
        Friend Function GetStr(el As JsonElement, name As String) As String
            Dim v As JsonElement
            If el.ValueKind = JsonValueKind.Object AndAlso el.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.String Then Return v.GetString()
            Return Nothing
        End Function

        <Extension>
        Friend Function GetInt(el As JsonElement, name As String, Optional fallback As Integer = 0) As Integer
            Dim v As JsonElement
            Dim n As Integer
            If el.ValueKind = JsonValueKind.Object AndAlso el.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.Number AndAlso v.TryGetInt32(n) Then Return n
            Return fallback
        End Function

        <Extension>
        Friend Function GetBool(el As JsonElement, name As String) As Boolean
            Dim v As JsonElement
            Return el.ValueKind = JsonValueKind.Object AndAlso el.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.True
        End Function

        ''' <summary>The items of an array property, or an empty sequence when it is missing or not an array.</summary>
        <Extension>
        Friend Function GetItems(el As JsonElement, name As String) As IEnumerable(Of JsonElement)
            Dim v As JsonElement
            If el.ValueKind = JsonValueKind.Object AndAlso el.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.Array Then Return v.EnumerateArray().ToList()
            Return Enumerable.Empty(Of JsonElement)()
        End Function

    End Module

    Friend NotInheritable Class DiscordUser
        Public Property Id As String = ""
        Public Property Username As String = ""
        Public Property GlobalName As String
        Public Property Avatar As String
        Public Property Discriminator As String = "0"
        Public Property IsBot As Boolean

        ''' <summary>What Discord shows: the display name, falling back to the username.</summary>
        Public ReadOnly Property DisplayName As String
            Get
                If Not String.IsNullOrEmpty(GlobalName) Then Return GlobalName
                If Not String.IsNullOrEmpty(Username) Then Return Username
                Return "Unknown user"
            End Get
        End Property

        Friend Shared Function FromJson(el As JsonElement) As DiscordUser
            Return New DiscordUser With {
                .Id = If(el.GetStr("id"), ""),
                .Username = If(el.GetStr("username"), ""),
                .GlobalName = el.GetStr("global_name"),
                .Avatar = el.GetStr("avatar"),
                .Discriminator = If(el.GetStr("discriminator"), "0"),
                .IsBot = el.GetBool("bot")
            }
        End Function
    End Class

    Friend NotInheritable Class DiscordChannel
        Public Property Id As String = ""
        Public Property Type As ChannelType
        Public Property GuildId As String
        Public Property Name As String
        Public Property ParentId As String
        Public Property Position As Integer
        Public Property LastMessageId As String
        Public Property Topic As String
        Public Property Icon As String
        Public Property Recipients As New List(Of DiscordUser)()

        Public ReadOnly Property IsDirect As Boolean
            Get
                Return Type = ChannelType.Dm OrElse Type = ChannelType.GroupDm
            End Get
        End Property

        ''' <summary>A channel you can read and write messages in (a DM, a group DM, a text or an announcement channel).</summary>
        Public ReadOnly Property IsChat As Boolean
            Get
                Return IsDirect OrElse Type = ChannelType.GuildText OrElse Type = ChannelType.GuildAnnouncement
            End Get
        End Property

        ''' <summary>Snowflake of the newest message as a number, for sorting. 0 when the channel has no messages.</summary>
        Public ReadOnly Property LastMessageValue As ULong
            Get
                Dim v As ULong
                Return If(Snowflake.TryParse(LastMessageId, v), v, 0UL)
            End Get
        End Property

        ''' <summary>The name to show in lists: the other person for a DM, the member names for an unnamed group, "name" for a guild channel.</summary>
        Public ReadOnly Property DisplayName As String
            Get
                If Type = ChannelType.Dm Then
                    Return If(Recipients.Count > 0, Recipients(0).DisplayName, "Unknown user")
                End If
                If Type = ChannelType.GroupDm Then
                    If Not String.IsNullOrWhiteSpace(Name) Then Return Name
                    Dim names = Recipients.Select(Function(r) r.DisplayName).ToList()
                    If names.Count = 0 Then Return "Group chat"
                    Return String.Join(", ", names.Take(3)) & If(names.Count > 3, ", ...", "")
                End If
                Return If(Name, "unnamed")
            End Get
        End Property

        Friend Shared Function FromJson(el As JsonElement) As DiscordChannel
            Return New DiscordChannel With {
                .Id = If(el.GetStr("id"), ""),
                .Type = CType(el.GetInt("type", 0), ChannelType),
                .GuildId = el.GetStr("guild_id"),
                .Name = el.GetStr("name"),
                .ParentId = el.GetStr("parent_id"),
                .Position = el.GetInt("position", 0),
                .LastMessageId = el.GetStr("last_message_id"),
                .Topic = el.GetStr("topic"),
                .Icon = el.GetStr("icon"),
                .Recipients = el.GetItems("recipients").Select(AddressOf DiscordUser.FromJson).ToList()
            }
        End Function
    End Class

    Friend NotInheritable Class DiscordGuild
        Public Property Id As String = ""
        Public Property Name As String = ""
        Public Property Icon As String
        Public Property IsOwner As Boolean
        Public Property Permissions As ULong

        Friend Shared Function FromJson(el As JsonElement) As DiscordGuild
            Dim perms As ULong
            ULong.TryParse(el.GetStr("permissions"), perms)
            Return New DiscordGuild With {
                .Id = If(el.GetStr("id"), ""),
                .Name = If(el.GetStr("name"), "Unnamed server"),
                .Icon = el.GetStr("icon"),
                .IsOwner = el.GetBool("owner"),
                .Permissions = perms
            }
        End Function
    End Class

End Namespace
