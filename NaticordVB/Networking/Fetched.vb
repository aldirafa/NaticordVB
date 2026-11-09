Namespace Networking

    ''' <summary>A parsed value together with the request outcome it came from. <see cref="Value"/> is only meaningful when <see cref="IsSuccess"/>.</summary>
    Friend NotInheritable Class Fetched(Of T)

        Public ReadOnly Property Result As ApiResult
        Public ReadOnly Property Value As T

        Public ReadOnly Property IsSuccess As Boolean
            Get
                Return Result.IsSuccess
            End Get
        End Property

        Private Sub New(result As ApiResult, value As T)
            Me.Result = result
            Me.Value = value
        End Sub

        Friend Shared Function Ok(result As ApiResult, value As T) As Fetched(Of T)
            Return New Fetched(Of T)(result, value)
        End Function

        Friend Shared Function Fail(result As ApiResult) As Fetched(Of T)
            Return New Fetched(Of T)(result, Nothing)
        End Function

        ''' <summary>A short, user-facing reason for a failure.</summary>
        Public ReadOnly Property FriendlyError As String
            Get
                Select Case Result.Error
                    Case ApiError.Offline, ApiError.Timeout : Return "Can't reach Discord."
                    Case ApiError.Unauthorized : Return "Your session has expired. Sign in again."
                    Case ApiError.Forbidden : Return "You don't have access to this."
                    Case ApiError.RateLimited : Return "Discord is asking us to slow down."
                    Case ApiError.Cancelled : Return "Cancelled."
                    Case Else : Return "Something went wrong (" & Result.ErrorMessage & ")."
                End Select
            End Get
        End Property

    End Class

End Namespace
