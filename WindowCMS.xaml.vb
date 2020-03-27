Imports System.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Threading
Imports System.IO
Imports TwitterVB2
Imports Transitionals
Imports System.Xml
Imports System.Text
'Imports WpfAppControl

Class WindowCMS

#Region "DIMS"

    'Dirs
    Dim app_root As String = System.AppDomain.CurrentDomain.BaseDirectory()

    Dim content_localsource_dir As String = "local_content\"
    Dim content_localsource_path As String = app_root + content_localsource_dir

    Dim content_local_dir As String = "cms_content\"
    Dim content_local_path As String = app_root + content_local_dir

    'Dim content_sync_path As String = ""
    Dim content_dir_name As String = "Slides" '???
    Dim content_dir As String = app_root + content_dir_name + "\" '???

    'Settings
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

    'Setup mode
    Dim setup_mode As Boolean = False
    'Timers
    'Dim SyncTimer As DispatcherTimer = New DispatcherTimer()
    'Dim TimeTimer As DispatcherTimer = New DispatcherTimer()
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

    'CMS
    Dim cms_content_root As String = ""
    Dim cms_content_dir As String = ""
    Dim cms_content_sync_path As String = ""
    Dim cms_content_packages_root As String = ""
    Dim cms_bl_xml_loaded As Boolean = False
    Dim cms_bl_tpl_xml_loaded As Boolean = False
    Dim cms_setup_xml_loaded As Boolean = False
    Dim cmsSetupParams() As Param
    Dim sel_bl As Integer = -1
    Dim sel_sl As Integer = -1
    Dim sel_med As Integer = -1
    Dim media_file() As String

    'PLAYER
    Dim Player1 As Player
    Dim player_interaction_mode = "edit" ' / "touch"

    Dim Layer() As BlocksScreen

    'TEMPLATES
    Dim BlockTpl As New BlockTemplate
    Dim SlideTpl As New SlideTemplate
    Dim MediaTpl As New MediaTemplate
    'MANAGERS
    Dim PackagesManager1 As PackagesManager
    Dim ScreensManager1 As ScreensManager
    Dim BlocksManager1 As BlocksManager
    Dim SlidesManager1 As SlidesManager
    Dim MediaManager1 As MediaManager
    Dim UpdatesPanel1 As UpdatesPanel
    'PANELS
    Dim PackagesPanel As ManagerEditorPanel
    Dim ScreensPanel As ManagerEditorPanel
    Dim LayersPanel As ManagerEditorPanel
    Dim BlocksPanel As ManagerEditorPanel
    Dim SlidesPanel As ManagerEditorPanel
    Dim MediaPanel As ManagerEditorPanel

