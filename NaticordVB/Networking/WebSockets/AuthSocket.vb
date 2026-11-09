Imports System.Net.Http
Imports System.Net.WebSockets
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports Org.BouncyCastle.Crypto
Imports Org.BouncyCastle.Crypto.Digests
Imports Org.BouncyCastle.Crypto.Encodings
Imports Org.BouncyCastle.Crypto.Engines
Imports Org.BouncyCastle.Crypto.Generators
Imports Org.BouncyCastle.Security
Imports Org.BouncyCastle.X509

Namespace Networking.WebSockets

    Friend Class AuthSocket
        Implements IDisposable

#Region "Variables used by AuthSocket"

        ' The required URL variables for this to actually work
        Private Const gatewayUrl As String = "wss://remote-auth-gateway.discord.gg/?v=2"
        Private Const loginEndpoint As String = "users/@me/remote-auth/login"

        Private Shared ReadOnly jsonOptions As New JsonSerializerOptions With {
            .Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }

        ' The client required for the WebSockets to function
        Private _ws As ClientWebSocket
        Private ReadOnly _sendLock As New SemaphoreSlim(1, 1)

        ' Cancellation token for the socket
        Private _cts As CancellationTokenSource

        Private ReadOnly _syncContext As SynchronizationContext
        Private _disposed As Boolean
        Private _heartbeatInterval As Integer

        Private ReadOnly _keyPair As AsymmetricCipherKeyPair
        Private ReadOnly _spkiBytes As Byte()

        ' All actions for the socket
        Public Event qrCodeReady As Action(Of String)
        Public Event tokenReceived As Action(Of String)

#End Region

        Public Sub New()
            _syncContext = SynchronizationContext.Current
            Dim gen As New RsaKeyPairGenerator()

            ' Generate a keypair for later...
            gen.Init(New KeyGenerationParameters(New SecureRandom(), 2048))
            _keyPair = gen.GenerateKeyPair()

            _spkiBytes = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(_keyPair.Public).GetDerEncoded()
        End Sub

#Region "Start and stop socket functions"

        Public Async Function StartSocket() As Task
            _cts = New CancellationTokenSource()
            _ws = New ClientWebSocket()

            ' Required so Discord doesn't kick us out for being an unknown source
            _ws.Options.SetRequestHeader("Origin", "https://discord.com")

            ' Connect to the gateway!
            Await _ws.ConnectAsync(New Uri(gatewayUrl), CancellationToken.None)
            RunInBackground(Function() ReceiveLoop())
        End Function

        Public Sub StopSocket()
            _cts?.Cancel()
        End Sub

#End Region

#Region "WebSocket loops"

        Private Async Function ReceiveLoop() As Task
            Dim buffer(16383) As Byte
            Dim sb As New StringBuilder()

            While _ws.State = WebSocketState.Open AndAlso Not _cts.IsCancellationRequested
                sb.Clear()
                Dim result As WebSocketReceiveResult

                Do
                    result = Await _ws.ReceiveAsync(New ArraySegment(Of Byte)(buffer), _cts.Token)
                    If result.MessageType = WebSocketMessageType.Close Then Return

                    sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count))
                Loop While Not result.EndOfMessage

                Await HandleMessage(sb.ToString())
            End While
        End Function

        Private Async Function HeartbeatLoop() As Task
            Await Task.Delay(_heartbeatInterval)
            While Not _cts.IsCancellationRequested AndAlso _ws.State = WebSocketState.Open
                Await SendMessage(JsonSerializer.Serialize(New Dictionary(Of String, Object) From {{"op", "heartbeat"}}, jsonOptions))
                Await Task.Delay(_heartbeatInterval, _cts.Token)
            End While
        End Function

#End Region

