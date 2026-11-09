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
