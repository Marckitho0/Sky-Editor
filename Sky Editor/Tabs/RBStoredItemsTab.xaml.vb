﻿Imports SkyEditorBase
Namespace Tabs
    Public Class RBStoredItemsTab
        Inherits EditorTab
        Public Overrides Sub RefreshDisplay(Save As GenericSave)
            'debug stored items
            If Save.IsOfType(GameStrings.RBSave) Then
                Dim x = Save.Convert(Of Saves.RBSave).StoredItemCounts
                For count As Integer = 0 To 238
                    If x(count) > 0 Then
                        txtDisplay.Text &= Lists.RBItemNames(count + 1) & ": " & x(count) & vbCrLf
                    End If
                Next
            End If
        End Sub

        Public Overrides ReadOnly Property SupportedGames As String()
            Get
                Return {GameStrings.RBSave}
            End Get
        End Property

        Public Overrides Function UpdateSave(Save As GenericSave) As GenericSave
            Return Save
        End Function

        Private Sub RBStoredItemsTab_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            Me.Header = PluginHelper.GetLanguageItem("Stored Items")
        End Sub
        Public Overrides ReadOnly Property SortOrder As Integer
            Get
                Return 24
            End Get
        End Property
    End Class
End Namespace