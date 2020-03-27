Imports System.Windows.Media.Animation

Public Class ManagerEditorPanel : Inherits Grid

    'OBJECTS
    Private PanelStack As New StackPanel With {.Orientation = Orientation.Horizontal}

    Private ManagerGrid As New Grid
    Private ManagerTitleArea As New Grid With {.Height = 60, .Margin = New Thickness(5), .VerticalAlignment = VerticalAlignment.Top}
    Public LabelPanelTitle As New Label With {.FontSize = 32, .Foreground = Brushes.White}
    Public ManagerCollapseButton As New Label With {.HorizontalAlignment = HorizontalAlignment.Right, .VerticalAlignment = VerticalAlignment.Top, _
                                                    .Height = 32, .Width = 32, .Content = ChrW(8594), .Foreground = Brushes.White, .Cursor = Cursors.Pen}
    Private ManagerContainer As New Grid With {.Margin = New Thickness(5, 70, 5, 70)}
    Public ManagerScroller As New ScrollViewer With {.VerticalScrollBarVisibility = ScrollBarVisibility.Auto, _
                                                .HorizontalAlignment = HorizontalAlignment.Stretch, .Margin = New Thickness(0, 0, -15, 0)}
    Public ManagerButtonsArea As New Grid With {.MinHeight = 60, .Margin = New Thickness(5), .VerticalAlignment = VerticalAlignment.Bottom}

    Private EditorGrid As New Grid With {.Margin = New Thickness(0, 0, 0, 0)}
    Public EditorTitleArea As New Grid With {.Height = 60, .Margin = New Thickness(5), .VerticalAlignment = VerticalAlignment.Top}
    Private EditorContainer As New Grid With {.Margin = New Thickness(5, 70, 5, 70)}
    Public EditorScroller As New ScrollViewer With {.VerticalScrollBarVisibility = ScrollBarVisibility.Auto, _
                                                .HorizontalAlignment = HorizontalAlignment.Stretch, .Margin = New Thickness(0)}
    Public EditorButtonsArea As New Grid With {.MinHeight = 60, .Margin = New Thickness(5), .VerticalAlignment = VerticalAlignment.Bottom}

    'DIMS
    Public EditorCollapsed As Boolean = False

    'SIZES
    Public ManagerWidth As Integer
    Public EditorWidth As Integer

    Public Sub New(ByVal panel_title As String, Optional manager_width As Integer = 180, Optional editor_width As Integer = 360)
        'VISUAL
        ManagerTitleArea.Children.Add(LabelPanelTitle)
        ManagerContainer.Children.Add(ManagerScroller)
        With ManagerGrid.Children
            .Add(ManagerTitleArea)
            .Add(ManagerCollapseButton)
            .Add(ManagerContainer)
            .Add(ManagerButtonsArea)
        End With
        EditorContainer.Children.Add(EditorScroller)
        With EditorGrid.Children
            .Add(EditorTitleArea)
            .Add(EditorContainer)
            .Add(EditorButtonsArea)
        End With
        With PanelStack.Children
            .Add(ManagerGrid)
            .Add(EditorGrid)
        End With
        Me.Children.Add(PanelStack)

        LabelPanelTitle.Content = panel_title
        Me.Margin = New Thickness(0, 0, 10, 0)
        Me.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#333333"))
        ManagerCollapseButton.Background = Brushes.CadetBlue 'New SolidColorBrush(ColorConverter.ConvertFromString("#444444"))

        'WIDTHS
        ManagerWidth = manager_width
        EditorWidth = editor_width
        ManagerGrid.Width = ManagerWidth
        EditorGrid.Width = EditorWidth
        'Me.MaxWidth = manager_width + editor_width

        'EVENTS
        AddHandler Me.ManagerCollapseButton.MouseUp, AddressOf Editor_Collapse
        Editor_Collapse()
    End Sub

    Public Sub Editor_Collapse()
        If Not EditorCollapsed Then
            'HIDE
            Dim anim As New DoubleAnimation(EditorGrid.ActualWidth, 0, TimeSpan.FromSeconds(0.5)) With {.EasingFunction = New PowerEase With {.EasingMode = EasingMode.EaseOut}}
            EditorGrid.BeginAnimation(Grid.WidthProperty, anim)
            EditorContainer.BeginAnimation(WrapPanel.OpacityProperty, New DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.25)))
            EditorCollapsed = True
            ManagerCollapseButton.Content = ChrW(8594)
        Else
            'SHOW
            Dim anim As New DoubleAnimation(0, EditorWidth, TimeSpan.FromSeconds(0.5)) With {.EasingFunction = New PowerEase With {.EasingMode = EasingMode.EaseOut}}
            EditorGrid.BeginAnimation(Grid.WidthProperty, anim)
            EditorContainer.Opacity = 0
            EditorContainer.BeginAnimation(WrapPanel.OpacityProperty, Nothing)
            EditorContainer.BeginAnimation(WrapPanel.OpacityProperty, New DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.25)) With {.BeginTime = TimeSpan.FromSeconds(0.25)})
            EditorCollapsed = False
            ManagerCollapseButton.Content = ChrW(8592)
        End If
    End Sub

End Class
