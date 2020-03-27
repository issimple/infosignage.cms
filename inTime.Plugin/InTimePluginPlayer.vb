Imports System.IO
Imports System.Xml
Imports System.Windows.Threading
Imports System.Windows.Media.Animation

Class ScheduleEvent
    'MAIN DATA
    Public FromDateTime As DateTime
    Public ToDateTime As DateTime
    Public EventTitle As String
    Public EventSubtitle As String
    Public Location As String
    'EXTRA DATA
    Public BookedBy As String
    Public Department As String
    Public Comments As String
    'SYSTEM
    Public EventActive As Boolean
    Public EventPast As Boolean
    'NEW EVENT
    Public Sub New(ByVal fromdatetime_preset As DateTime, ByVal todatetime_preset As DateTime, ByVal event_title As String, ByVal event_subtitle As String, ByVal event_location As String)
        'LOAD DATA
        FromDateTime = fromdatetime_preset
        ToDateTime = todatetime_preset
        EventTitle = event_title
        EventSubtitle = event_subtitle
        Location = event_location
        'SYS INITS
        EventActive = False
        EventPast = False
    End Sub
End Class

Public Class InTimePluginPlayer : Inherits Grid

    Dim app_dir As String = System.AppDomain.CurrentDomain.BaseDirectory()
    Public xml_root As String
    Public sets_filename As String = "settings.xml"
    Public data_filename As String = "data.xml"

    Public ContainerWidth As Integer
    Public ContainerHeight As Integer

    Dim sch_list_bak As New List(Of ScheduleEvent)
    Dim sch_list As New List(Of ScheduleEvent)

    Dim ScrollViewerSchedule As New ScrollViewer With {.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, .VerticalScrollBarVisibility = ScrollBarVisibility.Hidden}
    Dim StackPanelEvents As StackPanel

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

    Dim UpdateTimer As DispatcherTimer = New DispatcherTimer With {.Interval = TimeSpan.FromSeconds(5)}
    Dim sets_file_info As FileInfo
    Dim data_file_info As FileInfo

    Dim EnableAutoScroll As Boolean = True
    Dim AutoScrollTimer As DispatcherTimer = New DispatcherTimer With {.Interval = TimeSpan.FromSeconds(16)}
    Dim scroll_trans As New TranslateTransform

    Public Sub New(ByVal _xml_root As String, ByVal c_w As Integer, ByVal c_h As Integer)
        Me.xml_root = _xml_root
        If xml_root = "" Then xml_root = app_dir

        Me.ContainerWidth = c_w
        Me.ContainerHeight = c_h

        'LOAD SETTINGS XML
        OpenSettingsAsXML()
        'LOAD DATA XML
        OpenDataAsXML()

        Me.Children.Add(ScrollViewerSchedule)

        If settingsxml_loaded And dataxml_loaded Then
            'DATA BACKUP
            sch_list_bak = sch_list.ToList
            'VISUAL
            ReloadVisual()
        End If

        'UPDATES TIMER
        AddHandler UpdateTimer.Tick, AddressOf UpdateTimer_Tick
        UpdateTimer.Start()
        UpdateTimer_Tick()

        'UPDATES TIMER
        AddHandler AutoScrollTimer.Tick, AddressOf AutoScrollTimer_Tick
        If EnableAutoScroll Then
            AutoScrollTimer.Start()
            AutoScrollTimer_Tick()
            If Not IsNothing(StackPanelEvents) Then StackPanelEvents.RenderTransform = scroll_trans
        End If
    End Sub

    Private Sub AutoScrollTimer_Tick()
        If EnableAutoScroll Then
            If Not IsNothing(StackPanelEvents) Then
                If StackPanelEvents.ActualHeight > Me.ActualHeight Then
                    Dim anim_y As New DoubleAnimation(0, -(StackPanelEvents.ActualHeight - Me.ActualHeight), TimeSpan.FromSeconds(7))
                    With anim_y
                        .AutoReverse = True
                        .EasingFunction = New CubicEase With {.EasingMode = EasingMode.EaseInOut}
                    End With
                    scroll_trans.BeginAnimation(TranslateTransform.YProperty, anim_y)
                End If
            End If
        End If
    End Sub

    Private Sub ListViewVisual()
        'VISUAL - TEMPLATE 1
        StackPanelEvents = New StackPanel With {.Background = bg_color, .Margin = New Thickness(0), .Orientation = Orientation.Vertical, .VerticalAlignment = VerticalAlignment.Top}

        If sort_by = "no_sort" Then sch_list = sch_list_bak.ToList
        If sort_by = "by_time" Then sch_list.Sort(Function(x, y) DateTime.Compare(x.FromDateTime, y.FromDateTime))
        If sort_by = "by_location" Then sch_list.Sort(Function(x, y) x.Location.CompareTo(y.Location))

        'RECALC WIDTHS DEPENDING ON SHOW_COL
        col_wx = Me.ContainerWidth / 14
        col_w = {col_wx * 2 * -show_col(0), col_wx * 2, col_wx * (6 + 2 * -(Not show_col(0)) + 4 * -(Not show_col(3))), col_wx * 4 * -show_col(3)}

        'TITLES
        If show_title Then
            Dim StackPanelFileds As New StackPanel With {.Margin = New Thickness(0), .Orientation = Orientation.Horizontal}
            'DATE
            Dim LbFromToDateTitle As New Label With {.Width = col_w(0), .Margin = New Thickness(col_gap, col_gap, 0, 0), .Foreground = fg_color, .Content = "DATE"}
            'TIME
            Dim LbFromToTimeTitle As New Label With {.Width = col_w(1), .Margin = New Thickness(col_gap, col_gap, 0, 0), .Foreground = fg_color, .Content = "TIME"}
            'TOPIC
            Dim LbTopicTitle As New Label With {.Width = col_w(2), .Margin = New Thickness(col_gap, col_gap, 0, 0), .Foreground = fg_color, .Content = "TOPIC"}
            'LOCATION
            Dim LbLocationTitle As New Label With {.Width = col_w(3), .Margin = New Thickness(col_gap, col_gap, 0, 0), .Foreground = fg_color, .Content = "LOCATION"}

            'BUILD EVENT LINE
            With StackPanelFileds.Children
                If show_col(0) Then .Add(LbFromToDateTitle)
                If show_col(1) Then .Add(LbFromToTimeTitle)
                If show_col(2) Then .Add(LbTopicTitle)
                If show_col(3) Then .Add(LbLocationTitle)
            End With

            StackPanelEvents.Children.Add(StackPanelFileds)
        End If

        'FILTER HEADLINES
        If time_filter <> "no_filter" Then
            Dim headline_text As String = ""
            Select Case time_filter
                Case "today" : headline_text = "Today Events"
                Case "tomorrow" : headline_text = "Tomorrow Events"
                Case "next_week" : headline_text = "Next Week Events"
                Case "next_month" : headline_text = "Next Month Events" 'ADD MONTH NAME AT FRONT!
            End Select
            Dim LbFilter As New Label With {.Content = headline_text, .FontSize = 42, .Foreground = fg_color, .Margin = New Thickness(0, 0, 10, 0), _
                                            .HorizontalAlignment = HorizontalAlignment.Right, .VerticalAlignment = VerticalAlignment.Top}
            'If group_by <> "no_group" Then LbFilter.Margin = New Thickness(0, 0, 10, -40)
            StackPanelEvents.Children.Add(LbFilter)
        End If

        Dim items_in_group As Integer = 0

        'EVENTS LIST VISUAL
        For i = 0 To sch_list.LongCount - 1

            Dim show_line As Boolean = True

            'TIME FILTER
            If time_filter = "today" Then
                If Not DateTime.Equals(DateTime.Today, sch_list(i).FromDateTime.Date) Then show_line = False
            End If
            If time_filter = "tomorrow" Then
                If Not DateTime.Equals(DateTime.Today.AddDays(1), sch_list(i).FromDateTime.Date) Then show_line = False
            End If
            If time_filter = "next_week" Then
                Dim daystomonday As Integer = 7 - DateTime.Today.DayOfWeek + 1
                If Not CheckDateTimeInRange(sch_list(i).FromDateTime, DateTime.Today.AddDays(daystomonday), DateTime.Today.AddDays(daystomonday + 7)) Then show_line = False
            End If
            If time_filter = "next_month" Then
                Dim daystoendofmonth As Integer = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month) - DateTime.Today.Day
                If Not CheckDateTimeInRange(sch_list(i).FromDateTime, DateTime.Today.AddDays(daystoendofmonth), DateTime.Today.AddDays(daystoendofmonth + 30)) Then show_line = False
            End If

            Dim headline_break As Boolean = False

            'GROUP HEADLINES
            If group_by = "day" Then
                Dim LbDay As New Label With {.Content = sch_list.Item(i).FromDateTime.ToLongDateString, .FontSize = 32, .Foreground = fg_color, .Margin = New Thickness(0, 5, 0, 0)}
                If show_line Then
                    Dim add_line As Boolean = False
                    If Not sch_list(i).EventPast Or show_past Then
                        If i >= 1 Then
                            If sch_list.Item(i).FromDateTime.ToLongDateString <> sch_list.Item(i - 1).FromDateTime.ToLongDateString Then
                                add_line = True
                            End If
                        Else
                            add_line = True
                        End If
                    End If
                    If StackPanelEvents.Children.Count = 0 Then
                        add_line = True
                    End If

                    If add_line Then
                        StackPanelEvents.Children.Add(LbDay)
                        headline_break = True
                    End If
                End If
            End If

            If group_by = "location" Then
                Dim LbPlace As New Label With {.Content = sch_list.Item(i).Location, .FontSize = 32, .Foreground = fg_color, .Margin = New Thickness(0, 5, 0, 0)}
                If i >= 1 Then
                    If sch_list.Item(i).Location <> sch_list.Item(i - 1).Location Then
                        StackPanelEvents.Children.Add(LbPlace)
                        headline_break = True
                    End If
                Else
                    StackPanelEvents.Children.Add(LbPlace)
                    headline_break = True
                End If
            End If

            Dim StackPanelFileds As New StackPanel With {.Margin = New Thickness(0), .Orientation = Orientation.Horizontal}

            'DE-HL PAST EVENTS
            Dim text_color As SolidColorBrush = fg_color
            If sch_list(i).EventPast Then text_color = fg_past_color

            'DATE
            Dim LbFromDate As New Label With {.Content = sch_list.Item(i).FromDateTime.ToShortDateString, .FontSize = 18, .Foreground = text_color}
            Dim LbToDate As New Label With {.Content = "- " + sch_list.Item(i).FromDateTime.ToShortDateString, .FontSize = 12, .Foreground = text_color}
            Dim StackPanelDate As New StackPanel With {.Orientation = Orientation.Vertical, .Width = col_w(0), .Margin = New Thickness(col_gap, col_gap, 0, 0), .Background = cell_color}
            StackPanelDate.Children.Add(LbFromDate)
            'StackPanelDate.Children.Add(LbToDate)

            'TIME
            Dim LbFromTime As New Label With {.Content = sch_list.Item(i).FromDateTime.ToShortTimeString, .FontSize = 24, .Foreground = text_color}
            Dim LbToTime As New Label With {.Content = "- " + sch_list.Item(i).ToDateTime.ToShortTimeString, .FontSize = 12, .Foreground = text_color}
            Dim StackPanelTime As New StackPanel With {.Orientation = Orientation.Vertical, .Width = col_w(1), .Margin = New Thickness(col_gap, col_gap, 0, 0), .Background = cell_color}
            'HL ACTIVE
            If sch_list(i).EventActive And hl_active Then
                Dim anim_brsh As New SolidColorBrush With {.Color = cell_color.Color}
                StackPanelTime.Background = anim_brsh
                anim_brsh.BeginAnimation(SolidColorBrush.ColorProperty, New ColorAnimation(hl_color.Color, TimeSpan.FromSeconds(3)) _
                                         With {.BeginTime = TimeSpan.FromSeconds(0.5), .AutoReverse = True, .RepeatBehavior = RepeatBehavior.Forever})
            Else
                'NO HL
                StackPanelTime.Background = cell_color
            End If

            StackPanelTime.Children.Add(LbFromTime)
            StackPanelTime.Children.Add(LbToTime)

            'TOPIC
            Dim LbTitle As New TextBlock With {.Text = sch_list.Item(i).EventTitle, .FontSize = 24, .Foreground = text_color, .TextWrapping = TextWrapping.Wrap, .Padding = New Thickness(5)}
            Dim LbSubTitle As New TextBlock With {.Text = sch_list.Item(i).EventSubtitle, .Foreground = text_color, .TextWrapping = TextWrapping.Wrap, .Padding = New Thickness(5)}
            Dim StackPanelTitle As New StackPanel With {.Orientation = Orientation.Vertical, .Width = col_w(2), .Margin = New Thickness(col_gap, col_gap, 0, 0), .Background = cell_color}
            StackPanelTitle.Children.Add(LbTitle)
            If LbSubTitle.Text <> "" Then StackPanelTitle.Children.Add(LbSubTitle)

            'LOCATION
            Dim LbLocation As New Label With {.Content = sch_list.Item(i).Location, .Width = col_w(3), .Foreground = text_color, _
                                              .Margin = New Thickness(col_gap, col_gap, 0, 0), .Background = cell_color, .FontSize = 18}
            'EXTRAS
            'booked by
            'department

            If i >= 1 Then
                If Not headline_break Then
                    'GROUP DATE CELLS
                    If LbFromDate.Content = sch_list.Item(i - 1).FromDateTime.ToShortDateString Then
                        LbFromDate.Content = ""
                        StackPanelDate.Margin = New Thickness(col_gap, 0, 0, 0)
                    End If
                    'GROUP TIME CELLS
                    If LbFromTime.Content = sch_list.Item(i - 1).FromDateTime.ToShortTimeString Then
                        LbFromTime.Content = ""
                        StackPanelTime.Margin = New Thickness(col_gap, 0, 0, 0)
                    End If
                    'GROUP LOCATION CELLS
                    If LbLocation.Content = sch_list.Item(i - 1).Location Then
                        LbLocation.Content = ""
                        LbLocation.Margin = New Thickness(col_gap, 0, 0, 0)
                    End If
                End If
            End If

            'BUILD EVENT LINE
            With StackPanelFileds.Children
                If show_col(0) Then .Add(StackPanelDate)
                If show_col(1) Then .Add(StackPanelTime)
                If show_col(2) Then .Add(StackPanelTitle)
                If show_col(3) Then .Add(LbLocation)
            End With

            If show_line Then
                If show_past Then
                    StackPanelEvents.Children.Add(StackPanelFileds)
                Else
                    If Not sch_list.Item(i).EventPast Then
                        StackPanelEvents.Children.Add(StackPanelFileds)
                    End If
                End If
            End If

            StackPanelFileds.BeginAnimation(StackPanel.OpacityProperty, New DoubleAnimation(0.5, 1, TimeSpan.FromSeconds(0.5)) With {.BeginTime = TimeSpan.FromSeconds(0.1 * i)})
        Next
        ScrollViewerSchedule.Content = StackPanelEvents
    End Sub

    Private Sub GridViewVisual()

        'ContainerWidth = 1080
        'ContainerHeight = 1200
        ScrollViewerSchedule.Width = ContainerWidth

        Dim WrapPanelEvents As New WrapPanel With {.Background = bg_color, .Margin = New Thickness(0), _
                                                   .Width = ContainerWidth, .Orientation = Orientation.Horizontal}

        'If sort_by = "no_sort" Then sch_list = sch_list_bak.ToList
        'If sort_by = "by_time" Then sch_list.Sort(Function(x, y) DateTime.Compare(x.FromDateTime, y.FromDateTime))
        'If sort_by = "by_location" Then sch_list.Sort(Function(x, y) x.Location.CompareTo(y.Location))

        'sch_list.Sort(Function(x, y) DateTime.Compare(x.FromDateTime, y.FromDateTime))
        'sch_list.Sort(Function(x, y) x.Location.CompareTo(y.Location))

        Dim cells As New List(Of Integer)
        cells.Add(0)
        For i = 0 To sch_list.LongCount - 1
            If i >= 1 Then
                If sch_list.Item(i).Location <> sch_list.Item(i - 1).Location Then
                    cells.Add(i)
                End If
            End If
        Next
        cells.Add(sch_list.LongCount - 1)

        For i = 0 To cells.LongCount - 2
            Dim StackPanelCell As New StackPanel With {.Width = ContainerWidth / gridview_cols, .Height = ContainerHeight / gridview_rows}
            With StackPanelCell
                .Margin = New Thickness(col_gap, col_gap, 0, 0)
                .Width -= col_gap * (gridview_cols - 3)
                .Height -= col_gap * (gridview_rows - 3)
                .Background = bg_color
            End With

            'COLS TITLES
            Dim LbPlace As New Label With {.Content = sch_list.Item(cells.Item(i)).Location, .FontSize = 32, .Foreground = fg_color, .Margin = New Thickness(0, 0, 0, 0)}
            StackPanelCell.Children.Add(LbPlace)


            For j = cells.Item(i) To cells.Item(i + 1)

                'DE-HL PAST EVENTS
                Dim text_color As SolidColorBrush = fg_color
                If sch_list(j).EventPast Then text_color = fg_past_color

                Dim show_line As Boolean = True
                'TIME FILTER
                If time_filter = "today" Then
                    If Not DateTime.Equals(DateTime.Today, sch_list(j).FromDateTime.Date) Then show_line = False
                End If
                If time_filter = "tomorrow" Then
                    If Not DateTime.Equals(DateTime.Today.AddDays(1), sch_list(j).FromDateTime.Date) Then show_line = False
                End If
                If time_filter = "next_week" Then
                    Dim daystomonday As Integer = 7 - DateTime.Today.DayOfWeek + 1
                    If Not CheckDateTimeInRange(sch_list(j).FromDateTime, DateTime.Today.AddDays(daystomonday), DateTime.Today.AddDays(daystomonday + 7)) Then show_line = False
                End If
                If time_filter = "next_month" Then
                    Dim daystoendofmonth As Integer = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month) - DateTime.Today.Day
                    If Not CheckDateTimeInRange(sch_list(j).FromDateTime, DateTime.Today.AddDays(daystoendofmonth), DateTime.Today.AddDays(daystoendofmonth + 30)) Then show_line = False
                End If

                Dim GridEventItem As New Grid With {.Width = StackPanelCell.Width, .MinHeight = 90, .Background = cell_color, .Margin = New Thickness(0, col_gap, 0, 0)}

                Dim LbFromTime As New TextBlock With {.Text = sch_list.Item(j).FromDateTime.ToShortTimeString, .FontSize = 32, .Margin = New Thickness(5, 0, 0, 0), _
                                                  .TextWrapping = TextWrapping.NoWrap, .Padding = New Thickness(0), .Foreground = text_color}
                Dim LbDate As New TextBlock With {.Text = sch_list.Item(j).FromDateTime.ToShortDateString, .FontSize = 16, .Margin = New Thickness(7, 35, 0, 0), _
                                                  .TextWrapping = TextWrapping.NoWrap, .Padding = New Thickness(0), .Foreground = text_color}
                Dim LbToTime As New TextBlock With {.Text = "- " + sch_list.Item(j).ToDateTime.ToShortTimeString, .FontSize = 12, .Margin = New Thickness(5, 60, 0, 0), _
                                                  .TextWrapping = TextWrapping.NoWrap, .Padding = New Thickness(0), .Foreground = text_color}
                Dim LbTitle As New TextBlock With {.Text = sch_list.Item(j).EventTitle, .FontSize = 18, .Margin = New Thickness(90, 0, 0, 0), _
                                                   .TextWrapping = TextWrapping.Wrap, .Padding = New Thickness(5), .Foreground = text_color}
                'HL ACTIVE
                If sch_list(j).EventActive And hl_active Then
                    Dim anim_brsh As New SolidColorBrush With {.Color = cell_color.Color}
                    GridEventItem.Background = anim_brsh
                    anim_brsh.BeginAnimation(SolidColorBrush.ColorProperty, New ColorAnimation(hl_color.Color, TimeSpan.FromSeconds(3)) _
                                             With {.BeginTime = TimeSpan.FromSeconds(0.5), .AutoReverse = True, .RepeatBehavior = RepeatBehavior.Forever})
                Else
                    'NO HL
                    GridEventItem.Background = cell_color
                End If

                With GridEventItem.Children
                    .Add(LbFromTime)
                    .Add(LbDate)
                    .Add(LbToTime)
                    .Add(LbTitle)
                End With

                If show_line Then
                    If show_past Then
                        StackPanelCell.Children.Add(GridEventItem)
                    Else
                        If Not sch_list.Item(j).EventPast Then
                            StackPanelCell.Children.Add(GridEventItem)
                        End If
                    End If
                End If

                'StackPanelCell.Children.Add(GridEventItem)
            Next

            WrapPanelEvents.Children.Add(StackPanelCell)
        Next

        ScrollViewerSchedule.Content = WrapPanelEvents

    End Sub


    Public Sub ReloadVisual()
        If view_mode = "list" Then ListViewVisual()
        If view_mode = "grid" Then GridViewVisual()
    End Sub

    Public Function CheckDateTimeInRange(ByVal target As DateTime, ByVal from_lim As DateTime, ByVal to_lim As DateTime) As Boolean '--- PL
        Dim PresetInRange As Boolean = False
        Dim from_flag As Integer = 0
        Dim to_flag As Integer = 0

        from_flag = from_lim.CompareTo(target)
        to_flag = to_lim.CompareTo(target)

        'RESULT IF:
        '1- NO FROM LIMIT
        If from_flag = 0 And to_flag = 1 Then PresetInRange = True
        '2 - NO TO LIMIT
        If from_flag = -1 And to_flag = 0 Then PresetInRange = True
        '3 - BOTH LIMITS SET
        If from_flag = -1 And to_flag = 1 Then PresetInRange = True
        '4 - BOTH LIMITS NOT SET
        If from_flag = 0 And to_flag = 0 Then PresetInRange = True

        Return PresetInRange
    End Function

    Dim prev_states_for_active() As Boolean
    Dim prev_states_for_past() As Boolean

    Public Sub UpdateTimer_Tick()
        Dim raise_reload As Boolean = False

        'CHECK XML FILES FOR UPDATES
        If Not IsNothing(sets_file_info) And Not IsNothing(data_file_info) Then
            If sets_file_info.LastWriteTime <> New FileInfo(xml_root + sets_filename).LastWriteTime Then
                OpenSettingsAsXML()
                raise_reload = True
            End If
            If data_file_info.LastWriteTime <> New FileInfo(xml_root + data_filename).LastWriteTime Then
                OpenDataAsXML()
                sch_list_bak = sch_list.ToList
                raise_reload = True
            End If
        End If

        If settingsxml_loaded And dataxml_loaded Then
            For i = 0 To sch_list.LongCount - 1
                'ACTIVE EVENTS CHECK
                ReDim Preserve prev_states_for_active(i)
                prev_states_for_active(i) = sch_list(i).EventActive
                sch_list(i).EventActive = CheckDateTimeInRange(DateTime.Now, sch_list(i).FromDateTime, sch_list(i).ToDateTime)
                If prev_states_for_active(i) <> sch_list(i).EventActive Then raise_reload = True
                'PAST EVENTS CHECK
                ReDim Preserve prev_states_for_past(i)
                prev_states_for_past(i) = sch_list(i).EventPast
                If sch_list(i).ToDateTime.CompareTo(DateTime.Now) = -1 Then sch_list(i).EventPast = True
                If prev_states_for_past(i) <> sch_list(i).EventPast Then raise_reload = True
            Next

            If raise_reload Then ReloadVisual()
        End If
    End Sub

    '---------------------------------------------------------------- XML -----------------------------------

    Dim settingsxml_loaded As Boolean = False

    Public Sub OpenSettingsAsXML()
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
            sets_file_info = New FileInfo(xmlfile_path)
        Else
            'MsgBox("ERR: Missing " + xmlfile_path)
            settingsxml_loaded = False
        End If
    End Sub

    Dim dataxml_loaded As Boolean = False

    Public Sub OpenDataAsXML()
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

            'CLEAR DATA
            sch_list.Clear()
            'ASSIGN SETTINGS
            For i = 0 To _FromDateTime.LongCount - 1
                sch_list.Add(New ScheduleEvent(DateTime.Parse(_FromDateTime.Item(i)), DateTime.Parse(_ToDateTime.Item(i)), _
                                               _EventTitle.Item(i), _EventSubtitle.Item(i), _Location.Item(i)))
            Next

            dataxml_loaded = True
            data_file_info = New FileInfo(xmlfile_path)
        Else
            'MsgBox("ERR: Missing " + xmlfile_path)
            dataxml_loaded = False
        End If
    End Sub

End Class