#Region "WS message functions (Sending stuff to Discord)"

        Private Async Function SendInit() As Task
            Await SendMessage(JsonSerializer.Serialize(New Dictionary(Of String, Object) From {
                {"op", "init"},
                {"encoded_public_key", Convert.ToBase64String(_spkiBytes)}
            }, jsonOptions))
        End Function

        Private Async Function SendNonceProof(root As JsonElement) As Task
            Dim nonce As String = DecryptToUrlSafeBase64(root.GetProperty("encrypted_nonce").GetString())
            Await SendMessage(JsonSerializer.Serialize(New Dictionary(Of String, Object) From {
                {"op", "nonce_proof"},
                {"nonce", nonce}
            }, jsonOptions))
        End Function

        Private Sub HandlePendingRemoteInit(root As JsonElement)
            Dim fingerprint As String = root.GetProperty("fingerprint").GetString()
            Dim expected As String = ToUrlSafeBase64(SHA256.HashData(_spkiBytes))

            If fingerprint <> expected Then
                StopSocket()
                Return
            End If

            PostToUi(Sub() RaiseEvent qrCodeReady("https://discord.com/ra/" & fingerprint))
        End Sub

        Private Async Function HandlePendingLogin(root As JsonElement) As Task
            Dim ticket As String = root.GetProperty("ticket").GetString()
            Dim response As String = Await API.Instance.SendAPI(loginEndpoint, HttpMethod.Post, data:=New With {Key .ticket = ticket})

            Using doc As JsonDocument = JsonDocument.Parse(response)
                Dim rootEl As JsonElement = doc.RootElement
                Dim ignored As JsonElement

                If rootEl.TryGetProperty("captcha_key", ignored) OrElse rootEl.TryGetProperty("captcha_sitekey", ignored) Then
                    ' You are now stuck in a CAPTCHA loop, good luck!
                    PostToUi(Sub() MessageBox.Show("Discord is requesting for a CAPTCHA to be completed, Naticord does not support these as of now. Please try again later.",
                                                   "Naticord", MessageBoxButtons.OK, MessageBoxIcon.Error))
                    Return
                End If

                Dim tokenEl As JsonElement
                If Not rootEl.TryGetProperty("encrypted_token", tokenEl) OrElse tokenEl.ValueKind <> JsonValueKind.String Then Return

                Dim token As String = DecryptToUtf8(tokenEl.GetString())
                PostToUi(Sub() RaiseEvent tokenReceived(token))
            End Using
        End Function

#End Region

#Region "WS message functions (The actual backend of them)"

        Private Async Function HandleMessage(data As String) As Task
            Using doc As JsonDocument = JsonDocument.Parse(data)
                Dim root As JsonElement = doc.RootElement
                Dim op As String = If(root.GetProperty("op").GetString(), "")

                Select Case op
                    Case "hello"
                        ' Start the heartbeat loop then send the initial payload
                        _heartbeatInterval = root.GetProperty("heartbeat_interval").GetInt32()
                        RunInBackground(Function() HeartbeatLoop())

                        Await SendInit()
                    Case "nonce_proof"
                        Await SendNonceProof(root)
                    Case "pending_remote_init"
                        HandlePendingRemoteInit(root)
                    Case "pending_login"
                        Await HandlePendingLogin(root)
                        ' "pending_ticket" carries the scanning user's details; nothing needs to be sent back for it yet.
                End Select
            End Using
        End Function

        Private Async Function SendMessage(message As String) As Task
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(message)
            Await _sendLock.WaitAsync(CancellationToken.None)

            Try
                Await _ws.SendAsync(New ArraySegment(Of Byte)(bytes), WebSocketMessageType.Text, True, CancellationToken.None)
            Finally
                _sendLock.Release()
            End Try
        End Function

#End Region

#Region "Decrypt functions"

        Private Shared Function ToUrlSafeBase64(data As Byte()) As String
            Return Convert.ToBase64String(data).TrimEnd("="c).Replace("+"c, "-"c).Replace("/"c, "_"c)
        End Function

        Private Function DecryptToUrlSafeBase64(encBase64 As String) As String
            Return ToUrlSafeBase64(OaepDecrypt(encBase64))
        End Function

        Private Function DecryptToUtf8(encBase64 As String) As String
            Return Encoding.UTF8.GetString(OaepDecrypt(encBase64))
        End Function

        Private Function OaepDecrypt(encBase64 As String) As Byte()
            Dim engine As New OaepEncoding(New RsaEngine(), New Sha256Digest())
            engine.Init(False, _keyPair.Private)

            Dim enc As Byte() = Convert.FromBase64String(encBase64)
            Return engine.ProcessBlock(enc, 0, enc.Length)
        End Function

#End Region

        ' Marshals work back onto the thread that created the socket (the UI thread).
        Private Sub PostToUi(action As Action)
            If _syncContext IsNot Nothing Then
                _syncContext.Post(Sub(state) action(), Nothing)
            Else
                action()
            End If
        End Sub

        ' The loops run until the socket closes; their lifetime is controlled by _cts, not by the caller.
        Private Shared Sub RunInBackground(work As Func(Of Task))
            Task.Run(work)
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True

            _cts?.Cancel()
            _ws?.Dispose()
            _sendLock?.Dispose()
            _cts?.Dispose()
        End Sub

    End Class

End Namespace
