'For new template item should add:
'1 - that item to tpl.setup.xml (that is template for CMS, used in Players for init values)
'2 - that item to setup.xml (that is values for Player and CMS)
'3 - new value to Public at SettingsSet class
'4 - new item to xml processing here (read from xml to Public value)

'6 - and do final processing of item value in Player code (MainWindow.xaml.vb)


Imports System.IO
Imports System.Xml

Public Class SettingsSet

    Public XmlLoaded As Boolean = False

    Public Root As String = ""
    Public Width As Integer = 0
    Public Height As Integer = 0
    Public Background As String = "bg.jpg"
    Public Foreground As String = "fg.png"
    Public MarginLeft As Integer = 0
    Public MarginTop As Integer = 0
    Public MarginRight As Integer = 0
    Public MarginBottom As Integer = 0
    Public StackDirection As String = "HOR"
    Public Timedate As Boolean = False
    Public Sysinfo As Boolean = False
    Public EmgMode As Boolean = False

    'N E W S  E T T I N G S  S E T
    Public Sub New()
    End Sub

    'L O A D  S E T U P . X M L  (CLIENT+CMS)
    Public Sub LoadXml(ByVal from_dir As String, Optional to_dir As String = "")
        Dim filename As String = "setup.xml"
        'SYNC XML FILE TO LOCAL FOLDER
        If File.Exists(from_dir + filename) Then
            If Not Directory.Exists(app_dir + to_dir) Then Directory.CreateDirectory(app_dir + to_dir)
            File.Copy(from_dir + filename, app_dir + to_dir + filename, True)
        Else
            If File.Exists(to_dir + filename) Then File.Delete(to_dir + filename)
        End If
        'INIT
        Dim sets_count As Integer = 0
        XmlLoaded = False
        Dim _id As New ArrayList
        Dim _value As New ArrayList
        Dim xmlr As XmlTextReader
        'READ XML
        'Try
        If File.Exists(app_dir + to_dir + filename) Then
            xmlr = New XmlTextReader(app_dir + to_dir + filename)
            xmlr.WhitespaceHandling = WhitespaceHandling.None

            While xmlr.Read()
                'READ SET ATTRIBUTES
                If xmlr.Name.Equals("set") Then
                    _id.Add(xmlr.GetAttribute("id"))
                    _value.Add(xmlr.GetAttribute("value"))
                End If
            End While 'END READ XML
            xmlr.Close()
            xmlr = Nothing

            'DEFAULTS VALUE FROM TEMPLATE
            Dim stp_tpl As New SettingsTemplate
            Dim s_set() As String
            If stp_tpl.IsLoaded Then
                For ii = 0 To stp_tpl.Settings.Count - 1
                    ReDim Preserve s_set(ii)
                    s_set(ii) = stp_tpl.Settings(ii).def
                Next
            Else
                MsgBox("Settings Template load ERROR")
            End If


            'VALUES FROM XML
            For j = 0 To _id.Count - 1
                If _id.Item(j) = stp_tpl.Settings(j).id Then
                    s_set(j) = _value.Item(j)
                End If
            Next j

            'NEW SETTINGS SET
            Me.Root = s_set(0)
            If s_set(1) <> "" Then Me.Width = CInt(s_set(1)) Else Me.Width = 0
            If s_set(2) <> "" Then Me.Height = CInt(s_set(2)) Else Me.Height = 0
            Me.Background = s_set(3)
            Me.Foreground = s_set(4)
            If s_set(5).Contains(",") Then
                Dim margin_str() As String = s_set(5).Split(",")
                Me.MarginLeft = CInt(margin_str(0))
                Me.MarginTop = CInt(margin_str(1))
                Me.MarginRight = CInt(margin_str(2))
                Me.MarginBottom = CInt(margin_str(3))
            Else
                Me.MarginLeft = 0
                Me.MarginTop = 0
                Me.MarginRight = 0
                Me.MarginBottom = 0
            End If
            Me.StackDirection = s_set(6)
            Me.Timedate = CBool(s_set(7))
            Me.Sysinfo = CBool(s_set(8))
            Me.EmgMode = CBool(s_set(9))

            XmlLoaded = True
        Else
            AddToLog("ERR: Missing " + filename)
            XmlLoaded = False
        End If
        'Catch ex As Exception
        '    AddToLog(filename + " LOAD ERR: " + ex.ToString)
        'End Try
    End Sub

End Class
