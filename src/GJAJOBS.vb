Imports config = System.Configuration.ConfigurationManager
Imports System.IO
Imports System.Linq

Module GJAJOBS

    Private _strLocalHost As String
    Private _strLogFile As String
    Private _reportsFolder As String

    '
    ' This is the GJAJOBS module, version 8.5.2
    '
    ' This program replaces baseline GJAJOBS. It simply writes an entry in the jobsub table
    ' indicating a job was sent, rather than initiating the running of said job.
    '
    '

    Sub Main()

        Try
            _reportsFolder = config.AppSettings.Get("ReportsFolder")
            _strLocalHost = Net.Dns.GetHostName.ToUpper

            If Not Directory.Exists(config.AppSettings.Get("LogFolder")) Then
                Directory.CreateDirectory(config.AppSettings.Get("LogFolder"))
            End If

            _strLogFile = String.Format("{0}\{1}{2}{3}", config.AppSettings.Get("LogFolder"), config.AppSettings.Get("LogFilePrefix"), Format(Now, "yyyyMMdd"), config.AppSettings.Get("LogFileExtensions"))
        
            Main_Oracle()
            Main_Access()

        Catch ex As Exception
            Console.WriteLine("GJAJOBS Encountered the error : " & ex.Message)

        End Try

    End Sub

    Sub Main_Access()
        Dim adoConfig As New ADODB.Connection
        Dim adoGJAJOBS As New ADODB.Recordset

        Try
            With adoConfig
                .ConnectionString = config.AppSettings.Get("AccessConfigurationDatabase")
                .Open()
                With adoGJAJOBS
                    .Open("Select * from GJAJOBS", adoConfig, ADODB.CursorTypeEnum.adOpenForwardOnly, ADODB.LockTypeEnum.adLockOptimistic)
                    .Fields("Activity").Value = Now
                    .Update()
                    .Close()
                End With
                .Close()
            End With

        Catch ex As Exception

            If ex.Message.Contains("attempting to change the same data at the same time") Then
                ' no one cares. We're just updating an access flag and it appears someone else has done that for us.
            Else
                Put_Log("(Update Access) " & ex.Message)
            End If

        End Try

    End Sub

    Sub Put_Log(ByVal msg As String)
        Dim ctr As Integer

        Do Until ctr > 10
            Try
                File.AppendAllText(_strLogFile, Format(Now, "yyyyMMdd HH:mm:ss") & " GJAJOBS " & msg & vbCrLf)
                ctr = 99
            Catch ex As Exception
                ctr += 1
                Threading.Thread.Sleep(10)
            End Try
        Loop
    End Sub

    Sub Main_Oracle()
        Dim adoDB As ADODB.Connection
        Dim adoRS As ADODB.Recordset
        Dim sql As String
        Dim strlSID As String
        Dim strlService As String
        Dim strlP1 As String
        Dim strlP2 As String
        Dim strlP3 As String
        Dim strlP4 As String
        Dim strlP5 As String
        Dim strlP6 As String
        Dim strlP7 As String
        Dim strlP8 As String
        Dim strlP9 As String
        Dim strlMsg As String
        Dim strflogFile() As String
        Dim l As Integer
        Dim strlStep As String
        Dim boolRewrite As Boolean
        Dim strlFiles() As String

        Dim action As String

        strlStep = "Nuttin'"

        Try

            strlStep = "Initialize#1"

            Environment.SetEnvironmentVariable("nls_lang", "AMERICAN_AMERICA.US7ASCII")

            If My.Application.CommandLineArgs.Count > 0 Then
                strlP1 = My.Application.CommandLineArgs(0)                  ' job
            Else
                strlP1 = ""
            End If

            strlStep = "Initialize#2"

            If My.Application.CommandLineArgs.Count > 1 Then
                strlP2 = My.Application.CommandLineArgs(1)                  ' job type
            Else
                strlP2 = ""
            End If

            strlStep = "Initialize#3"

            If My.Application.CommandLineArgs.Count > 2 Then
                strlP3 = RemoveSpecialCharacters(My.Application.CommandLineArgs(2))                  ' user
            Else
                strlP3 = ""
            End If

            strlStep = "Initialize#4"

            If My.Application.CommandLineArgs.Count > 3 Then
                strlP4 = My.Application.CommandLineArgs(3)                  ' pwd
            Else
                strlP4 = ""
            End If

            strlStep = "Initialize#5"

            If My.Application.CommandLineArgs.Count > 4 Then
                strlP5 = My.Application.CommandLineArgs(4)                  ' seqno
            Else
                strlP5 = ""
            End If

            strlStep = "Initialize#6"

            If My.Application.CommandLineArgs.Count > 5 Then
                strlP6 = My.Application.CommandLineArgs(5)                  ' printer
                If strlP6 = "default" Then strlP6 = ""
            Else
                strlP6 = ""
            End If

            strlStep = "Initialize#7"

            If My.Application.CommandLineArgs.Count > 6 Then
                strlP7 = My.Application.CommandLineArgs(6)                  ' form name
            Else
                strlP7 = ""
            End If

            strlStep = "Initialize#8"

            If My.Application.CommandLineArgs.Count > 7 Then
                strlP8 = My.Application.CommandLineArgs(7)                  ' submit time
            Else
                strlP8 = ""
            End If

            strlStep = "Initialize#9"

            If My.Application.CommandLineArgs.Count > 8 Then
                strlP9 = My.Application.CommandLineArgs(8)                  ' completion time
            Else
                strlP9 = ""
            End If

            strlStep = "Initialize#10"

            strlSID = Environment.GetEnvironmentVariable("ORACLE_SID").ToString
            strlService = strlSID.Substring(0, 4)

            strlStep = "Initialize#11"

            If String.IsNullOrEmpty(strlP9)  Then
                action = "submitting"
            ElseIf strlP9 = "Sanitize" Then
                action = "sanitizing"
            Else
                action = "completing"
            End If

            Try
                Put_Log($"{action} {strlP1} # {strlP5} for {strlP3} on {strlSID}")
            Catch ex As Exception
                Console.WriteLine("Error " & ex.Message & " trying to write this to the log file : " & strlMsg)
            End Try

            strlStep = "Pre-Sanitize"

            If strlP9 = "Sanitize"  Then

                '
                ' We are in the sanitize cycle.
                ' Look for passwords and remove them.
                ' No Oracle stuff
                '
                If File.Exists( _reportsFolder & strlService & "\" & strlP3 & "\" & strlP1 & "_" & strlP5 & ".log") Then
                    strlStep = "Sanitize Read " &  _reportsFolder & strlService & "\" & strlP3 & "\" & strlP1 & "_" & strlP5 & ".log"
                    strflogFile = File.ReadAllLines( _reportsFolder & strlService & "\" & strlP3 & "\" & strlP1 & "_" & strlP5 & ".log")

                    For l = 0 To UBound(strflogFile)
                        If strflogFile(l).ToUpper.Contains(strlP4) Then
                            boolRewrite = True
                            strflogFile(l) = Replace(strflogFile(l), strlP4, "*********", , , CompareMethod.Text)
                        End If
                    Next

                    If boolRewrite Then
                        strlStep = "Sanitize Write " &  _reportsFolder & strlService & "\" & strlP3 & "\" & strlP1 & "_" & strlP5 & ".log"
                        File.WriteAllLines( _reportsFolder & strlService & "\" & strlP3 & "\" & strlP1 & "_" & strlP5 & ".log", strflogFile)
                    End If
                Else
                    strlMsg = Format(Now, "yyyy-MMM-dd HH:mm") & " can't find " &  _reportsFolder & strlService & "\" & strlP3 & "\" & strlP1 & "_" & strlP5 & ".log" & vbCrLf
                    Put_Log(strlMsg)
                End If

                If File.Exists( _reportsFolder & strlService & "\" & strlP3 & "\" & strlP1 & "_" & strlP5 & ".bat") Then
                    strlStep = "Sanitize Read " &  _reportsFolder & strlService & "\" & strlP3 & "\" & strlP1 & "_" & strlP5 & ".bat"
                    strflogFile = File.ReadAllLines( _reportsFolder & strlService & "\" & strlP3 & "\" & strlP1 & "_" & strlP5 & ".bat")

                    For l = 0 To UBound(strflogFile)
                        If strflogFile(l).ToUpper.Contains(strlP4) Then
                            boolRewrite = True
                            strflogFile(l) = Replace(strflogFile(l), strlP4, "*********", , , CompareMethod.Text)
                        End If
                    Next

                    If boolRewrite Then
                        strlStep = "Sanitize Write " &  _reportsFolder & strlService & "\" & strlP3 & "\" & strlP1 & "_" & strlP5 & ".bat"
                        File.WriteAllLines(_reportsFolder & strlService & "\" & strlP3 & "\" & strlP1 & "_" & strlP5 & ".bat", strflogFile)
                    End If
                Else
                    strlMsg = Format(Now, "yyyy-MMM-dd HH:mm") & " can't find " &  _reportsFolder & strlService & "\" & strlP3 & "\" & strlP1 & "_" & strlP5 & ".bat" & vbCrLf
                    Put_Log(strlMsg)
                End If

                If File.Exists( _reportsFolder & strlService & "\" & strlP3 & "\" & strlP1 & "_" & strlP5 & ".prm") Then
                    strlStep = "Sanitize Read " &  _reportsFolder & strlService & "\" & strlP3 & "\" & strlP1 & "_" & strlP5 & ".prm"
                    strflogFile = File.ReadAllLines( _reportsFolder & strlService & "\" & strlP3 & "\" & strlP1 & "_" & strlP5 & ".prm")

                    For l = 0 To UBound(strflogFile)
                        If strflogFile(l).ToUpper.Contains(strlP4) Then
                            boolRewrite = True
                            strflogFile(l) = Replace(strflogFile(l), strlP4, "*********", , , CompareMethod.Text)
                        End If
                    Next

                    If boolRewrite Then
                        strlStep = "Sanitize Write " &  _reportsFolder & strlService & "\" & strlP3 & "\" & strlP1 & "_" & strlP5 & ".prm"
                        File.WriteAllLines( _reportsFolder & strlService & "\" & strlP3 & "\" & strlP1 & "_" & strlP5 & ".prm", strflogFile)
                    End If
                Else
                    strlMsg = Format(Now, "yyyy-MMM-dd HH:mm") & " can't find " &  _reportsFolder & strlService & "\" & strlP3 & "\" & strlP1 & "_" & strlP5 & ".prm" & vbCrLf
                    Put_Log(strlMsg)
                End If

                '
                ' Do we have any *.lis or *.rpt that should be *.txt ?
                '

                strlFiles = Directory.GetFiles( _reportsFolder & strlService & "\" & strlP3 & "\")
                RenameReports(_reportsFolder & strlService & "\" & strlP3 & "\")
                'For l = 0 To strlFiles.GetUpperBound(0)

                '    If strlFiles(l).ToUpper.EndsWith(".LIS") Then
                '        If Not File.Exists(Replace(strlFiles(l), ".lis", ".txt", , , CompareMethod.Text)) Then
                '            strlStep = "Sanitize move *.lis ==> *.txt "
                '            File.Move(strlFiles(l), Replace(strlFiles(l), ".lis", ".txt", , , CompareMethod.Text))
                '        End If
                '    End If

                '    If strlFiles(l).ToUpper.EndsWith(".RPT") Then
                '        If Not File.Exists(Replace(strlFiles(l), ".rpt", ".txt", , , CompareMethod.Text)) Then
                '            strlStep = "Sanitize move *.rpt ==> *.txt "
                '            File.Move(strlFiles(l), Replace(strlFiles(l), ".rpt", ".txt", , , CompareMethod.Text))
                '        End If
                '    End If
                'Next

            Else
                '
                ' We are not sanitizing. We must be submitting or completing.
                '

                strlStep = "Checking Stuff"

                adoDB = New ADODB.Connection With {
                    .CursorLocation = Global.ADODB.CursorLocationEnum.adUseClient,
                    .ConnectionString = config.AppSettings.Get("OracleAdodbConnectionString")
                }
                Threading.Thread.Sleep(2000)
                adoDB.Open()
                adoRS = New ADODB.Recordset

                'With adoRS
                If strlP9 = "" Then
                    '
                    ' No completion date in p9, so job is being submitted but not submitted yet. Status blank
                    '
                    strlStep = "Adding     "
                    adoRS.Open("Select * from GWJSUBQ where 1=2", adoDB, Global.ADODB.CursorTypeEnum.adOpenForwardOnly, Global.ADODB.LockTypeEnum.adLockOptimistic)
                    adoRS.AddNew()
                    adoRS.Fields("gwjsubq_Server").Value = _strLocalHost
                    adoRS.Fields("gwjsubq_Status").Value = " "
                    adoRS.Fields("gwjsubq_Service").Value = strlService.ToUpper
                    adoRS.Fields("gwjsubq_Job").Value = strlP1.ToUpper
                    adoRS.Fields("gwjsubq_Seq").Value = Val(strlP5)
                    adoRS.Fields("gwjsubq_User").Value = strlP3.ToUpper
                    adoRS.Fields("gwjsubq_Submitted").Value = Now
                    strlStep = "Adding Finished"

                    Console.WriteLine("GJAJOBS is submitting " & strlP1.ToUpper & "#" & Val(strlP5) & " for " & strlP3.ToUpper)

                Else
                    '
                    ' Completion date in p9, so job is complete
                    '
                    ' Some jobs happen so damned fast it trips up the job submitter
                    '

                    Threading.Thread.Sleep(1000)

                    '
                    ' If a job is recently complete, its status is most likely S
                    '

                    strlStep = "Updating  Oracle"

                    sql = "Select * from GWJSUBQ where gwjsubq_Status = 'S' and gwjsubq_Service = '"
                    sql = sql & strlService.ToUpper & "' and gwjsubq_job = '" & strlP1.ToUpper
                    sql = sql & "' and gwjsubq_seq=" & strlP5 & " and gwjsubq_user = '" & strlP3.ToUpper & "' "
                    adoRS.Open(sql, adoDB, Global.ADODB.CursorTypeEnum.adOpenForwardOnly, Global.ADODB.LockTypeEnum.adLockOptimistic)

                    If adoRS.EOF Then
                        '
                        ' Although until we switch over, it might still be ' '
                        '

                        strlStep = "Updating Oracle (' ')"

                        adoRS.Close()
                        sql = "Select * from GWJSUBQ where gwjsubq_Status = ' ' and gwjsubq_Service = '"
                        sql = sql & strlService.ToUpper & "' and gwjsubq_job = '" & strlP1.ToUpper
                        sql = sql & "' and gwjsubq_seq=" & strlP5 & " and gwjsubq_user = '" & strlP3.ToUpper & "' "
                        adoRS.Open(sql, adoDB, Global.ADODB.CursorTypeEnum.adOpenForwardOnly, Global.ADODB.LockTypeEnum.adLockOptimistic)
                        If adoRS.EOF Then
                            '
                            ' OK - not an S or blank, maybe it's an F
                            '
                            strlStep = "Updating Oracle (not S)"
                            adoRS.Close()

                            sql = "Select * from GWJSUBQ where gwjsubq_Status = 'F' and gwjsubq_Service = '"
                            sql = sql & strlService.ToUpper & "' and gwjsubq_job = '" & strlP1.ToUpper
                            sql = sql & "' and gwjsubq_seq=" & strlP5 & " and gwjsubq_user = '" & strlP3.ToUpper & "' "
                            adoRS.Open(sql, adoDB, Global.ADODB.CursorTypeEnum.adOpenForwardOnly, Global.ADODB.LockTypeEnum.adLockOptimistic)
                            If adoRS.EOF Then
                                '
                                ' Well this is certainly strange - it's not either of the two stati we expected. See if it exists at all.
                                '
                                strlStep = "Updating Oracle (?)"
                                adoRS.Close()

                                sql = "Select * from GWJSUBQ where gwjsubq_Service = '"
                                sql = sql & strlService.ToUpper & "' and gwjsubq_job = '" & strlP1.ToUpper
                                sql = sql & "' and gwjsubq_seq=" & strlP5 & " and gwjsubq_user = '" & strlP3.ToUpper
                                sql = sql & "' order by gwjsubq_submitted desc "
                                adoRS.Open(sql, adoDB, Global.ADODB.CursorTypeEnum.adOpenForwardOnly, Global.ADODB.LockTypeEnum.adLockOptimistic)
                                If adoRS.EOF Then
                                    '
                                    ' OK, I give. Add a new record.
                                    '
                                    adoRS.AddNew()
                                    adoRS.Fields("gwjsubq_Server").Value = _strLocalHost
                                    adoRS.Fields("gwjsubq_Status").Value = "?"
                                    adoRS.Fields("gwjsubq_Service").Value = strlService.ToUpper
                                    adoRS.Fields("gwjsubq_Job").Value = strlP1.ToUpper
                                    adoRS.Fields("gwjsubq_Seq").Value = Val(strlP5)
                                    adoRS.Fields("gwjsubq_User").Value = strlP3.ToUpper
                                    adoRS.Fields("gwjsubq_Submitted").Value = Now
                                End If
                            End If
                        Else
                            '
                            ' Normal job, status ' ', change to 'X'
                            '
                            adoRS.Fields("gwjsubq_Status").Value = "X"
                        End If
                    Else
                        '
                        ' Normal job, status 'S', change to 'X'
                        '
                        adoRS.Fields("gwjsubq_Status").Value = "X"
                    End If

                    adoRS.Fields("gwjsubq_Message").Value = strlP9
                    adoRS.Fields("gwjsubq_Completed").Value = Now

                End If

                '
                ' p9 <> SANITIZE - ergo, we're submitting or completing a job
                '

                strlStep = "Updating Oracle (SID)"
                adoRS.Fields("gwjsubq_SID").Value = strlSID.ToUpper
                adoRS.Fields("gwjsubq_P1").Value = strlP1
                adoRS.Fields("gwjsubq_P2").Value = strlP2
                adoRS.Fields("gwjsubq_P3").Value = strlP3
                adoRS.Fields("gwjsubq_P4").Value = Encrypt(strlP4)
                adoRS.Fields("gwjsubq_P5").Value = strlP5
                adoRS.Fields("gwjsubq_P6").Value = strlP6
                adoRS.Fields("gwjsubq_P7").Value = strlP7
                adoRS.Fields("gwjsubq_P8").Value = strlP8
                adoRS.Fields("gwjsubq_P9").Value = strlP9
                adoRS.Update()
                adoRS.Close()
                adoRS = Nothing
            End If

        Catch ex As Exception
            Try
                Put_Log("error " & ex.Message & " at step " & strlStep)
            Catch
            End Try

            Console.Write("GJAJOBS error : " & ex.Message & " at step " & strlStep)
            Threading.Thread.Sleep(5000)

        End Try
    End Sub

    Private Function Encrypt(ByVal str As String) As String
        Dim i As Int16 = 0
        Dim j As Int16 = 0
        Dim strlPrefix As String = ""
        Dim intlPrefix As Int16 = 0
        Dim strlSuffix As String = ""
        Dim intlsuffix As Int16 = 0
        Dim strlEncrypt As String = ""

        Encrypt = ""

        Randomize()
        intlPrefix = Rnd() * 8 + 1

        For i = 1 To intlPrefix
            strlPrefix = strlPrefix & Chr(Rnd() * 9 + 48)
        Next

        intlsuffix = Rnd() * 20 + 1

        For i = 1 To intlsuffix
            strlSuffix = strlSuffix & Chr(Rnd() * 42 + 48)
        Next

        strlEncrypt = "1" & Format(intlPrefix, "0") & strlPrefix & Format(Len(str), "000") & str & strlSuffix

        For i = 1 To strlEncrypt.Length
            j = i Mod 10 - 5
            Mid(strlEncrypt, i, 1) = Chr(Asc(Mid(strlEncrypt, i, 1)) + j)
        Next

        Encrypt = strlEncrypt

        If UnEncrypt(strlEncrypt) <> str Then
            Encrypt = "," & str
        End If

    End Function

    Private Function UnEncrypt(ByVal str As String) As String
        Dim i As Int16 = 0
        Dim j As Int16 = 0
        Dim strlPrefix As String = ""
        Dim intlPrefix As Int16 = 0
        Dim intlUnEncrypt As Int16 = 0
        Dim strlversion As String = "0"
        Dim strlSuffix As String = ""
        Dim strlUnEncrypt As String = ""
        Dim strlPass1 As String = ""

        UnEncrypt = ""
        strlPass1 = str
        If strlPass1 = "" Then
            Return ""
        End If

        For i = 1 To strlPass1.Length
            j = i Mod 10 - 5
            Mid(strlPass1, i, 1) = Chr(Asc(Mid(strlPass1, i, 1)) - j)
        Next

        strlversion = Mid(strlPass1, 1, 1)
        If strlversion = "0" Then
            Return Mid(str, 2)
        End If

        If strlversion <> "1" Then
            Return ""
        End If

        intlPrefix = Val(Mid(strlPass1, 2, 1))
        strlUnEncrypt = Mid(strlPass1, intlPrefix + 3)
        intlUnEncrypt = Val(Mid(strlUnEncrypt, 1, 3))
        strlUnEncrypt = Mid(strlUnEncrypt, 4, intlUnEncrypt)

        UnEncrypt = strlUnEncrypt

    End Function

   

    ''' <summary>
    ''' Rename any *.lis or *.rpt files to *.txt as long as they don't already exists
    ''' </summary>
    ''' <param name="startFolder"></param>
    ''' <remarks></remarks>
    Public Sub RenameReports(startFolder As String)
        Try
            Dim newFileName As String
            Dim filesToRename As IEnumerable(Of FileInfo) = GetFilesToRename(startFolder)
            Dim fileToRename As FileInfo
            For Each fileToRename In filesToRename
                newFileName = fileToRename.DirectoryName + "\" + Path.GetFileNameWithoutExtension(fileToRename.FullName) + ".txt"
                If Not File.Exists(newFileName) Then
                    File.Move(fileToRename.FullName, newFileName)
                End If
            Next
        Catch ex As Exception
            Put_Log($"Error renaming lis and rpt files {ex.Message}")
        End Try
    End Sub

    Public Function GetFilesToRename(root As String) As IEnumerable(Of FileInfo)
        Return From file In Directory.EnumerateFiles(root)
            Where file.ToLower().EndsWith("lis") OrElse file.ToLower().EndsWith("rpt")
            Select New FileInfo(file)
    End Function

End Module
