Public Class BlockEditor : Inherits Grid

    'OBJECTS
    Public xLbBlockEditor As New Label With {.Content = "SETTINGS", .Style = CType(res_dict("SubHeader"), Style)}
    Public xLbInfo As New Label With {.Content = "Block info:", .Style = CType(res_dict("InfoLabel"), Style)}
    Public xTitleAreaStack As New StackPanel With {.Orientation = Orientation.Vertical}

    Dim xGridBlockEditor As New Grid
    Dim xStPanBlockEditorFields As New StackPanel With {.Orientation = Orientation.Vertical}
    Public BlockSetupPanel As New WrapPanel With {.Orientation = Orientation.Horizontal}

    Public ButtonsStack As New StackPanel With {.Orientation = Orientation.Horizontal, _
                                                  .VerticalAlignment = VerticalAlignment.Bottom, .HorizontalAlignment = HorizontalAlignment.Right}
    Public ButtonSave As New Label With {.Content = "SAVE", .Foreground = Brushes.GreenYellow, .Style = CType(res_dict("LabelButton"), Style)}
    Public ButtonDelete As New Label With {.Content = "DELETE BLOCK", .Foreground = Brushes.DarkOrange, .Style = CType(res_dict("LabelButton"), Style)}
    Public ButtonSetupPlugin As New Label With {.Content = "SETUP PLUGIN", .Foreground = Brushes.CadetBlue, .Style = CType(res_dict("LabelButton"), Style), .Opacity = 0.25}

    'TEMPLATE
    Public BlockTemplate As BlockTemplate

    'UI STATES
    Public EditorCollapsed As Boolean = False

    'C R E A T E  B L O C K  E D I T O R
    Public Sub New()
        With xTitleAreaStack.Children
            .Add(xLbBlockEditor)
            .Add(xLbInfo)
        End With
        With ButtonsStack.Children
            .Add(ButtonSetupPlugin)
            .Add(ButtonDelete)
            .Add(ButtonSave)
        End With

        AddHandler Me.BlockSetupPanel.MouseUp, AddressOf BlockSetupPanel_MouseUp
    End Sub

    'BREAKS TITLE COLLAPSING HANDLING 
    Private Sub BlockSetupPanel_MouseUp(sender As Object, e As MouseButtonEventArgs)
        Dim obj As TextBlock = TryCast(e.Source, TextBlock)
        Dim start_index As Integer = -1
        Dim end_index As Integer = -1
        If Not IsNothing(obj) Then
            If obj.TextDecorations.Count <> 0 Then
                For i = 0 To Me.BlockSetupPanel.Children.Count - 1
                    Dim stpanobj As StackPanel = TryCast(Me.BlockSetupPanel.Children(i), StackPanel)
                    If Not IsNothing(stpanobj) Then
                        If stpanobj.Children(0).Equals(obj) Then
                            start_index = i
                        End If
                        
                    End If
                    If start_index <> -1 And i > start_index And end_index = -1 Then
                        Dim next_break_obj As Grid = TryCast(Me.BlockSetupPanel.Children(i), Grid)
                        If Not IsNothing(next_break_obj) Then
                            If next_break_obj.Height = 1 Then
                                end_index = i
                            End If
                        End If

                    End If
                Next
                For i = 0 To Me.BlockSetupPanel.Children.Count - 1
                    Dim stpanobj As StackPanel = TryCast(Me.BlockSetupPanel.Children(i), StackPanel)
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

    'L O A D  B L O C K  S E T T I N G S  P A N E L
    Public Sub ReloadWrPanBlockSetup(ByVal _cmsBlocks() As Block, ByVal sel_bl As Integer)
        'MAIN FILEDS FROM TEMPLATE
        LoadTemplateVisual(BlockTemplate, Me.BlockSetupPanel, _cmsBlocks(sel_bl), "TbBlockSet")
        'BLOCK ORDER TEXTBOX
        LoadOrderVisual(Me.BlockSetupPanel, CStr(sel_bl + 1), "TextBoxBlockOrder")
    End Sub

    'T E M P L A T E  V I S U A L
    Public Sub LoadTemplateVisual(ByVal template As BlockTemplate, ByVal setup_panel As WrapPanel, ByVal selected_block As Block, ByVal names_root As String)
        If template.IsLoaded Then
            setup_panel.Children.Clear()
            GC.Collect()
            For i = 0 To template.Settings.Count - 1
                Dim value As String = selected_block.GetParamValueByIndex(i)
                setup_panel.Children.Add(LoadTemplateVisualItem(i, value, template, setup_panel, names_root))
            Next
        End If
    End Sub

    'O R D E R  V I S U A L
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