#End Region

    'L O A D  S E T U P  X M L
    Private Sub LoadXmlSettings(ByVal from_path As String, ByRef xParams() As Param, ByRef load_status As Boolean, Optional to_dir As String = "")
        'SETUP.XML TMP FIX !!!
        'Try
        '    If File.Exists(app_dir + "setup.xml") Then File.Copy(app_dir + "setup.xml", from_dir + "setup.xml")
        'Catch ex As Exception
        'End Try

        If Not IsNothing(SetupParams) Then SetupParams = Nothing
        'xml_setup_reloaded = False
        Dim loc_xmlpath As String = app_root + to_dir + "setup.xml"
        'SYNC XML FILE TO LOCAL FOLDER
        If Not Directory.Exists(app_root + to_dir) Then Directory.CreateDirectory(app_root + to_dir)
        If UseSync And File.Exists(from_path + "setup.xml") Then File.Copy(from_path + "setup.xml", loc_xmlpath, True)

        'Dim reader As XmlTextReader
        Dim index As Integer = 0
        'Try
        If File.Exists(loc_xmlpath) Then
            'reader = New XmlTextReader(loc_xmlpath)
            'reader.WhitespaceHandling = WhitespaceHandling.None
            'reader.Read()
            'reader.Read()
            'While Not reader.EOF
            '    reader.Read()
            '    If Not reader.IsStartElement() Then
            '        Exit While
            '    End If
            '    Dim p_type As String = reader.GetAttribute("type")
            '    Dim p_id As String = reader.GetAttribute("id")
            '    reader.Read()
            '    Dim p_title As String = reader.ReadElementString("title")
            '    Dim p_hint As String = reader.ReadElementString("hint")
            '    Dim p_value As String = reader.ReadElementString("value")
            '    ReDim Preserve xParams(index)
            '    xParams(index) = New Param(p_type, p_id, p_title, p_hint, p_value)
            '    index += 1
            'End While
            'reader.Close()
            'reader = Nothing
            'load_status = True
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
            SaveSetupDataToXML(app_root + to_dir)
            LoadXmlSettings(from_path, xParams, load_status, to_dir)
        End If
        'Catch ex As Exception
        'AddToLog("XML ERR: " + ex.ToString)
        'End Try
    End Sub

    'W I N  L O A D E D
    Private Sub Window_Loaded(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles MyBase.Loaded
        'pack://application:,,/ui_res/bg.jpg
        '/iNFOSignage;component/ui_res/bg.jpg
        'GridSetup.Background = New ImageBrush(New BitmapImage(New Uri("")))

        'CLEAR LOCAL STORE
        Try
            If Directory.Exists(content_local_path) Then Directory.Delete(content_local_path, True)
            Directory.CreateDirectory(content_local_path)
        Catch ex As Exception
            AddToLog("Cannot clear CMS dir")
        End Try

        'INI FILE
        cmsLoadIni() 'get content source path
        'TbRoot.Text = cms_content_sync_path
        'AddHandler LbBtnSaveCmsSetup.MouseUp, AddressOf UpdateContentSource

        'INIT ALL
        cmsInit()

        'UI INIT STATE
        TogglePanelVisibility(PackagesPanel, StackPanelPanelsControl.Children(0))
        TogglePanelVisibility(ScreensPanel, StackPanelPanelsControl.Children(1))
        TogglePanelVisibility(LayersPanel, StackPanelPanelsControl.Children(2))

        'LOAD PLAYER WITH EDIT MODE
        SwitchSelector(LbEditMode, LbTouchMode)
        LoadPlayer()
    End Sub

    'K E Y B O A R D  S H O R T C U T S
    'Dim split_view As Boolean = False
    Private Sub Window1_KeyUp(sender As Object, e As KeyEventArgs) Handles WindowCMS.KeyUp
        'SPLIT SCREEN
        'If e.Key = Key.F12 Then
        '    If Not split_view Then
        '        '--- ENABLE
        '        Me.WindowState = WindowState.Normal
        '        Me.Width = 2 * SystemParameters.PrimaryScreenWidth / 3
        '        Me.Height = SystemParameters.PrimaryScreenHeight
        '        Me.Left = SystemParameters.PrimaryScreenWidth / 3
        '        Me.Top = 0
        '        Me.WindowStyle = WindowStyle.ToolWindow
        '        Me.ResizeMode = ResizeMode.CanResizeWithGrip
        '        split_view = True
        '        Try
        '            If File.Exists("D:\issimple\iNFOSignage.Player\bin\Debug\iNFOSignage.Player.exe") Then
        '                System.Diagnostics.Process.Start("D:\issimple\iNFOSignage.Player\bin\Debug\iNFOSignage.Player.exe")
        '            End If
        '        Catch ex As Exception
        '        End Try
        '    Else
        '        '--- DISABLE
        '        Me.WindowStyle = WindowStyle.None
        '        Me.ResizeMode = ResizeMode.NoResize
        '        Me.WindowState = WindowState.Maximized
        '        Me.Width = Double.NaN
        '        Me.Height = Double.NaN
        '        Me.Left = 0
        '        Me.Top = 0
        '        split_view = False
        '    End If
        'End If
    End Sub

    'L O A D  S L I D E S  D A T A
    Public Sub cmsLoadSlidesData(ByVal _cmsBlocks() As Block, ByVal index As Integer)
        If Not IsNothing(_cmsBlocks) And index <> -1 Then
            If _cmsBlocks.Count <> 0 Then
                With _cmsBlocks(index)
                    .ContentSourceRoot = cms_content_sync_path
                    .ContentLocalRoot = content_local_path
                    .ContentDirsSync()
                    .UseSync = False
                    .LoadSlidesXml()
                    If .Exception <> "" Then AddToLog(.Exception)
                End With
            End If
        End If
    End Sub

    'I N I T  S E T U P
    Public Sub cmsInit()

        'LOAD SETUP.XML
        LoadXmlSettings(cms_content_sync_path, cmsSetupParams, cms_setup_xml_loaded, content_local_dir)

        'TMP!!! --- TO BE MOVE FOR LAYER OR SCREEN SETUP
        If File.Exists(app_root + content_local_dir + "bg.jpg") Then File.Delete(app_root + content_local_dir + "bg.jpg")
        If File.Exists(cms_content_sync_path + "bg.jpg") Then File.Copy(cms_content_sync_path + "bg.jpg", app_root + content_local_dir + "bg.jpg", True)
        If File.Exists(app_root + content_local_dir + "fg.png") Then File.Delete(app_root + content_local_dir + "fg.png")
        If File.Exists(cms_content_sync_path + "fg.png") Then File.Copy(cms_content_sync_path + "fg.png", app_root + content_local_dir + "fg.png", True)

        'INIT BLOCKS SET
        Layer = Nothing
        GC.Collect()
        ReDim Preserve Layer(0)
        Layer(0) = New BlocksScreen

        'LOAD BLOCKS.XML DATA
        Layer(0).LoadBlocksXml(cms_content_sync_path, content_local_dir, False)

        'PRELOAD SLIDES.XML DATA + FOLDERS
        If Not IsNothing(Layer(0).Blocks) Then
            For i = 0 To Layer(0).Blocks.Count - 1
                cmsLoadSlidesData(Layer(0).Blocks, i)
            Next
        End If

        'LOAD PLAYER --------------- >
        'Player1 = New Player(content_local_path) With {.Margin = New Thickness(10)}
        'GridPlayerBack.Children.Add(Player1)
        '< -------------------- LOAD PLAYER

        StackPanelCMS.Children.Clear()
        GC.Collect()

        '1 - PACKAGES
        PackagesPanel = Nothing
        PackagesManager1 = Nothing
        GC.Collect()

        PackagesPanel = New ManagerEditorPanel("PACKAGES")

        PackagesManager1 = New PackagesManager(content_localsource_path, cms_content_packages_root, cms_content_sync_path)
        With PackagesPanel
            .ManagerScroller.Content = PackagesManager1.PackagesStackPanel
            .ManagerButtonsArea.Children.Add(PackagesManager1.ButtonsStackPanel)
        End With
        StackPanelCMS.Children.Add(PackagesPanel)

        'PACKAGES MANAGER PANEL ACTIONS
        With PackagesManager1
            .SetPackageSelection()
            AddHandler .SetRootButton_MouseUpEvent, AddressOf SaveSetupIniFiles
            AddHandler .PackagesStackPanel_MouseUpEvent, AddressOf PackageReload
        End With


        '2 - SCREENS
        ScreensPanel = Nothing
        GC.Collect()

        ScreensPanel = New ManagerEditorPanel("SCREENS")
        With ScreensPanel
            .ManagerScroller.Content = New TextBlock With {.Text = "This features set are under development...", .Foreground = Brushes.White, .TextWrapping = TextWrapping.Wrap}
        End With
        StackPanelCMS.Children.Add(ScreensPanel)


        '3 - LAYERS
        LayersPanel = Nothing
        GC.Collect()

        LayersPanel = New ManagerEditorPanel("LAYERS")
        With LayersPanel
            .ManagerScroller.Content = New TextBlock With {.Text = "This features set are under development...", .Foreground = Brushes.White, .TextWrapping = TextWrapping.Wrap}
        End With
        StackPanelCMS.Children.Add(LayersPanel)


        '4 - BLOCKS MANAGER PANEL
        BlocksManager1 = Nothing
        BlocksPanel = Nothing
        GC.Collect()

        BlocksManager1 = New BlocksManager(Layer(0).Blocks, BlockTpl, SlideTpl) With {.ContentSyncPath = cms_content_sync_path}

        BlocksPanel = New ManagerEditorPanel("BLOCKS")
        With BlocksPanel
            .ManagerScroller.Content = BlocksManager1.BlocksPanel
            .ManagerButtonsArea.Children.Add(BlocksManager1.ButtonsStack)
            .EditorScroller.Content = BlocksManager1.BlockEditor1.BlockSetupPanel
            .EditorButtonsArea.Children.Add(BlocksManager1.BlockEditor1.ButtonsStack)
        End With
        StackPanelCMS.Children.Add(BlocksPanel)

        BlocksManager1.ReloadPanel()

        'BLOCK MANAGER PANEL ACTIONS
        With BlocksManager1
            AddHandler .ReloadAll, AddressOf cmsInit
            AddHandler .SaveBlockOnDrop, AddressOf BlockEditor_SaveBlockOnDrop
            AddHandler .xLbBtnAddNewBlock.MouseUp, AddressOf AddBlock
            AddHandler BlocksPanel.MouseUp, AddressOf ClickBlock
        End With

        With BlocksManager1.BlockEditor1
            AddHandler .ButtonSave.MouseUp, AddressOf BlockEditor_Save
            AddHandler .ButtonDelete.MouseUp, AddressOf BlockEditor_Delete
            AddHandler .ButtonSetupPlugin.MouseUp, AddressOf BlockEditor_SetupPlugin
        End With

        '5 - SLIDES MANAGER PANEL
        SlidesManager1 = Nothing
        SlidesPanel = Nothing
        GC.Collect()

        SlidesManager1 = New SlidesManager With {.Template = SlideTpl}

        SlidesPanel = New ManagerEditorPanel("PLAYLIST")
        With SlidesPanel
            .ManagerScroller.Content = SlidesManager1.WrapPanelItems
            .ManagerButtonsArea.Children.Add(SlidesManager1.WrapPanelItemsButtons)
            .EditorTitleArea.Children.Add(SlidesManager1.LabelInfo)
            .EditorScroller.Content = SlidesManager1.WrapPanelFeatures
            .EditorButtonsArea.Children.Add(SlidesManager1.WrapPanelButtons)
        End With
        StackPanelCMS.Children.Add(SlidesPanel)

        'SLIDES ACTIONS
        With SlidesManager1
            AddHandler .AddSlideButton.MouseUp, AddressOf AddSlideButton_MouseUp

            AddHandler .WrapPanelItems.MouseUp, AddressOf WrapPanelSlides_MouseUp
            AddHandler .ButtonSave.MouseUp, AddressOf SlideEditor_Save
            AddHandler .ButtonDelete.MouseUp, AddressOf RemoveSlide
        End With


        '6 - MEDIA MANAGER PANEL
        MediaManager1 = Nothing
        MediaPanel = Nothing
        GC.Collect()

        MediaManager1 = New MediaManager With {.Template = MediaTpl}

        MediaPanel = New ManagerEditorPanel("MEDIA", 240, 180)
        With MediaPanel
            .ManagerScroller.Content = MediaManager1.WrapPanelItems
            .ManagerButtonsArea.Children.Add(MediaManager1.WrapPanelItemsButtons)
            .EditorTitleArea.Children.Add(MediaManager1.LabelInfo)
            .EditorScroller.Content = MediaManager1.WrapPanelButtons
            '.EditorButtonsArea.Children.Add(MediaManager1.WrapPanelButtons)
        End With
        StackPanelCMS.Children.Add(MediaPanel)
        'StackPanelCMS.Children.Add(MediaManager1)

        'MEDIA ACTIONS
        With MediaManager1
            AddHandler .WrapPanelItems.MouseUp, AddressOf WrapPanelMedia_MouseUp

            AddHandler .AddMediaButton.MouseUp, AddressOf MediaManager_AddMedia
            AddHandler .NewImageButton.MouseUp, AddressOf MediaManager_NewImage

            AddHandler .ButtonSetSlide.MouseUp, AddressOf MediaEditor_SetSlide
            AddHandler .ButtonSetFront.MouseUp, AddressOf MediaEditor_SetFront
            AddHandler .ButtonSetMask.MouseUp, AddressOf MediaEditor_SetMask
            AddHandler .ButtonSetBack.MouseUp, AddressOf MediaEditor_SetBack
            AddHandler .ButtonRemove.MouseUp, AddressOf MediaEditor_Remove
        End With

        '7 - SET UPDATES PANEL
        GridCMSUpdates.Children.Clear()
        UpdatesPanel1 = Nothing
        GC.Collect()

        UpdatesPanel1 = New UpdatesPanel
        'GridSetup.ColumnDefinitions(2).Width = New GridLength(10)
        GridCMSUpdates.Children.Add(UpdatesPanel1)

        'UPDATES PANEL ACTIONS
        With UpdatesPanel1
            AddHandler .ButtonUpdate.MouseUp, AddressOf UpdatesPanel_Update
            AddHandler .ButtonReset.MouseUp, AddressOf UpdatesPanel_Reset
        End With

        'BLOCK SELECTION, LOADING SLIDES DATA
        If Not IsNothing(Layer(0).Blocks) Then
            SelectBlock(0)
        End If

    End Sub

    Private Sub PackageReload()
        SaveSetupIniFiles()
        Window_Loaded(Nothing, Nothing)
    End Sub


    '-------------------------------------------------------- B L O C K S --------------------------------------------------------

#Region "BLOCKS"

    'A D D  B L O C K  (BTN)
    Public Sub AddBlock()
        'UPD BLOCKS LIST WTH DEF DATA
        Dim bl_count As Integer = 0
        If Not IsNothing(Layer(0).Blocks) Then bl_count = Layer(0).Blocks.Count
        ReDim Preserve Layer(0).Blocks(bl_count)

        Dim b_w As Integer = 240
        Dim b_h As Integer = 240
        If Not IsNothing(Player1) Then
            b_w = Layer(0).Blocks(bl_count - 1).bWidth
            'Player1.GridClientBlocks.ActualWidth / 1
            b_h = Layer(0).Blocks(bl_count - 1).bHeight
            'Player1.GridClientBlocks.ActualHeight / 8
        End If

        Layer(0).Blocks(bl_count) = New Block("New Block " + CStr(bl_count + 1), "Block" + CStr(bl_count + 1),
                                              0, 0, b_w, b_h, "HOR", "", False, 0)
        Dim text_blocks_data() As String = {"Top text", 24, "#FFFFFFFF", "#66FFFFFF", "LEFT", _
                                            "New Block " + CStr(bl_count + 1), 64, "#FFFFFFFF", "#00000000", "CENTER", _
                                            "Bottom text", 16, "#FFFFFFFF", "#33FFFFFF", "RIGHT"}
        With Layer(0).Blocks(bl_count)
            .LoadTextBlocks(text_blocks_data)
            .ContentSourceRoot = cms_content_sync_path
            .ContentLocalRoot = content_local_path
            .WasUpdatedAtCMS = True
        End With

        'CREATE BLOCK FOLDER
        If Not Directory.Exists(content_local_path + Layer(0).Blocks(bl_count).bDir) Then
            Directory.CreateDirectory(content_local_path + Layer(0).Blocks(bl_count).bDir)
        End If

        'RELOAD VISUAL
        With BlocksManager1
            .Blocks = Layer(0).Blocks
            .ReloadPanel()
        End With
        SelectBlock(bl_count)

        'UPDATES LIST
        UpdatesPanel1.AddUpdate("[block] " + Layer(0).Blocks(bl_count).bTitle + " [added]")
        'GridSetup.ColumnDefinitions(2).Width = New GridLength(340)
        BlockEditor_Save()
    End Sub

    'C L I C K  B L O C K  (BTN)
    Private Sub ClickBlock(sender As Object, e As MouseButtonEventArgs)
        Dim obj As Grid = TryCast(e.Source, Grid)
        If Not IsNothing(Layer(0).Blocks) Then
            For i = 0 To Layer(0).Blocks.Count - 1
                Dim BlockIcon As Grid = BlocksPanel.FindName("BlockIcon" + CStr(i))

                Dim LbBlockOrderInfo As Label = TryCast(BlocksPanel.FindName("LbBlockOrderInfo" + CStr(i)), Label)
                Dim LbBlockSetupBtn As Label = TryCast(BlocksPanel.FindName("LbBlockSetupBtn" + CStr(i)), Label)
                Dim LbBlockDelBtn As Label = TryCast(BlocksPanel.FindName("LbBlockDelBtn" + CStr(i)), Label)

                If Not IsNothing(BlockIcon) And Not IsNothing(obj) Then
                    LbBlockOrderInfo.Cursor = Cursors.Arrow
                    LbBlockOrderInfo.Background = Brushes.DarkGray
                    LbBlockSetupBtn.Visibility = Visibility.Hidden
                    LbBlockDelBtn.Visibility = Visibility.Hidden

                    If BlockIcon.Name = obj.Name Then
                        'SELECT BLOCK
                        SelectBlock(i)
                        'PLAYER EDIT MODE SELECTION
                        If Not IsNothing(Player1) Then
                            If Player1.InteractionMode = "edit" Then Player1.EditorModeSelectBlockById(i)
                        End If
                    End If
                End If
            Next i
        End If

        'ICON BUTTONS
        Dim obj2 As Label = TryCast(e.Source, Label)
        If Not IsNothing(obj2) Then

            'DEL BTN CLICK
            If obj2.Name = "LbBlockDelBtn" + CStr(BlocksManager1.SelectedBlock) Then
                RemoveBlock()
            End If

            'SETUP BTN CLICK
            If obj2.Name = "LbBlockSetupBtn" + CStr(BlocksManager1.SelectedBlock) Then
                BlocksPanel.Editor_Collapse()
            End If

            'SELECT BTN CLICK
            For i = 0 To BlocksManager1.Blocks.Count - 1
                If obj2.Name = "LbBlockSelectBtn" + CStr(i) Then
                    MsgBox("Block #" + (i + 1).ToString + " was selected... multi-selection will be implemented at next versions")
                End If
            Next
        End If

    End Sub

    'S E L E C T  B L O C K
    Public Sub SelectBlock(ByVal index As Integer)

        'TRY TO SAVE IF SOME CHANGES WERE THERE
        BlockEditor_Save() '!!! ???

        'DESELECT ITEMS
        If Not IsNothing(Layer(0).Blocks) Then
            For i = 0 To Layer(0).Blocks.Count - 1
                Dim obj As Grid = BlocksPanel.FindName("BlockIcon" + CStr(i))
                If Not IsNothing(obj) Then
                    obj.HorizontalAlignment = HorizontalAlignment.Stretch
                    obj.Width = Double.NaN
                    obj.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#33000000"))
                End If
                'SELECTION FRAME
                Dim obj2 As Border = BlocksPanel.FindName("SelectionBorder" + CStr(i))
                If Not IsNothing(obj2) Then obj2.Visibility = Visibility.Hidden
                'SEL BTN
                Dim LbBlockSelectBtn As Label = TryCast(BlocksPanel.FindName("LbBlockSelectBtn" + CStr(i)), Label)
                LbBlockSelectBtn.Background = Brushes.DarkGray
                'DEL BTN
                Dim LbBlockDelBtn As Label = TryCast(BlocksPanel.FindName("LbBlockDelBtn" + CStr(i)), Label)
                LbBlockDelBtn.Visibility = Visibility.Hidden
                'SETUP BTN
                Dim LbBlockSetupBtn As Label = TryCast(BlocksPanel.FindName("LbBlockSetupBtn" + CStr(i)), Label)
                LbBlockSetupBtn.Visibility = Visibility.Hidden
                'ORDER BTN
                Dim LbBlockOrderInfo As Label = TryCast(BlocksPanel.FindName("LbBlockOrderInfo" + CStr(i)), Label)
                LbBlockOrderInfo.Cursor = Cursors.Arrow
                LbBlockOrderInfo.Background = Brushes.DarkGray
            Next
        End If

        'SELECT ITEM
        Dim SelectionBorder As Border = BlocksPanel.FindName("SelectionBorder" + CStr(index))
        If Not IsNothing(SelectionBorder) Then
            SelectionBorder.Visibility = Visibility.Visible
            SelectionBorder.BeginAnimation(Border.BorderThicknessProperty, _
                                            New ThicknessAnimation(New Thickness(15), New Thickness(5), TimeSpan.FromSeconds(0.5)))
        End If

        Dim BlockIcon As Grid = BlocksPanel.FindName("BlockIcon" + CStr(index))
        If Not IsNothing(BlockIcon) Then
            BlocksManager1.SelectedBlock = index
            BlockIcon.UpdateLayout()
            With BlockIcon
                .HorizontalAlignment = HorizontalAlignment.Right
                .Width = 0
                .Margin = New Thickness(0, 0, -9, 0)
            End With

            'SEL BTN
            Dim LbBlockSelectBtn As Label = TryCast(BlocksPanel.FindName("LbBlockSelectBtn" + CStr(index)), Label)
            LbBlockSelectBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#6ab4d8"))

            'DEL BTN
            Dim LbBlockDelBtn As Label = TryCast(BlocksPanel.FindName("LbBlockDelBtn" + CStr(index)), Label)
            LbBlockDelBtn.Visibility = Visibility.Visible

            'SETUP BTN
            Dim LbBlockSetupBtn As Label = TryCast(BlocksPanel.FindName("LbBlockSetupBtn" + CStr(index)), Label)
            LbBlockSetupBtn.Visibility = Visibility.Visible

            'ORDER BTN
            Dim LbBlockOrderInfo As Label = TryCast(BlocksPanel.FindName("LbBlockOrderInfo" + CStr(index)), Label)
            LbBlockOrderInfo.Cursor = Cursors.Hand
            LbBlockOrderInfo.Background = Brushes.Black
        End If

        'BLOCK INFO LABEL
        Dim slides_count As Integer = 0
        Dim tot_duration As Integer = 0
        If Not IsNothing(Layer(0).Blocks) Then
            If index < Layer(0).Blocks.Length Then
                If Not IsNothing(Layer(0).Blocks(index).Slides) Then
                    slides_count = Layer(0).Blocks(index).Slides.Count
                    For ii = 0 To Layer(0).Blocks(index).Slides.Count - 1
                        tot_duration += Layer(0).Blocks(index).Slides(ii).Duration
                    Next
                End If
            End If
        End If
        BlocksManager1.BlockEditor1.xLbInfo.Content = slides_count.ToString + " slides, total = " + tot_duration.ToString + " sec (excl. videos duration)"

        If Not IsNothing(Layer(0).Blocks) Then
            'LOAD BLOCK_EDITOR FROM TEMPLATE
            BlocksManager1.BlockEditor1.ReloadWrPanBlockSetup(Layer(0).Blocks, BlocksManager1.SelectedBlock)
            'HIGHLIGHT PLUGIN BUTTON
            If Layer(0).Blocks(BlocksManager1.SelectedBlock).bType = "PLUGIN" Then
                BlocksManager1.BlockEditor1.ButtonSetupPlugin.BeginAnimation(Label.OpacityProperty, New DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5)))
            Else
                BlocksManager1.BlockEditor1.ButtonSetupPlugin.Opacity = 0.25
                BlocksManager1.BlockEditor1.ButtonSetupPlugin.BeginAnimation(Label.OpacityProperty, Nothing)
            End If
            'LOAD SLIDES TO MANAGER
            With SlidesManager1
                .ReloadItemsPanel(Layer(0).Blocks, BlocksManager1.SelectedBlock)
                If Not IsNothing(Layer(0).Blocks) Then .LoadTemplateVisual(SlidesManager1.WrapPanelFeatures, Layer(0).Blocks(BlocksManager1.SelectedBlock), -1, "TbSlideSet")
                .LoadOrderVisual(SlidesManager1.WrapPanelFeatures, "", "TbSlideOrder")
            End With
            'LOAD MEDIA TO MANAGER
            With MediaManager1
                .ReloadItemsPanel(Layer(0).Blocks, BlocksManager1.SelectedBlock)
            End With
        End If

    End Sub

    'S A V E  B L O C K (BTN)
    Private Sub BlockEditor_Save()
        If BlocksManager1.SelectedBlock <> -1 Then

            'LABELS FOR UPDATES LIST
            For i = 0 To BlocksManager1.BlockTemplate.Settings.Count - 1
                Dim obj As Object = BlocksManager1.BlockEditor1.BlockSetupPanel.FindName("TbBlockSet" + CStr(i))
                Dim prev_value As String = Layer(0).Blocks(BlocksManager1.SelectedBlock).GetParamValueByIndex(i)
                If obj.Text <> prev_value Then
                    Dim elem_name As String = "block"
                    Dim param_name As String = BlocksManager1.BlockTemplate.Settings(i).title
                    Dim new_value = obj.Text
                    Dim id As Integer = BlocksManager1.SelectedBlock
                    If Not IsNothing(new_value) Then
                        If Layer(0).Blocks(BlocksManager1.SelectedBlock).GetParamValueByIndex(i) <> new_value.ToString Then
                            'UPDATES LIST LABEL
                            Dim update As String
                            update = "[" + elem_name + "]  " + CStr(id + 1) + "  [" + param_name + "]  " + Layer(0).Blocks(BlocksManager1.SelectedBlock).GetParamValueByIndex(i) + "  [>]  "
                            'NUMBERS INPUT
                            If BlocksManager1.BlockTemplate.Settings(i).type = "int" Then
                                If Not IsNumeric(new_value) Then
                                    obj.Focus()
                                    Exit Sub
                                End If
                                Dim int_max As Integer = CInt(BlocksManager1.BlockTemplate.Settings(i).minmax.Substring(BlocksManager1.BlockTemplate.Settings(i).minmax.IndexOf(",") + 1))
                                If CInt(new_value) > int_max Then new_value = int_max
                                Dim int_min As Integer = CInt(BlocksManager1.BlockTemplate.Settings(i).minmax.Substring(0, BlocksManager1.BlockTemplate.Settings(i).minmax.IndexOf(",") + 1))
                                If CInt(new_value) < int_min Then new_value = int_min
                                new_value = CInt(new_value)
                                obj.text = new_value
                            End If
                            'BOOLEAN INPUT
                            If BlocksManager1.BlockTemplate.Settings(i).type = "bool" Then new_value = CBool(new_value)
                            'COLOR INPUT
                            If BlocksManager1.BlockTemplate.Settings(i).type = "col" Then new_value = New SolidColorBrush(ColorConverter.ConvertFromString(new_value))
                            'SET PARAM VALUE
                            Layer(0).Blocks(BlocksManager1.SelectedBlock).SetParamValueByindex(i, new_value)

                            'RENAME BLOCK DIR
                            If param_name.ToUpper = "DIRECTORY" Then
                                CopyDirectory(content_local_path + prev_value, Me.content_local_path + new_value)
                                Directory.Delete(content_local_path + prev_value, True)
                            End If

                            'UPDATE
                            update += Layer(0).Blocks(BlocksManager1.SelectedBlock).GetParamValueByIndex(i)
                            UpdatesPanel1.AddUpdate(update)
                            Layer(0).Blocks(BlocksManager1.SelectedBlock).WasUpdatedAtCMS = True

                            BlocksManager1.Blocks = Layer(0).Blocks
                            BlocksManager1.ReloadPanel()
                        End If
                    End If
                End If
            Next

            'BLOCK ORDER
            Dim ord_obj As TextBox = TryCast(BlocksManager1.BlockEditor1.BlockSetupPanel.FindName("TextBoxBlockOrder"), TextBox)
            If Not IsNothing(ord_obj) Then
                If ord_obj.Text <> CInt(BlocksManager1.SelectedBlock + 1) And IsNumeric(ord_obj.Text) Then
                    Dim update As String
                    update = "[block] " + Layer(0).Blocks(BlocksManager1.SelectedBlock).bTitle + " [order] " _
                        + CStr(BlocksManager1.SelectedBlock + 1) + " [>] "
                    Dim new_order As Integer = CInt(ord_obj.Text) - 1
                    'REORDER ARRAYS
                    Layer(0).Blocks = ReplaceArrayElement(Layer(0).Blocks, GetType(Block), BlocksManager1.SelectedBlock, new_order)
                    Layer(0).Blocks(BlocksManager1.SelectedBlock).WasUpdatedAtCMS = True
                    update += CStr(new_order + 1)
                    UpdatesPanel1.AddUpdate(update)
                    BlocksManager1.Blocks = Layer(0).Blocks
                    BlocksManager1.ReloadPanel()
                    BlocksManager1.SelectedBlock = -1
                End If
            End If

            SaveBlocksDataToXML(BlocksManager1.ContentLocalPath, Layer(0).Blocks)

        End If
    End Sub

    Private Sub BlockEditor_SaveBlockOnDrop()
        'Layer(0).Blocks(BlocksManager1.SelectedBlock).bOrder = BlocksManager1.To_Order
        If BlocksManager1.To_Order <> -1 Then
            Dim ord_obj As TextBox = TryCast(BlocksManager1.BlockEditor1.BlockSetupPanel.FindName("TextBoxBlockOrder"), TextBox)
            If Not IsNothing(ord_obj) Then ord_obj.Text = BlocksManager1.To_Order
            BlockEditor_Save()
        End If
    End Sub

    'D E L E T E  B L O C K  (BTN)
    Private Sub BlockEditor_Delete()
        RemoveBlock()
    End Sub

    'D E L E T E  B L O C K 
    Private Sub RemoveBlock()
        Dim sel_bl As Integer = BlocksManager1.SelectedBlock
        If sel_bl <> -1 Then
            If MsgBox("Delete block: " + Layer(0).Blocks(sel_bl).bTitle + _
                      " (Folder: " + Layer(0).Blocks(sel_bl).bDir + ") ?", MsgBoxStyle.OkCancel) = MsgBoxResult.Ok Then
                'UPDATES LABEL
                Dim update As String
                update = "[block] " + Layer(0).Blocks(sel_bl).bDir
                'REMOVE LOCAL DIR
                Directory.Delete(content_local_path + Layer(0).Blocks(sel_bl).bDir, True)
                'REMOVE FROM ARRAY
                Layer(0).Blocks = RemoveElementFromArray(Layer(0).Blocks, sel_bl, GetType(Block))
                'RELOAD PANEL
                BlocksManager1.Blocks = Layer(0).Blocks
                BlocksManager1.ReloadPanel()
                'UPDATES LABEL
                update += " [deleted]"
                UpdatesPanel1.AddUpdate(update)
                SomeBlockWasUpdated = True
                BlocksManager1.SelectedBlock = -1
                SaveBlocksDataToXML(BlocksManager1.ContentLocalPath, Layer(0).Blocks)
            End If
        End If
    End Sub

    'S E T U P  P L U G I N  (BTN)
    Private Sub BlockEditor_SetupPlugin()
        Dim sel_bl As Integer = BlocksManager1.SelectedBlock
        If Layer(0).Blocks(sel_bl).bType = "PLUGIN" Then
            'inTime PLUGIN SETUP
            If Layer(0).Blocks(sel_bl).bSource.ToUpper = "INTIME" Then
                Dim update As String
                update = "[block] " + Layer(0).Blocks(sel_bl).bDir

                Dim inTimePluginWindow As New PluginWindow(Me.content_local_path + Layer(0).Blocks(sel_bl).bDir + "\")
                inTimePluginWindow.ShowDialog()
                Layer(0).Blocks(sel_bl).WasUpdatedAtCMS = True

                If inTimePluginWindow.ResultValue <> "" Then
                    update += " [was updated]"
                    UpdatesPanel1.AddUpdate(update)
                End If
            End If

        End If
    End Sub

#End Region

    '-------------------------------------------------------- U P D A T E S --------------------------------------------------------

#Region "UPDATES"

    Dim SomeBlockWasUpdated As Boolean = False

    'S Y N C  U P D A T E S (BTN)
    Private Sub UpdatesPanel_Update()
        'SETUP.XML
        'SaveSetupDataToXML(cms_content_sync_path)

        'BLOCKS.XML - SAVE TO SHARED FOLDER
        'check if any updates was done:

        For i = 0 To Layer(0).Blocks.Length - 1
            If Layer(0).Blocks(i).WasUpdatedAtCMS = True Then SomeBlockWasUpdated = True
        Next
        'if so, then save data:
        If SomeBlockWasUpdated Then SaveBlocksDataToXML(BlocksManager1.ContentSyncPath, Layer(0).Blocks)

        'SLIDES.XML - SAVE TO LOC FOLDER
        If Not IsNothing(Layer(0).Blocks) Then
            For i = 0 To Layer(0).Blocks.Count - 1
                If Layer(0).Blocks(i).SlidesWereUpdatedAtCMS Then SaveSlidesDataToXML(BlocksManager1.ContentLocalPath, Layer(0).Blocks(i))
            Next
        End If

        'MEDIA AND FOLDERS - FROM LOCAL TO SHARED
        If Not IsNothing(Layer(0).Blocks) Then
            For i = 0 To Layer(0).Blocks.Count - 1
                If Not IsNothing(Layer(0).Blocks(i)) Then
                    Layer(0).Blocks(i).ContentDirsBackSync()
                End If
            Next
        End If

        'CLEAR UPDATES LIST
        UpdatesPanel1.StackPanelUpdates.Children.Clear()
        SomeBlockWasUpdated = False
        'INIT ALL
        'cmsInit()
    End Sub

    'C L E A R  U P D A T E S (BTN)
    Private Sub UpdatesPanel_Reset()
        UpdatesPanel1.StackPanelUpdates.Children.Clear()
        SomeBlockWasUpdated = False
        cmsInit()
    End Sub

    'S A V E  B L O C K S . X M L
    Private Sub SaveBlocksDataToXML(ByVal _dir As String, ByVal blocks_data() As Block)
        'BLOCK DATA
        If Not IsNothing(blocks_data) Then
            'File.Delete(_dir + "blocks.xml")
            Using xmlw As XmlWriter = XmlWriter.Create(_dir + "blocks.xml", New XmlWriterSettings() With {.Indent = True})
                xmlw.WriteStartDocument()
                xmlw.WriteStartElement("blocks")
                For i = 0 To blocks_data.Count - 1
                    xmlw.WriteStartElement("block")
                    With blocks_data(i)
                        Dim set_value() As String = {.bTitle, .bDir, .bLeft, .bTop, .bWidth.ToString, .bHeight.ToString, _
                             .bType, .bSource, .bDots.ToString, .bMargin.ToString, _
                             .TopText.Text, .TopText.TextSize.ToString, .TopText.FrontColor.ToString, .TopText.BackColor.ToString, .TopText.TextAlign, _
                             .MidText.Text, .MidText.TextSize.ToString, .MidText.FrontColor.ToString, .MidText.BackColor.ToString, .MidText.TextAlign, _
                             .BtmText.Text, .BtmText.TextSize.ToString, .BtmText.FrontColor.ToString, .BtmText.BackColor.ToString, .BtmText.TextAlign, _
                                                     .bSimpleTouch.ToString, .bTimeLimit.ToString, .bFromTime, .bToTime, .bLinkTo}
                        For j = 0 To BlockTpl.Settings.Count - 1
                            xmlw.WriteStartElement(BlockTpl.Parameters(0)) 'SET
                            xmlw.WriteAttributeString(BlockTpl.Parameters(1), BlockTpl.Settings(j).id) 'ID
                            xmlw.WriteAttributeString(BlockTpl.Parameters(3), set_value(j)) 'VALUE
                            xmlw.WriteEndElement()
                        Next
                    End With
                    xmlw.WriteEndElement()
                Next i
                xmlw.WriteEndElement()
                xmlw.WriteEndDocument()
            End Using
        End If
    End Sub

#End Region

    '-------------------------------------------------------- M E D I A --------------------------------------------------------

#Region "MEDIA"

    'ADD MEDIA
    Private Sub MediaManager_AddMedia(sender As Object, e As MouseButtonEventArgs)
        If BlocksManager1.SelectedBlock > -1 Then
            Dim filter_str As String = "All supported formats "
            filter_str += "(*.jpg,*.png,*.avi,*.gif)|*.jpg;*.png;*.avi|JPG Images (*.jpg)|*.jpg|"
            filter_str += "PNG Images (*.png)|*.png|Videos (*.avi)|*.avi|Animation (*.gif)|*.gif"
            Dim dlg As New Microsoft.Win32.OpenFileDialog() With {.Multiselect = True, _
                                                                  .Filter = filter_str}
            If dlg.ShowDialog() Then
                Dim filepathes() As String = dlg.FileNames
                For Each filepath As String In filepathes
                    Dim filename As String = GetFileName(filepath)
                    Dim fileext As String = filename.Substring(filename.Length - 3, 3)
                    Dim filerename As String = InputBox("File name:", , filename.Substring(0, filename.LastIndexOf(".")))
                    'If Not Directory.Exists(Me.ContentSyncPath + content_dir_name) Then Directory.CreateDirectory(content_sync_path + content_dir_name)
                    If filerename <> "" Then File.Copy(filepath, Me.content_local_path + Layer(0).Blocks(BlocksManager1.SelectedBlock).bDir + "\" + filerename + "." + fileext, True)
                    'LOG UPDATE
                    Dim update As String = "[media file] " + filerename + "." + fileext + " [added]"
                    UpdatesPanel1.AddUpdate(update)
                Next
                MediaManager1.ReloadItemsPanel(Layer(0).Blocks, BlocksManager1.SelectedBlock)
            End If
        End If
    End Sub

    'CREATE NEW IMAGE
    Private Sub MediaManager_NewImage(sender As Object, e As MouseButtonEventArgs)
        If BlocksManager1.SelectedBlock > -1 Then
            Dim obj_w = CInt(InputBox("New empty image WIDTH", , Layer(0).Blocks(BlocksManager1.SelectedBlock).bWidth))
            Dim obj_h = CInt(InputBox("New empty image HEIGHT", , Layer(0).Blocks(BlocksManager1.SelectedBlock).bHeight))
            Dim dlg As New Forms.ColorDialog
            Dim result As Forms.DialogResult = dlg.ShowDialog()

            Dim drvis As New DrawingVisual
            Using drcont As DrawingContext = drvis.RenderOpen
                drcont.DrawRectangle(New SolidColorBrush(Color.FromArgb(dlg.Color.A, dlg.Color.R, dlg.Color.G, dlg.Color.B)), _
                                     Nothing, New Rect(New Point(), New Size(obj_w, obj_h)))
            End Using

            Dim rtb = New RenderTargetBitmap(obj_w, obj_h, 96D, 96D, PixelFormats.Default)
            rtb.Render(drvis)
            Dim encoder = New JpegBitmapEncoder
            encoder.Frames.Add(BitmapFrame.Create(rtb))

            Try
                Dim filerename As String = "new_" + DateTime.Now.ToString("HHMMss") + "_" + DateTime.Now.ToString("ddMMyyyy")
                Dim fileext As String = "jpg"
                Dim fs = File.Open(Me.content_local_path + Layer(0).Blocks(BlocksManager1.SelectedBlock).bDir + "\" + filerename + "." + fileext, FileMode.Create)
                encoder.Save(fs)
                fs.Close()
                'LOG UPDATE
                Dim update As String = "[media file] " + filerename + "." + fileext + " [added]"
                UpdatesPanel1.AddUpdate(update)
                MediaManager1.ReloadItemsPanel(Layer(0).Blocks, BlocksManager1.SelectedBlock)
            Catch ex As Exception
            End Try
        End If
    End Sub

    'C L I C K  M E D I A (BTN) : SELECT, ADD, NEW, DEL, VIEW
    Private Sub WrapPanelMedia_MouseUp(sender As Object, e As MouseButtonEventArgs)

        'SELECT MEDIA
        Dim obj As Grid = TryCast(e.Source, Grid)

        For i = 0 To MediaManager1.WrapPanelItems.Children.Count - 1

            Dim MediaIcon As Grid = MediaManager1.WrapPanelItems.FindName("MediaIcon" + CStr(i))
            Dim MediaSelectionBorder As Border = MediaManager1.WrapPanelItems.FindName("MediaSelectionBorder" + CStr(i))
            Dim MediaDelBtn As Label = MediaManager1.WrapPanelItems.FindName("MediaDelBtn" + CStr(i))
            Dim LbMediaSetupBtn As Label = TryCast(MediaManager1.WrapPanelItems.FindName("LbMediaSetupBtn" + CStr(i)), Label)
            Dim MediaAddBtn As Label = MediaManager1.WrapPanelItems.FindName("MediaAddBtn" + CStr(i))

            If Not IsNothing(MediaIcon) And Not IsNothing(obj) Then

                'ALL ITEMS
                With MediaIcon
                    .HorizontalAlignment = HorizontalAlignment.Stretch
                    .Width = Double.NaN
                    .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#33000000"))
                End With

                If Not IsNothing(MediaSelectionBorder) Then MediaSelectionBorder.Visibility = Visibility.Hidden
                If Not IsNothing(MediaDelBtn) Then MediaDelBtn.Visibility = Visibility.Hidden
                If Not IsNothing(LbMediaSetupBtn) Then LbMediaSetupBtn.Visibility = Visibility.Hidden
                If Not IsNothing(MediaAddBtn) Then MediaAddBtn.Visibility = Visibility.Hidden

                'SELECTED ITEM
                If MediaIcon.Name = obj.Name And BlocksManager1.SelectedBlock <> -1 Then

                    If Not IsNothing(MediaSelectionBorder) Then
                        MediaSelectionBorder.Visibility = Visibility.Visible
                        MediaSelectionBorder.BeginAnimation(Border.BorderThicknessProperty,
                                                            New ThicknessAnimation(New Thickness(10), New Thickness(5),
                                                                                   TimeSpan.FromSeconds(0.25)))
                    End If

                    With MediaIcon
                        .UpdateLayout()
                        .HorizontalAlignment = HorizontalAlignment.Right
                        .Width = 0
                        .Margin = New Thickness(0, 0, -9, 0)
                    End With

                    MediaManager1.SelectedMedia = i
                    MediaManager1.SelectedFilesCount = 1

                    MediaManager1.LabelInfo.Content = "" + GetSlideInfo(content_local_path + Layer(0).Blocks(BlocksManager1.SelectedBlock).bDir + "\" + MediaManager1.MediaFiles(i))
                    MediaDelBtn.Visibility = Visibility.Visible
                    If Not IsNothing(LbMediaSetupBtn) Then LbMediaSetupBtn.Visibility = Visibility.Visible
                    MediaAddBtn.Visibility = Visibility.Visible
                End If
            End If
        Next i

        'ADD BUTTON
        Dim obj1 As Label = TryCast(e.Source, Label)
        If Not IsNothing(obj1) Then
            If obj1.Name = "MediaAddBtn" + CStr(MediaManager1.SelectedMedia) Then
                MediaEditor_SetSlide()
            End If
        End If

        'DELETE BUTTON
        Dim obj2 As Label = TryCast(e.Source, Label)
        If Not IsNothing(obj2) Then
            If obj2.Name = "MediaDelBtn" + CStr(MediaManager1.SelectedMedia) Then
                MediaEditor_Remove()
            End If
        End If

        'SETUP BTN CLICK
        If Not IsNothing(obj2) Then
            If obj2.Name = "LbMediaSetupBtn" + CStr(MediaManager1.SelectedMedia) Then
                MediaPanel.Editor_Collapse()
                Exit Sub
            End If
        End If

        'IMAGE ICON CLICK - VIEW AT EXT APP
        Dim MediaImageIcon As Image = TryCast(e.Source, Image)
        If Not IsNothing(MediaImageIcon) Then
            If MediaImageIcon.Name = "MediaImageIcon" + CStr(MediaManager1.SelectedMedia) Then
                Process.Start(content_local_path + Layer(0).Blocks(BlocksManager1.SelectedBlock).bDir + "\" + MediaManager1.MediaFiles(MediaManager1.SelectedMedia))
            End If
        End If

        'MULTI SELECT --> SHIFT BTN
        Dim sel_obj As Border = TryCast(e.Source, Border)
        If Not IsNothing(sel_obj) Then
            For i = 0 To MediaManager1.MediaFiles.Count - 1
                If sel_obj.Name = "MediaSelBrd" + CStr(i) Then

                    Dim MediaIcon As Grid = MediaManager1.WrapPanelItems.FindName("MediaIcon" + CStr(i))
                    Dim MediaDelBtn As Label = MediaManager1.WrapPanelItems.FindName("MediaDelBtn" + CStr(i))
                    Dim MediaAddBtn As Label = MediaManager1.WrapPanelItems.FindName("MediaAddBtn" + CStr(i))

                    If sel_obj.Background.ToString = "#FFA9A9A9" Then
                        sel_obj.Background = Brushes.Yellow
                        MediaManager1.SelectedFilesCount += 1

                        'SELECT ITEMS
                        MediaIcon.VerticalAlignment = VerticalAlignment.Bottom
                        MediaIcon.Height = 5
                        MediaIcon.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FF000000"))
                        MediaIcon.BeginAnimation(Grid.OpacityProperty, New DoubleAnimation(0, 1, New TimeSpan(0, 0, 0, 0, 250)))

                        MediaManager1.LabelInfo.Content = "Media info: " + MediaManager1.SelectedFilesCount.ToString + " files selected"
                        MediaDelBtn.Visibility = Visibility.Hidden
                        MediaAddBtn.Visibility = Visibility.Hidden

                    Else
                        sel_obj.Background = Brushes.DarkGray
                        MediaManager1.SelectedFilesCount -= 1

                        'DESELECT ITEMS
                        MediaIcon.VerticalAlignment = VerticalAlignment.Stretch
                        MediaIcon.Height = Double.NaN
                        MediaIcon.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#22000000"))
                        If Not IsNothing(MediaDelBtn) Then MediaDelBtn.Visibility = Visibility.Hidden
                        If Not IsNothing(MediaAddBtn) Then MediaAddBtn.Visibility = Visibility.Hidden
                        MediaManager1.LabelInfo.Content = "Media info: "
                    End If
                End If
            Next
        End If

        'If MediaManager.SelectedFilesCount > 1 Then
        '    For i = 0 To MediaManager.MediaFiles.Count - 1
        '        Dim MediaIcon As Grid = MediaManager.WrapPanelItems.FindName("MediaIcon" + CStr(i))
        '        Dim MediaDelBtn As Label = MediaManager.WrapPanelItems.FindName("MediaDelBtn" + CStr(i))
        '        Dim MediaAddBtn As Label = MediaManager.WrapPanelItems.FindName("MediaAddBtn" + CStr(i))
        '        Dim MediaSelBrd As Border = MediaManager.WrapPanelItems.FindName("MediaSelBrd" + CStr(i))
        '        'ALL ITEMS
        '        MediaIcon.VerticalAlignment = VerticalAlignment.Stretch
        '        MediaIcon.Height = Double.NaN
        '        MediaIcon.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#22000000"))
        '        If Not IsNothing(MediaDelBtn) Then MediaDelBtn.Visibility = Visibility.Hidden
        '        If Not IsNothing(MediaAddBtn) Then MediaAddBtn.Visibility = Visibility.Hidden
        '    Next
        'End If


        'EDITOR STATE
        If MediaManager1.EditorCollapsed Then MediaManager1.CollapseEditor()
    End Sub


    'ADD EMPTY SLIDE
    Private Sub AddSlideButton_MouseUp() '!!!-!!!
        Dim sel_bl As Integer = BlocksManager1.SelectedBlock
        'If MediaManager1.SelectedMedia <> -1 Then
        'BLOCK UPDATE
        Dim new_slide As New Slide(SlidesManager1.Template.DefaultValues)
        new_slide.Source = "" 'MediaManager1.MediaFiles(MediaManager1.SelectedMedia)
        Dim to_pos As Integer = 0
        If Not IsNothing(Layer(0).Blocks(sel_bl).Slides) Then to_pos = Layer(0).Blocks(sel_bl).Slides.Count()
        new_slide.Title += " " + CStr(to_pos + 1)
        new_slide.Order = to_pos + 1
        'If MediaManager1.MediaFiles(MediaManager1.SelectedMedia).ToUpper.EndsWith("AVI") Then
        '    new_slide.Title = "New Video " + CStr(to_pos + 1)
        '    new_slide.Duration = 0
        'End If
        Layer(0).Blocks(sel_bl).Slides = InsertArrayElement(Layer(0).Blocks(sel_bl).Slides, GetType(Slide), to_pos, new_slide)
        Layer(0).Blocks(sel_bl).SlidesWereUpdatedAtCMS = True

        'LOG UPDATE
        Dim update As String
        Dim pos As Integer = Layer(0).Blocks(sel_bl).Slides.Count - 1
        With Layer(0).Blocks(sel_bl).Slides(pos)
            update = "[slide] " + .Source + " [added] " + CStr(.Order + 1) + " : " + .Title
        End With
        UpdatesPanel1.AddUpdate(update)

        SaveSlidesDataToXML(BlocksManager1.ContentLocalPath, Layer(0).Blocks(sel_bl))

        'PANELS RELOAD
        SlidesManager1.ReloadItemsPanel(Layer(0).Blocks, BlocksManager1.SelectedBlock)
        'MediaManager1.ReloadItemsPanel(Layer(0).Blocks, BlocksManager1.SelectedBlock)
        'End If
    End Sub


    'S E T  N E W  S L I D E (BTN)
    Private Sub MediaEditor_SetSlide() '!!!-!!!
        Dim sel_bl As Integer = BlocksManager1.SelectedBlock
        If MediaManager1.SelectedMedia <> -1 Then
            'BLOCK UPDATE
            Dim new_slide As New Slide(SlidesManager1.Template.DefaultValues)
            new_slide.Source = MediaManager1.MediaFiles(MediaManager1.SelectedMedia)
            Dim to_pos As Integer = 0
            If Not IsNothing(Layer(0).Blocks(sel_bl).Slides) Then to_pos = Layer(0).Blocks(sel_bl).Slides.Count()
            new_slide.Title += " " + CStr(to_pos + 1)
            new_slide.Order = to_pos + 1
            If MediaManager1.MediaFiles(MediaManager1.SelectedMedia).ToUpper.EndsWith("AVI") Then
                new_slide.Title = "New Video " + CStr(to_pos + 1)
                new_slide.Duration = 0
            End If
            Layer(0).Blocks(sel_bl).Slides = InsertArrayElement(Layer(0).Blocks(sel_bl).Slides, GetType(Slide), to_pos, new_slide)
            Layer(0).Blocks(sel_bl).SlidesWereUpdatedAtCMS = True

            'LOG UPDATE
            Dim update As String
            Dim pos As Integer = Layer(0).Blocks(sel_bl).Slides.Count - 1
            With Layer(0).Blocks(sel_bl).Slides(pos)
                update = "[media] " + .Source + " [added as slide] " + CStr(.Order + 1) + " : " + .Title
            End With
            UpdatesPanel1.AddUpdate(update)

            SaveSlidesDataToXML(BlocksManager1.ContentLocalPath, Layer(0).Blocks(sel_bl))

            'PANELS RELOAD
            SlidesManager1.ReloadItemsPanel(Layer(0).Blocks, BlocksManager1.SelectedBlock)
            MediaManager1.ReloadItemsPanel(Layer(0).Blocks, BlocksManager1.SelectedBlock)
        End If
    End Sub

    'A S S I G N  M E D I A  S U B
    Private Sub AssignMedia(ByVal el_code As String, el_title As String) '!!!-!!!
        Dim sel_med As Integer = MediaManager1.SelectedMedia
        Dim sel_bl As Integer = BlocksManager1.SelectedBlock
        Dim sel_sl As Integer = SlidesManager1.SelectedSlide
        If sel_med <> -1 Then
            Dim filetyp As String = ""
            If MediaManager1.MediaFiles(sel_med).EndsWith("png") Then filetyp = ".png"
            If MediaManager1.MediaFiles(sel_med).EndsWith("jpg") Then filetyp = ".jpg"

            If filetyp <> "" Then
                FileSystem.Rename(content_local_path + Layer(0).Blocks(sel_bl).bDir + "\" + MediaManager1.MediaFiles(sel_med), _
                              content_local_path + Layer(0).Blocks(sel_bl).bDir + "\" + el_code + filetyp)
                'LOG UPDATE
                UpdatesPanel1.AddUpdate("[media] " + MediaManager1.MediaFiles(sel_med) + " [assigned to " + el_title + "]")
                'RELOAD PANEL
                MediaManager1.ReloadItemsPanel(Layer(0).Blocks, BlocksManager1.SelectedBlock)
            End If
            
        End If
    End Sub

    'SET FOREGROUND
    Private Sub MediaEditor_SetFront()
        AssignMedia("fg", "foreground")
    End Sub

    'SET MASK
    Private Sub MediaEditor_SetMask()
        AssignMedia("mask", "mask")
    End Sub

    'SET BACKGROUND
    Private Sub MediaEditor_SetBack()
        AssignMedia("bg", "background")
    End Sub

    'D E L E T E  M E D I A  F I L E (BTN)
    Private Sub MediaEditor_Remove()
        Dim sel_med As Integer = MediaManager1.SelectedMedia
        If sel_med <> -1 Then
            If MsgBox("Delete file: " + MediaManager1.MediaFiles(sel_med).ToString + " ?", MsgBoxStyle.OkCancel) = MsgBoxResult.Ok Then
                'DELETE LOCAL FILE
                File.Delete(content_local_path + Layer(0).Blocks(BlocksManager1.SelectedBlock).bDir + "\" + MediaManager1.MediaFiles(sel_med))
                'LOG UPDATE
                UpdatesPanel1.AddUpdate("[media file] " + MediaManager1.MediaFiles(sel_med) + " [deleted]")
                'CLEAR ARRAY
                RemoveElementFromArray(MediaManager1.MediaFiles, sel_med, GetType(String))
                'RELOAD PANEL
                MediaManager1.ReloadItemsPanel(Layer(0).Blocks, BlocksManager1.SelectedBlock)
            End If
        End If
        '!!! DP RELOADED / ADD EVENT TO LOG FOR SYNC !!!
    End Sub

#End Region

    '-------------------------------------------------------- S L I D E S --------------------------------------------------------

#Region "SLIDES"

    'C L I C K  S L I D E (BTN)
    Private Sub WrapPanelSlides_MouseUp(sender As Object, e As MouseButtonEventArgs)

        Dim seleceted_block As Block = Layer(0).Blocks(BlocksManager1.SelectedBlock)

        'DELETE BUTTON CLICK
        Dim obj2 As Label = TryCast(e.Source, Label)
        If Not IsNothing(obj2) Then
            If obj2.Name = "SlideDelBtn" + CStr(SlidesManager1.SelectedSlide) Then
                RemoveSlide(Nothing, Nothing)
                Exit Sub
            End If
        End If

        'IMAGE ICON CLICK
        Dim SlideImageIcon As Image = TryCast(e.Source, Image)
        If Not IsNothing(SlideImageIcon) Then
            If SlideImageIcon.Name = "SlideImageIcon" + CStr(SlidesManager1.SelectedSlide) Then
                Process.Start(content_local_path + seleceted_block.bDir + "\" + seleceted_block.Slides(SlidesManager1.SelectedSlide).Source)
                Exit Sub
            End If
        End If

        'SETUP BTN CLICK
        If Not IsNothing(obj2) Then
            If obj2.Name = "LbSlideSetupBtn" + CStr(SlidesManager1.SelectedSlide) Then
                SlidesPanel.Editor_Collapse()
                Exit Sub
            End If
        End If

        SlideEditor_Save(Nothing, Nothing)

        'SELECTION
        Dim obj As Grid = TryCast(e.Source, Grid)
        If IsNothing(seleceted_block.Slides) Then Exit Sub

        For i = 0 To seleceted_block.Slides.Count - 1

            'DE-SELECTION
            Dim SlideIcon As Grid = SlidesManager1.WrapPanelItems.FindName("SlideIcon" + CStr(i))
            If Not IsNothing(SlideIcon) Then
                With SlideIcon
                    .HorizontalAlignment = HorizontalAlignment.Stretch
                    .Width = Double.NaN
                    .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#33000000"))
                End With
            End If
            'SELECTION FRAME
            Dim SlideSelectionBorder As Border = SlidesManager1.WrapPanelItems.FindName("SlideSelectionBorder" + CStr(i))
            If Not IsNothing(SlideSelectionBorder) Then SlideSelectionBorder.Visibility = Visibility.Hidden
            'SEL BTN
            Dim LbSlideSelectBtn As Label = TryCast(SlidesManager1.WrapPanelItems.FindName("LbSlideSelectBtn" + CStr(i)), Label)
            If Not IsNothing(LbSlideSelectBtn) Then LbSlideSelectBtn.Background = Brushes.DarkGray
            'DEL BTN
            Dim SlideDelBtn As Label = TryCast(SlidesManager1.WrapPanelItems.FindName("SlideDelBtn" + CStr(i)), Label)
            If Not IsNothing(SlideDelBtn) Then SlideDelBtn.Visibility = Visibility.Hidden
            'SETUP BTN
            Dim LbSlideSetupBtn As Label = TryCast(SlidesManager1.WrapPanelItems.FindName("LbSlideSetupBtn" + CStr(i)), Label)
            If Not IsNothing(LbSlideSetupBtn) Then LbSlideSetupBtn.Visibility = Visibility.Hidden
            'ORDER BTN
            Dim LbBlockOrderInfo As Label = TryCast(SlidesManager1.WrapPanelItems.FindName("LbSlideOrderInfo" + CStr(i)), Label)
            If Not IsNothing(LbBlockOrderInfo) Then
                LbBlockOrderInfo.Cursor = Cursors.Arrow
                LbBlockOrderInfo.Background = Brushes.DarkGray
            End If

            If Not IsNothing(SlideIcon) And Not IsNothing(obj) Then

                'SELECTED ITEM
                If SlideIcon.Name = obj.Name Then
                    SlidesManager1.SelectedSlide = i

                    If Not IsNothing(SlideSelectionBorder) Then
                        SlideSelectionBorder.Visibility = Visibility.Visible
                        SlideSelectionBorder.BeginAnimation(Border.BorderThicknessProperty, _
                                                        New ThicknessAnimation(New Thickness(10), New Thickness(5), TimeSpan.FromSeconds(0.25)))
                    End If

                    If Not IsNothing(LbSlideSelectBtn) Then LbSlideSelectBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#6ab4d8"))

                    With SlideIcon
                        .UpdateLayout()
                        .HorizontalAlignment = HorizontalAlignment.Right
                        .Width = 0
                        .Margin = New Thickness(0, 0, -9, 0)
                    End With

                    If Not IsNothing(SlideDelBtn) Then SlideDelBtn.Visibility = Visibility.Visible
                    If Not IsNothing(LbSlideSetupBtn) Then LbSlideSetupBtn.Visibility = Visibility.Visible
                    If Not IsNothing(LbBlockOrderInfo) Then
                        LbBlockOrderInfo.Cursor = Cursors.Hand
                        LbBlockOrderInfo.Background = Brushes.Black
                    End If

                    'UPDATE SLIDE EDITOR FIELDS

                    'MAIN FILEDS FROM TEMPLATE
                    SlidesManager1.LoadTemplateVisual(SlidesManager1.WrapPanelFeatures, seleceted_block, i, "TbSlideSet")
                    'BLOCK ORDER TEXTBOX
                    SlidesManager1.LoadOrderVisual(SlidesManager1.WrapPanelFeatures, CStr(i + 1), "TbSlideOrder")

                    'SLIDE INFO LABEL
                    SlidesManager1.LabelInfo.Content = "" _
                        + GetSlideInfo(content_local_path + seleceted_block.bDir + "\" + seleceted_block.Slides(i).Source)
                    SlidesManager1.StackPanelEditorFields.BeginAnimation(Grid.OpacityProperty, _
                                                New DoubleAnimation(SlidesManager1.StackPanelEditorFields.Opacity, 1, New TimeSpan(0, 0, 0, 0, 250)))
                End If
            End If
        Next i

    End Sub

    'R E M O V E  S L I D E (BTN)
    Private Sub RemoveSlide(sender As Object, e As MouseButtonEventArgs)
        Dim sel_bl As Integer = BlocksManager1.SelectedBlock
        Dim sel_sl As Integer = SlidesManager1.SelectedSlide
        If sel_sl <> -1 Then
            If MsgBox("Remove slide: " + Layer(0).Blocks(sel_bl).Slides(sel_sl).Title + " ?", MsgBoxStyle.OkCancel) = MsgBoxResult.Ok Then
                Dim update As String
                update = "[slide] " + CStr(sel_sl + 1) + " [REMOVED] "
                Layer(0).Blocks(sel_bl).Slides = RemoveElementFromArray(Layer(0).Blocks(sel_bl).Slides, sel_sl, GetType(Slide))
                Layer(0).Blocks(sel_bl).SlidesWereUpdatedAtCMS = True
                'REORDER
                For i = 0 To Layer(0).Blocks(sel_bl).Slides.Count - 1
                    Layer(0).Blocks(sel_bl).Slides(i).Order = i + 1
                Next
                sel_sl = -1

                SaveSlidesDataToXML(BlocksManager1.ContentLocalPath, Layer(0).Blocks(sel_bl))

                UpdatesPanel1.AddUpdate(update)
                SlidesManager1.ReloadItemsPanel(Layer(0).Blocks, BlocksManager1.SelectedBlock)
            End If
        End If
    End Sub

    'S A V E  S L I D E S . X M L
    Public Sub SaveSlidesDataToXML(ByVal _dir As String, ByVal block_data As Block)
        Dim sel_bl As Integer = BlocksManager1.SelectedBlock
        Dim xml_filename As String = _dir + block_data.bDir + "\" + "slides.xml"
        Using xmlw As XmlWriter = XmlWriter.Create(xml_filename, New XmlWriterSettings() With {.Indent = True})
            xmlw.WriteStartDocument()
            xmlw.WriteStartElement("slides")

            If Not IsNothing(block_data.Slides) Then
                For i = 0 To block_data.Slides.Count - 1
                    xmlw.WriteStartElement("slide")
                    With block_data.Slides(i)
                        Dim set_value() As String = {.Source, .Title, .Duration, .Mode, _
                                                     .TopText.Text, .TopText.TextSize, .TopText.FrontColor.ToString, .TopText.BackColor.ToString, .TopText.TextAlign, _
                                                     .MidText.Text, .MidText.TextSize, .MidText.FrontColor.ToString, .MidText.BackColor.ToString, .MidText.TextAlign, _
                                                     .BtmText.Text, .BtmText.TextSize, .BtmText.FrontColor.ToString, .BtmText.BackColor.ToString, .BtmText.TextAlign, _
                                                     .TimeLimit.ToString, .FromTime.ToString, .ToTime.ToString, .Location}
                        For j = 0 To SlidesManager1.Template.Settings.Count - 1
                            xmlw.WriteStartElement("set")
                            xmlw.WriteAttributeString("id", SlidesManager1.Template.Settings(j).id)
                            xmlw.WriteAttributeString("value", set_value(j))
                            xmlw.WriteEndElement()
                        Next
                    End With
                    xmlw.WriteEndElement()
                Next i
            End If

            xmlw.WriteEndElement()
            xmlw.WriteEndDocument()
        End Using
    End Sub

    'S L I D E  I N F O
    Function GetSlideInfo(ByVal slide_file As String) As String
        Dim result As String = "File not found"
        If File.Exists(slide_file) Then
            If slide_file.ToUpper.EndsWith("JPG") Then result = "JPG image"
            If slide_file.ToUpper.EndsWith("AVI") Then result = "AVI video"
            Dim info As New FileInfo(slide_file)
            result += ", " + (Math.Round(info.Length / 1024, 0)).ToString + " KB"
            If slide_file.ToUpper.EndsWith("JPG") Then
                Dim BitmapImg As New BitmapImage
                BitmapImg.BeginInit()
                BitmapImg.CacheOption = BitmapCacheOption.OnLoad
                BitmapImg.UriSource = New Uri(slide_file)
                BitmapImg.EndInit()
                result += ", " + (BitmapImg.Width * BitmapImg.DpiX / 96).ToString + " x " _
                    + (BitmapImg.Height * BitmapImg.DpiY / 96).ToString + " px"
                BitmapImg = Nothing
            End If
        End If
        Return result
    End Function

    'S A V E  S L I D E (BTN)
    Private Sub SlideEditor_Save(sender As Object, e As MouseButtonEventArgs)

        Dim sel_bl As Integer = BlocksManager1.SelectedBlock
        Dim sel_sl As Integer = SlidesManager1.SelectedSlide
        Dim was_updated As Boolean = False
        If sel_sl <> -1 And sel_bl <> -1 Then

            Layer(0).Blocks(sel_bl).SlidesWereUpdatedAtCMS = True

            'LABELS FOR UPDATES LIST / UPDATE VALUE
            For i = 0 To SlidesManager1.Template.Settings.Count - 1
                Dim obj As Object = SlidesManager1.WrapPanelFeatures.FindName("TbSlideSet" + CStr(i))
                Dim prev_value As String = Layer(0).Blocks(sel_bl).GetSlideParamValueByIndex(i, sel_sl)
                If obj.Text <> prev_value Then
                    Dim elem_name As String = "slide"
                    Dim tpl_setting As TemplateSettings = SlidesManager1.Template.Settings(i)
                    'Dim param As Object
                    Dim param_name As String = tpl_setting.title
                    Dim new_value As Object = obj.Text
                    Dim id As Integer = SlidesManager1.SelectedSlide
                    If Not IsNothing(new_value) Then
                        If prev_value <> new_value.ToString Then
                            'UPDATES LIST LABEL
                            Dim update As String
                            update = "[" + elem_name + "]  " + CStr(id + 1) + "  [" + param_name + "]  " + prev_value + "  [>]  "
                            'NUMBERS INPUT
                            If tpl_setting.type = "int" Then
                                If Not IsNumeric(new_value) Then
                                    obj.Focus()
                                    Exit Sub
                                End If
                                Dim int_max As Integer = CInt(tpl_setting.minmax.Substring(tpl_setting.minmax.IndexOf(",") + 1))
                                If CInt(new_value) > int_max Then new_value = int_max
                                Dim int_min As Integer = CInt(tpl_setting.minmax.Substring(0, tpl_setting.minmax.IndexOf(",") + 1))
                                If CInt(new_value) < int_min Then new_value = int_min
                                new_value = CInt(new_value)
                                obj.text = new_value
                            End If
                            'BOOLEAN INPUT
                            If tpl_setting.type = "bool" Then new_value = CBool(new_value)
                            'COLOR INPUT
                            If tpl_setting.type = "col" Then new_value = New SolidColorBrush(ColorConverter.ConvertFromString(new_value))
                            'SET PARAM VALUE
                            Layer(0).Blocks(sel_bl).SetSlideParamValueByindex(i, new_value, sel_sl)

                            'UPDATE SOURCE FILENAME
                            If param_name.ToUpper = "SOURCE" Then
                                File.Copy(Me.content_local_path + Layer(0).Blocks(BlocksManager1.SelectedBlock).bDir + "\" + prev_value, _
                                          Me.content_local_path + Layer(0).Blocks(BlocksManager1.SelectedBlock).bDir + "\" + new_value, True)
                                File.Delete(Me.content_local_path + Layer(0).Blocks(BlocksManager1.SelectedBlock).bDir + "\" + prev_value)
                            End If

                            'SHOW UPDATE AT LIST
                            Dim updated_value As String = Layer(0).Blocks(BlocksManager1.SelectedBlock).GetSlideParamValueByIndex(i, sel_sl)
                            update += updated_value
                            UpdatesPanel1.AddUpdate(update)
                            was_updated = True
                        End If
                    End If
                End If
            Next

            'UPDATE ORDER
            Dim ord_obj As TextBox = TryCast(SlidesManager1.WrapPanelFeatures.FindName("TbSlideOrder"), TextBox)
            If Not IsNothing(ord_obj) Then
                If ord_obj.Text <> CInt(Layer(0).Blocks(sel_bl).Slides(sel_sl).Order) And IsNumeric(ord_obj.Text) Then
                    Dim update As String
                    update = "[slide] " + Layer(0).Blocks(sel_bl).Slides(sel_sl).Title + " [order] " _
                        + CStr(Layer(0).Blocks(sel_bl).Slides(sel_sl).Order) + " [>] "
                    Dim new_order As Integer = CInt(ord_obj.Text) - 1
                    'REORDER ARRAYS
                    Layer(0).Blocks(sel_bl).Slides = ReplaceArrayElement(Layer(0).Blocks(sel_bl).Slides, GetType(Slide), sel_sl, new_order)
                    'UPDATES LIST
                    update += CStr(new_order + 1)
                    UpdatesPanel1.AddUpdate(update)
                    was_updated = True
                End If
            End If

            If was_updated Then
                SlidesManager1.ReloadItemsPanel(Layer(0).Blocks, BlocksManager1.SelectedBlock)
                MediaManager1.ReloadItemsPanel(Layer(0).Blocks, BlocksManager1.SelectedBlock)
            End If

        End If
    End Sub

#End Region

    '---------------------------------------------------------------------

    Private Sub SaveSetupIniFiles()
        Try
            'CMS SETTINGS
            Dim cms_objIniFile As New IniFile(app_root + "cms_setup.ini")
            With cms_objIniFile
                .WriteString("Setup", "ContentSyncPath", PackagesManager1.SelectedPackagePath)
                .WriteString("Setup", "PackagesRoot", PackagesManager1.CustomRoot)
            End With

            'PLAYER SETTINGS
            Dim pl_objIniFile As New IniFile(app_root + "player_setup.ini")
            With pl_objIniFile
                .WriteString("Setup", "ContentSyncPath", PackagesManager1.SelectedPackagePath)
                .WriteString("Setup", "Location", Player1.Location)
                .WriteString("Setup", "StandAlone", Player1.StandAlone)

                .WriteString("TestMode", "Enable", Player1.TestEnable)
                .WriteString("TestMode", "Width", Player1.TestWidth)
                .WriteString("TestMode", "Height", Player1.TestHeight)
            End With
        Catch ex As Exception
            AddToLog("CMS: SAVE SETUP.INI ERROR")
        End Try
    End Sub

    'L O A D  C M S  I N I  S E T T I N G S
    Private Sub cmsLoadIni()

        'NO INI FILE
        If Not File.Exists(app_root + "cms_setup.ini") Then
            'NO SETUP.INI FILE
            AddToLog("CMS: ERR: missing setup.ini")
            MsgBox("Missing cms_setup.ini file. Please select content folder for setup...")
            'SELECT CONTENT FOLDER
            Dim dlg As New Forms.FolderBrowserDialog
            Dim result As Forms.DialogResult = dlg.ShowDialog()
            If (result = Forms.DialogResult.OK) Then
                cms_content_sync_path = dlg.SelectedPath
                SaveSetupIniFiles()
            ElseIf (result = Forms.DialogResult.Cancel) Then
                MsgBox("Anyway you can setup it from cms_etup.ini file...")
                AddToLog("CMS: NO CONTENT FOLDER SELECTED")
                Application.Current.Shutdown()
            End If
        End If

        'HAVE CMS_SETUP.INI
        If File.Exists(app_root + "cms_setup.ini") Then
            Try
                Dim objIniFile As New IniFile(app_root + "cms_setup.ini")
                'MAIN SYNC PATH
                cms_content_sync_path = objIniFile.GetString("Setup", "ContentSyncPath", "")
                If cms_content_sync_path <> "" Then
                    If Not cms_content_sync_path.EndsWith("\") Then cms_content_sync_path += "\"
                Else
                    cms_content_sync_path = content_localsource_path
                End If
                If Not Directory.Exists(cms_content_sync_path) Then Directory.CreateDirectory(cms_content_sync_path)
                'PACKAGES ROOT
                cms_content_packages_root = objIniFile.GetString("Setup", "PackagesRoot", "")
            Catch ex As Exception
                AddToLog("CMS: ERR at Load INI Settings: " + ex.ToString)
            End Try
        Else
            MsgBox("We have some problem with ini file...")
            AddToLog("CMS: ERR at Load INI Settings: no setup.ini")
            Application.Current.Shutdown()
        End If

        'If File.Exists(app_dir + "cms_setup.ini") Then
        '    Try
        '        Dim objIniFile As New IniFile(app_dir + "cms_setup.ini")
        '        UseSync = objIniFile.GetBoolean("Setup", "ContentSync", False)
        '        'GET dBx PATH FROM APP_DATA
        '        Dim dBxPath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dropbox\\host.db")
        '        Dim lines As String() = File.ReadAllLines(dBxPath)
        '        Dim dbBase64Text As Byte() = Convert.FromBase64String(lines(1))
        '        Dim folderPath As String = System.Text.ASCIIEncoding.ASCII.GetString(dbBase64Text)
        '        If folderPath <> "" Then cms_content_root = folderPath Else cms_content_root = objIniFile.GetString("Setup", "ContentSyncRoot", "C:\Dropbox")
        '        If Not cms_content_root.EndsWith("\") Then cms_content_root += "\"
        '        'dB CONTENT DIR (e.g. LOC1)
        '        cms_content_dir = objIniFile.GetString("Setup", "ContentSyncDir", "iNFOSignage.Content")
        '        cms_content_sync_path = cms_content_root + cms_content_dir + "\"
        '        If Not Directory.Exists(cms_content_sync_path) Then Directory.CreateDirectory(cms_content_sync_path)
        '    Catch ex As Exception
        '        AddToLog("Load CMS settings ERR: " + ex.ToString)
        '    End Try
        'Else
        '    AddToLog("ERR: cms_missing setup.ini")
        'End If
    End Sub


    'S E E T I N G S  P A N E L  V I S I B I L I T Y
    Private Sub LabelSettingHeader_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles LabelSettingHeader.MouseUp
        If LabelSettingHeader.Foreground.ToString = "#FFFFFFFF" Then
            LabelSettingHeader.Foreground = Brushes.DarkGray
            WrapPanelSettings.Visibility = Windows.Visibility.Collapsed
            LabelBtnSettingsSave.Visibility = Windows.Visibility.Collapsed
            LabelBtnSettingsReset.Visibility = Windows.Visibility.Collapsed
        Else
            LabelSettingHeader.Foreground = Brushes.White
            WrapPanelSettings.Visibility = Windows.Visibility.Visible
            LabelBtnSettingsSave.Visibility = Windows.Visibility.Visible
            LabelBtnSettingsReset.Visibility = Windows.Visibility.Visible
            LoadSettingPanel()
        End If
    End Sub

    'L O A D  S E T T I N G S  P A N E L
    Private Sub LoadSettingPanel()
        If cms_setup_xml_loaded Then
            WrapPanelSettings.Children.Clear()
            For i = 0 To cmsSetupParams.Count - 1
                Dim StackPanelParam As New StackPanel
                StackPanelParam.Orientation = Orientation.Vertical
                StackPanelParam.Margin = New Thickness(5)
                StackPanelParam.MinWidth = 150
                Dim LabelParam As New Label
                LabelParam.Foreground = Brushes.White
                LabelParam.Content = cmsSetupParams(i).title
                If cmsSetupParams(i).hint <> "" Then LabelParam.ToolTip = cmsSetupParams(i).hint
                StackPanelParam.Children.Add(LabelParam)
                Dim ParamValue As Object = Nothing
                If cmsSetupParams(i).type = "str" Or cmsSetupParams(i).type = "int" Then
                    ParamValue = New TextBox
                    If cmsSetupParams(i).id = "content" Then ParamValue.isEnabled = False
                    ParamValue.Text = cmsSetupParams(i).value
                End If
                If cmsSetupParams(i).type = "bool" Then
                    ParamValue = New BooleanButton
                    ParamValue.Text = cmsSetupParams(i).value
                    If cmsSetupParams(i).value = "True" Then ParamValue.Foreground = Brushes.GreenYellow _
                        Else ParamValue.Foreground = Brushes.DarkOrange
                End If
                If Not IsNothing(WrapPanelSettings.FindName("ParamValue" + CStr(i))) Then
                    WrapPanelSettings.UnregisterName("ParamValue" + CStr(i))
                End If
                If Not IsNothing(ParamValue) Then
                    ParamValue.Name = "ParamValue" + CStr(i)
                    StackPanelParam.Children.Add(ParamValue)
                    WrapPanelSettings.RegisterName(ParamValue.Name, ParamValue)
                End If
                WrapPanelSettings.Children.Add(StackPanelParam)
            Next
        End If
    End Sub

    'S A V E  S E T U P  X M L
    Public Sub SaveSetupDataToXML(ByVal _dir As String)
        Dim settings As XmlWriterSettings = New XmlWriterSettings() With {.Indent = True}
        Using writer As XmlWriter = XmlWriter.Create(_dir + "setup.xml", settings)
            With writer
                .WriteStartDocument()
                .WriteStartElement("settings")
                For i = 0 To cmsSetupParams.Count - 1
                    .WriteStartElement("param")
                    .WriteAttributeString("type", cmsSetupParams(i).type)
                    .WriteAttributeString("id", cmsSetupParams(i).id)
                    .WriteElementString("title", cmsSetupParams(i).title)
                    .WriteElementString("hint", cmsSetupParams(i).hint)
                    .WriteElementString("value", cmsSetupParams(i).value)
                    .WriteEndElement()
                Next i
                .WriteEndElement()
                .WriteEndDocument()
            End With
        End Using
    End Sub

    'S A V E  S E T U P (BTN)
    Private Sub LabelBtnSettingsSave_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles LabelBtnSettingsSave.MouseUp
        Try
            For i = 0 To cmsSetupParams.Count - 1
                Dim obj As Object = WrapPanelSettings.FindName("ParamValue" + CStr(i)) '!!!
                'Add CheckSetupData
                If obj.text <> cmsSetupParams(i).value Then
                    Dim LabelUpdates As New Label
                    LabelUpdates.Content = "[" + cmsSetupParams(i).title + "] " + cmsSetupParams(i).value + " [>] " + obj.text
                    'StackPanelUpdates.Children.Add(LabelUpdates)
                    cmsSetupParams(i).value = obj.text
                End If
            Next
            'ApplySetupData ??? TMP
            For i = 0 To cmsSetupParams.Count - 1
                If cmsSetupParams(i).id = "content" Then content_dir_name = cmsSetupParams(i).value
                'If params(i).id = "logo" Then 
                'If params(i).id = "timedate" Then 
                If cmsSetupParams(i).id = "feed" Then use_twitter = CBool(cmsSetupParams(i).value)
                'If params(i).id = "eff" Then slides_effect = CInt(params(i).value)
                If cmsSetupParams(i).id = "vol" Then avi_volume = CInt(cmsSetupParams(i).value)
                'If params(i).id = "emg" Then 
            Next
        Catch ex As Exception
        End Try
    End Sub

    'R E S E T  S E T T I N G S (BTN)
    Private Sub LabelBtnSettingsReset_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles LabelBtnSettingsReset.MouseUp
        LoadXmlSettings(cms_content_sync_path, cmsSetupParams, cms_setup_xml_loaded, content_local_dir)
        LoadSettingPanel()
    End Sub

    'M I N I M I Z E (BTN)
    Private Sub MinimizeBtn_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles MinimizeBtn.MouseUp
        Application.Current.MainWindow.WindowState = Windows.WindowState.Minimized
        Me.WindowState = Windows.WindowState.Minimized
    End Sub

    'C L O S E (BTN)
    Private Sub CloseBtn_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles CloseBtn.MouseUp
        Application.Current.Shutdown()
    End Sub

    'S E L E C T  C O N T E N T  S O U R C E
    Private Sub TbRoot_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles TbRoot.MouseUp
        'SELECT CONTENT FOLDER
        Dim dlg As New Forms.FolderBrowserDialog 'With {}
        Dim result As Forms.DialogResult = dlg.ShowDialog()
        If (result = Forms.DialogResult.OK) Then
            TbRoot.Text = dlg.SelectedPath
        End If
    End Sub

    Private Sub UpdateContentSource()
        cms_content_sync_path = TbRoot.Text
        'SAVE INI
        Try
            Dim objIniFile As New IniFile(app_root + "cms_setup.ini")
            objIniFile.WriteString("Setup", "ContentSyncPath", cms_content_sync_path)
        Catch ex As Exception
            AddToLog("CMS: SAVE SETUP.INI ERROR")
        End Try
        cmsInit()
    End Sub

    'PANELS TOGGLE
    Private Sub StackPanelPanelsControl_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles StackPanelPanelsControl.MouseUp
        Dim obj As Label = TryCast(e.Source, Label)
        Dim obj_labels() As String = {"Packages", "Screens", "Layers", "Blocks", "Playlist", "Media"}
        If Not IsNothing(obj) Then
            Dim sel_itm As Integer = -1
            For i = 0 To obj_labels.Length - 1
                If obj.Content = obj_labels(i) Then
                    sel_itm = i
                End If
            Next
            Select Case sel_itm
                Case 0 : TogglePanelVisibility(PackagesPanel, obj)
                Case 1 : TogglePanelVisibility(ScreensPanel, obj)
                Case 2 : TogglePanelVisibility(LayersPanel, obj)
                Case 3 : TogglePanelVisibility(BlocksPanel, obj)
                Case 4 : TogglePanelVisibility(SlidesPanel, obj)
                Case 5 : TogglePanelVisibility(MediaPanel, obj)
            End Select
        End If
    End Sub
    Private Sub TogglePanelVisibility(ByVal panel As Grid, ByVal button As Label)
        If panel.IsEnabled Then
            'panel.BeginAnimation(Grid.WidthProperty, New DoubleAnimation(panel.ActualWidth, 0, TimeSpan.FromSeconds(0.25)))
            panel.Visibility = Visibility.Collapsed
            panel.IsEnabled = False
            button.Background = Brushes.Gray
        Else
            panel.Visibility = Visibility.Visible
            panel.IsEnabled = True
            button.Background = Brushes.White
            panel.Opacity = 0
            'panel.BeginAnimation(Grid.WidthProperty, Nothing)
            'panel.BeginAnimation(Grid.WidthProperty, New DoubleAnimation(0, panel.MaxWidth, TimeSpan.FromSeconds(0.25)))
            panel.BeginAnimation(Grid.OpacityProperty, Nothing)
            panel.BeginAnimation(Grid.OpacityProperty, New DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5)))  'With {.BeginTime = TimeSpan.FromSeconds(0.25)}
            'Dim transf As New TranslateTransform With {.X = -10}
            'panel.RenderTransform = transf
            'transf.BeginAnimation(TranslateTransform.XProperty, New DoubleAnimation(transf.X, 0, TimeSpan.FromSeconds(0.5)))
        End If
    End Sub

    Private Sub SwitchSelector(ByRef obj1 As Label, ByRef obj2 As Label)
        Dim anim1 As New ColorAnimation(Colors.Gray, ColorConverter.ConvertFromString("#6ab4d8"), TimeSpan.FromSeconds(0.5))
        Dim col1 As New SolidColorBrush
        col1.BeginAnimation(SolidColorBrush.ColorProperty, anim1)

        obj1.Foreground = Brushes.White
        obj1.Background = col1 'New SolidColorBrush(ColorConverter.ConvertFromString("#6ab4d8"))

        Dim anim2 As New ColorAnimation(ColorConverter.ConvertFromString("#6ab4d8"), Colors.Gray, TimeSpan.FromSeconds(0.25))
        Dim col2 As New SolidColorBrush
        col2.BeginAnimation(SolidColorBrush.ColorProperty, anim2)

        obj2.Foreground = Brushes.DarkGray
        obj2.Background = col2 'Brushes.Gray
    End Sub

    Dim player_source_mode As String = "preview"

    Private Sub LbPreviewMode_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles LbPreviewMode.MouseUp
        SwitchSelector(LbPreviewMode, LbLiveMode)
        player_source_mode = "preview"
        LoadPlayer()
    End Sub

    Private Sub LbLiveMode_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles LbLiveMode.MouseUp
        SwitchSelector(LbLiveMode, LbPreviewMode)
        player_source_mode = "live"
        LoadPlayer()
    End Sub

    Private Sub LoadPlayer()
        If Not IsNothing(Player1) Then
            RemoveHandler Player1.BlockSelected, AddressOf Player_BlockSelected
            RemoveHandler Player1.BlockResized, AddressOf Player_BlockResized
            Player1 = Nothing
            GridPlayerBack.Children.Clear()
            GC.Collect()
        End If

        'Dim player_content As String = ""
        'If player_source_mode = "preview" Then player_content = content_local_path
        'If player_source_mode = "live" Then player_content = ""

        Player1 = New Player(content_local_path) With {.Margin = New Thickness(10), .InteractionMode = player_interaction_mode, .SelectedBlock = BlocksManager1.SelectedBlock}
        AddHandler Player1.BlockSelected, AddressOf Player_BlockSelected
        AddHandler Player1.BlockResized, AddressOf Player_BlockResized
        GridPlayerBack.Children.Add(Player1)

        If LbOriginalSize.Foreground.Equals(Brushes.White) Then LbOriginalSize_MouseUp(Nothing, Nothing)
    End Sub

    Private Sub LbTouchMode_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles LbTouchMode.MouseUp
        SwitchSelector(LbTouchMode, LbEditMode)
        player_interaction_mode = "touch"
        LoadPlayer()
    End Sub

    Private Sub LbEditMode_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles LbEditMode.MouseUp
        SwitchSelector(LbEditMode, LbTouchMode)
        player_interaction_mode = "edit"
        LoadPlayer()
    End Sub

    Private Sub Player_BlockSelected()
        SelectBlock(Player1.SelectedBlock)
    End Sub

    Private Sub Player_BlockResized()
        BlocksManager1.BlockEditor1.ReloadWrPanBlockSetup(Player1.Screen(0).Blocks, BlocksManager1.SelectedBlock)
    End Sub

    'PLAYER - FIT TO SCREEN
    Private Sub LbFitScreen_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles LbFitScreen.MouseUp
        SwitchSelector(LbFitScreen, LbOriginalSize)
        Player1.Stretch = Stretch.Uniform
    End Sub

    'PLAYER - ORIGINAL SIZE
    Private Sub LbOriginalSize_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles LbOriginalSize.MouseUp
        SwitchSelector(LbOriginalSize, LbFitScreen)
        Player1.Stretch = Stretch.None
    End Sub
End Class