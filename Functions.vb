Imports System.IO
Imports System.Windows.Media.Animation



Class BooleanButton : Inherits TextBlock
    Public Sub New()
        Me.Padding = New Thickness(3)
        Me.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFEEEEEE"))
    End Sub
    Public Sub OnClick() Handles Me.MouseUp
        If Me.Text = "False" Then
            Me.Text = "True"
            Me.Foreground = Brushes.White
            Me.Background = Brushes.DarkGreen
        Else
            Me.Text = "False"
            Me.Foreground = Brushes.White
            Me.Background = Brushes.DarkGray
        End If

        Dim time_tr As New TranslateTransform
        Dim txt_eff As New TextEffect With {.Transform = time_tr, .PositionStart = 0, .PositionCount = Me.Text.Length}
        time_tr.BeginAnimation(TranslateTransform.XProperty, New DoubleAnimation(5, 0, TimeSpan.FromSeconds(0.25)))
        Me.TextEffects.Add(txt_eff)
    End Sub
End Class

Class SelectorButton : Inherits ComboBox
    Public Values() As String
    Public Sub New(ByRef _values() As String)
        Me.Values = _values
        For i = 0 To Me.Values.Count - 1
            Me.Items.Add(Me.Values(i))
        Next
        Me.MinWidth = 120
        Me.Padding = New Thickness(3)
        Dim tooltip As String = ""
        Me.ToolTip = tooltip
    End Sub
    Public Sub OnClick() Handles Me.MouseUp

    End Sub
End Class

Class DateTimeDialog : Inherits Window
    Public ResultValue As String = ""
    Dim DateTimePick As New Xceed.Wpf.Toolkit.DateTimePicker

    Public Sub New(ByVal datetime_text As String)
        With Me
            .Height = 90
            .Width = 240
            .WindowStartupLocation = WindowStartupLocation.CenterScreen
            .WindowStyle = WindowStyle.ToolWindow
        End With
        With DateTimePick
            .TextAlignment = TextAlignment.Center
            .TimePickerVisibility = Visibility.Visible
        End With

        If datetime_text <> "" Then
            Dim str_datetime() As String = datetime_text.Split(",")
            Dim str_time() As String = str_datetime(0).Split("-")
            Dim str_date() As String = str_datetime(1).Split("-")
            Dim str_lim As New DateTime(str_date(0), str_date(1), str_date(2), str_time(0), str_time(1), str_time(2))
            DateTimePick.Value = str_lim
        Else
            DateTimePick.Value = Date.Now
        End If

        Dim MainGrid As New Grid
        MainGrid.Children.Add(DateTimePick)

        Me.AddChild(MainGrid)
    End Sub
    Public Sub Termainate() Handles Me.Closing
        Dim dt_val As DateTime = DateTimePick.Value
        Me.ResultValue = dt_val.ToString("HH-mm-ss") + "," + dt_val.ToString("yyyy-MM-dd")
    End Sub
End Class

Class DateTimeButton : Inherits TextBlock
    Public Sub New()
        Me.Padding = New Thickness(3)
        Me.Background = Brushes.LightSlateGray
        Me.Foreground = Brushes.White
    End Sub
    Public Sub OnClick() Handles Me.MouseUp

        Dim datetime_wnd As New DateTimeDialog(Me.Text)

        'Dim somevalue As String = ""
        datetime_wnd.ShowDialog()
        'somevalue = datetime_wnd.ResultValue
        Me.Text = datetime_wnd.ResultValue
        datetime_wnd = Nothing

        Dim time_tr As New TranslateTransform
        Dim txt_eff As New TextEffect With {.Transform = time_tr, .PositionStart = 0, .PositionCount = Me.Text.Length}
        time_tr.BeginAnimation(TranslateTransform.XProperty, New DoubleAnimation(5, 0, TimeSpan.FromSeconds(0.25)))
        Me.TextEffects.Add(txt_eff)
    End Sub
End Class

