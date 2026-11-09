Imports System.ComponentModel
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports NaticordVB.Interop

''' <summary>
''' A <see cref="ContextMenuStrip"/> that shows a real Win32 popup menu instead of the WinForms-drawn one.
''' </summary>
<ToolboxItem(True), DefaultEvent("ItemClicked")>
Friend Class NativeContextGenerator
    Inherits ContextMenuStrip

#Region "Win32"

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function CreatePopupMenu() As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function DestroyMenu(hMenu As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
    Private Shared Function InsertMenuItem(hMenu As IntPtr, uItem As UInteger, fByPosition As Boolean, ByRef lpmii As MENUITEMINFO) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function TrackPopupMenuEx(hMenu As IntPtr, uFlags As UInteger, x As Integer, y As Integer, hwnd As IntPtr, lptpm As IntPtr) As Integer
    End Function

    <DllImport("user32.dll")>
    Private Shared Function SetForegroundWindow(hWnd As IntPtr) As Boolean
    End Function

    ' Not the shared declaration: here we need the pixel buffer pointer back.
    <DllImport("gdi32.dll", SetLastError:=True)>
    Private Shared Function CreateDIBSection(hdc As IntPtr, ByRef pbmi As BITMAPINFO, iUsage As UInteger,
                                             <Out> ByRef ppvBits As IntPtr, hSection As IntPtr, dwOffset As UInteger) As IntPtr
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Private Structure BITMAPINFOHEADER
        Public biSize As UInteger
        Public biWidth As Integer
        Public biHeight As Integer
        Public biPlanes As UShort
        Public biBitCount As UShort
        Public biCompression As UInteger
        Public biSizeImage As UInteger
        Public biXPelsPerMeter As Integer
        Public biYPelsPerMeter As Integer
        Public biClrUsed As UInteger
        Public biClrImportant As UInteger
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure BITMAPINFO
        Public bmiHeader As BITMAPINFOHEADER
        Public bmiColors As UInteger
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Private Structure MENUITEMINFO
        Public cbSize As UInteger
        Public fMask As UInteger
        Public fType As UInteger
        Public fState As UInteger
        Public wID As UInteger
        Public hSubMenu As IntPtr
        Public hbmpChecked As IntPtr
        Public hbmpUnchecked As IntPtr
        Public dwItemData As IntPtr
        <MarshalAs(UnmanagedType.LPWStr)>
        Public dwTypeData As String
        Public cch As UInteger
        Public hbmpItem As IntPtr

        Public Shared Function Create() As MENUITEMINFO
            Return New MENUITEMINFO With {.cbSize = CUInt(Marshal.SizeOf(GetType(MENUITEMINFO)))}
        End Function
    End Structure

    Private Const MIIM_STATE As UInteger = &H1UI
    Private Const MIIM_ID As UInteger = &H2UI
    Private Const MIIM_SUBMENU As UInteger = &H4UI
    Private Const MIIM_BITMAP As UInteger = &H80UI
    Private Const MIIM_FTYPE As UInteger = &H100UI
    Private Const MIIM_STRING As UInteger = &H40UI
    Private Const MFT_STRING As UInteger = &H0UI
    Private Const MFT_SEPARATOR As UInteger = &H800UI
    Private Const MFS_ENABLED As UInteger = &H0UI
    Private Const MFS_GRAYED As UInteger = &H3UI
    Private Const MFS_CHECKED As UInteger = &H8UI
    Private Const TPM_LEFTALIGN As UInteger = &H0UI
    Private Const TPM_RIGHTBUTTON As UInteger = &H2UI
    Private Const TPM_RETURNCMD As UInteger = &H100UI
    Private Const TPM_NONOTIFY As UInteger = &H80UI

#End Region

    Private ReadOnly _idToItem As New Dictionary(Of UInteger, ToolStripMenuItem)()
    Private ReadOnly _hBitmaps As New List(Of IntPtr)()
    Private _idCounter As UInteger

    Public Sub New()
    End Sub

    Public Sub New(container As IContainer)
        MyBase.New(container)
    End Sub

    Protected Overrides Sub OnOpening(e As CancelEventArgs)
        e.Cancel = True
        Dim pos As Point = Cursor.Position
        ShowNative(Nothing, pos.X, pos.Y)
    End Sub

    Public Shadows Sub Show()
        Dim pos As Point = Cursor.Position
        ShowNative(Nothing, pos.X, pos.Y)
    End Sub

    Public Shadows Sub Show(screenX As Integer, screenY As Integer)
        ShowNative(Nothing, screenX, screenY)
    End Sub

    Public Shadows Sub Show(screenPoint As Point)
        ShowNative(Nothing, screenPoint.X, screenPoint.Y)
    End Sub

    Public Shadows Sub Show(owner As Control, clientX As Integer, clientY As Integer)
        Show(owner, New Point(clientX, clientY))
    End Sub

    Public Shadows Sub Show(owner As Control, clientPoint As Point)
        If owner Is Nothing Then Throw New ArgumentNullException(NameOf(owner))
        Dim screenPoint As Point = owner.PointToScreen(clientPoint)
        ShowNative(owner, screenPoint.X, screenPoint.Y)
    End Sub

    Private Sub ShowNative(owner As Control, screenX As Integer, screenY As Integer)
        _idToItem.Clear()
        _hBitmaps.Clear()
        _idCounter = 1UI

        Dim hMenu As IntPtr = CreatePopupMenu()
        If hMenu = IntPtr.Zero Then
            Throw New Win32Exception(Marshal.GetLastWin32Error())
        End If

        Try
            PopulateMenu(hMenu, Items)

            Dim hwnd As IntPtr = If(owner?.Handle, IntPtr.Zero)
            If hwnd <> IntPtr.Zero Then
                SetForegroundWindow(hwnd)
            End If

            Dim flags As UInteger = TPM_LEFTALIGN Or TPM_RIGHTBUTTON Or TPM_RETURNCMD Or TPM_NONOTIFY
            Dim selectedId As Integer = TrackPopupMenuEx(hMenu, flags, screenX, screenY, hwnd, IntPtr.Zero)

            Dim item As ToolStripMenuItem = Nothing
            If selectedId > 0 AndAlso _idToItem.TryGetValue(CUInt(selectedId), item) Then
                item.PerformClick()
            End If
        Finally
            DestroyMenu(hMenu)

            For Each hBmp As IntPtr In _hBitmaps
                DeleteObject(hBmp)
            Next
            _hBitmaps.Clear()
        End Try
    End Sub

    Private Sub PopulateMenu(hMenu As IntPtr, items As ToolStripItemCollection)
        Dim position As UInteger = 0UI
        For Each item As ToolStripItem In items
            If TypeOf item Is ToolStripSeparator Then
                InsertSeparator(hMenu, position)
            Else
                Dim menuItem As ToolStripMenuItem = TryCast(item, ToolStripMenuItem)
                If menuItem IsNot Nothing Then
                    InsertItem(hMenu, position, menuItem)
                End If
            End If

            position += 1UI
        Next
    End Sub

    Private Shared Sub InsertSeparator(hMenu As IntPtr, position As UInteger)
        Dim info As MENUITEMINFO = MENUITEMINFO.Create()
        info.fMask = MIIM_FTYPE
        info.fType = MFT_SEPARATOR
        InsertMenuItem(hMenu, position, True, info)
    End Sub

    Private Sub InsertItem(hMenu As IntPtr, position As UInteger, menuItem As ToolStripMenuItem)
        Dim info As MENUITEMINFO = MENUITEMINFO.Create()
        info.fType = MFT_STRING
        info.fState = If(menuItem.Enabled, MFS_ENABLED, MFS_GRAYED)

        If menuItem.Checked Then
            info.fState = info.fState Or MFS_CHECKED
        End If

        If menuItem.Image IsNot Nothing Then
            Dim hBmp As IntPtr = CreatePremultipliedBitmap(menuItem.Image)
            If hBmp <> IntPtr.Zero Then
                _hBitmaps.Add(hBmp)
                info.fMask = info.fMask Or MIIM_BITMAP
                info.hbmpItem = hBmp
            End If
        End If

        If menuItem.HasDropDownItems Then
            Dim hSubMenu As IntPtr = CreatePopupMenu()
            If hSubMenu = IntPtr.Zero Then
                Throw New Win32Exception(Marshal.GetLastWin32Error())
            End If

            PopulateMenu(hSubMenu, menuItem.DropDownItems)

            info.fMask = info.fMask Or MIIM_STRING Or MIIM_FTYPE Or MIIM_SUBMENU Or MIIM_STATE
            info.hSubMenu = hSubMenu
            info.dwTypeData = menuItem.Text
        Else
            Dim id As UInteger = _idCounter
            _idCounter += 1UI
            _idToItem(id) = menuItem

            info.fMask = info.fMask Or MIIM_STRING Or MIIM_FTYPE Or MIIM_ID Or MIIM_STATE
            info.wID = id
            info.dwTypeData = menuItem.Text
        End If

        InsertMenuItem(hMenu, position, True, info)
    End Sub

    Private Shared Function CreatePremultipliedBitmap(source As Image) As IntPtr
        Dim w As Integer = source.Width
        Dim h As Integer = source.Height

        Dim bmpInfo As New BITMAPINFO()
        bmpInfo.bmiHeader.biSize = CUInt(Marshal.SizeOf(GetType(BITMAPINFOHEADER)))
        bmpInfo.bmiHeader.biWidth = w
        bmpInfo.bmiHeader.biHeight = -h
        bmpInfo.bmiHeader.biPlanes = 1
        bmpInfo.bmiHeader.biBitCount = 32
        bmpInfo.bmiHeader.biCompression = 0

        Dim ppvBits As IntPtr
        Dim hBitmap As IntPtr = CreateDIBSection(IntPtr.Zero, bmpInfo, 0UI, ppvBits, IntPtr.Zero, 0UI)
        If hBitmap = IntPtr.Zero Then Return IntPtr.Zero

        Using bmp As New Bitmap(w, h, PixelFormat.Format32bppArgb)
            Using g As Graphics = Graphics.FromImage(bmp)
                g.Clear(Color.Transparent)
                g.DrawImage(source, 0, 0, w, h)
            End Using

            Dim data As BitmapData = bmp.LockBits(New Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)

            Dim pixels(w * h * 4 - 1) As Byte
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length)
            bmp.UnlockBits(data)

            ' Windows wants premultiplied alpha for menu bitmaps.
            For i As Integer = 0 To pixels.Length - 1 Step 4
                Dim a As Integer = pixels(i + 3)
                pixels(i + 0) = CByte(pixels(i + 0) * a \ 255)
                pixels(i + 1) = CByte(pixels(i + 1) * a \ 255)
                pixels(i + 2) = CByte(pixels(i + 2) * a \ 255)
            Next

            Marshal.Copy(pixels, 0, ppvBits, pixels.Length)
        End Using

        Return hBitmap
    End Function

End Class
