Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports NaticordVB.Networking.Models

Namespace Networking

    ''' <summary>Reads and changes your status and custom status (<c>/users/@me/settings</c>, which the Docs don't cover but which works for user accounts).</summary>
    Friend NotInheritable Class UserSettingsService

        Private Sub New()
        End Sub

        Friend Const Online As String = "online"
        Friend Const Idle As String = "idle"
        Friend Const DoNotDisturb As String = "dnd"
        Friend Const Invisible As String = "invisible"

        Private Shared ReadOnly Property Token As String
            Get
                Return My.Settings.dscToken
            End Get
        End Property

        Private Shared ReadOnly Patch As New HttpMethod("PATCH")

        Friend Shared Async Function GetPresenceAsync(Optional cancellation As CancellationToken = Nothing) As Task(Of Fetched(Of PresenceSettings))
            Dim result = Await API.Instance.RequestAsync("users/@me/settings", HttpMethod.Get, Token, cancellation:=cancellation)
            If Not result.IsSuccess Then Return Fetched(Of PresenceSettings).Fail(result)

            Dim json As JsonElement? = result.ParseJson()
            If Not json.HasValue OrElse json.Value.ValueKind <> JsonValueKind.Object Then
                Return Fetched(Of PresenceSettings).Fail(ApiResult.Failure(ApiError.Other, "Unexpected response from Discord."))
            End If

            Dim settings As New PresenceSettings With {.Status = If(json.Value.GetStr("status"), Online)}
            Dim custom As JsonElement
            If json.Value.TryGetProperty("custom_status", custom) Then settings.Custom = CustomStatus.FromJson(custom)
            Return Fetched(Of PresenceSettings).Ok(result, settings)
        End Function

        ''' <summary>Sets online / idle / dnd / invisible.</summary>
        Friend Shared Function SetStatusAsync(status As String, Optional cancellation As CancellationToken = Nothing) As Task(Of ApiResult)
            Dim body As New Dictionary(Of String, Object) From {{"status", status}}
            Return API.Instance.RequestAsync("users/@me/settings", Patch, Token, data:=body, cancellation:=cancellation)
        End Function

        ''' <summary>Sets the custom status, or removes it when <paramref name="custom"/> is Nothing or empty.</summary>
        Friend Shared Function SetCustomStatusAsync(custom As CustomStatus, Optional cancellation As CancellationToken = Nothing) As Task(Of ApiResult)
            Dim body As New Dictionary(Of String, Object)()
            If custom Is Nothing OrElse custom.IsEmpty Then
                body("custom_status") = Nothing
            Else
                body("custom_status") = New Dictionary(Of String, Object) From {
                    {"text", If(String.IsNullOrWhiteSpace(custom.Text), Nothing, custom.Text)},
                    {"emoji_id", Nothing},
                    {"emoji_name", If(String.IsNullOrWhiteSpace(custom.EmojiName), Nothing, custom.EmojiName)},
                    {"expires_at", If(custom.ExpiresAt.HasValue, custom.ExpiresAt.Value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", Globalization.CultureInfo.InvariantCulture), Nothing)}
                }
            End If
            Return API.Instance.RequestAsync("users/@me/settings", Patch, Token, data:=body, cancellation:=cancellation)
        End Function

    End Class

End Namespace
