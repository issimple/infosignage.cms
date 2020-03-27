Imports System.Windows.Media.Animation
Imports System.IO


Public Class MediaManager

    Dim StackPanelManager As New StackPanel With {.Orientation = Orientation.Vertical}

    Public WrapPanelItems As New WrapPanel With {.HorizontalAlignment = HorizontalAlignment.Left, .Margin = New Thickness(0)}
    Public WrapPanelItemsButtons As New WrapPanel

    Dim StackPanelEditor As New StackPanel With {.Orientation = Orientation.Vertical}
    Dim LabelEditorHeader As New Label With {.Content = "FEATURES", .Style = CType(res_dict("SubHeader"), Style)}

    Public StackPanelEditorFields As New StackPanel With {.Orientation = Orientation.Vertical}
    Public WrapPanelFeatures As New WrapPanel With {.Orientation = Orientation.Horizontal}

    Public WrapPanelButtons As New WrapPanel With {.Orientation = Orientation.Horizontal}

    Public ButtonSetSlide As New Label With {.Content = "SET AS SLIDE", .Foreground = Brushes.GreenYellow, .Style = CType(res_dict("LabelButton"), Style)}
    Public ButtonSetBack As New Label With {.Content = "SET BACKGROUND", .Foreground = Brushes.LightGray, .Style = CType(res_dict("LabelButton"), Style)}
    Public ButtonSetMask As New Label With {.Content = "SET MASK", .Foreground = Brushes.LightGray, .Style = CType(res_dict("LabelButton"), Style)}
    Public ButtonSetFront As New Label With {.Content = "SET FOREGROUND", .Foreground = Brushes.LightGray, .Style = CType(res_dict("LabelButton"), Style)}
    Public ButtonRemove As New Label With {.Content = "REMOVE FILE", .Foreground = Brushes.DarkOrange, .Style = CType(res_dict("LabelButton"), Style)}

    Public LabelInfo As New Label With {.Content = "", .Style = CType(res_dict("InfoLabel"), Style)}

    'TEMPLATE
    Public Template As MediaTemplate

    Public SelectedMedia As Integer = -1
    Public MediaFiles() As String
    Public SelectedFilesCount As Integer
    Public SelectedFiles() As String

    'UI STATES
    Public EditorCollapsed As Boolean = False
    Public ManagerCollapsed As Boolean = False

    'BUTTONS
    Public AddMediaButton As New Label With {.Content = "ADD FILE...", .Foreground = Brushes.GreenYellow, .Style = CType(res_dict("LabelButton"), Style)}
    Public NewImageButton As New Label With {.Content = "EMPTY IMAGE...", .Foreground = Brushes.GreenYellow, .Style = CType(res_dict("LabelButton"), Style)}

    Public Sub New()
        'MAIN BUTTONS PANEL
        With WrapPanelItemsButtons
            .Children.Add(AddMediaButton)
            .Children.Add(NewImageButton)
        End With

        'EDITOR PANEL FIELDS
        'StackPanelEditorFields.Children.Add(WrapPanelFeatures) '<-- PARAMETERS
        With WrapPanelButtons.Children
            .Add(ButtonSetSlide)
            .Add(ButtonSetBack)
            .Add(ButtonSetMask)
            .Add(ButtonSetFront)
            .Add(ButtonRemove)
        End With
        'StackPanelEditorFields.Children.Add(WrapPanelButtons)
        'EDITOR PANEL
        StackPanelEditor.Children.Add(LabelEditorHeader)
        StackPanelEditor.Children.Add(StackPanelEditorFields)
        'StackPanelEditor.Children.Add(LabelInfo)
        'MANGER PANEL
        'StackPanelManager.Children.Add(LabelHeader)
        'StackPanelManager.Children.Add(WrapPanelItems) '<-- SLIDES
        StackPanelManager.Children.Add(StackPanelEditor)
        'FINAL GRID
        'Me.Children.Add(StackPanelManager)
        'ACTIONS
        'AddHandler LabelHeader.MouseUp, AddressOf CollapseManager
        AddHandler LabelEditorHeader.MouseUp, AddressOf CollapseEditor
    End Sub


    'ITEMS ICONS
    Public Sub ReloadItemsPanel(ByVal _cmsBlocks() As Block, ByVal sel_bl As Integer)

        'CLEAR NAMES
        With WrapPanelItems
            For i = 0 To 99
                If Not IsNothing(.FindName("MediaSelectionBorder" + CStr(i))) Then .UnregisterName("MediaSelectionBorder" + CStr(i))
                If Not IsNothing(.FindName("MediaIcon" + CStr(i))) Then .UnregisterName("MediaIcon" + CStr(i))
                If Not IsNothing(.FindName("MediaDelBtn" + CStr(i))) Then .UnregisterName("MediaDelBtn" + CStr(i))
                If Not IsNothing(.FindName("LbMediaSetupBtn" + CStr(i))) Then .UnregisterName("LbMediaSetupBtn" + CStr(i))
                If Not IsNothing(.FindName("MediaAddBtn" + CStr(i))) Then .UnregisterName("MediaAddBtn" + CStr(i))
            Next i
            'If Not IsNothing(.FindName("AddMediaButton")) Then .UnregisterName("AddMediaButton")
            'If Not IsNothing(.FindName("NewImageButton")) Then .UnregisterName("NewImageButton")
            .Children.Clear()
            GC.Collect()
        End With

        'LOAD ICONS
        If Not IsNothing(_cmsBlocks) And sel_bl <> -1 Then

            Dim icon_file() As String = getFiles(app_dir + "cms_content\" + _cmsBlocks(sel_bl).bDir, "*.jpg|*.png|*.avi|*.gif", SearchOption.AllDirectories)

            If Not IsNothing(icon_file) Then
                For i = 0 To icon_file.Count - 1
                    ReDim Preserve MediaFiles(i)
                    MediaFiles(i) = GetFileName(icon_file(i))

                    'MAIN GRID
                    Dim GridIcon As New Grid With {.Width = 100, .Margin = New Thickness(5), .Background = Brushes.White}

                    Dim MediaSelectionBorder As New Border With {.HorizontalAlignment = HorizontalAlignment.Stretch,
                                                                 .VerticalAlignment = VerticalAlignment.Stretch,
                                                                 .Name = "MediaSelectionBorder" + CStr(i),
                                                                 .BorderThickness = New Thickness(5),
                                                                 .Margin = New Thickness(-5),
                                                                 .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#6ab4d8")),
                                                                 .Visibility = Visibility.Hidden}

                    'TITLE - FILENAME
                    Dim LabelIconFile As New Label
                    With LabelIconFile
                        .VerticalAlignment = VerticalAlignment.Top
                        .HorizontalAlignment = HorizontalAlignment.Left
                        .Content = GetFileName(icon_file(i))
                        .Foreground = Brushes.DarkGray
                        .Margin = New Thickness(15, 0, 0, 0)
                    End With

                    'IMAGE ICON
                    Dim ImageIcon As New Image With {.VerticalAlignment = VerticalAlignment.Top, .Margin = New Thickness(5, 30, 5, 5)}
                    If Not IsNothing(icon_file(i)) Then
                        If icon_file(i).ToUpper.EndsWith("JPG") Or _
                            icon_file(i).ToUpper.EndsWith("PNG") Or _
                            icon_file(i).ToUpper.EndsWith("GIF") Then
                            Dim BitmapImg As New BitmapImage
                            Try
                                With BitmapImg
                                    .BeginInit()
                                    .CacheOption = BitmapCacheOption.OnLoad
                                    .UriSource = New Uri(icon_file(i))
                                    .DecodePixelWidth = GridIcon.Width
                                    '.DecodePixelHeight = 120
                                    .EndInit()
                                End With
                            Catch ex As Exception
                            End Try
                            'GIF
                            If icon_file(i).ToUpper.EndsWith("GIF") Then _
                                WpfAnimatedGif.ImageBehavior.SetAnimatedSource(ImageIcon, BitmapImg)
                            'JPG
                            ImageIcon.Source = BitmapImg
                        End If
                        If icon_file(i).ToUpper.EndsWith("AVI") Then
                            ImageIcon.Height = 55
                            'video preview missing
                        End If
                    End If
                    ImageIcon.Name = "MediaImageIcon" + CStr(i)

                    'VIDEO FILE LABEL
                    Dim LabelVideoFile As New Label
                    With LabelVideoFile
                        .VerticalAlignment = VerticalAlignment.Center
                        .HorizontalContentAlignment = HorizontalAlignment.Center
                        .Content = "VIDEO"
                        .FontSize = 24
                        .Foreground = Brushes.DarkGray
                    End With

                    'ASSIGNED FILES (ORDER LABEL)
                    Dim LbIconOrder As New Label With {.Background = Brushes.DarkGray, .Foreground = Brushes.White,
                                                       .HorizontalAlignment = HorizontalAlignment.Left,
                                                       .VerticalAlignment = VerticalAlignment.Top}
                    If Not IsNothing(_cmsBlocks(sel_bl).Slides) Then
                        For j = 0 To _cmsBlocks(sel_bl).Slides.Count - 1
                            If GetFileName(_cmsBlocks(sel_bl).Slides(j).Source) = GetFileName(icon_file(i)) Then
                                LbIconOrder.Content = _cmsBlocks(sel_bl).Slides(j).Order
                            End If
                        Next j
                    End If

                    Dim LbMediaSelectBtn As New Label With {.VerticalAlignment = Windows.VerticalAlignment.Top,
                                                            .HorizontalAlignment = Windows.HorizontalAlignment.Right,
                                                            .Content = ChrW(10004), .Foreground = Brushes.White,
                                                            .Background = Brushes.DarkGray, .Cursor = Cursors.Hand,
                                                            .Visibility = Visibility.Visible, .Name = "LbMediaSelectBtn" + CStr(i)}
                    'BG MASK FG LABEL
                    With LbIconOrder
                        If GetFileName(icon_file(i)) = "bg.png" Then
                            .Background = Brushes.DarkGray
                            .Foreground = Brushes.White
                            .HorizontalAlignment = HorizontalAlignment.Right
                            .VerticalAlignment = VerticalAlignment.Bottom
                            .Content = "BG"
                        End If
                        If GetFileName(icon_file(i)) = "mask.png" Then
                            .Background = Brushes.DarkGray
                            .Foreground = Brushes.White
                            .HorizontalAlignment = HorizontalAlignment.Right
                            .VerticalAlignment = VerticalAlignment.Bottom
                            .Content = "MASK"
                        End If
                        If GetFileName(icon_file(i)) = "fg.png" Then
                            .Background = Brushes.DarkGray
                            .Foreground = Brushes.White
                            .HorizontalAlignment = HorizontalAlignment.Right
                            .VerticalAlignment = VerticalAlignment.Bottom
                            .Content = "FG"
                        End If
                    End With

                    'ADD BTN
                    Dim MediaAddBtn As New Label With {.VerticalAlignment = VerticalAlignment.Center,
                                                       .HorizontalAlignment = HorizontalAlignment.Left,
                                                       .Content = "+", .Foreground = Brushes.White, .Cursor = Cursors.Hand,
                                                       .Background = Brushes.Green, .Visibility = Visibility.Hidden,
                                                       .Name = "MediaAddBtn" + CStr(i)}
                    'DEL BTN
                    Dim MediaDelBtn As New Label With {.VerticalAlignment = VerticalAlignment.Bottom,
                                                                          .HorizontalAlignment = HorizontalAlignment.Left,
                                                                          .Content = "x", .Foreground = Brushes.White,
                                                                          .Background = Brushes.IndianRed,
                                                                          .Visibility = Visibility.Hidden,
                                                                          .Cursor = Cursors.No, .Name = "MediaDelBtn" + CStr(i)}
                    'SETUP BTN - BTM-RT (def.action)
                    Dim LbMediaSetupBtn As New Label With {.VerticalAlignment = Windows.VerticalAlignment.Bottom, .HorizontalAlignment = Windows.HorizontalAlignment.Right,
                                                           .Content = ChrW(8594), .Foreground = Brushes.White, .Background = Brushes.CadetBlue, .Cursor = Cursors.Pen,
                                                           .Visibility = Visibility.Hidden, .Name = "LbMediaSetupBtn" + CStr(i)}

                    'CLICK AREA
                    Dim MediaIcon As New Grid
                    With MediaIcon
                        .VerticalAlignment = VerticalAlignment.Stretch
                        .HorizontalAlignment = HorizontalAlignment.Stretch
                        .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#22000000"))
                        .Name = "MediaIcon" + CStr(i)
                    End With

                    'PACK ALL
                    With GridIcon.Children
                        .Add(MediaSelectionBorder)
                        .Add(LabelIconFile)
                        .Add(ImageIcon)
                        If icon_file(i).ToUpper.EndsWith("AVI") Then .Add(LabelVideoFile)
                        .Add(LbIconOrder)
                        .Add(LbMediaSelectBtn)
                        .Add(MediaDelBtn)
                        .Add(LbMediaSetupBtn)
                        .Add(MediaAddBtn)
                        .Add(MediaIcon)
                    End With
                    With WrapPanelItems
                        .Children.Add(GridIcon)
                        .RegisterName(MediaSelectionBorder.Name, MediaSelectionBorder)
                        .RegisterName(MediaIcon.Name, MediaIcon)
                        .RegisterName(MediaDelBtn.Name, MediaDelBtn)
                        .RegisterName(LbMediaSetupBtn.Name, LbMediaSetupBtn)
                        .RegisterName(MediaAddBtn.Name, MediaAddBtn)
                    End With

                    'LOAD ANIMATION
                    'GridIcon.BeginAnimation(Grid.OpacityProperty, New DoubleAnimation(0, 1, New TimeSpan(0, 0, 0, 0, i * 100)))
                    'Dim transf As New TranslateTransform
                    'GridIcon.RenderTransform = transf
                    'transf.BeginAnimation(TranslateTransform.YProperty, New DoubleAnimation(15, 0, New TimeSpan(0, 0, 0, 0, i * 50)))
                Next i

                'LOAD ANIMATION
                WrapPanelItems.Opacity = 0
                WrapPanelItems.BeginAnimation(Grid.OpacityProperty, Nothing)
                WrapPanelItems.BeginAnimation(Grid.OpacityProperty, New DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5)) With {.BeginTime = TimeSpan.FromSeconds(0.25)})
                Dim transf As New TranslateTransform With {.X = -15}
                WrapPanelItems.RenderTransform = transf
                transf.BeginAnimation(TranslateTransform.XProperty, New DoubleAnimation(transf.X, 0, TimeSpan.FromSeconds(0.5)) With {.BeginTime = TimeSpan.FromSeconds(0.25)})
            Else
                'NO ICONS
                Dim LabelNoItems As New Label With {.Content = "No items here yet...", .Foreground = Brushes.Gray}
                WrapPanelItems.Children.Add(LabelNoItems)
            End If

            'WrapPanelItemsButtons.Children.Clear()

            ''ADD MEDIA BUTTON
            'Dim AddMediaButton As New Label
            'With AddMediaButton
            '    .BorderBrush = Brushes.Gray
            '    .BorderThickness = New Thickness(1)
            '    .Content = "ADD FILE..."
            '    .Margin = New Thickness(5)
            '    .Width = 100
            '    .Background = Brushes.Black
            '    .Foreground = Brushes.GreenYellow
            '    .VerticalContentAlignment = VerticalAlignment.Bottom
            '    .HorizontalContentAlignment = HorizontalAlignment.Right
            '    .Name = "AddMediaButton"
            'End With

            'With WrapPanelItemsButtons
            '    .Children.Add(AddMediaButton)
            '    If Not .FindName(AddMediaButton.Name) Then .RegisterName(AddMediaButton.Name, AddMediaButton)
            'End With


            ''NEW EMPTY IMAGE BUTTON
            'Dim NewImageButton As New Label
            'With NewImageButton
            '    .BorderBrush = Brushes.Gray
            '    .BorderThickness = New Thickness(1)
            '    .Content = "NEW EMPTY IMAGE..."
            '    .Margin = New Thickness(5)
            '    .Width = 100
            '    .Background = Brushes.Black
            '    .Foreground = Brushes.GreenYellow
            '    .VerticalContentAlignment = VerticalAlignment.Bottom
            '    .HorizontalContentAlignment = HorizontalAlignment.Right
            '    .Name = "NewImageButton"
            'End With

            'With WrapPanelItemsButtons
            '    .Children.Add(NewImageButton)
            '    If Not .FindName(NewImageButton.Name) Then _
            '        .RegisterName(NewImageButton.Name, NewImageButton)
            'End With

        End If

        'CLEAR SELECTION
        SelectedMedia = -1
        'ClearSlideEditorFields()
        'StackPanelEditorFields.BeginAnimation(Grid.OpacityProperty, New DoubleAnimation(StackPanelEditorFields.Opacity, 0.25, New TimeSpan(0, 0, 0, 0, 250)))
    End Sub

    'COLLAPSE MANAGER PANEL (BTN)
    Public Sub CollapseManager()
        'PanelCollapse(WrapPanelItems, LabelHeader, "", "MEDIA", StackPanelEditor) 'FILES COUNT AT HEADER?
        'ManagerCollapsed = Not ManagerCollapsed
    End Sub

    'COLLAPSE EDITOR PANEL (BTN)
    Public Sub CollapseEditor()
        'PanelCollapse(StackPanelEditorFields, LabelEditorHeader, " : add file, assign slide", "FEATURES")
        'EditorCollapsed = Not EditorCollapsed
    End Sub

    'CLEAR FIELDS
    Public Sub ClearSlideEditorFields()
        For i = 0 To Template.Settings.Count - 1
            Dim obj As Object = WrapPanelFeatures.FindName("TbMediaSet" + CStr(i))
            If Not IsNothing(obj) Then obj.text = ""
        Next
    End Sub

    'EDITOR TEMPLATE - FIELDS
    Public Sub LoadTemplateVisual(ByVal setup_panel As WrapPanel, ByVal selected_block As Block, ByVal sel_sl As Integer, ByVal names_root As String)
        If Template.IsLoaded Then
            setup_panel.Children.Clear()
            For i = 0 To Template.Settings.Count - 1
                Dim value As String = selected_block.GetSlideParamValueByIndex(i, sel_sl)
                'ITEM STACK
                Dim StackPanelParam As New StackPanel
                StackPanelParam.Orientation = Orientation.Vertical
                StackPanelParam.Margin = New Thickness(5)
                StackPanelParam.MinWidth = 120

                'TITLE
                Dim LabelParam As New Label
                LabelParam.Foreground = Brushes.White
                LabelParam.Content = Template.Settings(i).title
                If Template.Settings(i).hint <> "" Then LabelParam.ToolTip = Template.Settings(i).hint
                StackPanelParam.Children.Add(LabelParam)

                'VALUE
                Dim ParamValue As Object = Nothing
                ' ---> STRING / INT
                If Template.Settings(i).type = "str" Or Template.Settings(i).type = "int" Then
                    ParamValue = New TextBox
                    ParamValue.Text = value
                End If
                ' ---> COL
                If Template.Settings(i).type = "col" Then
                    ParamValue = New TextBox
                    ParamValue.Text = value
                    ParamValue.Background = New SolidColorBrush(ColorConverter.ConvertFromString(value))
                    Dim col As Color = ColorConverter.ConvertFromString(value)
                    col.A = 255
                    col.R = Math.Abs(127 - col.R)
                    col.G = Math.Abs(127 - col.G)
                    col.B = Math.Abs(127 - col.B)
                    Dim solcolbr As New SolidColorBrush(col)
                    ParamValue.Foreground = solcolbr
                End If
                ' ---> BOOLEAN
                If Template.Settings(i).type = "bool" Then
                    ParamValue = New BooleanButton
                    ParamValue.Text = value
                    If value = "True" Then ParamValue.Foreground = Brushes.DarkGreen _
                        Else ParamValue.Foreground = Brushes.DarkRed
                End If

                'ADD/CLEAR NAMES
                If Not IsNothing(setup_panel.FindName(names_root + CStr(i))) Then
                    setup_panel.UnregisterName(names_root + CStr(i))
                End If
                If Not IsNothing(ParamValue) Then
                    ParamValue.Name = names_root + CStr(i)
                    StackPanelParam.Children.Add(ParamValue)
                    setup_panel.RegisterName(ParamValue.Name, ParamValue)
                End If

                'BREAK
                If Template.Settings(i).break.ToUpper = "TRUE" Then
                    Dim GridBreak As New Grid With {.Background = Brushes.Gray, .Width = 360, .Height = 1, .Margin = New Thickness(5, 5, 5, 5)}
                    setup_panel.Children.Add(GridBreak)
                End If

                setup_panel.Children.Add(StackPanelParam)
            Next
        End If
    End Sub

    'EDITOR TEMPLATE - ORDER FIELD
    Public Sub LoadOrderVisual(ByVal setup_panel As WrapPanel, ByVal value As String, ByVal names_root As String)
        'ITEM STACK
        Dim StackPanelOrder As New StackPanel With {.Orientation = Orientation.Vertical, .Margin = New Thickness(5), .MinWidth = 120}
        'TITLE
        Dim LabelOrder As New Label With {.Foreground = Brushes.White, .Content = "Order"}
        StackPanelOrder.Children.Add(LabelOrder)
        'VALUE
        Dim TextBoxOrder As New TextBox With {.Text = value}
        'REGISTER NAMES AND ADD CHILD
        If Not IsNothing(setup_panel.FindName(names_root)) Then
            setup_panel.UnregisterName(names_root)
        End If
        If Not IsNothing(TextBoxOrder) Then
            TextBoxOrder.Name = names_root
            StackPanelOrder.Children.Add(TextBoxOrder)
            setup_panel.RegisterName(TextBoxOrder.Name, TextBoxOrder)
        End If

        'BREAK
        Dim GridBreak As New Grid With {.Background = Brushes.White, .Width = 1, .Height = 30, .VerticalAlignment = VerticalAlignment.Center}
        setup_panel.Children.Add(GridBreak)

        setup_panel.Children.Add(StackPanelOrder)
    End Sub


End Class
