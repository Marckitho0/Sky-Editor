﻿Imports SkyEditor.Core
Imports SkyEditor.Core.Interfaces
Imports SkyEditor.Core.IO
Imports SkyEditor.Core.Windows
''' <summary>
''' Most plugins need to call registration methods on load.  Sky Editor Base is no exception.  This class contains methods like the ones found in plugin definitions, without actually being its own plugin.
''' </summary>
Friend Class ConsoleCoreMod
    Inherits WindowsCoreSkyEditorPlugin

    Public Overrides ReadOnly Property Credits As String
        Get
            Return ""
        End Get
    End Property

    Public Overrides ReadOnly Property PluginAuthor As String
        Get
            Return ""
        End Get
    End Property

    Public Overrides ReadOnly Property PluginName As String
        Get
            Return My.Resources.Language.ConsolePluginName
        End Get
    End Property

    Public Overrides Sub UnLoad(Manager As SkyEditor.Core.PluginManager)

    End Sub

    Public Overrides Sub PrepareForDistribution()

    End Sub

    Public Overrides Function GetSettingsProvider(manager As SkyEditor.Core.PluginManager) As ISettingsProvider
        Return SettingsProvider.Open(EnvironmentPaths.GetSettingsFilename, manager)
    End Function

    Public Overrides Function GetExtensionDirectory() As String
        Return EnvironmentPaths.GetExtensionDirectory
    End Function
End Class
