﻿Imports System.Web.Script.Serialization

Public Class SdfSave
    Public Class SdfSaveExtraData
        Public Property Name As String
        Public Property Notes As String
        Public Sub New()
            Name = ""
            Notes = ""
        End Sub
    End Class
    Public Property Info As SdfSaveExtraData
    Public Property ExportDate As Date
    Public Property MiniTitleId As String
    Public Property Path As String
    Public Property IsValid As Boolean

    Public Sub CopyTo(Directory As SdfSaveDataDirectory)
        Dim dirName As String = ExportDate.ToString("yyyyMMddhhmmss")
        Dim dir As String = IO.Path.Combine(Directory.Path, dirName)
        If Not IO.Directory.Exists(dir) Then
            IO.Directory.CreateDirectory(dir)
        End If

        If Not IO.Directory.Exists(IO.Path.Combine(dir, MiniTitleId)) Then
            IO.Directory.CreateDirectory(IO.Path.Combine(dir, MiniTitleId))
        End If

        IO.File.Copy(IO.Path.Combine(Me.Path, MiniTitleId & ".dat"), IO.Path.Combine(dir, MiniTitleId & ".dat"), True)
        IO.File.Copy(IO.Path.Combine(Me.Path, MiniTitleId & "_.dat"), IO.Path.Combine(dir, MiniTitleId & "_.dat"), True)
        IO.File.Copy(IO.Path.Combine(Me.Path, "export.log"), IO.Path.Combine(dir, "export.log"), True)

        For Each item In IO.Directory.GetFiles(IO.Path.Combine(Me.Path, MiniTitleId), "*", IO.SearchOption.TopDirectoryOnly)
            IO.File.Copy(item, IO.Path.Combine(dir, MiniTitleId, IO.Path.GetFileName(item)), True)
        Next

        Dim j As New JavaScriptSerializer
        IO.File.WriteAllText(IO.Path.Combine(dir, "info.json"), j.Serialize(Info))
    End Sub

    Public Sub New(Path As String)
        Me.Path = Path
        Me.Info = New SdfSaveExtraData
        Dim dirName As String = IO.Path.GetFileNameWithoutExtension(Path)
        'Todo: see if Date.Parse works instead of the regex
        '                                                     Y   Y     Y   Y      M    M       D   D       H   H       m   m       s    s
        Dim dirRegex As New Text.RegularExpressions.Regex("([0-9][0-9][0-9][0-9])([0-9][0-9])([0-9][0-9])([0-9][0-9])([0-9][0-9])([0-9][0-9])")
        Dim m = dirRegex.Match(dirName)
        If m.Success Then
            Dim year As Integer = m.Groups(1).Value
            Dim month As Integer = m.Groups(2).Value
            Dim day As Integer = m.Groups(3).Value
            Dim hour As Integer = m.Groups(4).Value
            Dim minute As Integer = m.Groups(5).Value
            Dim second As Integer = m.Groups(6).Value
            Dim exportDate = New Date(year, month, day, hour, minute, second)
            Me.ExportDate = exportDate

            'get mini title id
            Dim dirs = IO.Directory.GetDirectories(Path, "*", IO.SearchOption.TopDirectoryOnly)
            If dirs.Any Then
                MiniTitleId = IO.Path.GetFileNameWithoutExtension(dirs.First)
                IsValid = True
            Else
                IsValid = False
            End If

            If IO.File.Exists(IO.Path.Combine(Path, "info.json")) Then
                Dim j As New JavaScriptSerializer
                Info = j.Deserialize(Of SdfSaveExtraData)(IO.File.ReadAllText(IO.Path.Combine(Path, "info.json")))
            End If
        Else
            IsValid = False
        End If
    End Sub
End Class