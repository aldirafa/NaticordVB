Imports System.Collections.Concurrent
Imports System.Net.Http
Imports System.Threading

Namespace Networking

    ''' <summary>
    ''' Downloads and remembers small images (avatars, server icons). Downloads are limited to a few at a time so opening a
    ''' long list doesn't fire hundreds of requests at once. A failed download is not retried for a minute.
    ''' </summary>
    Friend NotInheritable Class AvatarCache

        Private Sub New()
        End Sub

        Private Const MaxImages As Integer = 600
        Private Const ImageSize As Integer = 64

        Private Shared ReadOnly Http As HttpClient = CreateClient()
        Private Shared ReadOnly Images As New ConcurrentDictionary(Of String, Bitmap)()
        Private Shared ReadOnly FailedUntil As New ConcurrentDictionary(Of String, DateTime)()
        Private Shared ReadOnly Gate As New SemaphoreSlim(4)

        Private Shared Function CreateClient() As HttpClient
            Dim client As New HttpClient() With {.Timeout = TimeSpan.FromSeconds(10)}
            client.DefaultRequestHeaders.Add("User-Agent", API.UserAgent)
            Return client
        End Function

        ''' <summary>The image if it is already downloaded, otherwise Nothing (no download is started).</summary>
        Friend Shared Function TryGet(url As String) As Bitmap
            If String.IsNullOrEmpty(url) Then Return Nothing
            Dim bmp As Bitmap = Nothing
            Return If(Images.TryGetValue(url, bmp), bmp, Nothing)
        End Function

        ''' <summary>Returns the image, downloading it if needed. Never throws: Nothing means it isn't available.</summary>
        Friend Shared Async Function GetAsync(url As String) As Task(Of Bitmap)
            If String.IsNullOrEmpty(url) Then Return Nothing

            Dim cached As Bitmap = TryGet(url)
            If cached IsNot Nothing Then Return cached

            Dim retryAt As DateTime
            If FailedUntil.TryGetValue(url, retryAt) AndAlso retryAt > DateTime.UtcNow Then Return Nothing

            Await Gate.WaitAsync()
            Try
                ' Someone else may have fetched it while we waited for our turn.
                cached = TryGet(url)
                If cached IsNot Nothing Then Return cached

                Using response As HttpResponseMessage = Await Http.GetAsync(url)
                    If Not response.IsSuccessStatusCode Then
                        FailedUntil(url) = DateTime.UtcNow.AddMinutes(1)
                        Return Nothing
                    End If

                    Dim bytes As Byte() = Await response.Content.ReadAsByteArrayAsync()
                    Using ms As New IO.MemoryStream(bytes)
                        Using source As Image = Image.FromStream(ms)
                            ' Keep a private, fixed-size copy so the stream and the original can be released.
                            Dim bmp As New Bitmap(source, ImageSize, ImageSize)
                            If Images.Count >= MaxImages Then Images.Clear()
                            Images(url) = bmp
                            Return bmp
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                FailedUntil(url) = DateTime.UtcNow.AddMinutes(1)
                Return Nothing
            Finally
                Gate.Release()
            End Try
        End Function

    End Class

End Namespace
