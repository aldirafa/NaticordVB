Imports System.Globalization
Imports System.Text
Imports System.Text.Json

Friend Class ConfigMgr

    ' Launch info
    Public Property LaunchSignature As String
    Public Property ClientLaunchId As String

    ' System related options
    Public Property OperatingSystem As String = "Windows"
    Public Property BrowserName As String = "Firefox"
    Public Property DeviceName As String = String.Empty ' Discord leaves this empty for some reason?
    Public Property SystemLocale As String = CultureInfo.CurrentCulture.Name
    Public Property OSVersion As String = "10"

    ' Discord related options
    Public Property HasClientMods As Boolean = False ' Discord uses this in the XSP, don't know why they need this.
    Public Property DCReferrer As String = String.Empty
    Public Property DCReferringDomain As String = String.Empty
    Public Property DCReferringCurrent As String = "https://discord.com/"
    Public Property DCReferringCurrentDomain As String = "discord.com"
    Public Property DCReleaseChannel As String = "canary"
    Public Property DCClientBuild As Integer = 537475 ' Latest build as of 1/5/26
    Public Property DCClientEvtSrc As String = Nothing
    Public Property DCClientState As String = "unfocused"

    ' Browser related options
    Public Property BrowserUA As String = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/115.0"
    Public Property BrowserVer As String = "115.0"

    Public Function GetXSPJson() As String
        Dim xspJson As String = GenerateXSP()
        Dim xspBytes As Byte() = Encoding.UTF8.GetBytes(xspJson)
        Return Convert.ToBase64String(xspBytes)
    End Function

    Private Function GenerateXSP() As String
        ' Build the JSON required for XSP
        GenerateLaunchSignature()

        Dim dict As New Dictionary(Of String, Object) From {
            {"os", OperatingSystem},
            {"browser", BrowserName},
            {"device", DeviceName},
            {"system_locale", SystemLocale},
            {"has_client_mods", HasClientMods},
            {"browser_user_agent", BrowserUA},
            {"browser_version", BrowserVer},
            {"os_version", OSVersion},
            {"referrer", DCReferrer},
            {"referring_domain", DCReferringDomain},
            {"referrer_current", DCReferringCurrent},
            {"referring_domain_current", DCReferringCurrentDomain},
            {"release_channel", DCReleaseChannel},
            {"client_build_number", DCClientBuild},
            {"client_event_source", DCClientEvtSrc},
            {"client_launch_id", ClientLaunchId},
            {"launch_signature", LaunchSignature},
            {"client_app_state", DCClientState}
        }

        ' Returns the finished XSP!
        Return JsonSerializer.Serialize(dict)
    End Function

    ' These functions below were rewritten from the source code of Discord Messenger, the exact file can be found here:
    ' https://github.com/DiscordMessenger/dm/blob/master/src/core/config/DiscordClientConfig.cpp
    ' Credit goes to them for this code, technically since it's based off of theirs.
    Public Shared Function FormatUUID(partLeft As ULong, partRight As ULong) As String
        Dim buffer As String = partLeft.ToString("x16") & partRight.ToString("x16")
        Return buffer.Substring(0, 8) & "-" &
               buffer.Substring(8, 4) & "-" &
               buffer.Substring(12, 4) & "-" &
               buffer.Substring(16, 4) & "-" &
               buffer.Substring(20, 12)
    End Function

    Private Shared Function RandU64() As ULong
        Dim bytes(7) As Byte
        Random.Shared.NextBytes(bytes)
        Return BitConverter.ToUInt64(bytes, 0)
    End Function

    Public Sub GenerateLaunchSignature()
        Dim launchUuidPart1 As ULong = RandU64()
        Dim launchUuidPart2 As ULong = RandU64()

        launchUuidPart1 = launchUuidPart1 And Not (
            (1UL << 11) Or
            (1UL << 24) Or
            (1UL << 38) Or
            (1UL << 48) Or
            (1UL << 55) Or
            (1UL << 61))

        launchUuidPart2 = launchUuidPart2 And Not (
            (1UL << 11) Or
            (1UL << 20) Or
            (1UL << 27) Or
            (1UL << 36) Or
            (1UL << 44) Or
            (1UL << 55))

        LaunchSignature = FormatUUID(launchUuidPart1, launchUuidPart2)
        ClientLaunchId = FormatUUID(RandU64(), RandU64())
    End Sub

End Class
