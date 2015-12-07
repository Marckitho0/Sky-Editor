﻿Imports System.Web.Script.Serialization
Imports DS_ROM_Patcher

Public Class ThreeDSPatcherCore
    Inherits PatcherCore

    Public Overrides Sub PromptFilePath()
        Dim o As New OpenFileDialog
        o.Filter = "Supported Files (*.3ds;*.3dz;romfs.bin)|*.3ds;*.3dz;romfs.bin|3DS DS Roms (*.3ds;*.3dz)|*.3ds;*.3dz|Braindump romfs (romfs.bin)|romfs.bin|All Files (*.*)|*.*"
        If o.ShowDialog = DialogResult.OK Then
            SelectedFilename = o.FileName
        End If
    End Sub

    Public Overrides Async Function RunPatch(Mods As IEnumerable(Of ModJson), Optional DestinationPath As String = Nothing) As Task
        Dim currentDirectory = Environment.CurrentDirectory
        Dim ROMDirectory = IO.Path.Combine(currentDirectory, "Tools/ndstemp")
        Dim modTempDirectory = IO.Path.Combine(currentDirectory, "Tools/modstemp")

        Dim hansMode As Boolean
        If IO.Path.GetFileName(SelectedFilename).ToLower = "romfs.bin" Then
            hansMode = True
        Else
            hansMode = False
        End If

        'Extract the NDS ROM
        RaiseProgressChanged(0, "Extracting the 3DS ROM...")
        If Not IO.Directory.Exists(ROMDirectory) Then
            IO.Directory.CreateDirectory(ROMDirectory)
        End If

        If hansMode Then
            Dim exefsPath = IO.Path.Combine(ROMDirectory, "DecryptedExeFS.bin")
            Dim romfsPath = IO.Path.Combine(ROMDirectory, "DecryptedRomFS.bin")
            Dim romfsDir = IO.Path.Combine(ROMDirectory, "romfs")
            Dim exefsDir = IO.Path.Combine(ROMDirectory, "exefs")

            If Not IO.Directory.Exists(ROMDirectory) Then
                IO.Directory.CreateDirectory(ROMDirectory)
            End If

            IO.File.Copy(SelectedFilename, romfsPath, True)

            'Unpack exefs
            Dim exefsSource As String = IO.Path.Combine(IO.Path.GetDirectoryName(SelectedFilename), "exefs.bin")
            If IO.File.Exists(exefsSource) Then
                IO.File.Copy(exefsSource, exefsPath)

                If Not IO.Directory.Exists(exefsDir) Then
                    IO.Directory.CreateDirectory(exefsDir)
                End If
                Await ProcessHelper.RunCTRTool($"-t exefs --exefsdir=""{exefsDir}"" ""{exefsPath}"" --decompresscode")
            End If

            'Unpack romfs
            If Not IO.Directory.Exists(romfsDir) Then
                IO.Directory.CreateDirectory(romfsDir)
            End If

            Await ProcessHelper.RunCTRTool($"-t romfs --romfsdir=""{romfsDir}"" ""{romfsPath}""")
        Else
            'We're dealing with a .3DS file
            Dim exHeaderPath = IO.Path.Combine(ROMDirectory, "DecryptedExHeader.bin")
            Dim exefsPath = IO.Path.Combine(ROMDirectory, "DecryptedExeFS.bin")
            Dim romfsPath = IO.Path.Combine(ROMDirectory, "DecryptedRomFS.bin")
            Dim romfsDir = IO.Path.Combine(ROMDirectory, "romfs")
            Dim exefsDir = IO.Path.Combine(ROMDirectory, "exefs")
            'Unpack portions
            Await ProcessHelper.RunCTRTool($"-p --exheader=""{exHeaderPath}"" ""{SelectedFilename}""")
            Await ProcessHelper.RunCTRTool($"-p --exefs=""{exefsPath}"" ""{SelectedFilename}""")
            Await ProcessHelper.RunCTRTool($"-p --romfs=""{romfsPath}"" ""{SelectedFilename}""")
            'Unpack romfs
            Await ProcessHelper.RunCTRTool($"-t romfs --romfsdir=""{romfsDir}"" ""{romfsPath}""")
            'Unpack exefs
            Await ProcessHelper.RunCTRTool($"-t exefs --exefsdir=""{exefsDir}"" ""{exefsPath}"" --decompresscode")
            IO.File.Delete(exefsPath)
            IO.File.Delete(romfsPath)
        End If

        'Apply the Mods
        Const RepackMessage As String = "Repacking the ROM..."
        RaiseProgressChanged(1 / 3, RepackMessage)

        Dim j As New JavaScriptSerializer
        Dim patchers = j.Deserialize(Of List(Of FilePatcher))(IO.File.ReadAllText(IO.Path.Combine(currentDirectory, "Tools", "patchers.json")))
        Dim modFiles As New List(Of ModFile)
        For Each item In Mods
            modFiles.Add(New ModFile(item.Filename))
        Next

        For Each item In modFiles
            Await ModFile.ApplyPatch(modFiles, item, currentDirectory, ROMDirectory, patchers)
        Next

        'Repack the ROM
        RaiseProgressChanged(2 / 3, "Repacking the ROM...")

        Dim destination As String = Nothing

        If Not hansMode AndAlso DestinationPath Is Nothing Then
            If MessageBox.Show("Would you like to output to HANS?  (Say no to output to .3DS or .CIA)", "DS ROM Patcher", MessageBoxButtons.YesNo) Then
                hansMode = True
            End If
        End If

        If hansMode Then
            If DestinationPath Is Nothing Then
                Dim d As New FolderBrowserDialog
                d.Description = "Please select a folder to export romfs.bin and exefs.bin" '"Please select your Hans folder.  (Should be SD:/Hans)"
