Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports NaticordVB.Networking.Models

Namespace Networking

    ''' <summary>The Discord calls the client needs, returning typed results. Every method reports failure as a value (see <see cref="Fetched(Of T)"/>).</summary>
    Friend NotInheritable Class DiscordService

        Private Sub New()
        End Sub

        Private Shared ReadOnly Property Token As String
            Get
                Return My.Settings.dscToken
            End Get
        End Property

        ''' <summary>Direct messages and group DMs, newest activity first. (Discord returns them unsorted.)</summary>
        Friend Shared Async Function GetDirectChannelsAsync(Optional cancellation As CancellationToken = Nothing) As Task(Of Fetched(Of List(Of DiscordChannel)))
            Dim result = Await API.Instance.RequestAsync("users/@me/channels", HttpMethod.Get, Token, cancellation:=cancellation)
            Return Parse(result, Function(root)
                                     Return root.EnumerateArray().Select(AddressOf DiscordChannel.FromJson).
                                         Where(Function(c) c.IsDirect).
                                         OrderByDescending(Function(c) c.LastMessageValue).ToList()
                                 End Function)
        End Function

        ''' <summary>The servers the user is in, in the order Discord returns them.</summary>
        Friend Shared Async Function GetGuildsAsync(Optional cancellation As CancellationToken = Nothing) As Task(Of Fetched(Of List(Of DiscordGuild)))
            Dim result = Await API.Instance.RequestAsync("users/@me/guilds", HttpMethod.Get, Token, cancellation:=cancellation)
            Return Parse(result, Function(root) root.EnumerateArray().Select(AddressOf DiscordGuild.FromJson).ToList())
        End Function

        ''' <summary>All channels of a server (categories included). Threads are not part of this list.</summary>
        Friend Shared Async Function GetGuildChannelsAsync(guildId As String, Optional cancellation As CancellationToken = Nothing) As Task(Of Fetched(Of List(Of DiscordChannel)))
            Dim result = Await API.Instance.RequestAsync("guilds/" & guildId & "/channels", HttpMethod.Get, Token, cancellation:=cancellation)
            Return Parse(result, Function(root) root.EnumerateArray().Select(AddressOf DiscordChannel.FromJson).ToList())
        End Function

        ''' <summary>
        ''' Messages of a channel, oldest first. With no cursor you get the newest <paramref name="limit"/> (Discord's own order is newest first, and
        ''' we flip it). <paramref name="before"/> asks for older messages than that id, <paramref name="after"/> for newer ones.
        ''' </summary>
        Friend Shared Async Function GetMessagesAsync(channelId As String, Optional limit As Integer = 50,
                                                      Optional before As String = Nothing, Optional after As String = Nothing,
                                                      Optional cancellation As CancellationToken = Nothing) As Task(Of Fetched(Of List(Of DiscordMessage)))
            Dim query As New List(Of KeyValuePair(Of String, String)) From {
                New KeyValuePair(Of String, String)("limit", Math.Clamp(limit, 1, 100).ToString())
            }
            If before IsNot Nothing Then query.Add(New KeyValuePair(Of String, String)("before", before))
            If after IsNot Nothing Then query.Add(New KeyValuePair(Of String, String)("after", after))

            Dim result = Await API.Instance.RequestAsync("channels/" & channelId & "/messages", HttpMethod.Get, Token, query:=query, cancellation:=cancellation)
            Return Parse(result, Function(root)
                                     Return root.EnumerateArray().Select(Function(m) DiscordMessage.FromJson(m)).
                                         OrderBy(Function(m) m, Comparer(Of DiscordMessage).Create(Function(a, b) Snowflake.Compare(a.Id, b.Id))).ToList()
                                 End Function)
        End Function

        ''' <summary>
        ''' Sends a message, optionally as a reply. Never retried automatically, so a message can't be sent twice; on a timeout we
        ''' can't know whether it went through, and the caller says so.
        ''' </summary>
        Friend Shared Async Function SendMessageAsync(channelId As String, content As String,
                                                      Optional replyToMessageId As String = Nothing, Optional guildId As String = Nothing,
                                                      Optional cancellation As CancellationToken = Nothing) As Task(Of Fetched(Of DiscordMessage))
            ' Discord clients send a snowflake made from the current time as the nonce.
            Dim nonce As String = ((DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 1420070400000L) << 22).ToString()
            Dim body As New Dictionary(Of String, Object) From {
                {"content", content},
                {"nonce", nonce},
                {"tts", False}
            }

            If replyToMessageId IsNot Nothing Then
                Dim reference As New Dictionary(Of String, Object) From {
                    {"message_id", replyToMessageId},
                    {"channel_id", channelId},
                    {"fail_if_not_exists", False}
                }
                If guildId IsNot Nothing Then reference("guild_id") = guildId
                body("message_reference") = reference
            End If

            Dim result = Await API.Instance.RequestAsync("channels/" & channelId & "/messages", HttpMethod.Post, Token, data:=body, cancellation:=cancellation)
            If Not result.IsSuccess Then Return Fetched(Of DiscordMessage).Fail(result)

            Dim json As JsonElement? = result.ParseJson()
            If Not json.HasValue OrElse json.Value.ValueKind <> JsonValueKind.Object Then
                Return Fetched(Of DiscordMessage).Fail(ApiResult.Failure(ApiError.Other, "Unexpected response from Discord."))
            End If
            Return Fetched(Of DiscordMessage).Ok(result, DiscordMessage.FromJson(json.Value))
        End Function

        ''' <summary>Turns a successful response into a typed value. A body that isn't a JSON array is reported as a failure, never thrown.</summary>
        Private Shared Function Parse(Of T)(result As ApiResult, map As Func(Of JsonElement, T)) As Fetched(Of T)
            If Not result.IsSuccess Then Return Fetched(Of T).Fail(result)

            Dim json As JsonElement? = result.ParseJson()
            If Not json.HasValue OrElse json.Value.ValueKind <> JsonValueKind.Array Then
                Return Fetched(Of T).Fail(ApiResult.Failure(ApiError.Other, "Unexpected response from Discord."))
            End If

            Try
                Return Fetched(Of T).Ok(result, map(json.Value))
            Catch ex As Exception
                Return Fetched(Of T).Fail(ApiResult.Failure(ApiError.Other, "Could not read Discord's answer."))
            End Try
        End Function

    End Class

End Namespace
