﻿Imports System.Windows
Imports SkyEditorBase

Namespace MenuActions
    Public Class ExtractFarc
        Inherits MenuAction
        Public Overrides Function SupportedTypes() As IEnumerable(Of Type)
            Return {GetType(FileFormats.FarcF5)}
        End Function
        Public Overrides Async Function DoAction(Targets As IEnumerable(Of Object)) As Task
            For Each target As FileFormats.FarcF5 In Targets
                Dim item = target
                Dim f As New SkyEditorBase.Utilities.AsyncFor(PluginHelper.GetLanguageItem("Extracting files..."))
                Await Utilities.FileSystem.ReCreateDirectory(IO.Path.Combine(Environment.CurrentDirectory, IO.Path.GetFileNameWithoutExtension(item.OriginalFilename)))
                Await target.Extract(IO.Path.Combine(Environment.CurrentDirectory, IO.Path.GetFileNameWithoutExtension(item.OriginalFilename)), True)
                'Await f.RunFor(Sub(count As Integer)
                '                   Dim filename As String = IO.Path.Combine(Environment.CurrentDirectory, IO.Path.GetFileNameWithoutExtension(item.OriginalFilename))
                '                   If Not IO.Directory.Exists(filename) Then
                '                       IO.Directory.CreateDirectory(filename)
                '                   End If
                '                   filename = IO.Path.Combine(filename, count)
                '                   IO.File.WriteAllBytes(filename, item.GetFileData(count))
                '               End Sub, 0, item.FileCount - 1, 1)
            Next
        End Function
        Public Sub New()
            MyBase.New("Dev/ExtractFarc", TranslateItems:=True)
            DevOnly = True
            SortOrder = 10.5
        End Sub
    End Class
End Namespace