Class TextColorPicker : Inherits Xceed.Wpf.Toolkit.ColorPicker
    Public Sub New(ByVal value As String)
        Me.DisplayColorAndName = True
        Me.ShowDropDownButton = False
        Me.ColorMode = Xceed.Wpf.Toolkit.ColorMode.ColorCanvas
        Me.MinWidth = 120
        If value <> "" Then
            Me.SelectedColor = ColorConverter.ConvertFromString(value)
        Else
            Me.SelectedColor = Colors.Black
        End If
    End Sub

    Public Sub UpdColor() Handles Me.SelectedColorChanged
        Me.Text = Me.SelectedColor.ToString
    End Sub

    Public Text As String

End Class

Public Class TextInfo
    Public Text As String
    Public TextSize As Integer
    Public FrontColor As SolidColorBrush
    Public BackColor As SolidColorBrush
    Public TextAlign As String
    Public Sub New(ByVal _text As String, ByVal _size As String, ByVal _front As String, ByVal _back As String, ByVal _align As String)
        Me.Text = _text
        Me.TextSize = CInt(_size)
        Me.FrontColor = New SolidColorBrush(ColorConverter.ConvertFromString(_front))
        Me.BackColor = New SolidColorBrush(ColorConverter.ConvertFromString(_back))
        Me.TextAlign = _align
    End Sub

End Class

Public Class Param
    Public type As String
    Public id As String
    Public title As String
    Public hint As String
    Public value As String
    Public Sub New(ByVal _type As String, ByVal _id As String, ByVal _title As String, ByVal _hint As String, ByVal _value As String)
        Me.type = _type
        Me.id = _id
        Me.title = _title
        Me.hint = _hint
        Me.value = _value
    End Sub

End Class

Public Class TemplateSettings
    Public id As String
    Public type As String
    Public def As String
    Public minmax As String
    Public sel As String
    Public title As String
    Public hint As String
    Public break As String
End Class

