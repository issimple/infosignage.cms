Imports System.Windows.Media.Animation

Public Class UpdatesPanel : Inherits Grid

    'UPDATES LIST OBJ
    Dim StackPanelUpdatesHome As New StackPanel With {.Orientation = Orientation.Vertical}
    Public StackPanelUpdates As New StackPanel With {.Orientation = Orientation.Vertical}
    Dim LbUpdatesHeader As New Label With {.Content = "UPDATES", .FontSize = "32", .Foreground = Brushes.White, .Margin = New Thickness(0)}
    Public ButtonUpdate As New Label With {.Content = "UPDATE", .Foreground = Brushes.GreenYellow, .Style = CType(res_dict("LabelButton"), Style)}
    Public ButtonReset As New Label With {.Content = "RESET", .Foreground = Brushes.Orange, .Style = CType(res_dict("LabelButton"), Style)}

    Public Sub New()
        'UPDATES LIST PANEL
        With Me
            '.HorizontalAlignment = HorizontalAlignment.Right
            .VerticalAlignment = VerticalAlignment.Top
            .Margin = New Thickness(10)
            '.Width = 320
        End With
        With StackPanelUpdatesHome.Children
            .Add(LbUpdatesHeader)
            .Add(StackPanelUpdates)
            .Add(ButtonUpdate)
            .Add(ButtonReset)
        End With
        Me.Children.Add(StackPanelUpdatesHome)
        'UPDATES LIST ACTIONS
        AddHandler StackPanelUpdates.LayoutUpdated, AddressOf PanelVisibility
    End Sub

    Public Sub AddUpdate(ByVal update As String)
        Dim LabelUpdates As New Label
        LabelUpdates.Style = CType(res_dict("LabelUpdates"), Style)
        LabelUpdates.Content = update
        StackPanelUpdates.Children.Add(LabelUpdates)
        Me.Visibility = Visibility.Visible
    End Sub

    'U P D A T E S  V I S I B I L I T Y
    Private Sub PanelVisibility()
        If StackPanelUpdates.Children.Count <> 0 Then
            StackPanelUpdates.Visibility = Visibility.Visible
            ButtonUpdate.Visibility = Visibility.Visible
            ButtonReset.Visibility = Visibility.Visible
        Else
            StackPanelUpdates.Visibility = Visibility.Hidden
            ButtonUpdate.Visibility = Visibility.Hidden
            ButtonReset.Visibility = Visibility.Hidden
        End If
    End Sub

End Class
