Imports System.IO
Imports System.Xml

Public Class BlocksScreen

    Public Blocks() As Block
    Public BlocksXmlLoaded As Boolean = False

    Public LocationPreset As String = ""
    Public EditorMode As Boolean = False

    Dim blocks_count As Integer = 1

    'N E W  B L O C K S  V I E W
    Public Sub New()
    End Sub

    'L O A D  B L O C K S . X M L  (CLIENT+CMS)
    Public Sub LoadBlocksXml(ByVal from_dir As String, Optional to_dir As String = "", Optional use_sync As Boolean = True)
        Dim filename As String = "blocks.xml"
        'SYNC XML FILE TO LOCAL FOLDER
        If File.Exists(from_dir + filename) Then
            If Not Directory.Exists(app_dir + to_dir) Then Directory.CreateDirectory(app_dir + to_dir)
            File.Copy(from_dir + filename, app_dir + to_dir + filename, True)
        Else
            'DEL BLOCKS.XML AT DESTINATIO IF NO SUCH AT SOURCE
            If File.Exists(to_dir + filename) Then File.Delete(to_dir + filename)
            'DEL BLOCKS FOLDERS AS WELL
            Dim dirs As List(Of String) = New List(Of String)(Directory.EnumerateDirectories(app_dir + to_dir))
            For i = 0 To dirs.LongCount - 1
                Try
                    Directory.Delete(dirs(i), True)
                Catch ex As Exception
                    AddToLog("ERR: Cannot delete some file, while loading Blocks XML. " + ex.ToString)
                End Try
            Next
        End If
        'INIT
        blocks_count = 0
        Dim sets_count As Integer = 0
        BlocksXmlLoaded = False
        Dim _id() As ArrayList
        Dim _value() As ArrayList
        Dim xmlr As XmlTextReader

        Dim blTpl As New BlockTemplate
        'Dim blSets As New TemplateSettings

        'READ XML
        'Try
        If File.Exists(app_dir + to_dir + filename) Then
            xmlr = New XmlTextReader(app_dir + to_dir + filename)
            xmlr.WhitespaceHandling = WhitespaceHandling.None

            While xmlr.Read()
                Dim set_index As Integer = 0
                While xmlr.Read

                    'BLOCK SECTION
                    If xmlr.Name.Equals("block") And xmlr.IsStartElement Then
                        ReDim Preserve _id(blocks_count)
                        _id(blocks_count) = New ArrayList
                        ReDim Preserve _value(blocks_count)
                        _value(blocks_count) = New ArrayList
                        blocks_count += 1
                        set_index = 0
                    End If

                    'READ SET ATTRIBUTES
                    If xmlr.Name.Equals("set") Then
                        Dim attr As String = xmlr.GetAttribute("id")
                        'If attr = blTpl.Settings(set_index).id Then
                        _id(blocks_count - 1).Add(attr)
                        _value(blocks_count - 1).Add(xmlr.GetAttribute("value"))
                        'Else
                        '_id(blocks_count - 1).Add(blTpl.Settings(set_index).id)
                        '_value(blocks_count - 1).Add(blTpl.Settings(set_index).def)
                        'End If
                        set_index += 1
                    End If
                End While
                'End If
            End While 'END READ XML
            xmlr.Close()
            xmlr = Nothing

            'BLOCK DATA
            If Not IsNothing(_id) Then
                For i = 0 To _id.Count - 1

                    'DEFAULTS VALUE FROM TEMPLATE
                    Dim b_set() As String
                    Dim blk_tpl As New BlockTemplate
                    For ii = 0 To blk_tpl.Settings.Count - 1
                        ReDim Preserve b_set(ii)
                        b_set(ii) = blk_tpl.Settings(ii).def
                    Next

                    'VALUES FROM XML
                    For j = 0 To _id(i).Count - 1
                        For jj = 0 To blk_tpl.Settings.Count - 1
                            If _id(i).Item(j) = blk_tpl.Settings(jj).id Then
                                b_set(jj) = _value(i).Item(j)
                            End If
                        Next
                    Next j

                    'NEW BLOCK ITEM
                    ReDim Preserve Blocks(i)
                    Blocks(i) = New Block(b_set(0), b_set(1), b_set(2), b_set(3), b_set(4), b_set(5),
                                          b_set(6), b_set(7), CBool(b_set(8)), CInt(b_set(9)))

                    Dim text_data() As String = {b_set(10), b_set(11), b_set(12), b_set(13), b_set(14),
                                                 b_set(15), b_set(16), b_set(17), b_set(18), b_set(19),
                                                 b_set(20), b_set(21), b_set(22), b_set(23), b_set(24)}
                    With Blocks(i)
                        .LoadTextBlocks(text_data)
                        .bSimpleTouch = b_set(25)
                        .bOrder = i
                        .bTimeLimit = b_set(26)
                        .bFromTime = b_set(27)
                        .bToTime = b_set(28)
                        .bLinkTo = b_set(29)

                        .bLocationPreset = LocationPreset
                        .EditorMode = EditorMode
                        .UseSync = use_sync
                    End With
                Next i

                BlocksXmlLoaded = True
            Else
                BlocksXmlLoaded = False
            End If
            If blocks_count = 0 Then BlocksXmlLoaded = False
        Else
            AddToLog("ERR: Missing " + filename)
            BlocksXmlLoaded = False
        End If
        'Catch ex As Exception
        '    AddToLog(filename + " LOAD ERR: " + ex.ToString)
        'End Try
    End Sub

End Class
