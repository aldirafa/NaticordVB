Imports System.IO
Imports DirectUI.Net
Imports DirectUI.Net.Core
Imports NaticordVB.Interop

Friend Class DuiListItem
    Inherits DuiUserControl

    ' Status icons integers:
    ' 1 = Online
    ' 2 = Busy / Do not disturb
    ' 3 = Idle
    ' 4 = Offline

    ' Extra status icon integers:
    ' 1 = Playing music
    ' 2 = Playing a game
    ' 3 = Streaming on a platform
    ' 4 = Watching something
    ' 5 = No status / Custom status

    Private Const TemplatePath As String = "User Interface\ClientFlow\Sidebar\DuiListItem.xml"

    Private Shared ReadOnly _template As New Lazy(Of String)(
        Function() File.ReadAllText(DuiInitializer.Instance.ResolveXmlPath(TemplatePath)))

    Private ReadOnly _itemId As Integer

    Private _hProfilePic As IntPtr
    Private _hStatusIcon As IntPtr
    Private _hExtraStatusIcon As IntPtr

    Protected Overrides ReadOnly Property ResId As String
        Get
            Return $"listItemMain_{_itemId}"
        End Get
    End Property

    Protected Overrides ReadOnly Property XmlSource As String
        Get
            Return _template.Value.Replace("{N}", _itemId.ToString())
        End Get
    End Property

    Public Sub New(itemId As Integer)
        _itemId = itemId
    End Sub

    Public Sub SetUsername(name As String)
        Host?.SetText($"listItemName_{_itemId}", If(name, ""))
    End Sub

    Public Sub SetStatus(status As String)
        Host?.SetText($"listItemStatus_{_itemId}", If(status, ""))
    End Sub

    Public Sub SetProfilePicture(bmp As Bitmap)
        ReleaseHBitmap(_hProfilePic)
        If bmp IsNot Nothing Then _hProfilePic = bmp.GetHbitmap(Color.Black)

        Host?.SetContentBitmap($"listItemAvatar_{_itemId}", _hProfilePic)
    End Sub

    Public Sub SetStatusIcon(bmp As Bitmap)
        ReleaseHBitmap(_hStatusIcon)
        If bmp IsNot Nothing Then _hStatusIcon = bmp.GetHbitmap(Color.Black)

        Host?.SetContentBitmap($"listItemBadge_{_itemId}", _hStatusIcon)
    End Sub

    Public Sub SetStatusIcon(statusCode As Integer, Optional extraStatusCode As Integer = 5)
        LoadIconTo(StatusName(statusCode), $"listItemBadge_{_itemId}", _hStatusIcon)
        LoadIconTo(ExtraStatusName(extraStatusCode), $"listItemStatusIcon_{_itemId}", _hExtraStatusIcon)
    End Sub

    Private Sub LoadIconTo(name As String, elementId As String, ByRef hCache As IntPtr)
        ReleaseHBitmap(hCache)

        Dim hasIcon As Boolean = Not String.IsNullOrEmpty(name) AndAlso name <> "none"
        If hasIcon Then
            hCache = LoadExtResourceHBitmap(name)
            hasIcon = hCache <> IntPtr.Zero
        End If

        Host?.SetContentBitmap(elementId, hCache)
        Host?.SetLayoutPos(elementId, If(hasIcon, 0, -3))
    End Sub

    Private Shared Function StatusName(code As Integer) As String
        Select Case code
            Case 1 : Return "online"
            Case 2 : Return "busy"
            Case 3 : Return "idle"
            Case 4 : Return "offline"
            Case 5 : Return "none"
            Case Else : Return Nothing
        End Select
    End Function

    Private Shared Function ExtraStatusName(code As Integer) As String
        Select Case code
            Case 1 : Return "music"
            Case 2 : Return "gaming"
            Case 3 : Return "streaming"
            Case 4 : Return "watching"
            Case 5 : Return "none"
            Case Else : Return Nothing
        End Select
    End Function

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            ReleaseHBitmap(_hProfilePic)
            ReleaseHBitmap(_hStatusIcon)
            ReleaseHBitmap(_hExtraStatusIcon)
        End If
        MyBase.Dispose(disposing)
    End Sub

End Class
