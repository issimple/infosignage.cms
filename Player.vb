Imports System.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Threading
Imports System.IO
Imports TwitterVB2
Imports Transitionals
Imports ShaderEffectLibrary
Imports System.Xml
Imports System.Text

Public Class Player : Inherits Viewbox

    'PATHS
    Dim player_dir As String = "player_content\"

    'VISUAL OBJECTS
    Public GridPlayer As New Grid With {.Cursor = Cursors.None}
    Public ImagePlayerBg As New Image '---> should be Layer-BG
    Public GridScreensContainer As New Grid
    Public ScViewBlocksSet As New ScrollViewer With {.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled, .HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden}
    Public StackPanelBlocksSet As New StackPanel With {.Orientation = Orientation.Horizontal}
    Public GridClientBlocks As New Grid
    Public ScViewClientBlocks As New ScrollViewer With {.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled, .HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden}
    Public WrPanClientBlocks As New WrapPanel With {.HorizontalAlignment = HorizontalAlignment.Stretch, .VerticalAlignment = VerticalAlignment.Stretch, .Orientation = Orientation.Vertical}
    'wrap panel should be more flexible as grid for manual blocks positioning
    Public ImagePlayerFg As New Image With {.IsHitTestVisible = False}
    Public ImagePlayerFgMs As New Image With {.IsHitTestVisible = False, .Visibility = Windows.Visibility.Hidden} 'tmp, for motion sense

    'SIMPLE TOUCH LEVEL 1 ELEMENTS:
    Public GridClientBlocksL1 As New Grid
    Public ScViewClientBlocksL1 As New ScrollViewer With {.CanContentScroll = True, .VerticalScrollBarVisibility = ScrollBarVisibility.Hidden, .PanningMode = PanningMode.VerticalOnly, _
                                                           .VerticalAlignment = VerticalAlignment.Center, .HorizontalAlignment = HorizontalAlignment.Center}
    Public WrapPanelL1 As New WrapPanel

    'OTHER
    Public TextBlockStatus As New TextBlock With {.Foreground = Brushes.White, .Margin = New Thickness(5), _
                                                .HorizontalAlignment = HorizontalAlignment.Right, .VerticalAlignment = VerticalAlignment.Bottom}
    Public GridEMG As New Grid With {.Visibility = Windows.Visibility.Hidden}
    Public SlideEMG As New Transitionals.Controls.Slideshow With {.AutoAdvance = True}

    Public TestEnable As Integer = 0
    Public TestWidth As Integer = 0
    Public TestHeight As Integer = 0

    'INTERACTION MODES: TOUCH, EDIT
    Public InteractionMode As String = "touch"
    'edit-mode public data:
    Public SelectedScreen As Integer = -1
    Public SelectedLayer As Integer = -1
    Public SelectedBlock As Integer = -1
    'EDITOR
    Public Event BlockSelected()
    Public Event BlockResized()

    'NEW PLAYER VIEWBOX
    Public Sub New(Optional _content_source_root As String = "")
        override_content_source_root = _content_source_root
        BuildVisual()
    End Sub

    'BUILD MAIN VISUAL
    Public Sub BuildVisual()
        'MAIN
        Me.Child = GridPlayer

        With GridPlayer.Children
            .Add(ImagePlayerBg)
            .Add(GridScreensContainer)
            '.Add(ScViewBlocksSet) 'horizontal screens scroller
            'ScViewBlocksSet.Content = StackPanelBlocksSet
        End With

        GridScreensContainer.Children.Add(StackPanelBlocksSet) 'add home screen blocks
        'add all screens loop... ?
        'preload screens content
        'or load on display?

        StackPanelBlocksSet.Children.Add(GridClientBlocks) 'vertical scroller for overheight content
        GridClientBlocks.Children.Add(ScViewClientBlocks)
        ScViewClientBlocks.Content = WrPanClientBlocks 'main content goes here
        GridPlayer.Children.Add(ImagePlayerFg)
        GridPlayer.Children.Add(ImagePlayerFgMs)
        'L1:
        StackPanelBlocksSet.Children.Add(GridClientBlocksL1)
        GridClientBlocksL1.Children.Add(ScViewClientBlocksL1)
        ScViewClientBlocksL1.Content = WrapPanelL1
        'OTH:
        GridPlayer.Children.Add(TextBlockStatus)
        GridEMG.Children.Add(SlideEMG) 'tmp
        GridPlayer.Children.Add(GridEMG) 'tmp
    End Sub

    'W I N  L O A D E D
    Private Sub Player_Loaded(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles MyBase.Loaded
        'INI SETTINGS
        LoadIniSettings()
        'have to add: main cloud folder root-path, device-id, device-group-id

        'XML SETTINGS
        XmlSettings = New SettingsSet
        XmlSettings.LoadXml(source_root, player_dir)
        If XmlSettings.XmlLoaded Then ApplyXMLSettings()
        'have to add: cloud folder sub-root-path, content-groud-id processing

        'TOUCH MODE
        If touch_mode Then
            GridPlayer.Cursor = Cursors.Hand
            AddHandler GridPlayer.MouseUp, AddressOf TouchBlocks
        End If

        'LOAD SCREENS FROM XML
        'If File.Exists() Then

        'Else
        '    ReDim Preserve Screen(0)
        'End If

        ReDim Preserve Screen(0)

        Dim editor_mode As Boolean = False
        If InteractionMode = "edit" Then editor_mode = True

        'INIT BLOCKS VIEW
        Screen(0) = New BlocksScreen With {.LocationPreset = Me.Location, .EditorMode = editor_mode}

        'BLOCKS LOAD XML DATA (INCL. XML FILE COPY)
        Screen(0).LoadBlocksXml(source_root, player_dir)

        'FIND SUB BLOCKS --- FOR FUTURE IMPLEMENTATION
        'Dim subblocks_count As Integer = 0
        'If Not IsNothing(clientBlocksSet(0).Blocks) Then
        '    For i = 0 To clientBlocksSet(0).Blocks.Count - 1
        '        If File.Exists(source_root + clientBlocksSet(0).Blocks(i).bDir + "\" + "blocks.xml") Then
        '            subblocks_count += 1
        '            ReDim Preserve clientBlocksSet(subblocks_count)
        '            clientBlocksSet(subblocks_count) = New BlocksSet
        '            clientBlocksSet(subblocks_count).LoadBlocksXml(source_root + clientBlocksSet(0).Blocks(i).bDir + "\")
        '        End If
        '    Next
        'End If

        'BLOCKS VISUAL - UNDER PLAYER ONLY
        If Screen(0).BlocksXmlLoaded Then ReloadBlocksVisual(Screen(0).Blocks, WrPanClientBlocks, source_root, app_root + player_dir)

        'UPDATE TIMER INIT
        AddHandler SyncTimer.Tick, AddressOf SyncTimer_Tick
        SyncTimer.Interval = TimeSpan.FromSeconds(0.5)
        SyncTimer.Start()

        'TIME-DATE TIMER INIT
        'AddHandler TimeTimer.Tick, AddressOf TimeTimer_Tick
        'TimeTimer.Interval = TimeSpan.FromSeconds(0.5)
        'TimeTimer.Start()

        'EMG
        If Directory.Exists(source_root + "EMG") Then
            For i = 1 To 3
                If File.Exists(source_root + "EMG\emg" + CStr(i) + ".jpg") Then
                    Directory.CreateDirectory(app_root + "EMG")
                    File.Copy(source_root + "EMG\" + "emg" + CStr(i) + ".jpg", app_root + "EMG\" + "emg" + CStr(i) + ".jpg", True)
                    Dim ImageSlide As New Image
                    ImageSlide.Source = New BitmapImage(New Uri(app_root + "EMG\emg" + CStr(i) + ".jpg"))
                    ImageSlide.Stretch = Stretch.Fill
                    Dim SlideShowItem As New Transitionals.Controls.SlideshowItem
                    SlideShowItem.Content = ImageSlide
                    SlideEMG.Items.Add(SlideShowItem)
                End If
            Next i
            SlideEMG.Transition = TransitionPreset(2)
        End If

        AddToLog("INIT OK")

        'Dim LbDemo As New Label With {.Content = "iSSimple iNFOSignage.Client : for testing purpose only." + " V: " + "A.0.0.5",
        '                              .FontSize = "16", .Foreground = Brushes.Wheat, .Margin = New Thickness(5), .IsHitTestVisible = False,
        '                              .VerticalAlignment = VerticalAlignment.Top, .HorizontalAlignment = HorizontalAlignment.Left}
        'GridBack.Children.Add(LbDemo)

        'TIMERS
        AddHandler activity_timer.Tick, AddressOf activity_timer_tick
        activity_timer.Interval = TimeSpan.FromSeconds(0.5)
        activity_timer.Start()

        AddHandler home_timer.Tick, AddressOf home_timer_tick
        home_timer.Interval = TimeSpan.FromSeconds(0.5)

        'MOTION
        AddHandler motion_timer.Tick, AddressOf motion_timer_tick
        motion_timer.Interval = TimeSpan.FromSeconds(0.25)
        motion_timer.Start()
        GridPlayer.Children.Add(LbMotionLvl)
        If File.Exists(app_root + "motion.ini") Then
            Dim objIniFile As New IniFile(app_root + "motion.ini")
            msens_on = objIniFile.GetBoolean("MotionSense", "msens_on", False)
            msens_showlbl = objIniFile.GetBoolean("MotionSense", "msens_showlbl", False)
            msens_trig = objIniFile.GetInteger("MotionSense", "msens_trig", 10)
            msens_anim = objIniFile.GetInteger("MotionSense", "msens_anim", 150) / 100
        End If

        'KINECT
        AddHandler kinect_timer.Tick, AddressOf kinect_timer_tick
        kinect_timer.Interval = TimeSpan.FromSeconds(0.01)
        'kinect_timer.Start() 'OFF KINECT


        'If File.Exists(app_dir + "kinect.ini") Then
        '    Dim objIniFile As New IniFile(app_dir + "motion.ini")
        '    msens_on = objIniFile.GetBoolean("MotionSense", "msens_on", False)
        '    msens_showlbl = objIniFile.GetBoolean("MotionSense", "msens_showlbl", False)
        '    msens_trig = objIniFile.GetInteger("MotionSense", "msens_trig", 10)
        '    msens_anim = objIniFile.GetInteger("MotionSense", "msens_anim", 150) / 100
        'End If

        'VIEWPORT PTZ
        'mt_trRotate.CenterX = TestWidth / 2
        'mt_trRotate.CenterY = TestHeight / 2
        'mt_trScale.CenterX = TestWidth / 2
        'mt_trScale.CenterY = TestHeight / 2

        'mt_transf_grp.Children.Add(mt_trTranslate)
        'mt_transf_grp.Children.Add(mt_trRotate)
        'mt_transf_grp.Children.Add(mt_trScale)
        'Me.RenderTransform = mt_transf_grp

        'Dim anim_dur As Double = 20

        'Dim trTx_anim As New DoubleAnimation(-10, 10, TimeSpan.FromSeconds(anim_dur)) _
        '    With {.BeginTime = TimeSpan.FromSeconds(0), .AutoReverse = True, .RepeatBehavior = RepeatBehavior.Forever}
        'Dim trTy_anim As New DoubleAnimation(-10, 10, TimeSpan.FromSeconds(anim_dur * 1.5)) _
        '    With {.BeginTime = TimeSpan.FromSeconds(0), .AutoReverse = True, .RepeatBehavior = RepeatBehavior.Forever}
        'Dim trS_anim As New DoubleAnimation(0.95, 1.05, TimeSpan.FromSeconds(anim_dur * 2)) _
        '    With {.BeginTime = TimeSpan.FromSeconds(0), .AutoReverse = True, .RepeatBehavior = RepeatBehavior.Forever}
        'Dim trA_anim As New DoubleAnimation(-2.5, 2.5, TimeSpan.FromSeconds(anim_dur * 2.5)) _
        '    With {.BeginTime = TimeSpan.FromSeconds(0), .AutoReverse = True, .RepeatBehavior = RepeatBehavior.Forever}

        'mt_trTranslate.BeginAnimation(TranslateTransform.XProperty, trTx_anim)
        'mt_trTranslate.BeginAnimation(TranslateTransform.YProperty, trTy_anim)
        'mt_trScale.BeginAnimation(ScaleTransform.ScaleXProperty, trS_anim)
        'mt_trScale.BeginAnimation(ScaleTransform.ScaleYProperty, trS_anim)
        'mt_trRotate.BeginAnimation(RotateTransform.AngleProperty, trA_anim)

        'EDIT MODE
        If InteractionMode = "edit" Then
            EditorModeSelectBlockById(Me.SelectedBlock)
        End If

    End Sub


    '-------------------------------------------------------------------------------------------------------------------------

    'Dirs
    Dim app_root As String = System.AppDomain.CurrentDomain.BaseDirectory()
    Dim source_root As String
    Dim override_content_source_root As String = ""

    'Settings
    Public Location As String = ""
    Public StandAlone As Boolean = False

    Dim UseSync As Boolean = True
    Dim use_twitter As Boolean = False
    Dim def_duration As Integer = 3
    Dim avi_volume As Integer = 5
    Dim show_logo As Boolean = True
    Dim show_time As Boolean = True
    'Objects
    Dim SlideBlock As Transitionals.Controls.Slideshow
    Dim ImageItem As Image
    'Variables
    Dim cur_pic As Integer
    Dim next_pic As Integer
    'Client blocks
    Dim blocks_count As Integer = 1
    Dim ContentBlocks() As Block
    Public Screen() As BlocksScreen
    'Touch mode
    Dim touch_mode As Boolean = True
    'Effects
    Dim TransitionPreset(22) As Object
    'Sounds
    Dim emg_snd As New SoundPlayer
    'XML processing
    Dim xml_slides_reloaded As Boolean = False
    Dim xml_setup_reloaded As Boolean = False
    Dim xml_blocks_reloaded As Boolean = False
    'Params
    Dim SetupParams() As Param
    'Timers
    Dim SyncTimer As DispatcherTimer = New DispatcherTimer()
    'Dim TimeTimer As DispatcherTimer = New DispatcherTimer()

    Dim home_timer As DispatcherTimer = New DispatcherTimer()
    Dim activity_timer As DispatcherTimer = New DispatcherTimer()

    Dim motion_timer As DispatcherTimer = New DispatcherTimer()
    Dim kinect_timer As DispatcherTimer = New DispatcherTimer()

    'L O A D  S E T U P  I N I
    Private Sub LoadIniSettings()

        Dim player_setupini_filename As String = "player_setup.ini"

        GridPlayer.Width = System.Windows.SystemParameters.PrimaryScreenWidth
        GridPlayer.Height = System.Windows.SystemParameters.PrimaryScreenHeight

        'NO SETUP.INI FILE
        If Not File.Exists(app_root + player_setupini_filename) Then
            AddToLog("ERR: missing setup.ini")
            MsgBox("Hi! Running software first time? Then please select content folder...")
            'SELECT CONTENT FOLDER
            Dim dlg As New Forms.FolderBrowserDialog 'With {}
            Dim result As Forms.DialogResult = dlg.ShowDialog()
            If (result = Forms.DialogResult.OK) Then
                'SAVE INI
                Try
                    Dim objIniFile As New IniFile(app_root + player_setupini_filename)
                    objIniFile.WriteString("Setup", "ContentSyncPath", dlg.SelectedPath)
                Catch ex As Exception
                    AddToLog("SAVE SETUP.INI ERROR")
                End Try
            ElseIf (result = Forms.DialogResult.Cancel) Then
                MsgBox("Anyway you can setup it from setup.ini file...")
                AddToLog("NO CONTENT FOLDER SELECTED")
                Application.Current.Shutdown()
            End If
            'TEST MODE REQUEST
            Dim testmode As Integer = 0
            Dim testmode_w As Integer = 1920
            Dim testmode_h As Integer = 1080
            If MsgBox("Full screen content area?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                testmode = 1
                testmode_w = CInt(InputBox("Content area WIDTH", , "1920"))
                testmode_h = CInt(InputBox("Content area HEIGHT", , "1080"))
            End If
            'SAVE TESTMODE AT SETUP.INI
            Try
                Dim objIniFile As New IniFile(app_root + player_setupini_filename)
                objIniFile.WriteInteger("TestMode", "Enable", testmode)
                objIniFile.WriteInteger("TestMode", "Width", testmode_w)
                objIniFile.WriteInteger("TestMode", "Height", testmode_h)
            Catch ex As Exception
                AddToLog("SAVE SETUP.INI ERROR")
            End Try
            MsgBox("All done!")
        End If

        'SETUP.INI EXISTS
        If File.Exists(app_root + player_setupini_filename) Then
            'Try
            Dim objIniFile As New IniFile(app_root + player_setupini_filename)
            If override_content_source_root = "" Then
                source_root = objIniFile.GetString("Setup", "ContentSyncPath", "")
                If source_root <> "" Then
                    If Not source_root.EndsWith("\") Then source_root += "\"
                Else
                    source_root = app_root + "local_content\"
                End If
            Else
                source_root = override_content_source_root
            End If

            If Not Directory.Exists(source_root) Then Directory.CreateDirectory(source_root)
            'LOCATION
            Location = objIniFile.GetString("Setup", "Location", "")
            'STANDALONE
            StandAlone = objIniFile.GetBoolean("Setup", "StandAlone", False)
            If StandAlone Then UseSync = False
            'while set at setup.ini ContentSyncPath="" works as with StandAlone=0...

            'TESTMODE
            If objIniFile.GetInteger("TestMode", "Enable", 0) = 1 Then
                Dim test_width As Integer = objIniFile.GetInteger("TestMode", "Width", 1920)
                Dim test_height As Integer = objIniFile.GetInteger("TestMode", "Height", 1080)
                GridClientBlocks.Width = test_width
                GridClientBlocks.Height = test_height
                GridClientBlocksL1.Width = test_width
                GridClientBlocksL1.Height = test_height
                GridPlayer.Width = test_width
                GridPlayer.Height = test_height
                GridPlayer.VerticalAlignment = VerticalAlignment.Center
                GridPlayer.HorizontalAlignment = HorizontalAlignment.Center

                TestEnable = 1
                TestWidth = test_width
                TestHeight = test_height
            Else
                GridClientBlocks.Width = GridPlayer.Width
                GridClientBlocks.Height = GridPlayer.Height
                GridClientBlocksL1.Width = GridPlayer.Width
                GridClientBlocksL1.Height = GridPlayer.Height
            End If
            'Catch ex As Exception
            '    AddToLog("ERR at Load INI Settings: " + ex.ToString)
            'End Try
        Else
            MsgBox("We have some problem with ini file...")
            AddToLog("ERR at Load INI Settings: no setup.ini")
            Application.Current.Shutdown()
        End If
        'ADD DAILY FILES and BACKUP FOR LOG?
    End Sub

    'L O A D  S E T U P  X M L
    Private Sub LoadXmlSettings(ByVal from_dir As String, ByRef xParams() As Param, ByRef load_status As Boolean, Optional to_dir As String = "")
        If Not IsNothing(SetupParams) Then SetupParams = Nothing
        xml_setup_reloaded = False
        Dim loc_xmlpath As String = app_root + to_dir + "_setup.xml" '!!! TMP SKIP 
        'SYNC XML FILE TO LOCAL FOLDER
        Try
            'If UseSync And File.Exists(from_dir + "setup.xml") Then File.Copy(from_dir + "setup.xml", loc_xmlpath, True) '!!! TMP SKIP 
        Catch ex As Exception
            MsgBox("ERROR: Cannot copy setup.xml to local folder!")
        End Try

        Dim reader As XmlTextReader
        Dim index As Integer = 0
        Try
            If File.Exists(loc_xmlpath) Then
                reader = New XmlTextReader(loc_xmlpath)
                reader.WhitespaceHandling = WhitespaceHandling.None
                reader.Read()
                reader.Read()
                While Not reader.EOF
                    reader.Read()
                    If Not reader.IsStartElement() Then
                        Exit While
                    End If
                    Dim p_type As String = reader.GetAttribute("type")
                    Dim p_id As String = reader.GetAttribute("id")
                    reader.Read()
                    Dim p_title As String = reader.ReadElementString("title")
                    Dim p_hint As String = reader.ReadElementString("hint")
                    Dim p_value As String = reader.ReadElementString("value")
                    ReDim Preserve xParams(index)
                    xParams(index) = New Param(p_type, p_id, p_title, p_hint, p_value)
                    index += 1
                End While
                reader.Close()
                reader = Nothing
                load_status = True
            Else
                AddToLog("ERR: Missing setup.xml")
                'CREATE NEW setup.xml
                Dim params_count As Integer = 8
                For i = 0 To params_count - 1
                    Dim p_type() As String = {"str", "bool", "bool", "str", "int", "int", "bool", "int"}
                    Dim p_id() As String = {"content", "logo", "timedate", "feed", "eff", "vol", "emg", "blor"}
                    Dim p_title() As String = {"Content Folder", "Show Logo", "Show Time Date", "Twitter or RSS", "Transition Effect", "Video Volume", "EMG Mode", "Blocks Orientation"}
                    Dim p_hint() As String = {"", "", "", "", "", "", "", ""}
                    Dim p_value() As String = {"Slides", "True", "True", "No", "14", "5", "False", "Ver"}
                    ReDim Preserve xParams(i)
                    xParams(i) = New Param(p_type(i), p_id(i), p_title(i), p_hint(i), p_value(i))
                Next
                'SaveSetupDataToXML(app_root + to_dir)
                'UseSync = False 'no setup.xml - then switch off dBx source for session
                'LoadXmlSettings(from_dir, xParams, load_status, to_dir)
            End If
        Catch ex As Exception
            MsgBox("XML ERR: " + ex.ToString)
            AddToLog("XML ERR: " + ex.ToString)
        End Try
        If Not IsNothing(reader) Then
            reader.Close()
            reader = Nothing
        End If
    End Sub

    'S A V E  S E T U P  X M L
    Public Sub SaveSetupDataToXML(ByVal _dir As String)
        Dim settings As XmlWriterSettings = New XmlWriterSettings() With {.Indent = True}
        Using writer As XmlWriter = XmlWriter.Create(_dir + "setup.xml", settings)
            writer.WriteStartDocument()
            writer.WriteStartElement("settings")
            For i = 0 To SetupParams.Count - 1
                writer.WriteStartElement("param")
                writer.WriteAttributeString("type", SetupParams(i).type)
                writer.WriteAttributeString("id", SetupParams(i).id)
                writer.WriteElementString("title", SetupParams(i).title)
                writer.WriteElementString("hint", SetupParams(i).hint)
                writer.WriteElementString("value", SetupParams(i).value)
                writer.WriteEndElement()
            Next i
            writer.WriteEndElement()
            writer.WriteEndDocument()
        End Using
    End Sub

    'A P P L Y  X M L  S E T T I N G S
    Private Sub ApplyXMLSettings()

        'If XmlSettings.Root <> "" Then
        '    content_dir_name = XmlSettings.Root
        'End If
        'content_dir = app_dir + content_dir_name + "\" ' app_dir ???

        'CUSTOM WH
        If XmlSettings.Width <> 0 And XmlSettings.Height <> 0 Then
            GridClientBlocks.Width = XmlSettings.Width
            GridClientBlocks.Height = XmlSettings.Height
            GridClientBlocksL1.Width = XmlSettings.Width
            GridClientBlocksL1.Height = XmlSettings.Height
            GridPlayer.Width = XmlSettings.Width
            GridPlayer.Height = XmlSettings.Height
            GridPlayer.VerticalAlignment = VerticalAlignment.Center
            GridPlayer.HorizontalAlignment = HorizontalAlignment.Center
        Else
            GridClientBlocks.Width = GridPlayer.Width
            GridClientBlocks.Height = GridPlayer.Height
            GridClientBlocksL1.Width = GridPlayer.Width
            GridClientBlocksL1.Height = GridPlayer.Height
        End If

        'DONT KNOW WHERE TO PUT THAT --- COPY BG AND FG --- TMP!
        If UseSync Then
            If File.Exists(app_root + player_dir + "bg.jpg") Then File.Delete(app_root + player_dir + "bg.jpg")
            If File.Exists(source_root + "bg.jpg") Then File.Copy(source_root + "bg.jpg", app_root + player_dir + "bg.jpg", True)
            If File.Exists(app_root + player_dir + "fg.png") Then File.Delete(app_root + player_dir + "fg.png")
            If File.Exists(source_root + "fg.png") Then File.Copy(source_root + "fg.png", app_root + player_dir + "fg.png", True)
        End If

        'PLAYER BG
        GridPlayer.Background = Brushes.Black
        If XmlSettings.Background <> "" Then
            If File.Exists(app_root + player_dir + XmlSettings.Background) Then
                Dim BitmapImg As New BitmapImage
                With BitmapImg
                    .BeginInit()
                    .CreateOptions = BitmapCreateOptions.IgnoreImageCache
                    .CacheOption = BitmapCacheOption.OnLoad
                    .UriSource = New Uri(app_root + player_dir + XmlSettings.Background)
                    .EndInit()
                End With
                If Not IsNothing(BitmapImg) Then ImagePlayerBg.Source = BitmapImg
            End If
        End If

        'PLAYER FG
        If XmlSettings.Foreground <> "" Then
            If File.Exists(app_root + player_dir + XmlSettings.Foreground) Then
                Dim BitmapImg As New BitmapImage
                With BitmapImg
                    .BeginInit()
                    .CreateOptions = BitmapCreateOptions.IgnoreImageCache
                    .CacheOption = BitmapCacheOption.OnLoad
                    .UriSource = New Uri(app_root + player_dir + XmlSettings.Foreground)
                    .EndInit()
                End With
                If Not IsNothing(BitmapImg) Then ImagePlayerFg.Source = BitmapImg
            End If
        End If

        'PLAYER FG MoSENSE
        If File.Exists(app_root + player_dir + "fg-ms.png") Then
            ImagePlayerFgMs.Source = New BitmapImage(New Uri(app_root + player_dir + "fg-ms.png"))
            ImagePlayerFgMs.Visibility = Visibility.Visible
        End If

        'MARGIN
        ScViewBlocksSet.Margin = New Thickness(XmlSettings.MarginLeft, XmlSettings.MarginTop, XmlSettings.MarginRight, XmlSettings.MarginBottom)

        'STACK DIRECTION
        If XmlSettings.StackDirection.ToUpper = "HOR" Then
            ScViewClientBlocks.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            ScViewClientBlocks.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden
            WrPanClientBlocks.Orientation = Orientation.Horizontal
        End If
        If XmlSettings.StackDirection.ToUpper = "VER" Then
            ScViewClientBlocks.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
            ScViewClientBlocks.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
            WrPanClientBlocks.Orientation = Orientation.Vertical
        End If

        'SHOW TIMEDATE
        'If XmlSettings.Timedate Then
        '    StackPanelTimeDate.Visibility = Visibility.Visible
        'Else
        '    StackPanelTimeDate.Visibility = Visibility.Hidden
        'End If

        'EMG SCREEN
        If XmlSettings.EmgMode Then
            GridEMG.Visibility = Windows.Visibility.Visible
            SlideEMG.TransitionNext()
        Else
            GridEMG.Visibility = Windows.Visibility.Hidden
        End If

        'SHOW SYS INFO
        'XmlSettings.Sysinfo

        'VOLUME
        'avi_volume = XmlSettings.Volume

        'OFF SCHEDULE
        'XmlSettings.OffDays
        'XmlSettings.OffTime

        'POWER/MONITOR ON/OFF
        'Dim pwr_mode As String 'PWR, MON
        'Dim pwr_off, pwr_on, pwr_days As String

        'If SetupParams(i).id = "pwr" Then
        '    If SetupParams(i).value.ToUpper = "PWR" Then pwr_mode = "PWR"
        '    If SetupParams(i).value.ToUpper = "MON" Then pwr_mode = "MON"
        'End If
        'If SetupParams(i).id = "pwr-off" Then pwr_off = SetupParams(i).value
        'If SetupParams(i).id = "pwr-on" Then pwr_on = SetupParams(i).value
        'If SetupParams(i).id = "pwr-days" Then pwr_days = SetupParams(i).value

        'MONITOR OFF - MOVE TO APPLY SECTION:
        '        Private Const WM_SYSCOMMAND As Integer = &H112
        '        Private Const SC_MONITORPOWER As Integer = &HF170
        '        Private Const MonitorToLowPower As Integer = 1
        '        Private Const MonitorShutoff As Integer = 2
        '<DllImport("user32.dll")> _
        'Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal hMsg As Integer, _
        '                      ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
        'End Function
        '    SendMessage(Me.Handle, WM_SYSCOMMAND, CType(SC_MONITORPOWER, IntPtr), CType(MonitorShutoff, IntPtr))

        'POWER OFF - MOVE TO APPLY SECTION:
        'Process.Start("shutdown","-s")


        'OLD CODE ------------------------------------------------>
        'For i = 0 To SetupParams.Count - 1
        '    If SetupParams(i).id = "content" Then content_dir_name = SetupParams(i).value
        '    content_dir = app_dir + content_dir_name + "\"
        '    If SetupParams(i).id = "logo" Then show_logo = CBool(SetupParams(i).value)
        '    If show_logo Then SlideLogo.Visibility = Visibility.Visible Else SlideLogo.Visibility = Visibility.Hidden
        '    If SetupParams(i).id = "timedate" Then show_time = CBool(SetupParams(i).value)
        '    If show_time Then
        '        StackPanelTimeDate.Visibility = Visibility.Visible
        '    Else
        '        StackPanelTimeDate.Visibility = Visibility.Hidden
        '    End If
        '    If SetupParams(i).id = "vol" Then avi_volume = CInt(SetupParams(i).value)
        '    'EMG SCREEN
        '    If SetupParams(i).id = "emg" Then
        '        If SetupParams(i).value = "True" Then
        '            GridEMG.Visibility = Windows.Visibility.Visible
        '            SlideEMG.TransitionNext()
        '        Else
        '            GridEMG.Visibility = Windows.Visibility.Hidden
        '        End If
        '    End If
        '    'WRAP PANEL BLOCKS ORIENTATION
        '    If SetupParams(i).id = "blor" Then
        '        If SetupParams(i).value.ToUpper = "HOR" Then
        '            ScViewClientBlocks.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        '            ScViewClientBlocks.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden
        '            WrPanClientBlocks.Orientation = Orientation.Horizontal
        '        End If
        '        If SetupParams(i).value.ToUpper = "VER" Then
        '            ScViewClientBlocks.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
        '            ScViewClientBlocks.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
        '            WrPanClientBlocks.Orientation = Orientation.Vertical
        '        End If
        '    End If
        'Next i
        '<-----------------------------------------------------------

        'TRANSITIONS PRESETS
        TransitionPreset(1) = New Transitionals.Transitions.CheckerboardTransition
        TransitionPreset(2) = New Transitionals.Transitions.DiagonalWipeTransition
        TransitionPreset(3) = New Transitionals.Transitions.DiamondsTransition
        TransitionPreset(4) = New Transitionals.Transitions.DoorTransition
        TransitionPreset(5) = New Transitionals.Transitions.DotsTransition
        TransitionPreset(6) = New Transitionals.Transitions.DoubleRotateWipeTransition
        TransitionPreset(7) = New Transitionals.Transitions.ExplosionTransition
        TransitionPreset(8) = New Transitionals.Transitions.FadeAndBlurTransition
        TransitionPreset(9) = New Transitionals.Transitions.FadeAndGrowTransition
        TransitionPreset(10) = New Transitionals.Transitions.FadeTransition
        TransitionPreset(11) = New Transitionals.Transitions.FlipTransition
        TransitionPreset(12) = New Transitionals.Transitions.HorizontalBlindsTransition
        TransitionPreset(13) = New Transitionals.Transitions.HorizontalWipeTransition
        TransitionPreset(14) = New Transitionals.Transitions.MeltTransition
        TransitionPreset(15) = New Transitionals.Transitions.PageTransition
        TransitionPreset(16) = New Transitionals.Transitions.RollTransition
        TransitionPreset(17) = New Transitionals.Transitions.RotateTransition
        TransitionPreset(18) = New Transitionals.Transitions.RotateWipeTransition
        TransitionPreset(19) = New Transitionals.Transitions.StarTransition
        TransitionPreset(20) = New Transitionals.Transitions.TranslateTransition
        TransitionPreset(21) = New Transitionals.Transitions.VerticalBlindsTransition
        TransitionPreset(22) = New Transitionals.Transitions.VerticalWipeTransition

    End Sub

    'L O A D  B L O C K S  V I S U A L
    Private Sub ReloadBlocksVisual(ByVal Blocks() As Block, ByVal Panel As WrapPanel, ByVal SourceRoot As String, ByVal LocalRoot As String, Optional BlockId As Integer = -1)
        TextBlockStatus.Text = "BUILDING VISUAL... "
        If BlockId = -1 Then
            'RELOAD ALL BLOCKS
            Panel.Children.Clear()
            For i = 0 To Blocks.Count - 1
                With Blocks(i)
                    .ContentSourceRoot = SourceRoot 'content_sync_path
                    .ContentLocalRoot = LocalRoot 'app_dir
                    .UseSync = UseSync

                    .BuildVisual() 'block layout objects

                    Panel.Children.Add(Blocks(i)) '!!! - here to control layout for different blocks layout style: h-stack, v- stack, float (no stack, position control)

                    If InteractionMode = "edit" Then
                        AddHandler Blocks(i).ResizeControlRight_Event, AddressOf Blocks_ResizeControl_Event
                        AddHandler Blocks(i).ResizeControlBottom_Event, AddressOf Blocks_ResizeControl_Event
                    End If

                    TextBlockStatus.Text = "LOADING SLIDES XML... "

                    .LoadSlidesXml() '!!!

                    TextBlockStatus.Text = "LOADING SLIDES DATA... "
                    'LOAD CONTENT WITH TIMING TEST
                    If Not .bTimeLimit Then
                        .LoadContent()
                    Else
                        If CheckDateTimeLimit(.bFromTime, .bToTime) Then .LoadContent()
                    End If

                    Window1_ContentRendered(Nothing, Nothing)
                    If .Exception <> "" Then AddToLog(.Exception)
                End With
            Next
        Else
            'G-CODE! selective block rebuild
            With Blocks(BlockId)
                .ContentSourceRoot = SourceRoot 'content_sync_path
                .ContentLocalRoot = LocalRoot 'app_dir
                .UseSync = UseSync

                .BuildVisual()
                Panel.Children.RemoveAt(BlockId) '!!!
                Panel.Children.Insert(BlockId, Blocks(BlockId)) '!!!

                TextBlockStatus.Text = "LOADING SLIDES XML... "
                .LoadSlidesXml() '!!!

                TextBlockStatus.Text = "LOADING SLIDES DATA... "
                'LOAD CONTENT WITH TIMING TEST
                If Not .bTimeLimit Then
                    .LoadContent()
                Else
                    If CheckDateTimeLimit(.bFromTime, .bToTime) Then .LoadContent()
                End If

                Window1_ContentRendered(Nothing, Nothing)
                If .Exception <> "" Then AddToLog(.Exception)
            End With
        End If
    End Sub

    '>------------- EDITOR MODE BLOCK RESIZE EVENT TO CMS :
    Public Sub Blocks_ResizeControl_Event()
        For i = 0 To Screen(0).Blocks.Count - 1
            With Screen(0).Blocks(i)
                .bWidth = .ActualWidth
                .bHeight = .ActualHeight
            End With
        Next
        RaiseEvent BlockResized()
    End Sub
    '------------------------------------------------------------------------<

    Dim XmlSettings As SettingsSet

    Dim mt_transf_grp As New TransformGroup
    Dim mt_trTranslate As New TranslateTransform
    Dim mt_trRotate As New RotateTransform
    Dim mt_trScale As New ScaleTransform

    'T I M E R S

    'ACTIVITY TIMER PROC
    Dim home_screen As Boolean = True
    Dim activity_prev_x As Double = 0
    Dim activity_prev_y As Double = 0
    Private Sub activity_timer_tick(ByVal sender As Object, ByVal e As EventArgs)
        'ACTIVITY CHECK
        If Not home_screen Then
            home_timer.Start()
            If Mouse.GetPosition(GridPlayer).X <> activity_prev_x Then
                home_timer.Stop()
            End If
        End If
        activity_prev_x = Mouse.GetPosition(GridPlayer).X
        activity_prev_y = Mouse.GetPosition(GridPlayer).Y
    End Sub
    'HOME TIMER PROC
    Private Sub home_timer_tick(ByVal sender As Object, ByVal e As EventArgs)
        'If BorderSlide.Visibility = Visibility.Visible Then exit_action()
        'ImgLogo1_MouseDown(Nothing, Nothing)
        home_timer.Stop()
    End Sub

    'MOTION TIMER PROC --- 2EXTRAS
    Dim msens_on As Boolean = False
    Dim msens_showlbl As Boolean = False
    Dim msens_trig As Integer = 10
    Dim msens_anim As Double = 1.5
    Dim LbMotionLvl As New Label With {.FontSize = 16, .IsHitTestVisible = False, .Margin = New Thickness(20), _
                                       .VerticalAlignment = VerticalAlignment.Top, .HorizontalAlignment = HorizontalAlignment.Center}
    Dim msens_curlev As Integer = 0
    Dim msens_motion As Boolean = False
    Private Sub motion_timer_tick(ByVal sender As Object, ByVal e As EventArgs)
        If msens_on Then
            Try
                Dim proc As Process
                'proc = Process.GetProcessById(197584)
                'MsgBox(proc.ProcessName)
                For Each proc In Process.GetProcessesByName("motion.vshost")
                    If IsNumeric(proc.MainWindowTitle) Then msens_curlev = CInt(proc.MainWindowTitle)
                Next
                For Each proc In Process.GetProcessesByName("motion")
                    If IsNumeric(proc.MainWindowTitle) Then msens_curlev = CInt(proc.MainWindowTitle)
                Next
                If msens_showlbl Then LbMotionLvl.Content = msens_curlev.ToString
                If msens_curlev <= msens_trig Then
                    If msens_motion Then
                        ImagePlayerFgMs.BeginAnimation(Image.OpacityProperty, New DoubleAnimation(0, 1, TimeSpan.FromSeconds(msens_anim)))
                        msens_motion = False
                    End If
                End If
                If msens_curlev > msens_trig Then
                    If Not msens_motion Then
                        ImagePlayerFgMs.BeginAnimation(Image.OpacityProperty, New DoubleAnimation(1, 0, TimeSpan.FromSeconds(msens_anim)))
                        msens_motion = True
                    End If
                End If
                'else run 30 sec. delay for run back to idle no-motion-far-see mode
            Catch ex As Exception
            End Try
        Else
            ImagePlayerFgMs.Visibility = Visibility.Hidden
        End If
    End Sub

    'KINECT TIMER --- 2EXTRAS
    Dim kinect_str As String
    Private Sub kinect_timer_tick(ByVal sender As Object, ByVal e As EventArgs)
        'If msens_on Then
        Try
            Dim proc As Process
            'proc = Process.GetProcessById(197584)
            'MsgBox(proc.ProcessName)
            For Each proc In Process.GetProcessesByName("SkeletonBasics-WPF")
                kinect_str = proc.MainWindowTitle.ToString
            Next
            'If msens_showlbl Then
            LbMotionLvl.Content = "Kinect:" + kinect_str

            Dim ki_x As Double
            Dim ki_y As Double
            Dim ki_z As Double

            If kinect_str.Contains(",") Then
                Dim strs() As String = kinect_str.Split(",")
                Try
                    ki_x = CDbl(strs(0))
                    ki_y = CDbl(strs(1))
                    ki_z = CDbl(strs(2))
                Catch ex As Exception
                End Try
            End If

            If IsNumeric(ki_x) And IsNumeric(ki_y) And IsNumeric(ki_z) Then
                mt_trTranslate.X = -ki_x
                mt_trTranslate.Y = -ki_y
                'mt_trScale.ScaleX = ki_z / 100
                'mt_trScale.ScaleY = ki_z / 100
            End If

            'mt_trRotate.CenterX = ViewboxPlayer.ActualWidth / 2
            'mt_trRotate.CenterY = ViewboxPlayer.ActualHeight / 2
            'mt_trScale.CenterX = ViewboxPlayer.ActualWidth / 2
            'mt_trScale.CenterY = ViewboxPlayer.ActualHeight / 2

            'mt_transf_grp.Children.Add(mt_trTranslate)
            'mt_transf_grp.Children.Add(mt_trRotate)
            'mt_transf_grp.Children.Add(mt_trScale)
            'ViewboxPlayer.RenderTransform = mt_transf_grp

            'Dim anim_dur As Double = 0.5

            'Dim trTx_anim As New DoubleAnimation(0, 50, TimeSpan.FromSeconds(anim_dur)) _
            '    With {.BeginTime = TimeSpan.FromSeconds(1), .AutoReverse = True}
            'Dim trTy_anim As New DoubleAnimation(0, 50, TimeSpan.FromSeconds(anim_dur)) _
            '    With {.BeginTime = TimeSpan.FromSeconds(2), .AutoReverse = True}
            'Dim trS_anim As New DoubleAnimation(1, 0.95, TimeSpan.FromSeconds(anim_dur * 2)) _
            '    With {.BeginTime = TimeSpan.FromSeconds(3), .AutoReverse = True}
            'Dim trA_anim As New DoubleAnimation(0, 10, TimeSpan.FromSeconds(anim_dur * 2)) _
            '    With {.BeginTime = TimeSpan.FromSeconds(5), .AutoReverse = True}

            'mt_trTranslate.BeginAnimation(TranslateTransform.XProperty, trTx_anim)
            'mt_trTranslate.BeginAnimation(TranslateTransform.YProperty, trTy_anim)
            'mt_trScale.BeginAnimation(ScaleTransform.ScaleXProperty, trS_anim)
            'mt_trScale.BeginAnimation(ScaleTransform.ScaleYProperty, trS_anim)
            'mt_trRotate.BeginAnimation(RotateTransform.AngleProperty, trA_anim)

            'else run 30 sec. delay for run back to idle no-motion-far-see mode
        Catch ex As Exception
        End Try
        'Else
        'ImagePlayerFgMs.Visibility = Visibility.Hidden
        'End If
    End Sub


    Dim new_wp_pan As New WrapPanel With {.Name = "new_wp_pan"}
    Dim level As Integer = 0

    Public Sub EditorModeSelectBlockById(ByVal id As Integer)

        'DESELECT
        For i = 0 To WrPanClientBlocks.Children.Count - 1
            Dim blkx As Block = WrPanClientBlocks.Children(i)
            With blkx
                If Not IsNothing(.SelectionBorder) Then
                    .SelectionBorder.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#99000000"))
                    .ResizeControlRight.Visibility = Windows.Visibility.Hidden
                    .ResizeControlBottom.Visibility = Windows.Visibility.Hidden
                End If
            End With
        Next
        'SELECT
        If id <> -1 Then
            Dim blk As Block = WrPanClientBlocks.Children(id)
            With blk
                If Not IsNothing(.SelectionBorder) Then
                    .SelectionBorder.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#00000000"))
                    .ResizeControlRight.Visibility = Windows.Visibility.Visible
                    .ResizeControlBottom.Visibility = Windows.Visibility.Visible
                End If
            End With
        End If
        

        'If Not IsNothing(Layer) Then
        '    If Not IsNothing(Layer(0).Blocks) Then
        '        'DESELECT BLOCKS
        '        For i = 0 To Layer(0).Blocks.Count - 1
        '            With Layer(0).Blocks(i)
        '                If Not IsNothing(.SelectionBorder) Then
        '                    .SelectionBorder.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#99000000"))
        '                    .ResizeControlRight.Visibility = Windows.Visibility.Hidden
        '                    .ResizeControlBottom.Visibility = Windows.Visibility.Hidden
        '                End If
        '            End With
        '        Next
        '        'SELECT BLOCK BY ID
        '        If id <> -1 And id < Layer(0).Blocks.Length Then
        '            With Layer(0).Blocks(id)
        '                If Not IsNothing(.SelectionBorder) Then
        '                    .SelectionBorder.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#00000000"))
        '                    .ResizeControlRight.Visibility = Windows.Visibility.Visible
        '                    .ResizeControlBottom.Visibility = Windows.Visibility.Visible
        '                End If
        '            End With
        '        End If
        '    End If
        'End If

    End Sub

    'TOUCH BLOCKS
    Private Sub TouchBlocks(sender As Object, e As MouseButtonEventArgs)
        If InteractionMode = "edit" Then
            'SELECT AND RAISE EVENT TO CMS
            Dim obj As Object = TryCast(e.Source, Object)
            Dim dep_obj As DependencyObject = VisualTreeHelper.GetParent(obj)
            Dim blo_obj As Block = TryCast(dep_obj, Block)
            If Not IsNothing(blo_obj) Then
                Me.SelectedBlock = blo_obj.bOrder
                EditorModeSelectBlockById(blo_obj.bOrder)
                RaiseEvent BlockSelected()
            End If
        End If

        If InteractionMode = "touch" Then
            If level = 1 Then
                Dim transf As New TranslateTransform
                StackPanelBlocksSet.RenderTransform = transf
                transf.BeginAnimation(TranslateTransform.XProperty, _
                          New DoubleAnimation(-1080, 0, TimeSpan.FromSeconds(0.5)) _
                          With {.EasingFunction = New CubicEase With {.EasingMode = EasingMode.EaseInOut}})
                level = 0
            End If

            Dim obj As Object = TryCast(e.Source, Object)
            Dim dep_obj As DependencyObject = VisualTreeHelper.GetParent(obj)
            Dim blo_obj As Block = TryCast(dep_obj, Block)

            If Not IsNothing(blo_obj) Then
                If blo_obj.bSimpleTouch And level = 0 Then
                    WrapPanelL1.Children.Clear()
                    If Not IsNothing(blo_obj.Slides) Then
                        For i = 0 To blo_obj.Slides.Count - 1
                            'SCROLLVIEW PIC
                            Dim ImageSlide As New Image With {.Name = "ImageSlide" + CStr(blo_obj.bOrder) + "_" + CStr(i)}
                            Dim BitmapImg As New BitmapImage
                            If blo_obj.Slides(i).Source.ToUpper.EndsWith("JPG") Or _
                                blo_obj.Slides(i).Source.ToUpper.EndsWith("GIF") Or _
                                blo_obj.Slides(i).Source.ToUpper.EndsWith("PNG") Then

                                If File.Exists(app_root + player_dir + blo_obj.bDir + "\" + blo_obj.Slides(i).Source) Then
                                    With BitmapImg
                                        .BeginInit()
                                        .CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                        .CacheOption = BitmapCacheOption.OnLoad
                                        .UriSource = New Uri(app_root + player_dir + blo_obj.bDir + "\" + blo_obj.Slides(i).Source)
                                        .EndInit()
                                    End With
                                    If Not IsNothing(BitmapImg) Then
                                        'GIF
                                        If blo_obj.Slides(i).Source.ToUpper.EndsWith("GIF") Then _
                                            WpfAnimatedGif.ImageBehavior.SetAnimatedSource(ImageSlide, BitmapImg)
                                        ImageSlide.Source = BitmapImg
                                    End If
                                    With ImageSlide
                                        .Stretch = Stretch.None
                                        .Margin = New Thickness(5)
                                    End With

                                    WrapPanelL1.Children.Add(ImageSlide)

                                    'If Not IsNothing(FindName(ImageSlide.Name)) Then UnregisterName(ImageSlide.Name)
                                    'Me.RegisterName(ImageSlide.Name, ImageSlide)
                                    'If Not IsNothing(FindName(ScrollViewerImageSlide.Name)) Then UnregisterName(ScrollViewerImageSlide.Name)
                                    'Me.RegisterName(ScrollViewerImageSlide.Name, ScrollViewerImageSlide)
                                End If
                            End If
                        Next
                    End If
                    Dim transf As New TranslateTransform
                    StackPanelBlocksSet.RenderTransform = transf
                    transf.BeginAnimation(TranslateTransform.XProperty, _
                                          New DoubleAnimation(0, -1080, TimeSpan.FromSeconds(0.5)) _
                                          With {.EasingFunction = New CubicEase With {.EasingMode = EasingMode.EaseInOut}})
                    'ScViewBlocksSet.ScrollToHorizontalOffset(600)
                    level += 1
                Else

                    'SH EFF
                    Dim xy As New Point(Mouse.GetPosition(blo_obj).X / blo_obj.ActualWidth, Mouse.GetPosition(blo_obj).Y / blo_obj.ActualHeight)
                    Dim eff0 As New ShaderEffectLibrary.RippleEffect With {.Center = xy, .Frequency = 0.9}

                    blo_obj.xStackPanelContentScroller.Effect = eff0
                    eff0.BeginAnimation(RippleEffect.AmplitudeProperty, New DoubleAnimation(0, 0.01, TimeSpan.FromSeconds(0.25)) With {.AutoReverse = True})

                    Dim sel_eff As Integer = 0

                    ''Magnify
                    'If sel_eff = 0 Then
                    '    Dim coord As New Point(Mouse.GetPosition(Window1).X / SlShow.ActualWidth, Mouse.GetPosition(Window1).Y / SlShow.ActualHeight)
                    '    Dim easexy As New CubicEase
                    '    Dim animxy As New PointAnimation(coord, TimeSpan.FromSeconds(1))
                    '    animxy.EasingFunction = easexy
                    '    eff0.BeginAnimation(MagnifyEffect.CenterProperty, animxy)
                    'End If
                    ''Smooth Magnify
                    'If sel_eff = 1 Then
                    '    Dim coord As New Point(Mouse.GetPosition(Window1).X / SlShow.ActualWidth, Mouse.GetPosition(Window1).Y / SlShow.ActualHeight)
                    '    Dim easexy As New BackEase
                    '    Dim animxy As New PointAnimation(coord, TimeSpan.FromSeconds(1))
                    '    animxy.EasingFunction = easexy
                    '    eff1.BeginAnimation(SmoothMagnifyEffect.CenterProperty, animxy)
                    'End If
                    ''Ripple Move
                    'If sel_eff = 2 Then
                    '    Dim coord As New Point(Mouse.GetPosition(Window1).X / SlShow.ActualWidth, Mouse.GetPosition(Window1).Y / SlShow.ActualHeight)
                    '    Dim easexy As New CubicEase
                    '    Dim animxy As New PointAnimation(coord, TimeSpan.FromSeconds(1))
                    '    animxy.EasingFunction = easexy
                    '    eff2.BeginAnimation(RippleEffect.CenterProperty, animxy)
                    'End If
                    ''Ripple Touch
                    'If sel_eff = 3 Then
                    '    Dim anim As New DoubleAnimation(0, 0.1, TimeSpan.FromSeconds(0.5))
                    '    anim.AutoReverse = True
                    '    eff2.BeginAnimation(RippleEffect.AmplitudeProperty, anim)
                    '    Dim ease As New CubicEase
                    '    anim.EasingFunction = ease
                    '    eff2.Center = New Point(Mouse.GetPosition(Window1).X / SlShow.ActualWidth, Mouse.GetPosition(Window1).Y / SlShow.ActualHeight)
                    'End If
                    'If sel_eff = 4 Then
                    '    eff4.BeginAnimation(BrightExtractEffect.ThresholdProperty, New DoubleAnimation(0.8, 0, TimeSpan.FromSeconds(1)))
                    'End If
                    'If sel_eff = 5 Then
                    '    eff5.BeginAnimation(PixelateEffect.HorizontalPixelCountsProperty, New DoubleAnimation(0, 1920, TimeSpan.FromSeconds(3)))
                    '    eff5.BeginAnimation(PixelateEffect.VerticalPixelCountsProperty, New DoubleAnimation(0, 1080, TimeSpan.FromSeconds(3)))
                    'End If
                End If
                'BLOCKS VISUAL
                'If clientBlocksSet(1).BlocksXmlLoaded Then

                '    new_wp_pan.Width = GridPlayer.Width
                '    new_wp_pan.Height = GridPlayer.Height
                '    new_wp_pan.Background = Brushes.Black
                '    GridPlayer.Children.Add(new_wp_pan)
                '    If Not IsNothing(GridPlayer.FindName("new_wp_pan")) Then GridPlayer.UnregisterName("new_wp_pan")
                '    GridPlayer.RegisterName("new_wp_pan", new_wp_pan)

                '    Dim transf As New TranslateTransform
                '    new_wp_pan.RenderTransform = transf
                '    transf.BeginAnimation(TranslateTransform.XProperty, New DoubleAnimation(-500, 0, TimeSpan.FromSeconds(0.5)))
                '    new_wp_pan.BeginAnimation(WrapPanel.OpacityProperty, New DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.25)))

                '    Dim sub_dir As String = "Block1\"
                '    LoadBlocksVisual(clientBlocksSet(1).Blocks, new_wp_pan, source_root + sub_dir, app_dir + sub_dir)
                'End If
            End If
        End If
    End Sub

    'SYNC TIMER FOR SETUP.XML AND BLOCKS.XML
    Dim sync_proc As Boolean = False
    Dim timelimit_status(99) As Boolean '99 = g-code, ahtung!
    Dim timelimit_statusbefore(99) As Boolean
    Dim timelimit_timestatus(99) As Boolean
    Dim timelimit_timestatusbefore(99) As Boolean

    Public Sub SyncTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)

        'TIME LIMIT CHECK FOR BLOCKS
        If Not IsNothing(Screen(0).Blocks) Then
            For i = 0 To Screen(0).Blocks.Count - 1
                With Screen(0).Blocks(i)

                    'check if TimeLimit was enabled
                    timelimit_status(i) = .bTimeLimit
                    If timelimit_status(i) <> timelimit_statusbefore(i) Then
                        If Not .bTimeLimit Then
                            .LoadContent() ' no limits - just load content as common
                        Else
                            If CheckDateTimeLimit(.bFromTime, .bToTime) Then .LoadContent() Else .ClearBlockContent()
                            'have limit enabled? then check if now is right time to load content, if not - clear stuff
                        End If
                        '... or relaod all block data?
                    Else
                        'status was not chnages, so have to check if time is come
                        If .bTimeLimit Then
                            timelimit_timestatus(i) = CheckDateTimeLimit(.bFromTime, .bToTime)
                            If timelimit_timestatus(i) <> timelimit_timestatusbefore(i) Then
                                If CheckDateTimeLimit(.bFromTime, .bToTime) Then .LoadContent() Else .ClearBlockContent()
                            End If
                        End If
                        timelimit_timestatusbefore(i) = timelimit_timestatus(i)
                    End If
                    timelimit_statusbefore(i) = timelimit_status(i)
                End With
            Next
        End If

        'Try
        If UseSync Then
            TextBlockStatus.Text = ""
            Dim source_file_info, local_file_info As FileInfo

            'SETUP.XML
            source_file_info = New FileInfo(source_root + "setup.xml")
            local_file_info = New FileInfo(app_root + player_dir + "setup.xml")
            If local_file_info.LastWriteTime <> source_file_info.LastWriteTime Then
                TextBlockStatus.Text = "LOAD SETUP... "
                LoadXmlSettings(source_root, SetupParams, xml_setup_reloaded, player_dir)
                TextBlockStatus.Text = "APPLY SETUP... "
                If xml_setup_reloaded Then ApplyXMLSettings()
                TextBlockStatus.Text = "LOAD BLOCKS XML... "
                'BLOCKS LOAD XML DATA
                Screen(0).LoadBlocksXml(source_root, player_dir)
                'BLOCKS VISUAL
                TextBlockStatus.Text = "LOAD BLOCKS VISUAL... "
                If Screen(0).BlocksXmlLoaded Then _
                    ReloadBlocksVisual(Screen(0).Blocks, WrPanClientBlocks, source_root, app_root + player_dir)
            End If
            source_file_info = Nothing
            local_file_info = Nothing

            'BLOCKS.XML

            '!!! fix required: reload only changed block data / analyze xml, before visual reload

            source_file_info = New FileInfo(source_root + "blocks.xml")
            local_file_info = New FileInfo(app_root + player_dir + "blocks.xml")
            If local_file_info.LastWriteTime <> source_file_info.LastWriteTime Then
                'BLOCKS LOAD XML DATA
                TextBlockStatus.Text = "LOAD BLOCKS XML... "
                Dim PrevBlocks() As Block = Screen(0).Blocks
                Screen(0).LoadBlocksXml(source_root, player_dir)
                If Not IsNothing(Screen(0).Blocks) Then
                    For i = 0 To Screen(0).Blocks.Count - 1
                        Screen(0).Blocks(i).LoadSlidesXml()
                    Next
                End If
                'BLOCKS VISUAL
                TextBlockStatus.Text = "LOAD BLOCKS VISUAL... "
                If Screen(0).BlocksXmlLoaded Then
                    If Not IsNothing(PrevBlocks) Then
                        If PrevBlocks.Count = Screen(0).Blocks.Count Then
                            'same blocks q-ty after update
                            For i = 0 To PrevBlocks.Count - 1
                                If Not IsSameBlockData(Screen(0).Blocks(i), PrevBlocks(i)) Then
                                    'reload updated block only
                                    ReloadBlocksVisual(Screen(0).Blocks, WrPanClientBlocks, source_root, app_root + player_dir, i)
                                End If
                            Next
                        Else
                            'diff blocks q-ty after update
                            ReloadBlocksVisual(Screen(0).Blocks, WrPanClientBlocks, source_root, app_root + player_dir)
                        End If
                    Else
                        ReloadBlocksVisual(Screen(0).Blocks, WrPanClientBlocks, source_root, app_root + player_dir)
                    End If
                End If
            End If

            'SLIDES.XML --- SHOULD BE INSIDE BLOCK SYNC !!!
            'For i = 0 To Layer(0).Blocks.Count - 1
            '    Dim source_file As String = source_root + Layer(0).Blocks(i).bDir + "\" + "slides.xml"
            '    Dim local_file As String = app_root + player_dir + Layer(0).Blocks(i).bDir + "\" + "slides.xml"
            '    If File.Exists(source_file) Then
            '        If Not File.Exists(local_file) Then
            '            File.Copy(source_file, local_file)
            '            ReloadBlocksVisual(Layer(0).Blocks, WrPanClientBlocks, source_root, app_root + player_dir, i)
            '        Else
            '            source_file_info = New FileInfo(local_file)
            '            local_file_info = New FileInfo(local_file)
            '            If local_file_info.LastWriteTime <> source_file_info.LastWriteTime Then
            '                ReloadBlocksVisual(Layer(0).Blocks, WrPanClientBlocks, source_root, app_root + player_dir, i)
            '            End If
            '        End If
            '    End If
            'Next

            '-----------------EXTRAS:

            'SYS COMMAND - TERMINATE SINGLE STATION
            If File.Exists(source_root + "close.me") Then
                Application.Current.Shutdown()
            End If

            'SYS COMMAND - UPD .EXE
            Dim bat_file As String = app_root + "upd.cmd"
            Dim player_filename As String = "iNFOSignage.Player.exe"
            If File.Exists(bat_file) Then
                File.Delete(bat_file)
            Else
                If File.Exists(source_root + player_filename) Then
                    'Dim oldexe_file_info As New FileInfo(app_dir + "iNFOSignage.exe")
                    'Dim newexe_file_info As New FileInfo(dBx_dir + "iNFOSignage.exe")
                    Dim oldexe_file_info As FileVersionInfo = FileVersionInfo.GetVersionInfo(app_root + player_filename)
                    Dim newexe_file_info As FileVersionInfo = FileVersionInfo.GetVersionInfo(source_root + player_filename)
                    If oldexe_file_info.FileVersion <> newexe_file_info.FileVersion Then
                        'BAT DATA
                        Dim bat_data As String
                        bat_data = "ping -n 6 127.0.0.1 > nul" + vbCrLf '"TIMEOUT 5"
                        bat_data += "COPY " + Chr(34) + source_root + player_filename + Chr(34) + " "
                        bat_data += Chr(34) + app_root + player_filename + Chr(34) + vbCrLf
                        bat_data += "ping -n 6 127.0.0.1 > nul" + vbCrLf
                        bat_data += app_root + player_filename + vbCrLf
                        bat_data += "EXIT" + vbCrLf
                        'WRITE BAT FILE
                        Try
                            Dim StrWr As StreamWriter
                            If Not File.Exists(bat_file) Then File.Create(bat_file).Close()
                            StrWr = New StreamWriter(bat_file, True)
                            StrWr.WriteLine(bat_data)
                            StrWr.Flush()
                            StrWr.Close()
                        Catch ex As Exception
                        End Try
                        'RUN BAT FILE
                        Try
                            If File.Exists(bat_file) Then
                                System.Diagnostics.Process.Start(bat_file)
                                Application.Current.Shutdown()
                            End If
                        Catch ex As Exception
                        End Try
                    End If
                End If
            End If

        End If
        'Catch ex As Exception
        '    AddToLog("MAIN SYNC ERR: " + ex.ToString)
        'End Try
    End Sub

    'K E Y B O A R D  S H O R T C U T S
    'move out from player class
    'Dim split_view As Boolean = False
    'Private Sub Window1_KeyUp(sender As Object, e As KeyEventArgs) Handles Window1.KeyUp


    '    'SPLIT SCREEN
    '    If e.Key = Key.F12 Then
    '        If Not split_view Then
    '            '--- ENABLE
    '            Me.WindowState = WindowState.Normal
    '            Me.Width = SystemParameters.PrimaryScreenWidth / 3
    '            Me.Height = SystemParameters.PrimaryScreenHeight
    '            Me.Left = 0
    '            Me.Top = 0
    '            Me.WindowStyle = WindowStyle.ToolWindow
    '            Me.ResizeMode = ResizeMode.CanResizeWithGrip
    '            'Me.Topmost = True
    '            split_view = True
    '        Else
    '            '--- DISABLE
    '            Me.WindowStyle = WindowStyle.None
    '            Me.ResizeMode = ResizeMode.NoResize
    '            Me.WindowState = WindowState.Maximized
    '            Me.Width = Double.NaN
    '            Me.Height = Double.NaN
    '            Me.Left = 0
    '            Me.Top = 0
    '            split_view = False
    '        End If
    '    End If
    'End Sub

    'AUTO SCROLLER
    Private Sub Window1_ContentRendered(sender As Object, e As EventArgs) 'Handles Window1.ContentRendered '!!! it should be smth somewhere...

        Dim dump_bool As Boolean = False

        If WrPanClientBlocks.Children.Count <> 0 Then
            WrPanClientBlocks.UpdateLayout()
            WrPanClientBlocks.RenderTransform = Nothing

            'HORIZONTAL SCROLL

            'A - FLOW
            'If WrPanClientBlocks.ActualWidth > GridPlayer.ActualWidth Then
            '    Dim wrpan_transf As New TranslateTransform
            '    WrPanClientBlocks.RenderTransform = wrpan_transf
            '    Dim timesp As New Duration(TimeSpan.FromSeconds(30))
            '    Dim wrpan_anim As New DoubleAnimation(0, -WrPanClientBlocks.ActualWidth + GridPlayer.ActualWidth, timesp)
            '    wrpan_anim.AutoReverse = True
            '    wrpan_anim.RepeatBehavior = RepeatBehavior.Forever
            '    wrpan_transf.BeginAnimation(TranslateTransform.XProperty, wrpan_anim)
            'End If

            'B - STEP
            If WrPanClientBlocks.ActualWidth > GridPlayer.ActualWidth Then
                Dim wrpan_transf As New TranslateTransform
                WrPanClientBlocks.RenderTransform = wrpan_transf
                Dim anim_grp As New DoubleAnimationUsingKeyFrames
                anim_grp.RepeatBehavior = RepeatBehavior.Forever

                Dim steps() As Integer
                Dim steps_count As Integer = 0
                Dim bi_h As Integer = 0
                For i = 0 To Screen(0).Blocks.Count - 1
                    bi_h += Screen(0).Blocks(i).bHeight
                    If bi_h > GridPlayer.ActualHeight Then
                        bi_h = 0
                        steps_count += 1
                        ReDim Preserve steps(steps_count)
                        steps(steps_count) = Screen(0).Blocks(i - 1).bWidth
                    Else

                    End If
                Next

                Dim movestep As Integer = 0
                Dim movetime As Integer = 0
                Dim waittime As Integer = 5
                anim_grp.KeyFrames.Add(New LinearDoubleKeyFrame(0, TimeSpan.FromSeconds(waittime)))

                If Not IsNothing(steps) <> 0 Then
                    For i = 0 To steps.Count - 1
                        movestep += -steps(i)
                        movetime = waittime + 1
                        waittime += 5
                        Dim timesp As TimeSpan = TimeSpan.FromSeconds(movetime)
                        Dim delay_timesp As TimeSpan = TimeSpan.FromSeconds(waittime)
                        anim_grp.KeyFrames.Add(New EasingDoubleKeyFrame(movestep, timesp) With {.EasingFunction = New CubicEase With {.EasingMode = EasingMode.EaseInOut}})
                        anim_grp.KeyFrames.Add(New LinearDoubleKeyFrame(movestep, delay_timesp))
                    Next
                End If

                anim_grp.KeyFrames.Add(New EasingDoubleKeyFrame(0, TimeSpan.FromSeconds(movetime + 5)) With {.EasingFunction = New CubicEase() With {.EasingMode = EasingMode.EaseInOut}})
                wrpan_transf.BeginAnimation(TranslateTransform.XProperty, anim_grp)
            End If

            'VERTICAL SCROLL

            'A - FLOW
            'If WrPanClientBlocks.ActualHeight > GridPlayer.ActualHeight Then
            '    Dim wrpan_transf As New TranslateTransform
            '    WrPanClientBlocks.RenderTransform = wrpan_transf
            '    Dim timesp As New Duration(TimeSpan.FromSeconds(30))
            '    Dim wrpan_anim As New DoubleAnimation(0, -WrPanClientBlocks.ActualHeight + GridPlayer.ActualHeight, timesp)
            '    wrpan_anim.AutoReverse = True
            '    wrpan_anim.RepeatBehavior = RepeatBehavior.Forever
            '    wrpan_transf.BeginAnimation(TranslateTransform.YProperty, wrpan_anim)
            'End If

            'B - STEP
            If WrPanClientBlocks.ActualHeight > GridPlayer.ActualHeight Then
                Dim wrpan_transf As New TranslateTransform
                WrPanClientBlocks.RenderTransform = wrpan_transf
                Dim anim_grp As New DoubleAnimationUsingKeyFrames
                anim_grp.RepeatBehavior = RepeatBehavior.Forever

                Dim steps() As Integer
                Dim steps_count As Integer = 0
                Dim bi_w As Integer = 0
                Dim bi_h As Integer = 0
                'For i = 0 To clientBlocksSet(0).Blocks.Count - 1
                '    bi_w += clientBlocksSet(0).Blocks(i).bWidth
                '    If bi_w > GridPlayer.ActualWidth Then
                '        bi_w = 0
                '        steps_count += 1
                '        ReDim Preserve steps(steps_count)
                '        steps(steps_count) = clientBlocksSet(0).Blocks(i).bHeight 'i-1 !!!
                '    Else
                '    End If
                'Next

                For i = 0 To Screen(0).Blocks.Count - 2
                    bi_w += Screen(0).Blocks(i).bWidth
                    If bi_w >= GridPlayer.ActualWidth Then
                        bi_w = 0
                        bi_h += Screen(0).Blocks(i).bHeight
                    End If
                    If bi_h >= GridPlayer.ActualHeight Then
                        steps_count += 1
                        ReDim Preserve steps(steps_count)
                        steps(steps_count) = bi_h
                        bi_w = 0
                    End If
                Next

                Dim delay As Integer = 10
                Dim movestep As Integer = 0
                Dim movetime As Integer = 0
                Dim waittime As Integer = delay

                anim_grp.KeyFrames.Add(New LinearDoubleKeyFrame(0, TimeSpan.FromSeconds(waittime)))

                If Not IsNothing(steps) <> 0 Then
                    For i = 0 To steps.Count - 1
                        movestep += -steps(i)
                        movetime = waittime + 1
                        waittime += delay
                        Dim timesp As TimeSpan = TimeSpan.FromSeconds(movetime)
                        Dim delay_timesp As TimeSpan = TimeSpan.FromSeconds(waittime)
                        anim_grp.KeyFrames.Add(New EasingDoubleKeyFrame(movestep, timesp) With {.EasingFunction = New CubicEase With {.EasingMode = EasingMode.EaseInOut}})
                        anim_grp.KeyFrames.Add(New LinearDoubleKeyFrame(movestep, delay_timesp))
                    Next
                End If
                anim_grp.KeyFrames.Add(New EasingDoubleKeyFrame(0, TimeSpan.FromSeconds(movetime + delay)) With {.EasingFunction = New CubicEase() With {.EasingMode = EasingMode.EaseInOut}})
                wrpan_transf.BeginAnimation(TranslateTransform.YProperty, anim_grp)
            End If

        End If
    End Sub


End Class
