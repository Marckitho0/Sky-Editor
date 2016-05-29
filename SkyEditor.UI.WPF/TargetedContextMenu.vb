﻿Imports System.Windows
Imports System.Windows.Controls
Imports SkyEditor.Core
Imports SkyEditor.Core.UI
Imports SkyEditor.Core.Settings

Public Class TargetedContextMenu
    Inherits ContextMenu

    Public Shared ReadOnly CurrentPluginManagerProperty As DependencyProperty = DependencyProperty.Register(NameOf(CurrentPluginManager), GetType(PluginManager), GetType(TargetedContextMenu), New FrameworkPropertyMetadata(AddressOf OnCurrentPluginManagerChanged))
    Public Shared ReadOnly ObjectToEditProperty As DependencyProperty = DependencyProperty.Register(NameOf(Target), GetType(Object), GetType(TargetedContextMenu), New FrameworkPropertyMetadata(AddressOf OnTargetChanged))
    Private Shared Sub OnCurrentPluginManagerChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        DirectCast(d, ObjectControlPlaceholder).CurrentPluginManager = e.NewValue
    End Sub
    Private Shared Sub OnTargetChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        DirectCast(d, TargetedContextMenu).Target = e.NewValue
    End Sub

    Public Property Target As Object
        Get
            Return _target
        End Get
        Set(value As Object)
            _target = value
            Me.Items.Clear()
            For Each item In WPFUiHelper.GenerateMenuItems(UIHelper.GetContextMenuItemInfo(_target, CurrentPluginManager, CurrentPluginManager.CurrentSettingsProvider.GetIsDevMode), CurrentPluginManager)
                Me.Items.Add(item)
            Next
            'Todo: something to register event handler
        End Set
    End Property
    Dim _target As Object

    Public Property CurrentPluginManager As PluginManager

End Class
