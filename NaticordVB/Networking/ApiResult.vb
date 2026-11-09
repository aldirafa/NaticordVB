Imports System.Net.Http.Headers
Imports System.Text.Json

Namespace Networking

    ''' <summary>Why a request did not succeed. <see cref="None"/> means it did.</summary>
    Friend Enum ApiError
        None
        ''' <summary>The machine could not reach Discord (no network, DNS, proxy, TLS...).</summary>
        Offline
        ''' <summary>Discord did not answer within the request timeout.</summary>
        Timeout
        ''' <summary>HTTP 401: the token is wrong or expired.</summary>
        Unauthorized
        ''' <summary>HTTP 403: missing permission (Discord JSON code 50013 is the usual one).</summary>
        Forbidden
        NotFound
        ''' <summary>HTTP 429. <see cref="ApiResult.RetryAfterSeconds"/> says how long to wait.</summary>
        RateLimited
        Server
        ''' <summary>The caller cancelled the request.</summary>
        Cancelled
        Other
    End Enum

    ''' <summary>The outcome of <see cref="API.RequestAsync"/>. Never throws: errors are values.</summary>
    Friend NotInheritable Class ApiResult

        Public Property StatusCode As Integer
        Public Property Body As String = String.Empty
        Public Property [Error] As ApiError = ApiError.None
        ''' <summary>Human readable reason: Discord's own "message" when there is one, otherwise the exception text.</summary>
        Public Property ErrorMessage As String = String.Empty
        ''' <summary>Discord's JSON error code (for example 50013 "Missing Permissions"), or 0.</summary>
        Public Property DiscordCode As Integer
        Public Property RetryAfterSeconds As Double

        Public ReadOnly Property IsSuccess As Boolean
            Get
                Return [Error] = ApiError.None
            End Get
        End Property

        ''' <summary>True for failures caused by connectivity rather than by Discord answering with an error.</summary>
        Public ReadOnly Property IsNetworkFailure As Boolean
            Get
                Return [Error] = ApiError.Offline OrElse [Error] = ApiError.Timeout
            End Get
        End Property

        Friend Shared Function Failure(kind As ApiError, message As String) As ApiResult
            Return New ApiResult With {.[Error] = kind, .ErrorMessage = message}
        End Function

        Friend Shared Function FromResponse(statusCode As Integer, body As String, headers As HttpResponseHeaders) As ApiResult
            Dim result As New ApiResult With {.StatusCode = statusCode, .Body = If(body, String.Empty)}

            If statusCode >= 200 AndAlso statusCode < 300 Then Return result

            result.[Error] = MapStatus(statusCode)
            result.ErrorMessage = $"HTTP {statusCode}"

            ' Discord's error bodies look like {"message": "...", "code": 50013} and a 429 adds "retry_after".
            Try
                Using doc As JsonDocument = JsonDocument.Parse(result.Body)
                    Dim root As JsonElement = doc.RootElement
                    If root.ValueKind = JsonValueKind.Object Then
                        Dim el As JsonElement
                        If root.TryGetProperty("message", el) AndAlso el.ValueKind = JsonValueKind.String Then result.ErrorMessage = el.GetString()
                        If root.TryGetProperty("code", el) AndAlso el.ValueKind = JsonValueKind.Number Then result.DiscordCode = el.GetInt32()
                        If root.TryGetProperty("retry_after", el) AndAlso el.ValueKind = JsonValueKind.Number Then result.RetryAfterSeconds = el.GetDouble()
                    End If
                End Using
            Catch ex As JsonException
                ' Not JSON (an HTML error page, for example): keep the generic "HTTP nnn" message.
            End Try

            If result.RetryAfterSeconds <= 0 AndAlso headers IsNot Nothing AndAlso headers.RetryAfter IsNot Nothing Then
                If headers.RetryAfter.Delta.HasValue Then result.RetryAfterSeconds = headers.RetryAfter.Delta.Value.TotalSeconds
            End If

            Return result
        End Function

        Private Shared Function MapStatus(statusCode As Integer) As ApiError
            Select Case statusCode
                Case 401 : Return ApiError.Unauthorized
                Case 403 : Return ApiError.Forbidden
                Case 404 : Return ApiError.NotFound
                Case 429 : Return ApiError.RateLimited
                Case Is >= 500 : Return ApiError.Server
                Case Else : Return ApiError.Other
            End Select
        End Function

        ''' <summary>Parses <see cref="Body"/> as JSON. Returns Nothing when it is empty or not valid JSON.</summary>
        Public Function ParseJson() As JsonElement?
            If String.IsNullOrWhiteSpace(Body) Then Return Nothing
            Try
                Using doc As JsonDocument = JsonDocument.Parse(Body)
                    Return doc.RootElement.Clone()
                End Using
            Catch ex As JsonException
                Return Nothing
            End Try
        End Function

    End Class

End Namespace
