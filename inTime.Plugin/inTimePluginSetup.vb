Imports System.IO
Imports System.Xml
Imports System.Windows.Media.Animation

Public Class inTimePluginSetup : Inherits Grid

    Dim app_dir As String = System.AppDomain.CurrentDomain.BaseDirectory()
    Public xml_root As String
    Public sets_filename As String = "settings.xml"
    Public data_filename As String = "data.xml"

    Dim sch_list_bak As New List(Of ScheduleEvent)
    Dim sch_list As New List(Of ScheduleEvent)

    'PARAMETERS
    Dim sort_by As String = "no_sort"
    Dim group_by As String = "no_group"
    Dim time_filter As String = "no_filter"
    Dim show_title As Boolean = False
    Dim night_mode As Boolean = False
    Dim hl_active As Boolean = True
    Dim show_past As Boolean = False
    Dim show_col() As Boolean = {True, True, True, True}
    Dim col_wx As Integer = 60
    Dim col_wm() As Integer = {2, 2, 6, 4}
    Dim col_w() As Integer = {col_wx * col_wm(0), col_wx * col_wm(1), col_wx * col_wm(2), col_wx * col_wm(3)}
    Dim col_gap As Integer = 2

    Dim view_mode As String = "list" 'list, grid(Cols,Rows), timeline
    Dim gridview_cols As Integer = 4
    Dim gridview_rows As Integer = 2

    'COLORS - DAY SET
    Dim bg_color As New SolidColorBrush(Colors.White)
    Dim cell_color As New SolidColorBrush(Colors.LightGray)
    Dim fg_color As New SolidColorBrush(Colors.Black)
    Dim fg_past_color As New SolidColorBrush(ColorConverter.ConvertFromString("#999999"))
    Dim hl_color As New SolidColorBrush(Colors.YellowGreen)
    Dim pre_hl_color As New SolidColorBrush(Colors.Moccasin)
    'COLORS - NIGHT SET
    Dim night_bg_color As New SolidColorBrush(Colors.Black)
    Dim night_cell_color As New SolidColorBrush(ColorConverter.ConvertFromString("#111111"))
    Dim night_fg_color As New SolidColorBrush(Colors.White)
    Dim night_fg_past_color As New SolidColorBrush(ColorConverter.ConvertFromString("#333333"))
    Dim night_hl_color As New SolidColorBrush(Colors.YellowGreen)
    Dim night_pre_hl_color As New SolidColorBrush(Colors.Moccasin)

    'VISUAL OBJECTS
    Dim CbViewMode As New ComboBox
    Dim CbSortBy As New ComboBox
    Dim CbTimeFiler As New ComboBox
    Dim CbGroupBy As New ComboBox

    Dim CbShowTitle As New CheckBox With {.Content = "Show Title", .Margin = New Thickness(5)}
    Dim CbNightMode As New CheckBox With {.Content = "Night Mode", .Margin = New Thickness(5)}
    Dim CbHlActive As New CheckBox With {.Content = "Highlight Active", .Margin = New Thickness(5)}
    Dim CbShowPast As New CheckBox With {.Content = "Show Past Events", .Margin = New Thickness(5)}

    Dim CbCol1 As New CheckBox With {.Content = "Date", .Margin = New Thickness(5)}
    Dim CbCol2 As New CheckBox With {.Content = "Time", .Margin = New Thickness(5), .IsEnabled = False}
    Dim CbCol3 As New CheckBox With {.Content = "Topic", .Margin = New Thickness(5), .IsEnabled = False}
    Dim CbCol4 As New CheckBox With {.Content = "Location", .Margin = New Thickness(5)}

    Dim TbGapSize As New TextBox With {.Width = 25, .Padding = New Thickness(3)}

    Dim LbImportXLS As New Label With {.Content = "Import XLS", .Margin = New Thickness(5), .HorizontalAlignment = HorizontalAlignment.Left, .Background = Brushes.Orange}
    Dim LbUpdate As New Label With {.Content = "UPDATE", .Margin = New Thickness(5), .HorizontalAlignment = HorizontalAlignment.Right, .Background = Brushes.GreenYellow}

    Public Event UpdatePlayerTrigger()

    Public WasUpdated As Boolean = False

    Public Sub New(ByVal _xml_root As String)
        Me.xml_root = _xml_root
        If xml_root = "" Then xml_root = app_dir

        'LOAD SETTINGS XML
        OpenSettingsAsXML()
        UpdateSettingsVisuals()
        'LOAD DATA XML
        OpenDataAsXML()

        BuildVisual()
    End Sub

    Private Sub BuildVisual()
        Dim MainStack As New StackPanel With {.Orientation = Orientation.Vertical}
        Me.Children.Add(MainStack)

        'SORT BY
        Dim Line1Stack As New StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 5, 0, 5)}

        Dim ViewModeLabel As New Label With {.Content = "View mode:"}
        With CbViewMode.Items
            .Add(New Label With {.Content = "List"})
            .Add(New Label With {.Content = "Grid"})
            .Add(New Label With {.Content = "Timeline"})
        End With

        Dim SortByLabel As New Label With {.Content = "Sort by:"}
        With CbSortBy.Items
            .Add(New Label With {.Content = "No Sort"})
            .Add(New Label With {.Content = "Date and Time"})
            .Add(New Label With {.Content = "Location"})
        End With

        'TIME FILTER
        Dim TimeFilterLabel As New Label With {.Content = "Time Filter:"}
        With CbTimeFiler.Items
            .Add(New Label With {.Content = "No Filter"})
            .Add(New Label With {.Content = "Today"})
            .Add(New Label With {.Content = "Tomorrow"})
            .Add(New Label With {.Content = "Next Week"})
            .Add(New Label With {.Content = "Next Month"})
        End With

        'GROUP BY
        Dim GroupByLabel As New Label With {.Content = "Group by:"}
        With CbGroupBy.Items
            .Add(New Label With {.Content = "No Group"})
            .Add(New Label With {.Content = "Day"})
            .Add(New Label With {.Content = "Location"})
        End With

        With Line1Stack.Children
            .Add(ViewModeLabel)
            .Add(CbViewMode)

            .Add(SortByLabel)
            .Add(CbSortBy)

            .Add(TimeFilterLabel)
            .Add(CbTimeFiler)

            .Add(GroupByLabel)
            .Add(CbGroupBy)
        End With

        'SETTINGS
        Dim Line2Stack As New StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 5, 0, 5)}
        Dim SettingsLabel As New Label With {.Content = "Settings:"}
        With Line2Stack.Children
            .Add(SettingsLabel)
            .Add(CbShowTitle)
            .Add(CbNightMode)
            .Add(CbHlActive)
            .Add(CbShowPast)
        End With

        'COLUMNS
        Dim SubStack3 As New StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 5, 0, 5)}
        Dim SubStack3Label As New Label With {.Content = "Columns:"}
        With SubStack3.Children
            .Add(SubStack3Label)
            .Add(CbCol1)
            .Add(CbCol2)
            .Add(CbCol3)
            .Add(CbCol4)
        End With

        'STYLE
        Dim SubStack6 As New StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 5, 0, 5)}
        Dim SubStack6Label As New Label With {.Content = "Style:"}
        Dim SubStack6LabelSub As New Label With {.Content = "gap size ="}
        With SubStack6.Children
            .Add(SubStack6Label)
            .Add(SubStack6LabelSub)
            .Add(TbGapSize)
        End With

        'BUTTONS
        Dim GridButtons As New Grid
        With GridButtons.Children
            .Add(LbImportXLS)
            .Add(LbUpdate)
        End With

        'HANDLERS
        AddHandler LbImportXLS.MouseUp, AddressOf LbImportXLS_MouseUp
        AddHandler LbUpdate.MouseUp, AddressOf LbUpdate_MouseUp

        With MainStack.Children
            .Add(Line1Stack)
            .Add(Line2Stack)
            .Add(SubStack3)
            .Add(SubStack6)
            .Add(GridButtons)
        End With

    End Sub

    Dim settingsxml_loaded As String = False

    Private Sub OpenSettingsAsXML()
        Dim xmlfile_path As String = xml_root + sets_filename
        'INIT
        settingsxml_loaded = False
        Dim _id As New ArrayList
        Dim _value As New ArrayList
        Dim xmlr As XmlTextReader
        'READ XML
        If File.Exists(xmlfile_path) Then
            xmlr = New XmlTextReader(xmlfile_path) With {.WhitespaceHandling = WhitespaceHandling.None}
            While xmlr.Read()
                'READ SET ATTRIBUTES
                If xmlr.Name.Equals("set") Then
                    _id.Add(xmlr.GetAttribute("id"))
                    _value.Add(xmlr.GetAttribute("value"))
                End If
            End While 'END READ XML
            xmlr.Close()
            xmlr = Nothing

            'ASSIGN SETTINGS
            view_mode = _value.Item(_id.IndexOf("view_mode"))
            sort_by = _value.Item(_id.IndexOf("sort_by"))
            group_by = _value.Item(_id.IndexOf("group_by"))
            time_filter = _value.Item(_id.IndexOf("time_filter"))

            show_title = CBool(_value.Item(_id.IndexOf("show_title")))
            night_mode = CBool(_value.Item(_id.IndexOf("night_mode")))
            hl_active = CBool(_value.Item(_id.IndexOf("hl_active")))
            show_past = CBool(_value.Item(_id.IndexOf("show_past")))

            Dim show_col_str() As String = _value.Item(_id.IndexOf("show_col")).Split(",")
            For i = 0 To show_col_str.Length - 1
                show_col(i) = CBool(show_col_str(i))
            Next

            col_wx = CInt(_value.Item(_id.IndexOf("col_wx")))
            Dim col_wm_str() As String = _value.Item(_id.IndexOf("col_wm")).Split(",")
            For i = 0 To col_wm_str.Length - 1
                col_wm(i) = CInt(col_wm_str(i))
            Next
            col_gap = CInt(_value.Item(_id.IndexOf("col_gap")))

            bg_color.Color = ColorConverter.ConvertFromString(_value.Item(_id.IndexOf("bg_color")))
            cell_color.Color = ColorConverter.ConvertFromString(_value.Item(_id.IndexOf("cell_color")))
            fg_color.Color = ColorConverter.ConvertFromString(_value.Item(_id.IndexOf("fg_color")))
            fg_past_color.Color = ColorConverter.ConvertFromString(_value.Item(_id.IndexOf("fg_past_color")))
            hl_color.Color = ColorConverter.ConvertFromString(_value.Item(_id.IndexOf("hl_color")))
            pre_hl_color.Color = ColorConverter.ConvertFromString(_value.Item(_id.IndexOf("pre_hl_color")))

            night_bg_color.Color = ColorConverter.ConvertFromString(_value.Item(_id.IndexOf("night_bg_color")))
            night_cell_color.Color = ColorConverter.ConvertFromString(_value.Item(_id.IndexOf("night_cell_color")))
            night_fg_color.Color = ColorConverter.ConvertFromString(_value.Item(_id.IndexOf("night_fg_color")))
            night_fg_past_color.Color = ColorConverter.ConvertFromString(_value.Item(_id.IndexOf("night_fg_past_color")))
            night_hl_color.Color = ColorConverter.ConvertFromString(_value.Item(_id.IndexOf("night_hl_color")))
            night_pre_hl_color.Color = ColorConverter.ConvertFromString(_value.Item(_id.IndexOf("night_pre_hl_color")))

            settingsxml_loaded = True
        Else
            MsgBox("ERR: Missing " + xmlfile_path)
            settingsxml_loaded = False
        End If
    End Sub

    Private Sub UpdateSettingsVisuals()
        With CbViewMode
            If view_mode = "list" Then .SelectedIndex = 0
            If view_mode = "grid" Then .SelectedIndex = 1
            If view_mode = "timeline" Then .SelectedIndex = 2
        End With

        With CbSortBy
            If sort_by = "no_sort" Then .SelectedIndex = 0
            If sort_by = "by_time" Then .SelectedIndex = 1
            If sort_by = "by_location" Then .SelectedIndex = 2
        End With

        With CbGroupBy
            If group_by = "no_group" Then .SelectedIndex = 0
            If group_by = "day" Then .SelectedIndex = 1
            If group_by = "location" Then .SelectedIndex = 2
        End With

        With CbTimeFiler
            If time_filter = "no_filter" Then .SelectedIndex = 0
            If time_filter = "today" Then .SelectedIndex = 1
            If time_filter = "tomorrow" Then .SelectedIndex = 2
            If time_filter = "next_week" Then .SelectedIndex = 3
            If time_filter = "next_month" Then .SelectedIndex = 4
        End With

        CbShowTitle.IsChecked = CBool(show_title)
        CbNightMode.IsChecked = CBool(night_mode)
        CbHlActive.IsChecked = CBool(hl_active)
        CbShowPast.IsChecked = CBool(show_past)

        CbCol1.IsChecked = CBool(show_col(0))
        CbCol2.IsChecked = CBool(show_col(1))
        CbCol3.IsChecked = CBool(show_col(2))
        CbCol4.IsChecked = CBool(show_col(3))

        TbGapSize.Text = CStr(col_gap)
    End Sub

    Dim dataxml_loaded As String = False

    Private Sub OpenDataAsXML()
        Dim xmlfile_path As String = xml_root + data_filename
        'INIT
        dataxml_loaded = False
        Dim _FromDateTime As New List(Of String)
        Dim _ToDateTime As New List(Of String)

        Dim _EventTitle As New List(Of String)
        Dim _EventSubtitle As New List(Of String)
        Dim _Location As New List(Of String)

        Dim _BookedBy As New List(Of String)
        Dim _Department As New List(Of String)
        Dim _Comments As New List(Of String)

        Dim xmlr As XmlTextReader
        'READ XML
        If File.Exists(xmlfile_path) Then
            xmlr = New XmlTextReader(xmlfile_path) With {.WhitespaceHandling = WhitespaceHandling.None}
            While xmlr.Read()
                'READ SET ATTRIBUTES
                If xmlr.Name.Equals("event") Then
                    _FromDateTime.Add(xmlr.GetAttribute("FromDateTime"))
                    _ToDateTime.Add(xmlr.GetAttribute("ToDateTime"))

                    _EventTitle.Add(xmlr.GetAttribute("EventTitle"))
                    _EventSubtitle.Add(xmlr.GetAttribute("EventSubtitle"))
                    _Location.Add(xmlr.GetAttribute("Location"))

                    _BookedBy.Add(xmlr.GetAttribute("BookedBy"))
                    _Department.Add(xmlr.GetAttribute("Department"))
                    _Comments.Add(xmlr.GetAttribute("Comments"))
                End If
            End While 'END READ XML
            xmlr.Close()
            xmlr = Nothing

            'ASSIGN SETTINGS
            For i = 0 To _FromDateTime.LongCount - 1
                sch_list.Add(New ScheduleEvent(DateTime.Parse(_FromDateTime.Item(i)), DateTime.Parse(_ToDateTime.Item(i)), _
                                               _EventTitle.Item(i), _EventSubtitle.Item(i), _Location.Item(i)))
            Next

            dataxml_loaded = True
        Else
            MsgBox("ERR: Missing " + xmlfile_path)
            dataxml_loaded = False
        End If
    End Sub

    Private Sub SaveSettingsAsXML()
        Dim xmlfile_path As String = xml_root + sets_filename

        Dim _id() As String = {"view_mode", "sort_by", "group_by", "time_filter", _
                               "show_title", "night_mode", "hl_active", "show_past", _
                               "show_col", "col_wx", "col_wm", "col_gap", _
                               "bg_color", "cell_color", "fg_color", _
                               "fg_past_color", "hl_color", "pre_hl_color", _
                               "night_bg_color", "night_cell_color", "night_fg_color", _
                               "night_fg_past_color", "night_hl_color", "night_pre_hl_color"}

        Dim _val() As String = {view_mode, sort_by, group_by, time_filter, _
                                CStr(show_title), CStr(night_mode), CStr(hl_active), CStr(show_past), _
                                String.Join(",", show_col), CStr(col_wx), String.Join(",", col_wm), CStr(col_gap), _
                                bg_color.Color.ToString, cell_color.Color.ToString, fg_color.Color.ToString, _
                                fg_past_color.Color.ToString, hl_color.Color.ToString, pre_hl_color.Color.ToString, _
                                night_bg_color.Color.ToString, night_cell_color.Color.ToString, night_fg_color.Color.ToString, _
                                night_fg_past_color.Color.ToString, night_hl_color.Color.ToString, night_pre_hl_color.Color.ToString}

        Using xmlw As XmlWriter = XmlWriter.Create(xmlfile_path, New XmlWriterSettings() With {.Indent = True})
            With xmlw
                .WriteStartDocument()
                .WriteStartElement("settings")

                'SETS LOOP
                For i = 0 To _id.Length - 1
                    .WriteStartElement("set")
                    .WriteAttributeString("id", _id(i))
                    .WriteAttributeString("value", _val(i))
                    .WriteEndElement()
                Next
                'END LOOP

                .WriteEndElement()
            End With
        End Using

        WasUpdated = True
    End Sub

    Private Sub SaveDataAsXML()
        Dim xmlfile_path As String = xml_root + data_filename
        Using xmlw As XmlWriter = XmlWriter.Create(xmlfile_path, New XmlWriterSettings() With {.Indent = True})
            With xmlw
                .WriteStartDocument()
                .WriteStartElement("schedule")

                For i = 0 To sch_list.LongCount - 1
                    .WriteStartElement("event")

                    .WriteAttributeString("FromDateTime", sch_list(i).FromDateTime.ToString)
                    .WriteAttributeString("ToDateTime", sch_list(i).ToDateTime.ToString)

                    .WriteAttributeString("EventTitle", sch_list(i).EventTitle)
                    .WriteAttributeString("EventSubtitle", sch_list(i).EventSubtitle)
                    .WriteAttributeString("Location", sch_list(i).Location)

                    .WriteAttributeString("BookedBy", sch_list(i).BookedBy)
                    .WriteAttributeString("Department", sch_list(i).Department)
                    .WriteAttributeString("Comments", sch_list(i).Comments)

                    .WriteEndElement()
                Next

                .WriteEndElement()
            End With
        End Using

        WasUpdated = True
    End Sub

    Dim xlsSheetsArray()(,) As String

    Private Sub LbImportXLS_MouseUp(sender As Object, e As MouseButtonEventArgs)
        Dim filter_str As String = "Excel files (*.xls,*.xlsx)|*.xls;*.xlsx"
        Dim dlg As New Microsoft.Win32.OpenFileDialog() With {.Multiselect = False, .Filter = filter_str}
        Dim xlsData As ExcelImporter
        If dlg.ShowDialog() Then
            xlsData = New ExcelImporter
            xlsData.xlsImport(dlg.FileName, xlsSheetsArray)
        End If
        If Not IsNothing(xlsData) Then
            sch_list.Clear()
            For i = 1 To xlsSheetsArray.Count - 1
                If Not IsNothing(xlsSheetsArray(i)) Then
                    Dim eventlocation As String = xlsData.xlsSheetNames(i - 1)
                    Dim bound As Integer = UBound(xlsSheetsArray(i), 1)
                    For j = 2 To bound
                        Dim fromdatetime As String = ""
                        If Not IsNothing(xlsSheetsArray(i)(j, 2)) Then fromdatetime = xlsSheetsArray(i)(j, 2)
                        Dim todatetime As String = ""
                        If Not IsNothing(xlsSheetsArray(i)(j, 3)) Then todatetime = xlsSheetsArray(i)(j, 3)
                        Dim eventtitle As String = ""
                        If Not IsNothing(xlsSheetsArray(i)(j, 4)) Then eventtitle = xlsSheetsArray(i)(j, 4)

                        sch_list.Add(New ScheduleEvent(DateTime.Parse(fromdatetime), DateTime.Parse(todatetime), eventtitle, "", eventlocation))
                    Next
                End If
            Next
        End If
    End Sub

    Private Sub LbUpdate_MouseUp(sender As Object, e As MouseButtonEventArgs)
        show_past = CbShowPast.IsChecked
        show_title = CbShowTitle.IsChecked
        hl_active = CbHlActive.IsChecked
        night_mode = CbNightMode.IsChecked
        'SORT BY
        Select Case CbViewMode.SelectedIndex
            Case 0 : view_mode = "list"
            Case 1 : view_mode = "grid"
            Case 2 : view_mode = "timeline"
        End Select
        'SORT BY
        Select Case CbSortBy.SelectedIndex
            Case 0 : sort_by = "no_sort"
            Case 1 : sort_by = "by_time"
            Case 2 : sort_by = "by_location"
        End Select
        'FILTER BY
        Select Case CbTimeFiler.SelectedIndex
            Case 0 : time_filter = "no_filter"
            Case 1 : time_filter = "today"
            Case 2 : time_filter = "tomorrow"
            Case 3 : time_filter = "next_week"
            Case 4 : time_filter = "next_month"
        End Select
        'SHOW COLS
        show_col(0) = CbCol1.IsChecked
        show_col(1) = CbCol2.IsChecked
        show_col(2) = CbCol3.IsChecked
        show_col(3) = CbCol4.IsChecked
        'NO GROUP
        If CbGroupBy.SelectedIndex = 0 Then
            group_by = "no_group"
        End If
        'GROUP BY DAY
        If CbGroupBy.SelectedIndex = 1 Then
            sort_by = "by_time"
            show_col = {False, True, True, True}
            group_by = "day"
        End If
        'GROUP BY LOCATION
        If CbGroupBy.SelectedIndex = 2 Then
            sort_by = "by_location"
            show_col = {True, True, True, False}
            group_by = "location"
        End If
        'STYLE
        If IsNumeric(CInt(TbGapSize.Text)) Then
            If CInt(TbGapSize.Text) >= 0 And CInt(TbGapSize.Text) <= 20 Then
                col_gap = CInt(TbGapSize.Text)
            Else
                TbGapSize.Text = col_gap.ToString
            End If
        Else
            TbGapSize.Text = col_gap.ToString
        End If
        'THEME
        If night_mode Then
            bg_color = New SolidColorBrush(Colors.Black)
            cell_color = New SolidColorBrush(ColorConverter.ConvertFromString("#111111"))
            fg_past_color = New SolidColorBrush(ColorConverter.ConvertFromString("#333333"))
            fg_color = New SolidColorBrush(Colors.White)
        Else
            bg_color = New SolidColorBrush(Colors.White)
            cell_color = New SolidColorBrush(Colors.LightGray)
            fg_color = New SolidColorBrush(Colors.Black)
        End If

        SaveSettingsAsXML()
        SaveDataAsXML()

        RaiseEvent UpdatePlayerTrigger()

        LbUpdate.BeginAnimation(Label.OpacityProperty, New DoubleAnimation(0.5, 1, TimeSpan.FromSeconds(0.5)))
    End Sub

End Class