ShowFolderDialog3DS: If d.ShowDialog = DialogResult.OK Then
                    destination = d.SelectedPath
                Else
                    If MessageBox.Show("Are you sure you want to cancel the patching process?", "DS ROM Patcher", MessageBoxButtons.YesNo) = DialogResult.No Then
                        GoTo ShowSaveDialog3DS
                    End If
                End If
            Else
                destination = DestinationPath
            End If

            If destination IsNot Nothing Then
                Dim romfsDir = IO.Path.Combine(ROMDirectory, "romfs")
                Dim romfsPath = IO.Path.Combine(ROMDirectory, "romfsRepacked.bin")
                Dim romfsTrimmedPath = IO.Path.Combine(ROMDirectory, "romfsRepacked.bin")

                'Repack romfs
                'TODO: run tool to repack romfs
                Throw New NotImplementedException("Repacking the romfs is currently not implemented.")

                'Trim the first part of the romfs
                Using source As New IO.FileStream(romfsPath, IO.FileMode.Open, IO.FileAccess.ReadWrite)
                    Using dest As New IO.FileStream(romfsTrimmedPath, IO.FileMode.OpenOrCreate, IO.FileAccess.ReadWrite)
                        dest.SetLength(source.Length - &HFF0)
                        source.Seek(&HFF0, IO.SeekOrigin.Begin)
                        For i = 0 To dest.Length - 1
                            dest.WriteByte(source.ReadByte)
                            source.Seek(1, IO.SeekOrigin.Current)
                            dest.Seek(1, IO.SeekOrigin.Current)
                        Next
                        dest.Flush()
                    End Using
                End Using

                IO.File.Copy(romfsTrimmedPath, IO.Path.Combine(destination, "romfs.bin"), True)
                'TODO: copy code.bin

                'TODO: make Hans integration nicer
            Else
                RaiseProgressChanged(1, "Patching canceled by user")
            End If

        Else
            'Choose an output file
            If DestinationPath Is Nothing Then
                Dim s As New SaveFileDialog
                s.Filter = "3DS Files (*.3ds)|*.3ds|All Files (*.*)|*.*"
ShowSaveDialog3DS: If s.ShowDialog = DialogResult.OK Then
                    destination = s.FileName
                Else
                    If MessageBox.Show("Are you sure you want to cancel the patching process?", "DS ROM Patcher", MessageBoxButtons.YesNo) = DialogResult.No Then
                        GoTo ShowSaveDialog3DS
                    End If
                End If
            Else
                destination = DestinationPath
            End If

            'If output file chosen, repack then copy
            If destination IsNot Nothing Then
                If destination.ToLower.EndsWith(".cia") Then
                    'Todo: output to cia
                    MessageBox.Show("Cia conversion is currently not supported.  Exporting as a .3DS file.  It is recommended to rename the output file once conversion is complete.")
                End If
                'Else
                'Output to .3DS
                Dim exeFS As String = IO.Path.Combine(ROMDirectory, "exefs")
                Dim romFS As String = IO.Path.Combine(ROMDirectory, "romfs")
                Dim exHeader As String = IO.Path.Combine(ROMDirectory, "DecryptedExHeader.bin")
                Dim output As String = IO.Path.Combine(currentDirectory, "PatchedROM.3ds")
                Await ProcessHelper.RunProgram(IO.Path.Combine(currentDirectory, "Tools/3DS Builder.exe"),
                                     $"""{exeFS}"" ""{romFS}"" ""{exHeader}"" ""{output}""")
                IO.File.Copy(IO.Path.Combine(currentDirectory, "PatchedROM.3ds"), destination, True)
                'End If
                RaiseProgressChanged(1, "Ready")
            Else
                RaiseProgressChanged(1, "Patching canceled by user")
            End If
        End If

        'Cleanup
        If IO.Directory.Exists(ROMDirectory) Then IO.Directory.Delete(ROMDirectory, True)
        If IO.File.Exists(IO.Path.Combine(currentDirectory, "PatchedROM.3ds")) Then IO.File.Delete(IO.Path.Combine(currentDirectory, "PatchedROM.3ds"))
    End Function

    Public Overrides Function SupportsMod(ModToCheck As ModJson) As Boolean
        Return True
    End Function
End Class