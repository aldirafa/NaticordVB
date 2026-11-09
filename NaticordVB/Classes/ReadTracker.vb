Imports System.IO
Imports System.Text.Json
Imports NaticordVB.Networking

''' <summary>
''' Remembers, on this computer, the newest message you have seen in each chat, so chats with something newer can be marked as unread.
'''
''' This is a local approximation: Discord only shares your real read state over the Gateway, which the app does not use yet. So a
''' message you read in another app still shows here as unread, and the first time a chat is seen it counts as read.
''' </summary>
Friend Module ReadTracker

    Private _seen As Dictionary(Of String, String)
    Private _loadedFrom As String
    Private _dirty As Boolean

    Private ReadOnly Property FilePath As String
        Get
            Dim user As String = If(String.IsNullOrEmpty(My.Settings.dscUid), "unknown", My.Settings.dscUid)
            Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NaticordVB", "read-state-" & user & ".json")
        End Get
    End Property

    Private Sub EnsureLoaded()
        Dim path As String = FilePath
        If _seen IsNot Nothing AndAlso _loadedFrom = path Then Return

        _seen = New Dictionary(Of String, String)()
        _loadedFrom = path
        _dirty = False
        Try
            If File.Exists(path) Then
                Dim loaded = JsonSerializer.Deserialize(Of Dictionary(Of String, String))(File.ReadAllText(path))
                If loaded IsNot Nothing Then _seen = loaded
            End If
        Catch ex As Exception
            Debug.WriteLine("Could not read the read-state file: " & ex.Message) ' a damaged file just means everything counts as read
        End Try
    End Sub

    ''' <summary>True when the chat's newest message is newer than the newest one you have seen here.</summary>
    Friend Function IsUnread(channelId As String, lastMessageId As String) As Boolean
        If String.IsNullOrEmpty(channelId) OrElse String.IsNullOrEmpty(lastMessageId) Then Return False
        EnsureLoaded()

        Dim seen As String = Nothing
        Return _seen.TryGetValue(channelId, seen) AndAlso Snowflake.Compare(lastMessageId, seen) > 0
    End Function

    ''' <summary>The first time a chat is seen, its current newest message counts as read; only later messages make it unread.</summary>
    Friend Sub Observe(channelId As String, lastMessageId As String)
        If String.IsNullOrEmpty(channelId) OrElse String.IsNullOrEmpty(lastMessageId) Then Return
        EnsureLoaded()
        If Not _seen.ContainsKey(channelId) Then
            _seen(channelId) = lastMessageId
            _dirty = True
        End If
    End Sub

    ''' <summary>Records that you have now seen everything up to this message.</summary>
    Friend Sub MarkRead(channelId As String, messageId As String)
        If String.IsNullOrEmpty(channelId) OrElse String.IsNullOrEmpty(messageId) Then Return
        EnsureLoaded()

        Dim seen As String = Nothing
        If Not _seen.TryGetValue(channelId, seen) OrElse Snowflake.Compare(messageId, seen) > 0 Then
            _seen(channelId) = messageId
            _dirty = True
            Save()
        End If
    End Sub

    ''' <summary>Writes the file if anything changed. Failing to save is harmless: the marks are just forgotten.</summary>
    Friend Sub Save()
        If Not _dirty OrElse _seen Is Nothing Then Return
        Try
            Directory.CreateDirectory(Path.GetDirectoryName(_loadedFrom))
            File.WriteAllText(_loadedFrom, JsonSerializer.Serialize(_seen))
            _dirty = False
        Catch ex As Exception
            Debug.WriteLine("Could not save the read-state file: " & ex.Message)
        End Try
    End Sub

End Module
