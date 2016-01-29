﻿Imports SkyEditorBase

Namespace Projects
    Public Class SkyBackModProject
        Inherits GenericModProject

        Public Overrides Function GetFilesToCopy() As IEnumerable(Of String)
            Return {IO.Path.Combine("data", "BACK")}
        End Function

        Public Overrides Function GetSupportedGameCodes() As IEnumerable(Of String)
            Return {GameStrings.SkyCode}
        End Function

        Public Overrides Async Function Initialize(Solution As Solution) As Task
            Await MyBase.Initialize(Solution)

            Dim projectDir = GetRootDirectory()
            Dim sourceDir = GetRawFilesDir()

            Dim BACKdir As String = IO.Path.Combine(projectDir, "Backgrounds")
            Me.CreateDirectory("Backgrounds")
            Dim backFiles = IO.Directory.GetFiles(IO.Path.Combine(sourceDir, "Data", "BACK"), "*.bgp")
            Dim f As New Utilities.AsyncFor(PluginHelper.GetLanguageItem("Converting backgrounds..."))

            Await f.RunForEach(Async Function(Item As String) As Task
                                   Using b As New FileFormats.BGP(Item)
                                       Dim image = Await b.GetImage
                                       Dim newFilename = IO.Path.Combine(BACKdir, IO.Path.GetFileNameWithoutExtension(Item) & ".bmp")
                                       If Not IO.Directory.Exists(IO.Path.GetDirectoryName(newFilename)) Then
                                           IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(newFilename))
                                       End If
                                       image.Save(newFilename, Drawing.Imaging.ImageFormat.Bmp)
                                       IO.File.Copy(newFilename, newFilename & ".original")
                                       Await Me.AddExistingFile("Backgrounds", newFilename)
                                   End Using
                               End Function, backFiles)
        End Function

        Public Overrides Async Function Build(Solution As Solution) As Task
            'Convert BACK
            Dim projectDir = GetRootDirectory()
            Dim rawDir = GetRawFilesDir()
            If IO.Directory.Exists(IO.Path.Combine(projectDir, "Backgrounds")) Then
                For Each background In IO.Directory.GetFiles(IO.Path.Combine(projectDir, "Backgrounds"), "*.bmp")
                    Dim includeInPack As Boolean

                    If IO.File.Exists(background & ".original") Then
                        Using bmp As New IO.FileStream(background, IO.FileMode.Open)
                            Using orig As New IO.FileStream(background & ".original", IO.FileMode.Open)
                                Dim equal As Boolean = (bmp.Length = orig.Length)
                                While equal
                                    Dim b = bmp.ReadByte
                                    Dim o = orig.ReadByte
                                    equal = (b = o)
                                    If b = -1 OrElse o = -1 Then
                                        Exit While
                                    End If
                                End While
                                includeInPack = Not equal
                            End Using
                        End Using
                    Else
                        includeInPack = True
                    End If

                    If includeInPack Then
                        Dim bgp = FileFormats.BGP.ConvertFromBitmap(Drawing.Bitmap.FromFile(background))
                        bgp.Save(IO.Path.Combine(rawDir, "Data", "BACK", IO.Path.GetFileNameWithoutExtension(background) & ".bgp"))
                        bgp.Dispose()
                        Await FileFormats.BGP.RunCompress(IO.Path.Combine(rawDir, "Data", "BACK", IO.Path.GetFileNameWithoutExtension(background) & ".bgp"))
                    End If

                Next
            End If
            'Cleanup
            '-Data/Back/Decompressed
            If IO.Directory.Exists(IO.Path.Combine(rawDir, "Data", "BACK", "Decompressed")) Then
                IO.Directory.Delete(IO.Path.Combine(rawDir, "Data", "BACK", "Decompressed"), True)
            End If

            Await MyBase.Build(Solution)
        End Function
    End Class
End Namespace
