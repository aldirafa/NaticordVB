Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading

Namespace Networking

    Friend Class API

        Private ReadOnly configMgr As New ConfigMgr()

        ' Singleton, so we don't create multiple HttpClient clients
        Private Shared ReadOnly _instance As New Lazy(Of API)(Function() New API())

        Public Shared ReadOnly Property Instance As API
            Get
                Return _instance.Value
            End Get
        End Property

        ' Reuse the HttpClient throughout the API
        Friend ReadOnly InternalHttpClient As HttpClient

        ' Current Discord API version (v9, has been for a while!)
        Private Const API_VERSION As Integer = 9

        ' Configuration (Firefox 115 ESR on Windows 10)
        Public XSuperProperties As String = Nothing
        Public Const UserAgent As String = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/115.0"

        Private Sub New()
            Dim compressionHandler As New HttpClientHandler With {
                .MaxConnectionsPerServer = 10,
                .AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate ' Possibly add Brotli and zstd later?
            }

            InternalHttpClient = New HttpClient(compressionHandler)

            ' Set default headers through out the system
            InternalHttpClient.DefaultRequestHeaders.Add("Accept", "*/*")
            InternalHttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent)

            ' Required for endpoints like /users/@me/remote-auth/login - Discord rejects
            ' the request (returning an error JSON without encrypted_token) when Origin is absent
            InternalHttpClient.DefaultRequestHeaders.Add("Origin", "https://discord.com")

            XSuperProperties = configMgr.GetXSPJson()
            InternalHttpClient.DefaultRequestHeaders.Add("X-Super-Properties", XSuperProperties)
        End Sub

        Public Async Function SendAPI(endpoint As String, httpMethod As HttpMethod,
                                      Optional token As String = Nothing,
                                      Optional data As Object = Nothing,
                                      Optional fileData As Byte() = Nothing,
                                      Optional fileName As String = Nothing,
                                      Optional headers As Dictionary(Of String, String) = Nothing) As Task(Of String)

            Dim url As String = "https://discord.com/api/v" & API_VERSION & "/" & endpoint.TrimStart("/"c)

            Using request As New HttpRequestMessage(httpMethod, url)

                If Not String.IsNullOrEmpty(token) Then
                    Try
                        request.Headers.TryAddWithoutValidation("Authorization", token)
                    Catch ex As Exception
                        Return $"[API/ParseError] An error occurred while sending the request: {ex.Message}{vbLf}{vbLf}[API] URL used when the error occurred: {url}"
                    End Try
                End If

                If headers IsNot Nothing Then
                    For Each kvp In headers
                        request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value)
                    Next
                End If

                If fileData IsNot Nothing AndAlso Not String.IsNullOrEmpty(fileName) Then
                    Dim fileContent As New ByteArrayContent(fileData)
                    fileContent.Headers.Add("Content-Type", "application/octet-stream")

                    Dim content As New MultipartFormDataContent()
                    content.Add(fileContent, "file", fileName)

                    If data IsNot Nothing Then
                        Dim jsonData As String = JsonSerializer.Serialize(data)
                        content.Add(New StringContent(jsonData, Encoding.UTF8, "application/json"), "payload_json")
                    End If

                    request.Content = content
                ElseIf httpMethod <> HttpMethod.Get AndAlso data IsNot Nothing Then
                    Dim jsonData As String = JsonSerializer.Serialize(data)
                    request.Content = New StringContent(jsonData, Encoding.UTF8, "application/json")
                End If

                Try
                    Using response As HttpResponseMessage = Await InternalHttpClient.SendAsync(request)
                        Return Await response.Content.ReadAsStringAsync()
                    End Using
                Catch ex As OperationCanceledException
                    Throw
                Catch ex As Exception
                    Return $"[API/RequestError]{ex.Message}{vbLf}URL: {url}"
                End Try
            End Using
        End Function

        ' How long we wait for Discord before reporting ApiError.Timeout (HttpClient's own default is 100 s).
        Private Shared ReadOnly RequestTimeout As TimeSpan = TimeSpan.FromSeconds(15)

        ''' <summary>
        ''' Sends one API request and reports the outcome as an <see cref="ApiResult"/>. It never throws for network or
        ''' HTTP failures, so callers (including async event handlers) can just check <see cref="ApiResult.[Error]"/>.
        ''' A rate-limited GET waits for Discord's Retry-After (at most 10 s) and is retried once; other methods are not
        ''' retried, so a message is never sent twice.
        ''' </summary>
        ''' <param name="query">Query-string pairs; values are URL-encoded here.</param>
        Public Async Function RequestAsync(endpoint As String, httpMethod As HttpMethod,
                                           Optional token As String = Nothing,
                                           Optional data As Object = Nothing,
                                           Optional query As IEnumerable(Of KeyValuePair(Of String, String)) = Nothing,
                                           Optional cancellation As CancellationToken = Nothing) As Task(Of ApiResult)

            Dim url As String = BuildUrl(endpoint, query)
            Dim attempt As Integer = 0

            Do
                attempt += 1
                Dim result As ApiResult = Await SendOnceAsync(url, httpMethod, token, data, cancellation)

                Dim canRetry As Boolean = result.Error = ApiError.RateLimited AndAlso httpMethod = HttpMethod.Get AndAlso attempt = 1 AndAlso
                                          result.RetryAfterSeconds > 0 AndAlso result.RetryAfterSeconds <= 10
                If Not canRetry Then Return result

                Try
                    Await Task.Delay(TimeSpan.FromSeconds(result.RetryAfterSeconds + 0.25), cancellation)
                Catch ex As OperationCanceledException
                    Return ApiResult.Failure(ApiError.Cancelled, "Cancelled")
                End Try
            Loop
        End Function

        Private Shared Function BuildUrl(endpoint As String, query As IEnumerable(Of KeyValuePair(Of String, String))) As String
            Dim url As String = "https://discord.com/api/v" & API_VERSION & "/" & endpoint.TrimStart("/"c)
            If query Is Nothing Then Return url

            Dim pairs = query.Where(Function(p) p.Value IsNot Nothing).
                              Select(Function(p) Uri.EscapeDataString(p.Key) & "=" & Uri.EscapeDataString(p.Value)).ToList()
            Return If(pairs.Count = 0, url, url & "?" & String.Join("&", pairs))
        End Function

        Private Async Function SendOnceAsync(url As String, httpMethod As HttpMethod, token As String, data As Object,
                                             cancellation As CancellationToken) As Task(Of ApiResult)
            Using request As New HttpRequestMessage(httpMethod, url)
                If Not String.IsNullOrEmpty(token) Then request.Headers.TryAddWithoutValidation("Authorization", token)

                If data IsNot Nothing AndAlso httpMethod <> HttpMethod.Get Then
                    request.Content = New StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json")
                End If

                Using timeout As CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellation)
                    timeout.CancelAfter(RequestTimeout)

                    Try
                        Using response As HttpResponseMessage = Await InternalHttpClient.SendAsync(request, timeout.Token)
                            Dim body As String = Await response.Content.ReadAsStringAsync(timeout.Token)
                            Return ApiResult.FromResponse(CInt(response.StatusCode), body, response.Headers)
                        End Using
                    Catch ex As OperationCanceledException
                        ' Either the caller gave up, or our own timeout fired.
                        If cancellation.IsCancellationRequested Then Return ApiResult.Failure(ApiError.Cancelled, "Cancelled")
                        Return ApiResult.Failure(ApiError.Timeout, "Discord did not answer in time.")
                    Catch ex As HttpRequestException
                        Return ApiResult.Failure(ApiError.Offline, ex.Message)
                    Catch ex As Exception
                        Return ApiResult.Failure(ApiError.Other, ex.Message)
                    End Try
                End Using
            End Using
        End Function

    End Class

End Namespace
