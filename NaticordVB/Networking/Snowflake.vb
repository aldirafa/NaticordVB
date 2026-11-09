Namespace Networking

    ''' <summary>Helpers for Discord snowflake IDs (always strings in JSON; see Docs "API Reference > Snowflakes").</summary>
    Friend Module Snowflake

        Private Const DiscordEpochMs As Long = 1420070400000L

        ''' <summary>Parses a snowflake string. Returns False for Nothing, empty or non-numeric input.</summary>
        Friend Function TryParse(id As String, ByRef value As ULong) As Boolean
            Return ULong.TryParse(id, Globalization.NumberStyles.None, Globalization.CultureInfo.InvariantCulture, value)
        End Function

        ''' <summary>The moment the ID was created (UTC), or Nothing when the ID is not a valid snowflake.</summary>
        Friend Function ToDateTime(id As String) As DateTime?
            Dim value As ULong
            If Not TryParse(id, value) Then Return Nothing
            Dim ms As Long = CLng(value >> 22) + DiscordEpochMs
            Return DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime
        End Function

        ''' <summary>Orders two snowflakes by age: negative when a is older than b. Unparseable IDs sort first.</summary>
        Friend Function Compare(a As String, b As String) As Integer
            Dim x, y As ULong
            Dim okX As Boolean = TryParse(a, x)
            Dim okY As Boolean = TryParse(b, y)
            If Not okX AndAlso Not okY Then Return 0
            If Not okX Then Return -1
            If Not okY Then Return 1
            Return x.CompareTo(y)
        End Function

    End Module

End Namespace
