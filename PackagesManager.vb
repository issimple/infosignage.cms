Imports System.IO

Public Class PackagesManager

    'VALUES
    Public LocalSourcePath As String
    Public CustomRoot As String
    Public SelectedPackagePath As String

    'OBJECTS
    Public PackagesStackPanel As StackPanel
    Public CustomRootStackPanel As StackPanel

    Public SetRootButton As Label
    Public AddPackageButton As Label
    Public ButtonsStackPanel As StackPanel

    'EVENTS
    Public Event PackagesStackPanel_MouseUpEvent()
    Public Event SetRootButton_MouseUpEvent()

    Public Sub New(ByVal localsource_path As String, ByVal custom_root As String, ByVal sel_package As String)

        Me.LocalSourcePath = localsource_path
        Me.CustomRoot = custom_root
        Me.SelectedPackagePath = sel_package

        Dim LocalContentPreTitle As New TextBlock With {.Text = "LOCAL:", .FontSize = 14, .Foreground = Brushes.White, .Margin = New Thickness(10, 0, 0, 5)}

        Dim LocalContentIcon As New Grid With {.Width = 150, .Height = 75, .Background = Brushes.White, .Margin = New Thickness(5), .Name = "LocalContentIcon"}
        Dim LocalContentSelectionBorder As New Border With {.HorizontalAlignment = HorizontalAlignment.Stretch, .VerticalAlignment = VerticalAlignment.Stretch,
                                                    .Name = "LocalContentSelectionBorder", .BorderThickness = New Thickness(5), .Margin = New Thickness(-5),
                                                    .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#6ab4d8")), .Visibility = Visibility.Hidden}
        Dim LocalContentTitle As New TextBlock With {.Text = "Local Content", .FontSize = 18, .Margin = New Thickness(5, 5, 5, 5)}
        Dim LocalContentPath As New TextBlock With {.Text = Me.LocalSourcePath, .FontSize = 10,
                                                    .TextWrapping = TextWrapping.Wrap, .Margin = New Thickness(5, 30, 5, 5)}
        Dim LocalContentFrontGrid As New Grid With {.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#99000000")),
                                                    .Name = "LocalContentFrontGrid", .Visibility = Visibility.Visible}

        With LocalContentIcon.Children
            .Add(LocalContentSelectionBorder)
            .Add(LocalContentTitle)
            .Add(LocalContentPath)
            .Add(LocalContentFrontGrid)
        End With

        Dim SeparatorGrid As New Grid With {.Width = 150, .Height = 2, .Background = Brushes.White, .Margin = New Thickness(5)}
        Dim CustomContentPreTitle As New TextBlock With {.Text = "CUSTOM ROOT:", .FontSize = 14, .Foreground = Brushes.White, .Margin = New Thickness(10, 10, 0, 5)}

        PackagesStackPanel = New StackPanel With {.Orientation = Orientation.Vertical}
        NameScope.SetNameScope(PackagesStackPanel, New NameScope())
        CustomRootStackPanel = New StackPanel With {.Orientation = Orientation.Vertical}
        NameScope.SetNameScope(CustomRootStackPanel, New NameScope())

        With PackagesStackPanel.Children
            .Add(LocalContentPreTitle)
            .Add(LocalContentIcon)
            .Add(CustomContentPreTitle)
            .Add(CustomRootStackPanel)
        End With

        If Not IsNothing(PackagesStackPanel.FindName(LocalContentFrontGrid.Name)) Then PackagesStackPanel.UnregisterName(LocalContentFrontGrid.Name)
        PackagesStackPanel.RegisterName(LocalContentFrontGrid.Name, LocalContentFrontGrid)

        If Not IsNothing(PackagesStackPanel.FindName(LocalContentIcon.Name)) Then PackagesStackPanel.UnregisterName(LocalContentIcon.Name)
        PackagesStackPanel.RegisterName(LocalContentIcon.Name, LocalContentIcon)

        ReloadCustomRootItems()
        AddHandler PackagesStackPanel.MouseUp, AddressOf PackagesStackPanel_MouseUp

        SetRootButton = New Label With {.Content = "SET ROOT", .Foreground = Brushes.Gray, .Style = CType(res_dict("LabelButton"), Style)}
        AddHandler SetRootButton.MouseUp, AddressOf SetRootButton_MouseUp

        AddPackageButton = New Label With {.Content = "ADD PACKAGE", .Foreground = Brushes.GreenYellow, .Style = CType(res_dict("LabelButton"), Style)}
        AddHandler AddPackageButton.MouseUp, AddressOf AddPackageButton_MouseUp

        ButtonsStackPanel = New StackPanel With {.Orientation = Orientation.Vertical}

        With ButtonsStackPanel.Children
            .Add(AddPackageButton)
            .Add(SetRootButton)
        End With

    End Sub

    Public Sub ReloadCustomRootItems()
        CustomRootStackPanel.Children.Clear()
        If Directory.Exists(CustomRoot) Then
            Dim dirs As List(Of String) = New List(Of String)(Directory.EnumerateDirectories(CustomRoot))
            For i = 0 To dirs.LongCount - 1

                Dim PackageIcon As New Grid With {.Width = 150, .Height = 75, .Background = Brushes.White, .Margin = New Thickness(5), .Name = "PackageIcon" + CStr(i)}

                Dim PackageIconSelectionBorder As New Border With {.HorizontalAlignment = HorizontalAlignment.Stretch, .VerticalAlignment = VerticalAlignment.Stretch,
                                                   .Name = "PackageIconSelectionBorder", .BorderThickness = New Thickness(5), .Margin = New Thickness(-5),
                                                   .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#6ab4d8")), .Visibility = Visibility.Hidden}

                Dim icontitle As String = dirs(i).Substring(dirs(i).LastIndexOf("\") + 1, dirs(i).Length - dirs(i).LastIndexOf("\") - 1)
                For j = 0 To icontitle.Length - 1 - 1
                    If Char.IsLower(icontitle.Chars(j)) And Char.IsUpper(icontitle.Chars(j + 1)) Then
                        icontitle = icontitle.Insert(j + 1, " ")
                    End If
                Next
                Dim PackageIconTitle As New TextBlock With {.Text = icontitle, .FontSize = 18, .Margin = New Thickness(5, 5, 5, 5)}

                Dim PackageIconPath As New TextBlock With {.Text = dirs(i), .FontSize = 10,
                                                            .TextWrapping = TextWrapping.Wrap, .Margin = New Thickness(5, 30, 5, 5)}
                If Not File.Exists(dirs(i) + "\blocks.xml") Then PackageIconPath.Foreground = Brushes.Red

                Dim PackageIconFrontGrid As New Grid With {.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#99000000")), .Name = "PackageIconFrontGrid" + CStr(i)}

                With PackageIcon.Children
                    .Add(PackageIconSelectionBorder)
                    .Add(PackageIconTitle)
                    .Add(PackageIconPath)
                    .Add(PackageIconFrontGrid)
                End With

                CustomRootStackPanel.Children.Add(PackageIcon)

                If Not IsNothing(CustomRootStackPanel.FindName(PackageIconFrontGrid.Name)) Then CustomRootStackPanel.UnregisterName(PackageIconFrontGrid.Name)
                CustomRootStackPanel.RegisterName(PackageIconFrontGrid.Name, PackageIconFrontGrid)
            Next
        End If
    End Sub

    Public Sub SetPackageSelection()

        'DESELECT - LOCAL
        Dim loc_grd As Grid = PackagesStackPanel.FindName("LocalContentIcon")
        Dim loc_frontgrd As Grid = loc_grd.Children(3)
        loc_frontgrd.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#99000000"))
        loc_frontgrd.Visibility = Visibility.Visible
        Dim loc_border As Border = loc_grd.Children(0)
        loc_border.Visibility = Visibility.Hidden
        'SELECT - LOCAL
        Dim loc_pathtext As TextBlock = loc_grd.Children(2)
        If Me.SelectedPackagePath = loc_pathtext.Text Then
            loc_frontgrd.Background = Brushes.Transparent
            loc_frontgrd.Visibility = Visibility.Visible
            loc_border.Visibility = Visibility.Visible
        End If

        'DESELECT - CUSTOM ROOT
        For i = 0 To CustomRootStackPanel.Children.Count - 1
            Dim grd As Grid = CustomRootStackPanel.Children(i)
            Dim frontgrd As Grid = grd.Children(3)
            frontgrd.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#99000000"))
            Dim border As Border = grd.Children(0)
            border.Visibility = Visibility.Hidden
            'SELECT - CUSTOM ROOT
            Dim itm_pathtext As TextBlock = grd.Children(2)
            If Me.SelectedPackagePath = itm_pathtext.Text + "\" Then
                frontgrd.Background = Brushes.Transparent
                frontgrd.Visibility = Visibility.Visible
                border.Visibility = Visibility.Visible
            End If
        Next

    End Sub

    Public Sub PackagesStackPanel_MouseUp(sender As Object, e As MouseButtonEventArgs)
        'SELECT
        Dim obj As Grid = TryCast(e.Source, Grid)
        If Not IsNothing(obj) Then
            'LOCAL CONTENT ICON
            If obj.Name.Contains("LocalContentFrontGrid") Then
                Me.SelectedPackagePath = ""
                RaiseEvent PackagesStackPanel_MouseUpEvent()
            End If

            'CUSTOM ROOT ICONS
            If obj.Name.Contains("PackageIconFrontGrid") Then
                Dim parentobj As Grid = obj.Parent
                Dim border As Border = parentobj.Children(0)
                Dim rootstr As TextBlock = parentobj.Children(2)
                Me.SelectedPackagePath = rootstr.Text
                RaiseEvent PackagesStackPanel_MouseUpEvent()
            End If
        End If
    End Sub

    Public Sub SetRootButton_MouseUp()
        Dim dlg As New Forms.FolderBrowserDialog
        Dim result As Forms.DialogResult = dlg.ShowDialog()
        If (result = Forms.DialogResult.OK) Then

            CustomRoot = dlg.SelectedPath
            ReloadCustomRootItems()
            RaiseEvent SetRootButton_MouseUpEvent() 'save ini file here
        ElseIf (result = Forms.DialogResult.Cancel) Then
            Exit Sub
        End If
    End Sub

    Public Sub AddPackageButton_MouseUp()
        Dim packname As String = InputBox("Package name", "Please specify package name:", "New Package")
        Try
            Directory.CreateDirectory(CustomRoot + "\" + packname)
        Catch ex As Exception
        End Try
        ReloadCustomRootItems()
    End Sub

End Class
