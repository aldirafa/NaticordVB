Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json

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

    End Class

End Namespace
