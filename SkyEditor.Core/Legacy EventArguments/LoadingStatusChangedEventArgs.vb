﻿Namespace EventArguments
    Public Class LoadingStatusChangedEventArgs
        Inherits EventArgs
        Public Property Complete As Boolean
        Public Property Progress As Single
        Public Property Message As String
        Public Property Completed As Integer
        Public Property Total As Integer
    End Class
End Namespace
