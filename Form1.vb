Imports System.IO
Imports System
Imports System.ComponentModel
Imports System.Threading

Public Class Form1

    ' v0.9.4 - 2016-1113
    ' This version reads backuptarget from SD_Backup_Move.exe.config
    ' Change: only .mxf files are moved to backup
    ' change: copy-process now runs as background thread via BackGroundWorker
    ' change: do a writeprotect-check before attempting to copy

    Public Sub New() ' should this method have a better name?

        ' This call is required by the designer.
        InitializeComponent()
        BackgroundWorker1.WorkerReportsProgress = True
        BackgroundWorker1.WorkerSupportsCancellation = True

    End Sub
    ' 

    Dim sourceDrive As String                       ' the drive that is assigned to the SD-card
    Dim sourcePath As String                        ' location of files&folders to move
    Dim sourcePathCopy As String                    ' location of files to copy to backup destination
    Dim sourceFileList As New List(Of String)       ' list of files to be copied from source
    Dim sourceFileListDest As New List(Of String)   ' list of files copied to backup destination

    Dim filesFound As Integer = 0                   ' number of confirmed files in dest dir

    Dim numberOfSourceFiles As Integer = 0          ' returns # of files in source \clip

    Dim destinationDrivePath As String = My.Settings.destinationDrivePath ' retrieve backup target from config-file
    Dim destinationPath As String ' complete path to backup-dest, including folder named with date&time


    Private Sub BackgroundWorker1_DoWork(sender As Object, e As ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Dim bw As BackgroundWorker = CType(sender, BackgroundWorker)  ' bw gets passed local status-updates from actual background-thread
        'Dim arg As Integer = e.Argument

        e.Result = CopyFilesToDestination(sourcePathCopy, destinationPath, bw)

    End Sub

    Private Sub BackgroundWorker1_ProgressChanged(sender As Object, e As ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged
        Dim a As String = e.ProgressPercentage.ToString

        CopyBegin.Visible = True             ' indicate copy-process as running in main-form via label CopyBegin
        CopyBegin.Text = "Copying to backup! " & CStr(a) & " / " & CStr(numberOfSourceFiles)
        Me.Refresh()

    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(sender As Object, e As ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        If e.Cancelled = True Then
            'resultLabel.Text = "Cancelled!" ' create new and find a better name for label
            ExitApplication()
        ElseIf e.Error IsNot Nothing Then
            'resultLabel.Text = "Error: " & e.Error.Message ' create new and find a better name for label
            ExitApplication()
        Else
            CopyBegin.Visible = False
            'MessageBox.Show("RunWorker completed here - moving on")

            MainWorker2() ' MainWorker2 handles copy-verification and move-operation
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        PopulateListBox()
    End Sub

    Private Sub SelectDrive_Click(sender As Object, e As EventArgs) Handles SelectDrive.Click   ' when user presses "START!"
        'If BackgroundWorker1.IsBusy <> True Then
        '    Me.BackgroundWorker1.RunWorkerAsync()
        'End If
        MainWorker1()
    End Sub

    Public Sub MainWorker1()
        ' part1 of primary work, ie. detecting sourcefiles & #sourcefiles, creating backupfolder 
        ' and sending copy-job to background thread.

        sourceDrive = Me.SelectSource.SelectedItem         ' user-selected item in combobox SelectSource
        sourcePath = Path.Combine(sourceDrive, "PRIVATE\XDROOT\")    ' Global var,  path element 1
        sourcePathCopy = Path.Combine(sourcePath, "Clip\")           ' Global var, path element 2; where source clips for copy sits

        sourceFileList = Me.GetFileList(sourcePathCopy, ".mxf")                     ' calls function while passing path of interest defined above
        ' GetFileList() sets the global var sourceFileList which is all files to be copied to backup

        numberOfSourceFiles = Me.GetNumberOfFiles(sourceFileList) ' calls function to determine number of files to be copied to backup-destination. 
        ' Returns the value to the global var numberOfSourceFiles

        If CheckWriteProtect() = True Then ' Calls function to verify if sourcedisk/movetarget is WriteProtected
            GoNoGO("Stopping Process - Disk is WriteProtected", 0, "WriteProtect!")
            ExitApplication()
        End If

        If numberOfSourceFiles > 0 Then
            ' only continue if files are actually found

            Me.CreateDestDir()                              ' calls function to create backup destination

            If BackgroundWorker1.IsBusy <> True Then
                Me.BackgroundWorker1.RunWorkerAsync()       ' calls CopyFilesToDestination via the event-handler BackgrounderWorker1.DoWork
                SelectDrive.Enabled = False   ' disable user from hitting START while copy is in progress
                destDisplay.Visible = True
                destDisplay.Text = "Backup to " & CStr(destinationDrivePath)
            End If

        Else
            ' No files to copy
            GoNoGO("Source directory is empty", 0, "No files in sourcedrive")
        End If
    End Sub

    Public Sub MainWorker2() ' to be called from BackgroundWorker_Completed
        ' part2 of primary job, started when background thread with copy-job terminates via BackGroundWorker1_DoWork_Completed
        ' this part verifies copy and asks user to accept moving files.

        If VerifyFilesOK((GetFileList(sourcePathCopy, ".mxf")), (GetFileList(destinationPath, ".mxf"))) = True Then
            ' function checks if same # files exists in dest as in source

            If GoNoGO(filesFound & " files copied." & vbCrLf & "Now Moving.", 1, "File backup success!") = 1 Then
                ' if user clicks OK, all files and folders found below sourcePath are moved

                If MoveFiles(sourcePath, sourceDrive) = True Then
                    ' if MoveFiles return True then inform user of success
                    GoNoGO("All files moved." & vbCrLf & "Drive ready for Ingest.", 0, "Move Complete.")
                    ExitApplication()
                Else
                    ' if MoveFile return False the application is closed immediatly
                    ExitApplication()
                End If

            Else
                ' the user cancelled the move.
                GoNoGO("MOVE cancelled by user.", 0, "Move Cancelled")
                ExitApplication()
            End If

        Else
            ' VerifyFilesOK returns False and user is informed
            GoNoGO("Errors during copy. Stopping process!", 0, "Copy not Complete!")
            ExitApplication()
        End If



    End Sub

    Private Sub BeginOperation_Click(sender As Object, e As EventArgs) Handles BeginOperation.Click
        ' user clicked "Refresh Drives"-button 
        PopulateListBox()
    End Sub

    Public Sub PopulateListBox()
        ListDrives.Items.Clear() 'sub populates listbox ListDrives with enabled drives on host

        Dim driveNames As List(Of String)
        Dim driveLabel As DriveInfo
        Dim driveLabelString As String = ""

        driveNames = getAvailableDriveLetters()   ' call function to get all drives on host

        For Each driveName In driveNames
            driveLabel = My.Computer.FileSystem.GetDriveInfo(driveName)
            driveLabelString = CStr(driveLabel.VolumeLabel)
            ListDrives.Items.Add(driveName & vbTab & driveLabelString)   ' populate listbox with drivenames and Volumenames
        Next

        PopulateAcceptedDrives(driveNames)
        ' call function to populate combobox SelectSource 
        ' with plausible source-drices
    End Sub

    Public Function getAvailableDriveLetters() As List(Of String)
        Dim driveLetters As New List(Of String)

        For Each drive As DriveInfo In DriveInfo.GetDrives()   ' Get all drives on host
            If drive.IsReady Then                              ' verify it's ready
                driveLetters.Add(drive.Name)                   ' populate list
            End If
        Next

        Return driveLetters
    End Function

    Private Sub PopulateAcceptedDrives(AvailableDrives As List(Of String))
        ' sub populates combobox SelectSource with validated drives
        SelectSource.Items.Clear()

        For Each drive In AvailableDrives
            If ValidateSourceDrive(drive) = True Then      ' calls function to check if drive is plausible
                SelectSource.Items.Add(drive)              ' then add drive to list
            End If
        Next

        If SelectSource.Items.Count = 0 Then    ' if no plausible drive found on host
            GoNoGO("No meaningfull drive found." & vbCrLf & "Exiting.", 0, "No Files Found!")
            ExitApplication()
        End If

    End Sub

    Private Function ValidateSourceDrive(drive) As Boolean ' validation consist of finding expected file.
        Dim checkPath As String = drive & "PRIVATE\XDROOT\MEDIAPRO.XML"  ' location and name of expected file. 
        'The second part of path should be derived from My.Settings !!!!

        If File.Exists(checkPath) Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Function GetFileList(ByVal searchPath As String, ByVal extName As String)  ' path to search in, extension to filter for

        Dim FileList As String() = Directory.GetFiles(searchPath)
        Dim ListOfFoundFiles As New List(Of String)
        Dim fileName As String

        For Each fileName In FileList
            If UCase((Path.GetExtension(fileName))) = UCase(extName) Then
                ListOfFoundFiles.Add(fileName)
            End If
        Next

        Return ListOfFoundFiles    ' returns list of matched files
    End Function

    Private Function CreateDestDir()   ' create dir for backup based on date+time

        Dim time As DateTime
        Dim timeString As String
        Dim timeformat1 As String = "yyyyMMdd"
        Dim timeformat2 As String = "HHmmss"
        time = Now

        timeString = CStr(time.ToString(timeformat1)) & "_" & CStr(time.ToString(timeformat2))
        destinationPath = destinationDrivePath & timeString & "\"

        If Not Directory.Exists(destinationPath) Then
            Directory.CreateDirectory(destinationPath)
        End If

        Return destinationPath
    End Function

    Private Function CopyFilesToDestination(ByVal sourcePathCopy As String, ByVal destinationPath As String, _bw As BackgroundWorker) As Boolean
        ' sub copies source to backup - 
        ' this function is called from BackgroundWorker.DoWork, event RunWorkerAsync raised in MainWorker

        Try
            If Not _bw.CancellationPending Then

                Dim file As String
                Dim readFile As String
                Dim writeFile As String
                Dim a As Integer = 0

                'MessageBox.Show("count of sourceFileList: " & sourceFileList.Count)
                For Each file In sourceFileList      ' iterate through all files to copy
                    If Not _bw.CancellationPending Then   ' to be used when/if btn EXIT permits user to abort copy
                        readFile = sourcePathCopy & Path.GetFileName(file)
                        writeFile = destinationPath & Path.GetFileName(file)

                        'MessageBox.Show("readfile: " & readFile & vbCrLf & "writeFile: " & writeFile)

                        My.Computer.FileSystem.CopyFile(readFile, writeFile)
                        a = a + 1
                        _bw.ReportProgress(a) ' updates progress via event-call to BackgroundWorker_ProgressChanged
                    End If
                Next
            End If
            Return True
        Catch ex As Exception
            MessageBox.Show("An error Occurred." & vbCrLf & ex.Message.ToString)
            Return False
        End Try
    End Function

    Private Function VerifyFilesOK(ByVal sourceFileList As List(Of String), ByVal destFileList As List(Of String)) As Boolean
        ' compare source and destination files for backup state

        Dim destFile As String
        Dim sourceFile As String

        If numberOfSourceFiles > 0 Then              ' global var; if sourcefiles exists then compare source/dest

            For Each destFile In destFileList
                'MessageBox.Show("VerifyFiles - destfiles: " & destFile)

                For Each sourceFile In sourceFileList
                    'MessageBox.Show("VerifyFiles - sourcefiles: " & sourceFile)

                    If String.Compare(Path.GetFileName(destFile), Path.GetFileName(sourceFile)) Then
                        filesFound = filesFound + 1 ' Public Variable - # of clips copied OK - this could be a public event instead?
                        Exit For
                    End If
                Next
            Next
        End If

        If filesFound = numberOfSourceFiles Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Function MoveFiles(source, destination) As Boolean

        ' First, move all files in source, ie. /PRIVATE/XDROOT
        ' Next, move alle directories and their files to destination

        Dim moveState As Boolean = False
        Dim rootFiles As String() = Directory.GetFiles(source)
        Dim rootFolders As String() = Directory.GetDirectories(source)

        Dim NewFolderDest As String
        Dim CreateDir As String             ' used to create destination dir for directory move
        Dim destinationShort As String      ' workaround for dealing with "\" in path. Needs fixing!

        Try

            For Each element In rootFiles       ' first, move all files
                My.Computer.FileSystem.MoveFile(element, destination & "\" & Path.GetFileName(element), True)
            Next

            For Each element In rootFolders     'then, move all folders with their files
                NewFolderDest = Strings.Replace(element, Path.GetDirectoryName(element), "")
                destinationShort = Strings.Left(destination, (destination.length - 1))
                CreateDir = destinationShort & NewFolderDest    ' required because .MoveDirectory needs destination dir

                'MessageBox.Show("folder element: " & CStr(element) & vbCrLf _
                '                  & "folder destination: " & Path.GetDirectoryName(element) & vbCrLf _
                '                  & "New Destination: " & NewFolderDest & vbCrLf _
                '                  & "CreateDir: " & CreateDir)
                My.Computer.FileSystem.MoveDirectory(element, CreateDir, True)
            Next
            moveState = True

        Catch errorState As Exception
            MessageBox.Show("Could not move. Media is locked?" & vbCrLf & "Error: " & CStr(errorState.Message))
            moveState = False
        End Try
        Return moveState
    End Function

    Private Function GoNoGO(ByVal message As String, ByVal boxStyle As Integer, ByVal title As String)
        ' function receives message and title and returns user-selection, boxStyle defines button-options
        Dim UserInput As MsgBoxResult
        Dim style As MsgBoxStyle

        style = MsgBoxStyle.OkOnly

        Select Case boxStyle
            Case 0
                style = MsgBoxStyle.OkOnly
            Case 1
                style = MsgBoxStyle.OkCancel
            Case 3
                style = MsgBoxStyle.YesNoCancel
            Case 4
                style = MsgBoxStyle.YesNo
            Case 5
                style = MsgBoxStyle.RetryCancel
            Case Else
                style = MsgBoxStyle.OkOnly
        End Select

        UserInput = MsgBox(message, style, title)
        Return UserInput
    End Function

    Private Sub SelectSource_SelectedIndexChanged(sender As Object, e As EventArgs) Handles SelectSource.SelectedIndexChanged
        SetEnabled()    ' calls function to enable "START!" when suitable drive selected by user from combobox
    End Sub

    Private Function GetNumberOfFiles(ByVal list As List(Of String)) As Integer
        Dim number As Integer = list.Count      'Public variable - returns # of files in source \clip.
        Return number
    End Function

    Private Sub SetEnabled()    ' used to disable "START" button until suitable drive selected by user
        Dim anySelected As Boolean = (SelectSource.SelectedItem IsNot Nothing)
        SelectDrive.Enabled = anySelected    ' SelectDrive is called "START!" in GUI/Form
    End Sub

    Private Function CheckWriteProtect() As Boolean ' function to verify if sourcedisk/movetarget is WriteProtect
        ' write directory test file to drive-root - catch error if fail and return TRUE if locked
        Dim TestDir As String
        TestDir = sourceDrive & "TEST"

        Try
            If Not Directory.Exists(TestDir) Then
                Directory.CreateDirectory(TestDir)   ' try to create dir TEST in move-target
                Directory.Delete(TestDir)
            End If
            Return False                            ' dir TEST succesfully created+deleted. Return FALSE for succces
        Catch ex As Exception
            Return True                             ' if write failed, then return TRUE
        End Try
    End Function

    Private Sub CloseApplication_Click(sender As Object, e As EventArgs) Handles CloseApplication.Click
        ' calls function to close app, when user clicks "exit" in form
        ExitApplication()
    End Sub

    Private Sub ExitApplication()
        ' exit application by closing form
        Me.Close()
    End Sub




    'Documentation & functional overview:

    'Purpose: to facilitate ingest via SONAPS Ingest Terminal. 
    'First, backup files from SD card generated with X70-camera to local folder (named by date+time-stamp)
    'When backup is complete and validated, the requested files+folders are moved to root of drive
    'When operation is completed OK, the drive can be ingested via Sonaps Ingest Terminal.


    'Functional flow:

    'Get available drives				

    'filter list Of drives To SD-cards			allow only selection Of :
    '                                           folder-structure confirmed  as expected:
    '						                    locate MEDIAPRO.XML In Private/xdroot

    'request user-input: Select source-drive	show avail drives

    'verify user-selection, 				    confirm choice by user (Yes/No)

    'create filelist Of source /clip

    'Get current Date+time
    'create destination folder, named As Date+time

    'copy all files+folders from source-drive\Private\root To destination folder

    'confirm copy complete				        compare filelist between source and destination

    'move all files+folder from below source-drive\Private\root To source-drive\[root]

    'Exit



    'Global variables:

    'sourceDrive		source-drive Of copy
    'destinationDrive	destination-drive Of copy
    'sourcePath		    fully qualified path To source directory
    'destinationPath	fully qualified path To destination directory
    'sourceFileList		list Of sourcefiles To be copied
    'sourceFileListDest	list Of files at destination
    'filesFound		    number Of files succesfully files copied To backup
    'numberOfSourceFiles number of files to copy to backup 


    'Form-objects
    'ListDrives		    ListBox: shows content Of availDrives
    'SelectSource		Combobox: displays acceptedDrives
    'SelectDrive		Button: user choice Of source drive, labeled as "START!"
    'BeginOperation	    Button: start process Of listing available drives, labeled as "Find Drives"
    'CopyBegin		    Label, visible When copy To backup Is running



    'Functions:

    'GetFileList(path)		    List of sourcefiles
    '	Return :  List

    'CreateDestDir			    Create destination dir based On dateTime
    '	Return :  Path

    'VerifyFilesOK			    compare source And dest dir
    '	Return : Boolean

    'GetFileListDestination		list Of files at destination
    '	Return :  List

    'MoveFiles			        moves all files On SD To [root]
    '   Return :  Boolean (on succes/fail)

    'CheckWriteProtect()        Check if move-target is writeprotected
    '   Return:   Boolean (False=not writeprotect, True=writeprotected)



    'Subs:

    'CopyFilesToDest(path, path)	Copy all source files to destination

    'ExitApplication            closes Form1, and thus exits application.

End Class
