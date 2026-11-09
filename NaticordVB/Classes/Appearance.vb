''' <summary>Shared appearance settings: layout mode names and the icon style.</summary>
Friend Module Appearance

    ' Values stored in My.Settings.layoutMode
    Friend Const LayoutWindows11 As String = "Windows 11 - 10"
    Friend Const LayoutWindows81 As String = "Windows 8.1 - 7"

    ' Values stored in My.Settings.iconStyle
    Friend Const IconStyleSkeuomorphic As String = "Skeuomorphic"

    ''' <summary>"-skeuo" when the skeuomorphic icon set is selected, otherwise empty.</summary>
    Friend ReadOnly Property IconSuffix As String
        Get
            Return If(String.Equals(My.Settings.iconStyle, IconStyleSkeuomorphic, StringComparison.OrdinalIgnoreCase), "-skeuo", "")
        End Get
    End Property

End Module
