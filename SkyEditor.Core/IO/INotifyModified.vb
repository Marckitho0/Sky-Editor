﻿Namespace IO
    ''' <summary>
    ''' Marks an object that supports raising an event when modified.
    ''' </summary>
    <Obsolete> Public Interface INotifyModified
        Event Modified(sender As Object, e As EventArgs)
        Sub RaiseModified()
    End Interface

End Namespace
