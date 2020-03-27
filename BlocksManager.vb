Imports System.IO
Imports System.Windows.Media.Animation
Imports System.Xml


Public Class BlocksManager

    Public SelectedBlock As Integer = -1
    Public Blocks() As Block
    Public BlockTemplate As BlockTemplate
    Public BlockEditor1 As New BlockEditor

    Public ContentSyncPath As String
    Public ContentLocalPath As String = app_dir + "cms_content\"

    Public Event ReloadAll()
    Public Event BlockSelection()
    Public Event SaveBlockOnDrop()

    'BLOCKS LIST OBJ
    Public LbHeader As New Label With {.Content = "BLOCKS", .Style = CType(res_dict("Header"), Style)}
    Public BlocksPanel As New WrapPanel With {.AllowDrop = True}

    Public ButtonsStack As New StackPanel With {.VerticalAlignment = VerticalAlignment.Bottom, .HorizontalAlignment = HorizontalAlignment.Left}

    Public xLbBtnAddNewBlock As New Label With {.Content = "ADD BLOCK", .Foreground = Brushes.GreenYellow, .Width = 150, .Style = CType(res_dict("LabelButton"), Style)}

    'N E W  M A N A G E R
    Public Sub New(ByVal _Blocks() As Block, ByVal _BlocksTemplate As BlockTemplate, ByVal _SlidesTemplate As SlideTemplate)
        'ASSIGN BLOCKS DB
        Me.Blocks = _Blocks

        'ASSIGN TEMPLATES
        Me.BlockTemplate = _BlocksTemplate
        Me.BlockEditor1.BlockTemplate = _BlocksTemplate
        'Me.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#333333"))

        'CREATE BLOCKS STACK PANEL
        ButtonsStack.Children.Add(xLbBtnAddNewBlock)

        'DRAF AND DROP 
        AddHandler BlocksPanel.PreviewMouseLeftButtonDown, AddressOf BlocksPanel_PreviewMouseLeftButtonDown
        AddHandler BlocksPanel.PreviewMouseMove, AddressOf BlocksPanel_PreviewMouseMove
        AddHandler BlocksPanel.DragEnter, AddressOf BlocksPanel_DragEnter
        AddHandler BlocksPanel.DragOver, AddressOf BlocksPanel_DragOver
        AddHandler BlocksPanel.PreviewDragLeave, AddressOf BlocksPanel_PreviewDragLeave
        AddHandler BlocksPanel.Drop, AddressOf BlocksPanel_Drop
    End Sub

    '--------------------------------------------------------  D R A G - A N D - D R O P --------------------------------------------------------

    Dim startPoint As New Point

    Private Sub BlocksPanel_PreviewMouseLeftButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
        startPoint = e.GetPosition(Nothing)
    End Sub

    Private Sub BlocksPanel_PreviewMouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
        Dim mousePos As Point = e.GetPosition(Nothing)
        Dim diff As Vector = startPoint - mousePos

        If e.LeftButton.Equals(MouseButtonState.Pressed) Then
            'and Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||  Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance )
            Dim icon_obj As Label = TryCast(e.Source, Label)
            If Not IsNothing(icon_obj) Then
                If icon_obj.Name.Contains("LbBlockOrderInfo") Then
                    If CInt(icon_obj.Name.Replace("LbBlockOrderInfo", "")) = Me.SelectedBlock Then
                        Dim drag_data As DataObject = New DataObject("format", icon_obj.Name)
                        DragDrop.DoDragDrop(icon_obj, (Me.SelectedBlock + 1).ToString, DragDropEffects.Move)
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub BlocksPanel_DragEnter(ByVal sender As Object, ByVal e As DragEventArgs)
        Dim icon_obj As Grid = TryCast(e.Source, Grid)
        If Not IsNothing(icon_obj) Then
            If icon_obj.Name.Contains("BlockIcon") Then
                Me.To_Order = CInt(icon_obj.Name.Replace("BlockIcon", "")) + 1
                Dim icon_obj_host As Grid = VisualTreeHelper.GetParent(icon_obj)
                If Me.To_Order > Me.SelectedBlock Then
                    icon_obj_host.Margin = New Thickness(5, 5, 5, 20)
                Else
                    icon_obj_host.Margin = New Thickness(5, 20, 5, 5)
                End If
            End If
        End If
    End Sub

    Private Sub BlocksPanel_DragOver(ByVal sender As Object, ByVal e As DragEventArgs)

    End Sub


    Private Sub BlocksPanel_PreviewDragLeave(ByVal sender As Object, ByVal e As DragEventArgs)
        Dim icon_obj As Grid = TryCast(e.Source, Grid)
        If Not IsNothing(icon_obj) Then
            If icon_obj.Name.Contains("BlockIcon") Then
                Dim icon_obj_host As Grid = VisualTreeHelper.GetParent(icon_obj)
                icon_obj_host.Margin = New Thickness(5, 5, 5, 5)
            End If
        End If
    End Sub

    Public To_Order As Integer

    Private Sub BlocksPanel_Drop(ByVal sender As Object, ByVal e As DragEventArgs)
        Me.To_Order = -1
        Dim obj As Grid = TryCast(e.Source, Grid)
        If Not IsNothing(obj) Then
            If obj.Name.Contains("BlockIcon") Then
                Me.To_Order = CInt(obj.Name.Replace("BlockIcon", "")) + 1
                RaiseEvent SaveBlockOnDrop()
            End If
        End If
    End Sub

    '-------------------------------------------------------- B L O C K S --------------------------------------------------------

    'L O A D  B L O C K S  P A N E L  D A T A
    Public Sub ReloadPanel()
        'CLEAR NAMES
        Dim i As Integer = 0
        Do While Not IsNothing(BlocksPanel.FindName("BlockIcon" + CStr(i)))
            With BlocksPanel
                .UnregisterName("GridIcon" + CStr(i))
                .UnregisterName("SelectionBorder" + CStr(i))
                .UnregisterName("BlockIcon" + CStr(i))
                .UnregisterName("LbBlockIconInfo" + CStr(i))
                .UnregisterName("LbBlockOrderInfo" + CStr(i))
                .UnregisterName("LbBlockDelBtn" + CStr(i))
                .UnregisterName("LbBlockSelectBtn" + CStr(i))
                .UnregisterName("LbBlockSetupBtn" + CStr(i))
            End With
            i += 1
        Loop

        If Not IsNothing(BlocksPanel.FindName("AddBlockButton")) Then BlocksPanel.UnregisterName("AddBlockButton")
        BlocksPanel.Children.Clear()

        'ADD BLOCK ICONS
        If Not IsNothing(Blocks) Then
            For i = 0 To Blocks.Count - 1
                Dim GridIcon As New Grid With {.Width = 150, .Background = Brushes.White, .Name = "GridIcon" + CStr(i), _
                                               .Margin = New Thickness(5, 5, 5, 5)}
                Dim SelectionBorder As New Border With {.HorizontalAlignment = HorizontalAlignment.Stretch, .VerticalAlignment = VerticalAlignment.Stretch,
                                                        .Name = "SelectionBorder" + CStr(i), .BorderThickness = New Thickness(5), .Margin = New Thickness(-5),
                                                        .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#6ab4d8")), .Visibility = Visibility.Hidden}
                'BLOCK TITLE
                Dim LabelTitle As New TextBlock With {.Foreground = Brushes.Black, .FontSize = 18, .Text = Blocks(i).bTitle,
                                                  .Margin = New Thickness(15, 0, 0, 0), .MaxWidth = 120, .TextWrapping = TextWrapping.Wrap}
                'BLOCK FOLDER
                Dim LabelFolder As New TextBlock With {.Foreground = Brushes.DarkGray, .Text = "/" + Blocks(i).bDir, .Margin = New Thickness(25, 2, 0, 0)}
                'BLOCK SIZE
                Dim LabelSize As New TextBlock With {.Foreground = Brushes.DarkGray, .Margin = New Thickness(25, 0, 0, 0),
                                                     .Text = Blocks(i).bWidth.ToString + "x" + Blocks(i).bHeight.ToString}
                'CLICK AREA
                Dim BlockIcon As New Grid With {.VerticalAlignment = VerticalAlignment.Stretch, .HorizontalAlignment = HorizontalAlignment.Stretch,
                                                .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#22000000")),
                                                .Name = "BlockIcon" + CStr(i)}
                'INFO LABEL - CENTER-RT
                Dim LbBlockIconInfo As New Label With {.Background = Brushes.LightGray, .Foreground = Brushes.White,
                                                       .HorizontalAlignment = HorizontalAlignment.Right, .VerticalAlignment = VerticalAlignment.Center,
                                                       .Content = Blocks(i).bType, .Name = "LbBlockIconInfo" + CStr(i)}
                'ORDER LABEL - TOP-LT
                Dim LbBlockOrderInfo As New Label With {.Background = Brushes.DarkGray, .Foreground = Brushes.LightGray, .FontSize = 10,
                                                        .HorizontalAlignment = HorizontalAlignment.Left, .VerticalAlignment = VerticalAlignment.Top,
                                                        .Content = CStr(i + 1), .Name = "LbBlockOrderInfo" + CStr(i)}
                'DEL BTN - BTM-LEFT
                Dim LbBlockDelBtn As New Label With {.VerticalAlignment = Windows.VerticalAlignment.Bottom, .HorizontalAlignment = Windows.HorizontalAlignment.Left,
                                                     .Content = "x", .Foreground = Brushes.White, .Background = Brushes.IndianRed, .Cursor = Cursors.No,
                                                     .Visibility = Visibility.Hidden, .Name = "LbBlockDelBtn" + CStr(i)}

                'SELECT BTN - TOP-RT
                Dim LbBlockSelectBtn As New Label With {.VerticalAlignment = Windows.VerticalAlignment.Top, .HorizontalAlignment = Windows.HorizontalAlignment.Right,
                                                        .Content = ChrW(10004), .Foreground = Brushes.White, .Background = Brushes.DarkGray, .Cursor = Cursors.Hand,
                                                        .Visibility = Visibility.Visible, .Name = "LbBlockSelectBtn" + CStr(i)}
                'SETUP BTN - BTM-RT (def.action)
                Dim LbBlockSetupBtn As New Label With {.VerticalAlignment = Windows.VerticalAlignment.Bottom, .HorizontalAlignment = Windows.HorizontalAlignment.Right,
                                                       .Content = ChrW(8594), .Foreground = Brushes.White, .Background = Brushes.CadetBlue, .Cursor = Cursors.Pen,
                                                       .Visibility = Visibility.Hidden, .Name = "LbBlockSetupBtn" + CStr(i)}

                'BLOCK ICON
                Dim StackPanelIcon As New StackPanel With {.Orientation = Orientation.Vertical, .Height = 68}

                With StackPanelIcon.Children
                    .Add(LabelTitle)
                    .Add(LabelFolder)
                    .Add(LabelSize)
                End With

                With GridIcon.Children
                    .Add(SelectionBorder)
                    .Add(StackPanelIcon)
                    .Add(BlockIcon)
                    .Add(LbBlockIconInfo)
                    .Add(LbBlockOrderInfo)
                    .Add(LbBlockDelBtn)
                    .Add(LbBlockSelectBtn)
                    .Add(LbBlockSetupBtn)
                End With

                BlocksPanel.Children.Add(GridIcon)

                With BlocksPanel
                    .RegisterName(GridIcon.Name, GridIcon)
                    .RegisterName(SelectionBorder.Name, SelectionBorder)
                    .RegisterName(BlockIcon.Name, BlockIcon)
                    .RegisterName(LbBlockIconInfo.Name, LbBlockIconInfo)
                    .RegisterName(LbBlockOrderInfo.Name, LbBlockOrderInfo)
                    .RegisterName(LbBlockDelBtn.Name, LbBlockDelBtn)
                    .RegisterName(LbBlockSelectBtn.Name, LbBlockSelectBtn)
                    .RegisterName(LbBlockSetupBtn.Name, LbBlockSetupBtn)
                End With
            Next
        End If
    End Sub


    '-------------------------------------------------------- B L O C K  E D I T O R --------------------------------------------------------

    'S A V E  B L O C K (BTN)   ---- MOVED OUTISDE ???
    Private Sub BlockEditor_Save()
        'Try
        'LABELS FOR UPDATES LIST
        For i = 0 To BlockTemplate.Settings.Count - 1
            Dim obj As Object = BlockEditor1.BlockSetupPanel.FindName("TbBlockSet" + CStr(i))
            Dim prev_value As String = Blocks(Me.SelectedBlock).GetParamValueByIndex(i)
            If obj.Text <> prev_value Then
                Dim elem_name As String = "block"
                'Dim param As Object
                Dim param_name As String = BlockTemplate.Settings(i).title
                Dim new_value As Object = obj.Text
                Dim id As Integer = Me.SelectedBlock
                If Blocks(Me.SelectedBlock).GetParamValueByIndex(i) <> new_value.ToString Then
                    'UPDATES LIST LABEL
                    Dim update As String
                    update = "[" + elem_name + "]  " + CStr(id + 1) + "  [" + param_name + "]  " + Blocks(Me.SelectedBlock).GetParamValueByIndex(i) + "  [>]  "
                    'NUMBERS INPUT
                    If BlockTemplate.Settings(i).type = "int" Then
                        If Not IsNumeric(new_value) Then
                            obj.Focus()
                            Exit Sub
                        End If
                        Dim int_max As Integer = CInt(BlockTemplate.Settings(i).minmax.Substring(BlockTemplate.Settings(i).minmax.IndexOf(",") + 1))
                        If CInt(new_value) > int_max Then new_value = int_max
                        Dim int_min As Integer = CInt(BlockTemplate.Settings(i).minmax.Substring(0, BlockTemplate.Settings(i).minmax.IndexOf(",") + 1))
                        If CInt(new_value) < int_min Then new_value = int_min
                        new_value = CInt(new_value)
                        obj.text = new_value
                    End If
                    'BOOLEAN INPUT
                    If BlockTemplate.Settings(i).type = "bool" Then new_value = CBool(new_value)
                    'COLOR INPUT
                    If BlockTemplate.Settings(i).type = "col" Then new_value = New SolidColorBrush(ColorConverter.ConvertFromString(new_value))
                    'SET PARAM VALUE
                    Blocks(Me.SelectedBlock).SetParamValueByindex(i, new_value)

                    'RENAME BLOCK DIR
                    If param_name.ToUpper = "DIRECTORY" Then
                        CopyDirectory(ContentLocalPath + prev_value, Me.ContentLocalPath + new_value)
                        Directory.Delete(ContentLocalPath + prev_value, True)
                    End If

                    'UPDATE
                    update += Blocks(Me.SelectedBlock).GetParamValueByIndex(i)
                    'UpdatesPanel0.AddUpdate(update) '!!!-!!!
                    Blocks(Me.SelectedBlock).WasUpdatedAtCMS = True
                End If
            End If
        Next

        'BLOCK ORDER
        Dim ord_obj As TextBox = TryCast(BlockEditor1.BlockSetupPanel.FindName("TextBoxBlockOrder"), TextBox)
        If Not IsNothing(ord_obj) Then
            If ord_obj.Text <> CInt(Me.SelectedBlock + 1) And IsNumeric(ord_obj.Text) Then
                Dim update As String
                update = "[block] " + Blocks(Me.SelectedBlock).bTitle + " [order] " _
                    + CStr(Me.SelectedBlock + 1) + " [>] "
                Dim new_order As Integer = CInt(ord_obj.Text) - 1
                'REORDER ARRAYS
                Blocks = ReplaceArrayElement(Blocks, GetType(Block), Me.SelectedBlock, new_order)
                update += CStr(new_order + 1)
                'UpdatesPanel0.AddUpdate(update)'!!!-!!!
            End If
            Me.ReloadPanel()
            'Me.SelectBlock(0)
        End If

        'Catch ex As Exception
        'AddToLog("ERR SAVING BLOCK: " + ex.Message)
        'End Try
    End Sub


End Class
