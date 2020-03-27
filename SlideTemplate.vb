Imports System.IO
Imports System.Xml

Public Class SlideTemplate

    Public Sub New()
        Me.LoadTemplateData("tpl.slides.xml")
    End Sub

    Public IsLoaded As Boolean = False
    Public Settings() As TemplateSettings

    Public Parameters() As String = {"set", "id", "type", "value", "def", "minmax", "sel", "title", "hint", "break"}

    Public Identificators() As String = {"src", "ttl", "dur", "mod", _
                               "tt", "tt_fs", "tt_fc", "tt_bc", "tt_alg", _
                               "mt", "mt_fs", "mt_fc", "mt_bc", "mt_alg", _
                               "bt", "bt_fs", "bt_fc", "bt_bc", "bt_alg", _
                                "t_lim_en", "t_lim_from", "t_lim_to", "loc"}

    Public DefaultValues() As String = {"", "New Slide", "7", "", _
                               "", "24", "#FFFFFFFF", "#99000000", "LEFT", _
                               "", "32", "#FFFFFFFF", "#99000000", "CENTER", _
                               "", "16", "#FFFFFFFF", "#99000000", "LEFT", _
                                        "False", "", "", ""}

    'L O A D   . T P L . X M L
    Public Sub LoadTemplateData(ByVal template_file As String)
        'READ XML
        Me.IsLoaded = False
        Dim xmlr As XmlTextReader
        Try
            If File.Exists(template_file) Then
                xmlr = New XmlTextReader(template_file)
                xmlr.WhitespaceHandling = WhitespaceHandling.None

                While xmlr.Read()

                    If xmlr.Name.Equals("template") Then
                        Settings = Nothing
                        Dim set_count As Integer = 0

                        While xmlr.Read
                            If xmlr.Name.Equals(Parameters(0)) Then
                                ReDim Preserve Settings(set_count)
                                Settings(set_count) = New TemplateSettings
                                With Settings(set_count)
                                    .id = xmlr.GetAttribute(Parameters(1))
                                    .type = xmlr.GetAttribute(Parameters(2))
                                    '.value = xmlr.GetAttribute(set_param(3))
                                    .def = xmlr.GetAttribute(Parameters(4))
                                    .minmax = xmlr.GetAttribute(Parameters(5))
                                    .sel = xmlr.GetAttribute(Parameters(6))
                                    .title = xmlr.GetAttribute(Parameters(7))
                                    .hint = xmlr.GetAttribute(Parameters(8))
                                    .break = xmlr.GetAttribute(Parameters(9))
                                    If IsNothing(.break) Then .break = ""
                                End With
                                set_count += 1
                            End If
                        End While
                    End If
                End While
                xmlr.Close()
                xmlr = Nothing
                Me.IsLoaded = True
            Else
                AddToLog("ERR: Missing " + template_file)
                Me.IsLoaded = False
            End If
        Catch ex As Exception
            AddToLog(template_file + " LOAD ERR: " + ex.ToString)
        End Try
    End Sub

End Class