Module Functions

    Public app_dir As String = System.AppDomain.CurrentDomain.BaseDirectory()

    Public Function RemoveElementFromArray(ByVal array As System.Array, ByVal index As Object, ByVal type As System.Type)
        Dim objArrayList As New ArrayList(array)
        objArrayList.RemoveAt(index)
        Return objArrayList.ToArray(type)
    End Function

    Public Function ReplaceArrayElement(ByVal array As System.Array, ByVal type As System.Type, ByVal FromPos As Object, ByVal ToPos As Object)
        Dim objArrayList As New ArrayList(array)
        Dim Value As Object = array(FromPos)
        If FromPos > ToPos Then
            objArrayList.Insert(ToPos, Value)
            objArrayList.RemoveAt(FromPos + 1)
        Else
            objArrayList.Insert(ToPos + 1, Value)
            objArrayList.RemoveAt(FromPos)
        End If
        Return objArrayList.ToArray(type)
    End Function

    Public Function InsertArrayElement(ByVal array As System.Array, ByVal array_type As System.Type, ByVal to_pos As Object, ByVal value As Object)
        Dim objArrayList As ArrayList
        If IsNothing(array) Then
            Dim arr()
            ReDim arr(0)
            arr(0) = value
            array = arr
            objArrayList = New ArrayList(array)
        Else
            If to_pos >= array.Length + 1 Then to_pos = array.Length - 1
            If array.Length = 0 Then to_pos = 0
            objArrayList = New ArrayList(array)
            objArrayList.Insert(to_pos, value)
        End If
        'Dim objArrayList As New ArrayList(array)
        Return objArrayList.ToArray(array_type)
    End Function


    Public Function getFiles(ByVal SourceFolder As String, ByVal Filter As String, ByVal searchOption As System.IO.SearchOption) As String()
        If Directory.Exists(SourceFolder) Then
            Dim alFiles As ArrayList = New ArrayList() ' ArrayList will hold all file names
            Dim MultipleFilters() As String = Filter.Split("|") ' Create an array of filter string
            For Each FileFilter As String In MultipleFilters ' for each filter find mathing file names
                alFiles.AddRange(Directory.GetFiles(SourceFolder, FileFilter, searchOption)) ' add found file names to array list
            Next
            Return alFiles.ToArray(Type.GetType("System.String")) ' returns string array of relevant file names
        Else
            Return Nothing
        End If
    End Function

    Public Function GetFileName(ByVal path As String) As String
        Return path.Substring(path.LastIndexOf("\") + 1, path.Length - path.LastIndexOf("\") - 1)
    End Function

    'L O G
    Public Sub AddToLog(ByVal value As String)
        Try
            Dim log_dir As String = app_dir + "log\"
            Dim log_file As String = log_dir + "log.txt"
            If Not Directory.Exists(log_dir) Then Directory.CreateDirectory(log_dir)
            Dim StrWr As StreamWriter
            If Not File.Exists(log_file) Then File.Create(log_file).Close()
            StrWr = New StreamWriter(log_file, True)
            StrWr.WriteLine(Date.Now + " : " + value)
            StrWr.Flush()
            StrWr.Close()
        Catch ex As Exception
            MsgBox("Report this error to the system administrator: " + vbCr + ex.Message)
        End Try
    End Sub

    'G U I : C O L L A P S E  S U B
    Public Sub PanelCollapse(ByVal Obj As Object, ByVal Lb As Label, ByVal new_content As String, ByVal old_content As String, Optional chld As Object = Nothing)
        If Obj.Visibility = Visibility.Visible Then
            Obj.Visibility = Visibility.Collapsed
            Lb.Foreground = Brushes.DarkGray
            Lb.Content += new_content
            Dim transf As New TranslateTransform
            Lb.RenderTransform = transf
            transf.BeginAnimation(TranslateTransform.XProperty, New DoubleAnimation(-100, 0, TimeSpan.FromSeconds(0.2)))
            Lb.BeginAnimation(Label.OpacityProperty, New DoubleAnimation(0.5, 1, TimeSpan.FromSeconds(0.2)))
            If Not IsNothing(chld) Then
                If chld.Visibility = Visibility.Visible Then chld.Visibility = Visibility.Collapsed
            End If
        Else
            Obj.Visibility = Visibility.Visible
            Lb.Foreground = Brushes.White
            Lb.Content = old_content
            Dim transf As New TranslateTransform
            Obj.RenderTransform = transf
            transf.BeginAnimation(TranslateTransform.XProperty, New DoubleAnimation(-100, 0, TimeSpan.FromSeconds(0.2)))
            Obj.BeginAnimation(Label.OpacityProperty, New DoubleAnimation(0.5, 1, TimeSpan.FromSeconds(0.2)))
            If Not IsNothing(chld) Then
                If chld.Visibility = Visibility.Collapsed Then
                    chld.Visibility = Visibility.Visible
                    Dim chld_transf As New TranslateTransform
                    chld.RenderTransform = chld_transf
                    chld_transf.BeginAnimation(TranslateTransform.XProperty, New DoubleAnimation(-100, 0, TimeSpan.FromSeconds(0.45)))
                    chld.BeginAnimation(StackPanel.OpacityProperty, New DoubleAnimation(0.25, 1, TimeSpan.FromSeconds(0.15)))
                End If
            End If
        End If
    End Sub

    Dim breakhappens As Boolean = False
    Dim breakcount As Integer = -1

    Function LoadTemplateVisualItem(ByVal i As Integer, ByVal value As String, ByVal template As Object, ByVal setup_panel As WrapPanel, ByVal names_root As String) As StackPanel
        'ITEM STACK
        Dim StackPanelParam As New StackPanel
        StackPanelParam.Orientation = Orientation.Horizontal
        StackPanelParam.Margin = New Thickness(5)
        'TITLE
        'Dim LabelParam As New Label With {.Width = 160, .Foreground = Brushes.White, .Content = template.Settings(i).title}
        'If template.Settings(i).hint <> "" Then LabelParam.ToolTip = template.Settings(i).hint
        'StackPanelParam.Children.Add(LabelParam)

        Dim LabelParam As New TextBlock With {.Width = 160, .Foreground = Brushes.White, .Text = template.Settings(i).title}
        If template.Settings(i).hint <> "" Then LabelParam.ToolTip = template.Settings(i).hint
        StackPanelParam.Children.Add(LabelParam)
        'VALUE
        Dim ParamValue As Object = Nothing
        ' ---> STRING / INT
        If template.Settings(i).type = "str" Or template.Settings(i).type = "int" Then
            ParamValue = New TextBox With {.Padding = New Thickness(3), .MinWidth = 120, .Text = value}
        End If
        ' ---> SELECTION MODE
        If template.Settings(i).type = "sel" Then
            If template.Settings(i).sel <> "" Then
                Dim sel_values() As String = template.Settings(i).sel.ToString.Split(",")
                ParamValue = New SelectorButton(sel_values)
                ParamValue.Text = value
            Else
                ParamValue = New TextBox
                ParamValue.Text = value
            End If
        End If
        ' ---> COL
        If template.Settings(i).type = "col" Then
            Dim TextColPicker As New TextColorPicker(value)
            ParamValue = TextColPicker
        End If
        ' ---> BOOLEAN
        If template.Settings(i).type = "bool" Then
            ParamValue = New BooleanButton With {.MinWidth = 40}
            If value = "" Then
                ParamValue.text = template.Settings(i).def
            Else
                ParamValue.Text = value
            End If
            If value = "True" Then
                ParamValue.Foreground = Brushes.White
                ParamValue.Background = Brushes.DarkGreen
            Else
                ParamValue.Foreground = Brushes.White
                ParamValue.Background = Brushes.DarkGray
            End If
        End If
        ' ---> TIMEDATE
        If template.Settings(i).type = "timedate" Then
            ParamValue = New DateTimeButton With {.Text = value, .MinWidth = 120}
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

        'COMMON BREAK
        If template.Settings(i).break.ToUpper = "TRUE" Then
            Dim GridBreak As New Grid With {.Background = Brushes.Gray, .Width = 360, .Height = 1, .Margin = New Thickness(5, 5, 5, 5)}
            setup_panel.Children.Add(GridBreak)
            breakcount = -1
        End If

        'BREAK WITH COLLAPSE
        breakhappens = False
        If template.Settings(i).break.ToUpper = "COLLAPSE" Then
            Dim GridBreak As New Grid With {.Background = Brushes.Gray, .Width = 360, .Height = 1, .Margin = New Thickness(5, 5, 5, 5)}
            setup_panel.Children.Add(GridBreak)
            LabelParam.TextDecorations = TextDecorations.Underline
            LabelParam.Cursor = Cursors.Hand
            breakhappens = True
            breakcount = 0
        End If
        If breakcount >= 0 Then breakcount += 1
        'COLLAPSE ITEMS BELOW BREAK
        If breakcount > 1 Then
            StackPanelParam.Visibility = Visibility.Collapsed
        End If

        Return StackPanelParam
    End Function

    'COPY DIRECTORY SUB
    Public Sub CopyDirectory(ByVal sourceDir As String, ByVal destDir As String)
        Try
            If Not Directory.Exists(destDir) Then Directory.CreateDirectory(destDir)
            For Each strEntry As String In System.IO.Directory.GetFiles(sourceDir)
                Dim fileNew As System.IO.FileInfo
                fileNew = New System.IO.FileInfo(strEntry)
                'disableReadOnly(fileNew)
                If fileNew.Exists Then fileNew.CopyTo(destDir & "\" & fileNew.Name, True)
            Next
            For Each strEntry As String In System.IO.Directory.GetDirectories(sourceDir)
                Dim strDest As String = System.IO.Path.Combine(destDir, System.IO.Path.GetFileName(strEntry))
                CopyDirectory(strEntry, strDest)
            Next
        Catch ex As Exception
            AddToLog("CopyDirectory ERR: " + ex.ToString)
        End Try
    End Sub

    'COMPARE BLOCKS DATA
    Public Function IsSameBlockData(ByVal BlockA As Block, ByVal BlockB As Block) As Boolean
        Dim IsSame As Boolean = True
        'check main block parameters
        For i = 0 To 20
            If BlockA.GetParamValueByIndex(i) <> BlockB.GetParamValueByIndex(i) Then IsSame = False
        Next
        If IsSame = True Then
            'check slides data
            If Not IsNothing(BlockA.Slides) And Not IsNothing(BlockB.Slides) Then
                If BlockA.Slides.Count = BlockB.Slides.Count Then
                    For i = 0 To BlockA.Slides.Count - 1
                        For j = 0 To 15
                            If BlockA.GetSlideParamValueByIndex(j, i) <> BlockB.GetSlideParamValueByIndex(j, i) Then IsSame = False
                        Next
                    Next
                Else
                    IsSame = False
                End If
            End If
        End If
        Return IsSame
    End Function
End Module
