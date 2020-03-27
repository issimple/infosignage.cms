Imports System.Windows.Media.Animation
Imports System.IO


Public Class SlidesManager ': Inherits Grid

    Dim StackPanelManager As New StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 70, 0, 0)}

    Dim LabelHeader As New Label With {.Content = "PLAYLIST", .Style = CType(res_dict("Header"), Style)}

    Public WrapPanelItems As New WrapPanel With {.HorizontalAlignment = HorizontalAlignment.Left, .Margin = New Thickness(0), .Width = 180}
    Public WrapPanelItemsButtons As New WrapPanel

    Dim StackPanelEditor As New StackPanel With {.Orientation = Orientation.Vertical}
    'Dim LabelEditorHeader As New Label With {.Content = "FEATURES", .Style = CType(res_dict("SubHeader"), Style)}

    Public StackPanelEditorFields As New StackPanel With {.Orientation = Orientation.Vertical}
    Public WrapPanelFeatures As New WrapPanel With {.Orientation = Orientation.Vertical}

    Public WrapPanelButtons As New WrapPanel With {.Orientation = Orientation.Horizontal}
    Public ButtonSave As New Label With {.Content = "SAVE", .Foreground = Brushes.GreenYellow, .Style = CType(res_dict("LabelButton"), Style)}
    Public ButtonDelete As New Label With {.Content = "REMOVE SLIDE", .Foreground = Brushes.DarkOrange, .Style = CType(res_dict("LabelButton"), Style)}
    Public LabelInfo As New Label With {.Content = "", .Style = CType(res_dict("InfoLabel"), Style)}

    'TEMPLATE
    Public Template As SlideTemplate

    Public SelectedSlide As Integer = -1

    'UI STATES
    Public EditorCollapsed As Boolean = False
    Public ManagerCollapsed As Boolean = False

    'BUTTONS
    Public AddSlideButton As New Label With {.Content = "ADD EMPTY SLIDE", .Foreground = Brushes.GreenYellow,
                                             .Style = CType(res_dict("LabelButton"), Style)}

    Public Sub New()
        'MAIN BUTTONS PANEL
        With WrapPanelItemsButtons.Children
            .Add(AddSlideButton)
        End With
        'EDITOR PANEL FIELDS
        With WrapPanelButtons.Children
            .Add(ButtonDelete)
            .Add(ButtonSave)
        End With
        'EDITOR PANEL
        StackPanelEditor.Children.Add(StackPanelEditorFields)
        'MANAGER PANEL
        StackPanelManager.Children.Add(StackPanelEditor)

        AddHandler Me.WrapPanelFeatures.MouseUp, AddressOf WrapPanelFeatures_MouseUp
    End Sub

    'BREAKS TITLE COLLAPSING HANDLING 
    Private Sub WrapPanelFeatures_MouseUp(sender As Object, e As MouseButtonEventArgs)
        Dim obj As TextBlock = TryCast(e.Source, TextBlock)
        Dim start_index As Integer = -1
        Dim end_index As Integer = -1
        If Not IsNothing(obj) Then
            If obj.TextDecorations.Count <> 0 Then
                For i = 0 To Me.WrapPanelFeatures.Children.Count - 1
                    Dim stpanobj As StackPanel = TryCast(Me.WrapPanelFeatures.Children(i), StackPanel)
                    If Not IsNothing(stpanobj) Then
                        If stpanobj.Children(0).Equals(obj) Then
                            start_index = i
                        End If

                    End If
                    If start_index <> -1 And i > start_index And end_index = -1 Then
                        Dim next_break_obj As Grid = TryCast(Me.WrapPanelFeatures.Children(i), Grid)
                        If Not IsNothing(next_break_obj) Then
                            If next_break_obj.Height = 1 Then
                                end_index = i
                            End If
                        End If

                    End If
                Next
                For i = 0 To Me.WrapPanelFeatures.Children.Count - 1
                    Dim stpanobj As StackPanel = TryCast(Me.WrapPanelFeatures.Children(i), StackPanel)
                    If start_index <> -1 And end_index <> -1 Then
                        If i > start_index And i < end_index Then
                            If Not IsNothing(stpanobj) Then
                                If stpanobj.Visibility = Windows.Visibility.Collapsed Then
                                    stpanobj.Visibility = Windows.Visibility.Visible
                                Else
                                    stpanobj.Visibility = Windows.Visibility.Collapsed
                                End If
                            End If
                        End If
                    End If
                Next
            End If
        End If
    End Sub

    'ITEMS ICONS
    Public Sub ReloadItemsPanel(ByVal _cmsBlocks() As Block, ByVal sel_bl As Integer)
        'CLEAR NAMES
        With WrapPanelItems
            For i = 0 To 99
                If Not IsNothing(.FindName("SlideSelectionBorder" + CStr(i))) Then .UnregisterName("SlideSelectionBorder" + CStr(i))
                If Not IsNothing(.FindName("SlideIcon" + CStr(i))) Then .UnregisterName("SlideIcon" + CStr(i))
                If Not IsNothing(.FindName("SlideImageIcon" + CStr(i))) Then .UnregisterName("SlideImageIcon" + CStr(i))
                If Not IsNothing(.FindName("SlideDelBtn" + CStr(i))) Then .UnregisterName("SlideDelBtn" + CStr(i))
                If Not IsNothing(.FindName("LbSlideSelectBtn" + CStr(i))) Then .UnregisterName("LbSlideSelectBtn" + CStr(i))
                If Not IsNothing(.FindName("LbSlideOrderInfo" + CStr(i))) Then .UnregisterName("LbSlideOrderInfo" + CStr(i))
                If Not IsNothing(.FindName("LbSlideSetupBtn" + CStr(i))) Then .UnregisterName("LbSlideSetupBtn" + CStr(i))
            Next i
            'If Not IsNothing(.FindName("AddSlideButton")) Then .UnregisterName("AddSlideButton")
            .Children.Clear()
            GC.Collect()
        End With

        'LOAD ICONS
        If Not IsNothing(_cmsBlocks) And sel_bl <> -1 Then
            If Not IsNothing(_cmsBlocks(sel_bl).Slides) Then
                For i = 0 To _cmsBlocks(sel_bl).Slides.Count - 1

                    'MAIN GRID
                    Dim GridIcon As New Grid With {.Width = 150, .MinHeight = 75, .Margin = New Thickness(5), .Background = Brushes.White}
                    If _cmsBlocks(sel_bl).Slides(i).Source.ToUpper.EndsWith("AVI") Then GridIcon.Background = Brushes.LightSlateGray

                    Dim SlideSelectionBorder As New Border With {.HorizontalAlignment = HorizontalAlignment.Stretch, .VerticalAlignment = VerticalAlignment.Stretch,
                                                       .Name = "SlideSelectionBorder" + CStr(i), .BorderThickness = New Thickness(5), .Margin = New Thickness(-5),
                                                       .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#6ab4d8")), .Visibility = Visibility.Hidden}

                    'SLIDE TITLE
                    Dim TextBlockIconTitle As New TextBlock With {.Text = _cmsBlocks(sel_bl).Slides(i).Title, .TextWrapping = TextWrapping.NoWrap,
                                                                   .Margin = New Thickness(20, 3, 0, 0), .FontSize = 18, .VerticalAlignment = VerticalAlignment.Top}
                    'FILENAME
                    Dim LabelIconFile As New Label With {.Content = _cmsBlocks(sel_bl).Slides(i).Source, .Foreground = Brushes.DarkGray}

                    'DURATION
                    Dim LabelIconDuration As New Label With {.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#99FFFFFF")), .Foreground = Brushes.Black,
                                                             .VerticalAlignment = VerticalAlignment.Center, .HorizontalAlignment = HorizontalAlignment.Right}
                    Dim units_str As String = " sec"
                    If _cmsBlocks(sel_bl).Slides(i).Source.ToUpper.EndsWith("AVI") Then units_str = " loops"
                    LabelIconDuration.Content = CStr(_cmsBlocks(sel_bl).Slides(i).Duration) + units_str

                    'SELECT BTN - TOP-RT
                    Dim LbSlideSelectBtn As New Label With {.VerticalAlignment = Windows.VerticalAlignment.Top, .HorizontalAlignment = Windows.HorizontalAlignment.Right,
                                                            .Content = ChrW(10004), .Foreground = Brushes.White, .Background = Brushes.DarkGray, .Cursor = Cursors.Hand,
                                                            .Visibility = Visibility.Visible, .Name = "LbSlideSelectBtn" + CStr(i)}

                    'ICON PIC
                    Dim ImageIcon As New Image With {.VerticalAlignment = Windows.VerticalAlignment.Top, .Margin = New Thickness(5, 30, 5, 5)}
                    If Not IsNothing(_cmsBlocks(sel_bl).Slides(i).Source) Then
                        If _cmsBlocks(sel_bl).Slides(i).Source.ToUpper.EndsWith("JPG") Or _
                            _cmsBlocks(sel_bl).Slides(i).Source.ToUpper.EndsWith("PNG") Or _
                            _cmsBlocks(sel_bl).Slides(i).Source.ToUpper.EndsWith("GIF") Then
                            If File.Exists(app_dir + "cms_content\" + _cmsBlocks(sel_bl).bDir + "\" + _cmsBlocks(sel_bl).Slides(i).Source) Then
                                Dim BitmapImg As New BitmapImage
                                With BitmapImg
                                    .BeginInit()
                                    .CacheOption = BitmapCacheOption.OnLoad
                                    .UriSource = New Uri(app_dir + "cms_content\" + _cmsBlocks(sel_bl).bDir + "\" + _cmsBlocks(sel_bl).Slides(i).Source)
                                    .DecodePixelWidth = GridIcon.Width
                                    '.DecodePixelHeight = 120
                                    .EndInit()
                                End With

                                'GIF
                                If _cmsBlocks(sel_bl).Slides(i).Source.ToUpper.EndsWith("GIF") Then _
                                    WpfAnimatedGif.ImageBehavior.SetAnimatedSource(ImageIcon, BitmapImg)
                                'JPG
                                ImageIcon.Source = BitmapImg
                            End If
                        End If
                    End If
                    ImageIcon.Name = "SlideImageIcon" + CStr(i)

                    'CLICK AREA
                    Dim SlideIcon As New Grid With {.VerticalAlignment = VerticalAlignment.Stretch, .HorizontalAlignment = HorizontalAlignment.Stretch,
                                                    .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#33000000")), .Name = "SlideIcon" + CStr(i)}

                    'ORDER LABEL - TOP-LT
                    Dim LbSlideOrderInfo As New Label With {.Background = Brushes.DarkGray, .Foreground = Brushes.White, .HorizontalAlignment = HorizontalAlignment.Left,
                                                          .VerticalAlignment = VerticalAlignment.Top, .Content = _cmsBlocks(sel_bl).Slides(i).Order,
                                                          .Name = "LbSlideOrderInfo" + CStr(i)}

                    'DEL BTN - BTM-LT
                    Dim SlideDelBtn As New Label With {.VerticalAlignment = VerticalAlignment.Bottom, .HorizontalAlignment = HorizontalAlignment.Left,
                                                       .Content = "x", .Foreground = Brushes.White, .Background = Brushes.IndianRed, .Visibility = Visibility.Hidden,
                                                       .Name = "SlideDelBtn" + CStr(i), .Cursor = Cursors.No}

                    'SETUP BTN - BTM-RT (def.action)
                    Dim LbSlideSetupBtn As New Label With {.VerticalAlignment = Windows.VerticalAlignment.Bottom, .HorizontalAlignment = Windows.HorizontalAlignment.Right,
                                                           .Content = ChrW(8594), .Foreground = Brushes.White, .Background = Brushes.CadetBlue, .Cursor = Cursors.Pen,
                                                           .Visibility = Visibility.Hidden, .Name = "LbSlideSetupBtn" + CStr(i)}

                    'PACK ALL
                    With GridIcon.Children
                        .Add(SlideSelectionBorder)
                        .Add(ImageIcon)
                        .Add(TextBlockIconTitle)
                        .Add(LabelIconDuration)
                        .Add(LbSlideOrderInfo)
                        .Add(SlideIcon)
                        .Add(SlideDelBtn)
                        .Add(LbSlideSetupBtn)
                        .Add(LbSlideSelectBtn)
                    End With

                    With WrapPanelItems
                        .Children.Add(GridIcon)

                        .RegisterName(SlideSelectionBorder.Name, SlideSelectionBorder)
                        .RegisterName(SlideIcon.Name, SlideIcon)
                        .RegisterName(ImageIcon.Name, ImageIcon)
                        .RegisterName(SlideDelBtn.Name, SlideDelBtn)
                        .RegisterName(LbSlideSelectBtn.Name, LbSlideSelectBtn)
                        .RegisterName(LbSlideOrderInfo.Name, LbSlideOrderInfo)
                        .RegisterName(LbSlideSetupBtn.Name, LbSlideSetupBtn)
                    End With

                    'LOAD ANIMATION
                    'GridIcon.BeginAnimation(Grid.OpacityProperty, New DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.25 * i)))
                    'Dim transf As New TranslateTransform
                    'GridIcon.RenderTransform = transf
                    'transf.BeginAnimation(TranslateTransform.XProperty, New DoubleAnimation(15, 0, TimeSpan.FromSeconds(0.25 * i)))
                Next i

                'LOAD ANIMATION
                WrapPanelItems.BeginAnimation(Grid.OpacityProperty, New DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5)))
                Dim transf As New TranslateTransform
                WrapPanelItems.RenderTransform = transf
                transf.BeginAnimation(TranslateTransform.XProperty, New DoubleAnimation(-15, 0, TimeSpan.FromSeconds(0.5)))
            Else
                'NO ICONS
                Dim LabelNoItems As New Label With {.Content = "No items here yet...", .Foreground = Brushes.Gray}
                WrapPanelItems.Children.Add(LabelNoItems)
            End If
        End If

        'CLEAR SELECTION
        SelectedSlide = -1
        ClearSlideEditorFields()
        StackPanelEditorFields.BeginAnimation(Grid.OpacityProperty, New DoubleAnimation(StackPanelEditorFields.Opacity, 0.25, New TimeSpan(0, 0, 0, 0, 250)))
    End Sub

    'COLLAPSE MANAGER PANEL (BTN)
    Public Sub CollapseManager()
        PanelCollapse(WrapPanelItems, LabelHeader, "", "PLAYLIST", StackPanelEditor) 'SLIDES COUNT AT HEADER?
        ManagerCollapsed = Not ManagerCollapsed
    End Sub

    'COLLAPSE EDITOR PANEL (BTN)
    Public Sub CollapseEditor()
        'PanelCollapse(StackPanelEditorFields, LabelEditorHeader, " : edit or remove slide", "FEATURES")
        EditorCollapsed = Not EditorCollapsed
    End Sub

    'CLEAR FIELDS
    Public Sub ClearSlideEditorFields()
        For i = 0 To Template.Settings.Count - 1
            Dim obj As Object = WrapPanelFeatures.FindName("TbSlideSet" + CStr(i))
            If Not IsNothing(obj) Then obj.text = ""
        Next
    End Sub

    'EDITOR TEMPLATE - FIELDS
    Public Sub LoadTemplateVisual(ByVal setup_panel As WrapPanel, ByVal selected_block As Block, ByVal sel_sl As Integer, ByVal names_root As String)
        If Template.IsLoaded Then
            setup_panel.Children.Clear()
            GC.Collect()
            For i = 0 To Template.Settings.Count - 1
                Dim value As String = ""
                If sel_sl <> -1 Then value = selected_block.GetSlideParamValueByIndex(i, sel_sl)
                setup_panel.Children.Add(LoadTemplateVisualItem(i, value, Template, setup_panel, names_root))
            Next
        End If
    End Sub

    'EDITOR TEMPLATE - ORDER FIELD
    Public Sub LoadOrderVisual(ByVal setup_panel As WrapPanel, ByVal value As String, ByVal names_root As String)
        'ITEM STACK
        Dim StackPanelOrder As New StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(5), .MinWidth = 120}
        'TITLE
        Dim LabelOrder As New Label With {.Foreground = Brushes.White, .Content = "Order", .Width = 160}
        StackPanelOrder.Children.Add(LabelOrder)
        'VALUE
        Dim TextBoxOrder As New TextBox With {.Text = value, .Width = 30}
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
        Dim GridBreak As New Grid With {.Background = Brushes.Gray, .Width = 360, .Height = 1, .Margin = New Thickness(5, 5, 5, 5)}
        setup_panel.Children.Add(GridBreak)

        setup_panel.Children.Add(StackPanelOrder)
    End Sub

End Class
