'*************************** Block Module ******************************'
'
' Developed:    iSSimple Co., Ltd. (Thailand), iSSimple Pte (Singapore)
' Project:	    iNFOSignage
' Application:	iNFOSignage.Player
' Client:       N/A 123
'
' Copyright (C) iSSimple Co., Ltd. (Thailand) - All Rights Reserved
' Unauthorized copying of this file, via any medium is strictly prohibited
' Proprietary and confidential
' Written by Gregory Notchenko <gregory@issimple.co>, 2013-2014
'*************************************************************************'

Imports System.IO
Imports System.Xml
Imports System.ComponentModel
Imports System.Windows.Threading
Imports System.Windows.Media.Animation
Imports System.Collections.ObjectModel
Imports TwitterVB2
Imports System.Xml.XPath
Imports System.Threading
Imports Transitionals

Public Class Block : Inherits Grid
    Implements INotifyPropertyChanged
    'SETTINGS
    Public bTitle As String
    Public bDir As String

    Public bLeft As Integer
    Public bTop As Integer
    Public bWidth As Integer
    Public bHeight As Integer

    Public bType As String
    Public bDots As Boolean
    Public bMargin As Integer
    Public bOrder As Integer
    Public bSimpleTouch As Boolean = False
    Public bComplexTouch As Boolean = False
    Public UseSync As Boolean = True
    Public bLocationPreset As String = ""

    Public bLinkTo As String = ""

    Private bSourceValue As String = ""

    Public Property bSource As String
        Get
            Return Me.bSourceValue
        End Get
        Set(ByVal value As String)
            If Not (value = bSourceValue) Then
                Me.bSourceValue = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("bStatus"))
                'If bType.ToUpper = "TXT" Then xTextBlockMiddle.Text = Me.bSource
            End If
        End Set
    End Property

    'TIMING - LIMITS MODE
    Public bTimeLimit As Boolean = False
    Public bFromTime As String = ""
    Public bToTime As String = ""

    Public WasUpdatedAtCMS As Boolean = False
    Public SlidesWereUpdatedAtCMS As Boolean = False

    'ROOTS
    Public ContentLocalRoot As String
    Public ContentSourceRoot As String
    'Public dBxRoot As String
    Dim app_dir As String = System.AppDomain.CurrentDomain.BaseDirectory()

    'LEVEL
    Public bLevel As Integer = 1

    'SLIDES
    Public Slides_bak() As Slide
    Public Slides() As Slide
    Dim CurrentSlide As Integer

    'FEED
    Public UseFeed As Boolean = False
    Public FeedType As String
    Public FeedSource As String

    'TEXT BLOCKS
    Public TopText As TextInfo
    Public MidText As TextInfo
    Public BtmText As TextInfo

    'OBJECTS
    Dim xTextBlockStatus As New TextBlock
    Public xStackPanelContentScroller As New StackPanel With {.Name = "xStackPanelContentScroller"}
    Dim xSlideShow As New Transitionals.Controls.Slideshow
    Dim xStackPanelBlockDots As New StackPanel
    Dim xImageBlockMask As New Image
    Dim xImageBlockBg As New Image
    Dim xImageBlockFg As New Image
    'Dim xTextBlockFeed As New TextBlock
    Dim xViewboxFeed As New Viewbox
    Dim xTextBlockTop As New TextBlock
    Dim xTextBlockMiddle As New TextBlock
    Dim xTextBlockBottom As New TextBlock
    'EDITOR CONTROLS
    Public SelectionBorder As New Border With {.Name = "SelectionBorder"}
    'Public ResizeControlLeft As TextBlock
    'Public ResizeControlTop As TextBlock
    Public ResizeControlRight As New TextBlock With {.Name = "ResizeControlRight"}
    Public ResizeControlBottom As New TextBlock With {.Name = "ResizeControlBottom"}
    'Public OrderPrev As TextBlock - ...to-do or drang-n-drop will be better
    'Public OrderNext As TextBlock

    'TIMERS
    Dim SlidesTimer As DispatcherTimer = New DispatcherTimer()
    Dim UpdateTimer As DispatcherTimer = New DispatcherTimer()

    'TRANSFORM
    Dim slide_trans_gr As New TransformGroup
    Dim slide_trans_xy As New TranslateTransform
    Dim slide_trans_z As New ScaleTransform

    'STATUS
    Private StatusValue As String = ""
    Public Exception As String = ""
    Public ContentXmlReloaded As Boolean = False
    Public ContentFilesReloaded As Boolean = False
    Public Property Status As String
        Get
            Return Me.StatusValue
        End Get
        Set(ByVal value As String)
            If Not (value = StatusValue) Then
                Me.StatusValue = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Status"))
                xTextBlockStatus.Text = Me.Status
            End If
        End Set
    End Property
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    'EDITOR MODE
    Public EditorMode As Boolean = False

    'N E W  B L O C K
    Public Sub New(ByVal _title As String, ByVal _dir As String,
                   ByVal _left As Integer, ByVal _top As Integer, ByVal _width As Integer, ByVal _height As Integer,
               ByVal _type As String, ByVal _source As String, ByVal _dots As Boolean, ByVal _margin As Integer)
        Me.bTitle = _title
        Me.bDir = _dir
        Me.bLeft = _left
        Me.bTop = _top
        Me.bWidth = _width
        Me.bHeight = _height
        Me.bType = _type
        Me.bSource = _source
        Me.bDots = _dots
        Me.bMargin = _margin
    End Sub

    'L O A D  T E X T  B L O C K S
    Public Sub LoadTextBlocks(ByVal stg() As String)
        Me.TopText = New TextInfo(stg(0), stg(1), stg(2), stg(3), stg(4))
        Me.MidText = New TextInfo(stg(5), stg(6), stg(7), stg(8), stg(9))
        Me.BtmText = New TextInfo(stg(10), stg(11), stg(12), stg(13), stg(14))
    End Sub

    Public Sub BuildVisual()

        Me.Children.Clear()

        'GRID INIT
        Me.Width = bWidth
        Me.Height = bHeight
        Me.VerticalAlignment = VerticalAlignment.Top
        Me.HorizontalAlignment = HorizontalAlignment.Left
        Me.IsHitTestVisible = True

        'BG IMAGE
        xImageBlockBg.Stretch = Stretch.Fill
        Me.Children.Add(xImageBlockBg)

        'INIT FEEDS
        If bType.ToUpper = "RSS" Then
            InitRSSFeed()
            'TWITTER-TO-RSS: http://twitrss.me/
            'http://twitrss.me/twitter_user_to_rss/?user=BreakingNews
            'Dim thread As New Thread(AddressOf InitRSSFeed)            'thread.Start()
        End If
        If bType.ToUpper = "TWI" Then InitTwitterFeed()
        If bType.ToUpper = "TIME" Then InitTimeBlock()

        'CONTENT SCROLLER
        Dim xScrollViewerContent As New ScrollViewer
        xScrollViewerContent.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
        xScrollViewerContent.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden
        Me.Margin = New Thickness(bMargin)

        'CONTENT STACK
        If bType.ToUpper = "HOR" Or bType.ToUpper = "BLK" Then xStackPanelContentScroller.Orientation = Orientation.Horizontal
        If bType.ToUpper = "VER" Then xStackPanelContentScroller.Orientation = Orientation.Vertical

        'HIDE SLIDES STACK
        If bType.ToUpper = "RSS" Or bType.ToUpper = "TWI" Or bType.ToUpper = "TXT" Or bType.ToUpper = "TIME" Then
            xStackPanelContentScroller.Visibility = Visibility.Hidden
        End If
        xScrollViewerContent.Content = xStackPanelContentScroller
        Me.Children.Add(xScrollViewerContent)

        'TOP MASK
        xImageBlockMask.Opacity = 0
        xImageBlockMask.Stretch = Stretch.Fill
        Me.Children.Add(xImageBlockMask)

        'FG IMAGE
        xImageBlockFg.Stretch = Stretch.Fill
        Me.Children.Add(xImageBlockFg)

        'TOP TEXT BLOCK
        With xTextBlockTop
            .VerticalAlignment = VerticalAlignment.Top
            .HorizontalAlignment = HorizontalAlignment.Stretch
            .Padding = New Thickness(7)
            .TextWrapping = TextWrapping.Wrap
            .FontSize = Me.TopText.TextSize
            .Foreground = Me.TopText.FrontColor
            .Background = Me.TopText.BackColor
            .Text = Me.TopText.Text
            If Me.TopText.TextAlign = "LEFT" Then .TextAlignment = TextAlignment.Left
            If Me.TopText.TextAlign = "CENTER" Then .TextAlignment = TextAlignment.Center
            If Me.TopText.TextAlign = "RIGHT" Then .TextAlignment = TextAlignment.Right
            If .Text = "" Then .Visibility = Visibility.Hidden
        End With
        Me.Children.Add(xTextBlockTop)

        'CENTER TEXT BLOCK
        'Dim vBoxTextBlockMiddle As New Viewbox
        With xTextBlockMiddle
            .VerticalAlignment = VerticalAlignment.Center
            .HorizontalAlignment = HorizontalAlignment.Stretch
            .Padding = New Thickness(7)
            .TextWrapping = TextWrapping.Wrap
            .FontSize = Me.MidText.TextSize
            .Foreground = Me.MidText.FrontColor
            .Background = Me.MidText.BackColor
            .Text = Me.MidText.Text
            If Me.MidText.TextAlign = "LEFT" Then .TextAlignment = TextAlignment.Left
            If Me.MidText.TextAlign = "CENTER" Then .TextAlignment = TextAlignment.Center
            If Me.MidText.TextAlign = "RIGHT" Then .TextAlignment = TextAlignment.Right
            'If bType.ToUpper <> "RSS" And bType.ToUpper <> "TWI" And bType.ToUpper <> "TIME" Then
            If .Text = "" Then .Visibility = Visibility.Hidden
            'End If
            Dim sh_eff As New Effects.DropShadowEffect With {.Color = Me.MidText.BackColor.Color, .BlurRadius = 1, .ShadowDepth = 0}
            .Effect = sh_eff
        End With
        'vBoxTextBlockMiddle.Child = xTextBlockMiddle
        Me.Children.Add(xTextBlockMiddle)

        'BOTTOM TEXT BLOCK
        With xTextBlockBottom
            .VerticalAlignment = VerticalAlignment.Bottom
            .HorizontalAlignment = HorizontalAlignment.Stretch
            .Padding = New Thickness(7)
            .TextWrapping = TextWrapping.Wrap
            .FontSize = Me.BtmText.TextSize
            .Foreground = Me.BtmText.FrontColor
            .Background = Me.BtmText.BackColor
            .Text = Me.BtmText.Text
            If Me.BtmText.TextAlign = "LEFT" Then .TextAlignment = TextAlignment.Left
            If Me.BtmText.TextAlign = "CENTER" Then .TextAlignment = TextAlignment.Center
            If Me.BtmText.TextAlign = "RIGHT" Then .TextAlignment = TextAlignment.Right
            If .Text = "" Then .Visibility = Visibility.Hidden
        End With
        Me.Children.Add(xTextBlockBottom)

        'DOTS AND STATUS
        If Me.bDots Then
            'DOTS
            With xStackPanelBlockDots
                .Background = Brushes.Black
                .HorizontalAlignment = HorizontalAlignment.Left
                .VerticalAlignment = VerticalAlignment.Bottom
                .Orientation = Orientation.Horizontal
            End With
            Me.Children.Add(xStackPanelBlockDots)

            'STATUS TEXTBOX
            With xTextBlockStatus
                .Foreground = Brushes.White
                .Margin = New Thickness(10)
                .HorizontalAlignment = HorizontalAlignment.Left
                .VerticalAlignment = VerticalAlignment.Bottom
            End With
            Me.Children.Add(xTextBlockStatus)
        End If

        'TRANSFORMATIONS
        slide_trans_gr.Children.Add(slide_trans_xy)
        slide_trans_gr.Children.Add(slide_trans_z)
        xStackPanelContentScroller.RenderTransform = slide_trans_gr

        'TIMERS
        AddHandler SlidesTimer.Tick, AddressOf SlidesTimer_Tick
        SlidesTimer.Interval = TimeSpan.FromSeconds(0.5)
        AddHandler UpdateTimer.Tick, AddressOf UpdateTimer_Tick
        UpdateTimer.Interval = TimeSpan.FromSeconds(0.5) 'New TimeSpan(0, 0, 3)
        Me.Status = "BLOCK CREATED..."

        If bType.ToUpper = "PLUGIN" Then
            If bSource.ToUpper = "INTIME" Then
                Me.Children.Add(New InTimePluginPlayer(ContentLocalRoot + Me.bDir + "\", Me.bWidth, Me.bHeight))
            End If
        End If

        If EditorMode Then
            SelectionBorder = New Border With {.BorderThickness = New Thickness(5), .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#99000000")), _
                                             .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#996ab4d8"))}

            'ResizeControlLeft = New TextBlock With {.VerticalAlignment = VerticalAlignment.Center, .HorizontalAlignment = HorizontalAlignment.Left, _
            '                               .Height = 40, .Width = 40, .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#996ab4d8")), _
            '                               .Margin = New Thickness(4), .Foreground = Brushes.White, .TextAlignment = TextAlignment.Center, .FontSize = 24, .Text = ChrW(8612)}
            'ResizeControlTop = New TextBlock With {.VerticalAlignment = VerticalAlignment.Top, .HorizontalAlignment = HorizontalAlignment.Center, _
            '                   .Height = 40, .Width = 40, .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#996ab4d8")), _
            '                   .Margin = New Thickness(4), .Foreground = Brushes.White, .TextAlignment = TextAlignment.Center, .FontSize = 24, .Text = ChrW(8613)}

            ResizeControlRight = New TextBlock With {.VerticalAlignment = VerticalAlignment.Center, .HorizontalAlignment = HorizontalAlignment.Right, .Visibility = Visibility.Hidden, _
                                           .Height = 40, .Width = 40, .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#996ab4d8")), _
                                           .Margin = New Thickness(4), .Foreground = Brushes.White, .TextAlignment = TextAlignment.Center, .FontSize = 24, .Text = ChrW(8614)}
            ResizeControlBottom = New TextBlock With {.VerticalAlignment = VerticalAlignment.Bottom, .HorizontalAlignment = HorizontalAlignment.Center, .Visibility = Visibility.Hidden, _
                                           .Height = 40, .Width = 40, .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#996ab4d8")), _
                                           .Margin = New Thickness(4), .Foreground = Brushes.White, .TextAlignment = TextAlignment.Center, .FontSize = 24, .Text = ChrW(8615)}

            With Me.Children
                .Add(SelectionBorder)
                '.Add(ResizeControlLeft)
                '.Add(ResizeControlTop)
                .Add(ResizeControlRight)
                .Add(ResizeControlBottom)
            End With

            AddHandler ResizeControlRight.MouseDown, AddressOf ResizeControlRight_MouseDown
            AddHandler ResizeControlRight.MouseMove, AddressOf ResizeControlRight_MouseMove
            AddHandler ResizeControlRight.MouseUp, AddressOf ResizeControlRight_MouseUp

            AddHandler ResizeControlBottom.MouseDown, AddressOf ResizeControlBottom_MouseDown
            AddHandler ResizeControlBottom.MouseMove, AddressOf ResizeControlBottom_MouseMove
            AddHandler ResizeControlBottom.MouseUp, AddressOf ResizeControlBottom_MouseUp
        End If

    End Sub

    Public Event ResizeControlRight_Event()
    Public Event ResizeControlBottom_Event()

    Dim init_pos As Point

    'WIDTH RESIZE
    Public Sub ResizeControlRight_MouseDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
        init_pos = e.GetPosition(Me.Parent)
    End Sub
    Public Sub ResizeControlRight_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
        Dim ystep As Integer = 10
        Dim x_abs As Integer = e.GetPosition(Me.Parent).X
        Dim x_pos As Integer = e.GetPosition(ResizeControlRight).X
        If e.LeftButton.Equals(MouseButtonState.Pressed) Then
            If init_pos.X < x_abs Then Me.Width += x_pos
            If init_pos.X > x_abs Then Me.Width -= ResizeControlRight.ActualWidth - x_pos
        End If
    End Sub
    Public Sub ResizeControlRight_MouseUp()
        init_pos.X = 0
        Me.Width = Math.Round(Me.Width, 10)
        RaiseEvent ResizeControlRight_Event()
    End Sub

    'HEIGHT RESIZE
    Public Sub ResizeControlBottom_MouseDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
        init_pos = e.GetPosition(Me.Parent)
    End Sub
    Public Sub ResizeControlBottom_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
        Dim ystep As Integer = 10
        Dim y_abs As Integer = e.GetPosition(Me.Parent).Y
        Dim y_pos As Integer = e.GetPosition(ResizeControlBottom).Y
        If e.LeftButton.Equals(MouseButtonState.Pressed) Then
            If init_pos.Y < y_abs Then Me.Height += y_pos
            If init_pos.Y > y_abs Then Me.Height -= ResizeControlBottom.ActualHeight - y_pos
        End If
    End Sub
    Public Sub ResizeControlBottom_MouseUp() '!!! LEAVE PROBLEM HERE
        Me.Height = Math.Round(Me.Height, 1)
        init_pos.Y = 0
        RaiseEvent ResizeControlBottom_Event()
    End Sub

    '-----------------------------------------------------------------------------------------------------------------

    Dim SlidesXmlLoaded As Boolean = False

    'L O A D  S L I D E S  X M L
    Public Sub LoadSlidesXml()
        If UseSync Then ContentDirsSync()
        Dim xml_filepath As String = ContentLocalRoot + bDir + "\" + "slides.xml"
        Dim SlidesCount As Integer = 0
        Dim sets_count As Integer = 0
        SlidesXmlLoaded = False
        Dim _id() As ArrayList
        Dim _value() As ArrayList
        Dim xmlr As XmlTextReader
        'READ XML
        'Try
        If File.Exists(xml_filepath) Then
            xmlr = New XmlTextReader(xml_filepath)
            xmlr.WhitespaceHandling = WhitespaceHandling.None

            While xmlr.Read()

                While xmlr.Read
                    If xmlr.NodeType = XmlNodeType.EndElement And xmlr.Name = "slides" Then Exit While
                    'BLOCK SECTION
                    If xmlr.Name.Equals("slide") And xmlr.IsStartElement Then
                        ReDim Preserve _id(SlidesCount)
                        _id(SlidesCount) = New ArrayList
                        ReDim Preserve _value(SlidesCount)
                        _value(SlidesCount) = New ArrayList
                        SlidesCount += 1
                    End If
                    'READ SET ATTRIBUTES
                    If xmlr.Name.Equals("set") Then
                        _id(SlidesCount - 1).Add(xmlr.GetAttribute("id"))
                        _value(SlidesCount - 1).Add(xmlr.GetAttribute("value"))
                    End If
                End While

            End While 'END READ XML
            xmlr.Close()
            xmlr = Nothing

            'SLIDES DATA
            Slides = Nothing
            Slides_bak = Nothing

            If Not IsNothing(_id) Then
                For i = 0 To _id.Count - 1
                    Dim template As New SlideTemplate
                    Dim settings() As String = template.DefaultValues
                    For j = 0 To _id(i).Count - 1
                        If _id(i).Item(j) = template.Identificators(j) Then settings(j) = _value(i).Item(j)
                    Next j
                    'NEW SLIDE ITEM
                    ReDim Preserve Slides(i)
                    Slides(i) = New Slide(settings)
                    Slides(i).Order = i + 1
                Next i
            End If

            Slides_bak = Slides

            SlidesXmlLoaded = True
            If SlidesCount = 0 Then SlidesXmlLoaded = False
        Else
            'CREATE NEW slides.xml
            '...
            AddToLog("ERR: Missing " + xml_filepath)
            SlidesXmlLoaded = False
        End If
        'Catch ex As Exception
        '    AddToLog(filename + " LOAD ERR: " + ex.ToString)
        'End Try
    End Sub

    Public Sub ClearBlockContent()
        'CLEAR OBJECTS
        xStackPanelBlockDots.Children.Clear()
        xStackPanelContentScroller.Children.Clear()
        xImageBlockBg.Source = Nothing
        xImageBlockFg.Source = Nothing
        xImageBlockMask.Source = Nothing
        '... text and other stuff to clear
    End Sub

    'L O A D  I M A G E S  A N D  V I D E O S
    Public Sub LoadContent()
        'Try
        If Directory.Exists(ContentLocalRoot + bDir) Then
            ContentFilesReloaded = False
            'CLEAR OBJECTS
            xStackPanelBlockDots.Children.Clear()
            xStackPanelContentScroller.Children.Clear()

            'ADD ITEMS (IMG/VDO SLIDES)
            If Not IsNothing(Slides) Then


                'CLEAR SLIDES() FROM NOT DISPLAYED
                Slides = Slides_bak
                Dim skip_slide_onload() As Boolean
                For i = 0 To Slides.Count - 1
                    ReDim Preserve skip_slide_onload(i)
                    skip_slide_onload(i) = False
                    'CHECK LOCATION
                    If Slides(i).Location <> "" Then
                        If Slides(i).Location <> Me.bLocationPreset Then skip_slide_onload(i) = True
                    End If
                    'CHECK TIMING
                    If Slides(i).TimeLimit Then
                        If Not CheckDateTimeLimit(Slides(i).FromTime, Slides(i).ToTime) Then skip_slide_onload(i) = True
                    End If
                Next
                For i = Slides.Count - 1 To 0 Step -1
                    If skip_slide_onload(i) Then
                        Slides = RemoveElementFromArray(Slides, i, GetType(Slide))
                    End If
                Next

                For i = 0 To Slides.Count - 1

                    'MAIN CONTAINER FOR SLIDE
                    Dim GridSlideContainer As New Grid With {.Name = "SlideContainer" + CStr(i), .Width = Me.bWidth, .Height = Me.bHeight}
                    Dim ScrollViewerImageSlide As New ScrollViewer With {.Name = "ScrollViewerImageSlide" + CStr(Me.bOrder) + "_" + CStr(i)}
                    With ScrollViewerImageSlide
                        .Width = Me.bWidth
                        .Height = Me.bHeight
                        .HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
                        .VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
                    End With
                    'If Not IsNothing(FindName(SlideContainer.Name)) Then UnregisterName(SlideContainer.Name)
                    'RegisterName(SlideContainer.Name, SlideContainer)

                    'SCROLLVIEW PIC
                    Dim ImageSlide As New Image With {.Name = "ImageSlide" + CStr(Me.bOrder) + "_" + CStr(i)}
                    Dim BitmapImg As New BitmapImage
                    If Slides(i).Source.ToUpper.EndsWith("JPG") Or _
                        Slides(i).Source.ToUpper.EndsWith("GIF") Or _
                        Slides(i).Source.ToUpper.EndsWith("PNG") Then
                        If File.Exists(ContentLocalRoot + bDir + "\" + Slides(i).Source) Then
                            With BitmapImg
                                .BeginInit()
                                .CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                .CacheOption = BitmapCacheOption.OnLoad
                                .UriSource = New Uri(ContentLocalRoot + bDir + "\" + Slides(i).Source)
                                .EndInit()
                            End With
                            If Not IsNothing(BitmapImg) Then
                                'GIF
                                If Slides(i).Source.ToUpper.EndsWith("GIF") Then _
                                    WpfAnimatedGif.ImageBehavior.SetAnimatedSource(ImageSlide, BitmapImg)
                                ImageSlide.Source = BitmapImg
                            End If
                            With ImageSlide
                                .Stretch = Stretch.Fill
                                '.Width = bWidth
                                '.Height = bHeight
                                If Me.Slides(i).Mode = "NOFILL" Then .Stretch = Stretch.None
                                If Me.Slides(i).Mode = "UNI" Then .Stretch = Stretch.Uniform
                                If Me.Slides(i).Mode = "UNIFILL" Then .Stretch = Stretch.UniformToFill
                            End With

                            'If Not IsNothing(FindName(ImageSlide.Name)) Then UnregisterName(ImageSlide.Name)
                            'RegisterName(ImageSlide.Name, ImageSlide)

                            ScrollViewerImageSlide.Content = ImageSlide
                            GridSlideContainer.Children.Add(ScrollViewerImageSlide)

                            If Not IsNothing(FindName(ImageSlide.Name)) Then UnregisterName(ImageSlide.Name)
                            Try
                                Me.RegisterName(ImageSlide.Name, ImageSlide) '!!! G-CODE !!!
                            Catch ex As Exception
                                AddToLog(ImageSlide.Name + " registed error")
                            End Try

                            If Not IsNothing(FindName(ScrollViewerImageSlide.Name)) Then UnregisterName(ScrollViewerImageSlide.Name)
                            Try 'AHTUNG G-CODE !!!
                                Me.RegisterName(ScrollViewerImageSlide.Name, ScrollViewerImageSlide)
                            Catch ex As Exception

                            End Try

                        Else
                            Me.Status = "IMAGE CONTENT MISSING!"
                        End If
                    End If

                    'SCROLLVIEW VDO
                    If Slides(i).Source.ToUpper.EndsWith("AVI") Then
                        'MEDIA-KIT
                        Dim MediaKitItem As New WPFMediaKit.DirectShow.Controls.MediaUriElement
                        With MediaKitItem
                            .Name = "MediaElementItem" + CStr(i)
                            .Width = bWidth
                            .Height = bHeight
                            .BeginInit()
                            'STRETCH MODES
                            .Stretch = Stretch.Fill
                            .LoadedBehavior = WPFMediaKit.DirectShow.MediaPlayers.MediaState.Manual
                            .VideoRenderer = WPFMediaKit.DirectShow.MediaPlayers.VideoRendererType.VideoMixingRenderer9
                            If File.Exists(ContentLocalRoot + bDir + "\" + Slides(i).Source) Then
                                .Source = New Uri(ContentLocalRoot + bDir + "\" + Slides(i).Source)
                                .Volume = 5 / 100 'TO FIX!!!
                            Else
                                Me.Status = "VIDEO CONTENT MISSING!"
                            End If
                            .EndInit()
                        End With
                        GridSlideContainer.Children.Add(MediaKitItem)
                        If Not IsNothing(FindName(MediaKitItem.Name)) Then UnregisterName(MediaKitItem.Name)
                        Me.RegisterName(MediaKitItem.Name, MediaKitItem)
                        AddHandler MediaKitItem.MediaEnded, AddressOf MediaScroller_MediaEnded
                    End If

                    'SLIDE TEXT BLOCKS

                    'TOP TEXT BLOCK
                    Dim slideTextBlockTop As New TextBlock With {.VerticalAlignment = VerticalAlignment.Top, .HorizontalAlignment = HorizontalAlignment.Stretch, _
                                                                 .Padding = New Thickness(7), .TextWrapping = TextWrapping.Wrap, _
                                                                 .Text = Me.Slides(i).TopText.Text, .FontSize = Me.Slides(i).TopText.TextSize, _
                                                                 .Foreground = Me.Slides(i).TopText.FrontColor, .Background = Me.Slides(i).TopText.BackColor}
                    If Me.Slides(i).TopText.Text = "" Then slideTextBlockTop.Visibility = Visibility.Hidden
                    GridSlideContainer.Children.Add(slideTextBlockTop)

                    'MID TEXT BLOCK
                    Dim vBoxSlideTextBlockMid As New Viewbox
                    Dim slideTextBlockMid As New TextBlock With {.VerticalAlignment = VerticalAlignment.Center, .HorizontalAlignment = HorizontalAlignment.Center, _
                                                                 .Padding = New Thickness(7), .TextWrapping = TextWrapping.Wrap, _
                                                                 .Text = Me.Slides(i).MidText.Text, .FontSize = Me.Slides(i).MidText.TextSize, _
                                                                 .Foreground = Me.Slides(i).MidText.FrontColor, .Background = Me.Slides(i).MidText.BackColor}
                    If Me.Slides(i).MidText.Text = "" Then slideTextBlockMid.Visibility = Visibility.Hidden
                    Dim sh_eff As New Effects.DropShadowEffect With {.Color = Me.Slides(i).MidText.BackColor.Color, .BlurRadius = 2, .ShadowDepth = 0}
                    slideTextBlockMid.Effect = sh_eff
                    With Me.Slides(i).MidText
                        If .TextAlign = "LEFT" Then vBoxSlideTextBlockMid.HorizontalAlignment = HorizontalAlignment.Left
                        If .TextAlign = "CENTER" Then vBoxSlideTextBlockMid.HorizontalAlignment = HorizontalAlignment.Center
                        If .TextAlign = "RIGHT" Then vBoxSlideTextBlockMid.HorizontalAlignment = HorizontalAlignment.Right

                        If .TextAlign = "LEFT" Then slideTextBlockMid.TextAlignment = TextAlignment.Left
                        If .TextAlign = "CENTER" Then slideTextBlockMid.TextAlignment = TextAlignment.Center
                        If .TextAlign = "RIGHT" Then slideTextBlockMid.TextAlignment = TextAlignment.Right
                    End With

                    slideTextBlockMid.MaxWidth = bWidth 'for correct wrapping in viewbox

                    vBoxSlideTextBlockMid.Child = slideTextBlockMid
                    GridSlideContainer.Children.Add(vBoxSlideTextBlockMid)

                    'MED TEXT BLOCK
                    Dim slideTextBlockBtm As New TextBlock With {.VerticalAlignment = VerticalAlignment.Bottom, .HorizontalAlignment = HorizontalAlignment.Stretch, _
                                                                 .Padding = New Thickness(7), .TextWrapping = TextWrapping.Wrap, _
                                                                 .Text = Me.Slides(i).BtmText.Text, .FontSize = Me.Slides(i).BtmText.TextSize, _
                                                                 .Foreground = Me.Slides(i).BtmText.FrontColor, .Background = Me.Slides(i).BtmText.BackColor}
                    If Me.Slides(i).BtmText.Text = "" Then slideTextBlockBtm.Visibility = Visibility.Hidden
                    GridSlideContainer.Children.Add(slideTextBlockBtm)

                    'STACK ITEM
                    If bType <> "EFF" Then
                        xStackPanelContentScroller.Children.Add(GridSlideContainer)
                    End If

                    'EFF TYPE ITEM
                    If bType = "EFF" Then
                        ImageSlide.Width = bWidth
                        ImageSlide.Height = bHeight
                        Dim SlideShowItem As New Transitionals.Controls.SlideshowItem With {.Content = GridSlideContainer}
                        xSlideShow.Items.Add(SlideShowItem)
                    End If

                    'INIT SLIDES SHIFT
                    If bType.ToUpper = "HOR" Or bType.ToUpper = "BLK" Then
                        slide_trans_xy.X = bWidth
                        slide_trans_xy.BeginAnimation(TranslateTransform.XProperty, Nothing)
                    End If
                    If bType.ToUpper = "VER" Then
                        slide_trans_xy.Y = bHeight
                        slide_trans_xy.BeginAnimation(TranslateTransform.YProperty, Nothing)
                    End If
                    'DOTS
                    If bDots Then
                        Dim dot As New Border
                        dot.Width = 5
                        dot.Height = 5
                        dot.Margin = New Thickness(2)
                        dot.Background = Brushes.White
                        If Slides(i).Source.EndsWith("avi") Then dot.Background = Brushes.Red
                        xStackPanelBlockDots.Children.Add(dot)
                    End If

                    'End If

                Next i

                'EFF TYPE SLIDESHOW CONTAINER
                If bType = "EFF" Then
                    Dim transel As New Transitionals.RandomTransitionSelector
                    Dim ttrans(22) As Object
                    ttrans(1) = New Transitionals.Transitions.CheckerboardTransition
                    ttrans(2) = New Transitionals.Transitions.DiagonalWipeTransition
                    ttrans(3) = New Transitionals.Transitions.DiamondsTransition
                    ttrans(4) = New Transitionals.Transitions.DoorTransition
                    ttrans(5) = New Transitionals.Transitions.DotsTransition
                    ttrans(6) = New Transitionals.Transitions.DoubleRotateWipeTransition
                    ttrans(7) = New Transitionals.Transitions.ExplosionTransition
                    ttrans(8) = New Transitionals.Transitions.FadeAndBlurTransition
                    ttrans(9) = New Transitionals.Transitions.FadeAndGrowTransition
                    ttrans(10) = New Transitionals.Transitions.FadeTransition
                    ttrans(11) = New Transitionals.Transitions.FlipTransition
                    ttrans(12) = New Transitionals.Transitions.HorizontalBlindsTransition
                    ttrans(13) = New Transitionals.Transitions.HorizontalWipeTransition
                    ttrans(14) = New Transitionals.Transitions.MeltTransition
                    ttrans(15) = New Transitionals.Transitions.PageTransition
                    ttrans(16) = New Transitionals.Transitions.RollTransition
                    ttrans(17) = New Transitionals.Transitions.RotateTransition
                    ttrans(18) = New Transitionals.Transitions.RotateWipeTransition
                    ttrans(19) = New Transitionals.Transitions.StarTransition
                    ttrans(20) = New Transitionals.Transitions.TranslateTransition
                    ttrans(21) = New Transitionals.Transitions.VerticalBlindsTransition
                    ttrans(22) = New Transitionals.Transitions.VerticalWipeTransition
                    For i = 1 To 22
                        transel.Transitions.Add(ttrans(i))
                    Next i
                    xSlideShow.TransitionSelector = transel
                    If IsNumeric(bSource) And bSource <> "" Then
                        If CInt(bSource) <= 22 Then
                            xSlideShow.TransitionSelector = Nothing
                            xSlideShow.Transition = ttrans(CInt(bSource))
                        End If
                    End If

                    xStackPanelContentScroller.Children.Add(xSlideShow)
                    'xSlideShow.AutoAdvance = True
                    'xSlideShow.AutoAdvanceDuration = TimeSpan.FromSeconds(5)
                End If

                ContentFilesReloaded = True
                'START SLIDES TIMER
                If Slides.Count > 0 Then SlidesTimer.Start()
            End If
        Else
            Me.Status = "No slides at: " + ContentLocalRoot + bDir
        End If

        'START SYNC TIMER
        If UseSync Then UpdateTimer.Start()

        'BG IMG
        Dim bgPNGfile As String = ContentLocalRoot + bDir + "\" + "bg.png"
        Dim bgJPGfile As String = ContentLocalRoot + bDir + "\" + "bg.jpg"
        Dim bgfile As String = ""
        If File.Exists(bgPNGfile) Then bgfile = bgPNGfile
        If File.Exists(bgJPGfile) Then bgfile = bgJPGfile
        If bgfile <> "" Then
            Dim BmpImgBg As New BitmapImage
            With BmpImgBg
                .BeginInit()
                .CreateOptions = BitmapCreateOptions.IgnoreImageCache
                .CacheOption = BitmapCacheOption.OnLoad
                .UriSource = New Uri(bgfile)
                .EndInit()
            End With
            If Not IsNothing(BmpImgBg) Then xImageBlockBg.Source = BmpImgBg
        End If
        'MASK
        If File.Exists(ContentLocalRoot + bDir + "\" + "mask.png") Then
            Dim BmpImgMask As New BitmapImage
            With BmpImgMask
                .BeginInit()
                .CreateOptions = BitmapCreateOptions.IgnoreImageCache
                .CacheOption = BitmapCacheOption.OnLoad
                .UriSource = New Uri((ContentLocalRoot + bDir + "\" + "mask.png"))
                .EndInit()
            End With
            If Not IsNothing(BmpImgMask) Then xImageBlockMask.Source = BmpImgMask
        End If
        'FG IMG
        If File.Exists(ContentLocalRoot + bDir + "\" + "fg.png") Then
            Dim BmpImgFg As New BitmapImage
            With BmpImgFg
                .BeginInit()
                .CreateOptions = BitmapCreateOptions.IgnoreImageCache
                .CacheOption = BitmapCacheOption.OnLoad
                .UriSource = New Uri((ContentLocalRoot + bDir + "\" + "fg.png"))
                .EndInit()
            End With
            If Not IsNothing(BmpImgFg) Then xImageBlockFg.Source = BmpImgFg
        End If
        CurrentSlide = -1
        'Catch ex As Exception
        'Me.Exception = "CONTENT LOAD ERR: " + ex.ToString
        'Me.Status = "CONTENT LOAD ERR: " + ex.ToString
        'MsgBox(ex.ToString)
        'End Try
    End Sub

    'S L I D E S  T I M E R
    Dim next_slide As Integer

    Private Sub SlidesTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)

        pic_loops = 0
        CurrentSlide += 1
        If CurrentSlide = Slides.Count Then CurrentSlide = 0

        'DURATION SET FOR NEXT PIC
        If Slides.Count >= 2 Then
            next_slide = CurrentSlide + 1
            If CurrentSlide = Slides.Count - 1 Then next_slide = 0
            SlidesTimer.Interval = TimeSpan.FromSeconds(Slides(CurrentSlide).Duration)
            If Slides(CurrentSlide).Source.ToUpper.EndsWith("AVI") Then _
                SlidesTimer.Interval = New TimeSpan(0, 0, Slides(next_slide).Duration) 'NextSlide ???
        End If

        'DOTS
        If bDots Then
            For i = 0 To Slides.Count - 1
                Dim imgitm As New Border
                If Not IsNothing(xStackPanelBlockDots.Children(i)) Then imgitm = xStackPanelBlockDots.Children(i)
                imgitm.Opacity = 0.5
                If i = CurrentSlide Then If Not IsNothing(imgitm) Then imgitm.Opacity = 1
            Next i
        End If

        'SCROLLER MOVE
        Dim w As Integer = bWidth
        Dim h As Integer = bHeight
        slide_trans_z.CenterX = w / 2
        slide_trans_z.CenterY = h / 2
        Dim anim_xy As DoubleAnimation = Nothing
        Dim ease As New BackEase With {.Amplitude = 0.25, .EasingMode = EasingMode.EaseInOut}
        Dim anim_duration As Double = 2 'sec

        If CurrentSlide = 0 Then
            'move to start:
            If bType.ToUpper = "HOR" Then
                slide_trans_xy.X = w
                slide_trans_xy.BeginAnimation(TranslateTransform.XProperty, Nothing)
                anim_xy = New DoubleAnimation(-w * (Slides.Count - 1), slide_trans_xy.X - w, TimeSpan.FromSeconds(anim_duration))
            End If
            If bType.ToUpper = "VER" Then
                slide_trans_xy.Y = h
                slide_trans_xy.BeginAnimation(TranslateTransform.YProperty, Nothing)
                anim_xy = New DoubleAnimation(-h * (Slides.Count - 1), slide_trans_xy.Y - h, TimeSpan.FromSeconds(anim_duration))
            End If
            If bType.ToUpper = "BLK" Then slide_trans_xy.X = w
        Else
            'or move to next item:
            If bType.ToUpper = "HOR" Then _
                anim_xy = New DoubleAnimation(slide_trans_xy.X, slide_trans_xy.X - w, TimeSpan.FromSeconds(anim_duration))
            If bType.ToUpper = "VER" Then _
                anim_xy = New DoubleAnimation(slide_trans_xy.Y, slide_trans_xy.Y - h, TimeSpan.FromSeconds(anim_duration))
        End If

        If Not IsNothing(anim_xy) Then
            anim_xy.EasingFunction = ease
            If bType.ToUpper = "HOR" Then slide_trans_xy.BeginAnimation(TranslateTransform.XProperty, anim_xy)
            If bType.ToUpper = "VER" Then slide_trans_xy.BeginAnimation(TranslateTransform.YProperty, anim_xy)
        End If

        'BLINK ANIMATION
        If bType.ToUpper = "BLK" Then
            slide_trans_xy.X -= w
            If Slides.Count > 1 Then
                Dim img_obj As Image = FindName("ImageSlide" + CStr(CurrentSlide)) 'WTF ???
                xStackPanelContentScroller.BeginAnimation(StackPanel.OpacityProperty, New DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5)))
            End If
        End If

        'Z AND MASK ANIMATION
        If bType.ToUpper <> "BLK" And bType.ToUpper <> "EFF" Then
            xImageBlockMask.BeginAnimation(Image.OpacityProperty, New DoubleAnimation(0, 1, TimeSpan.FromSeconds(1)) With {.AutoReverse = True})
        End If

        If bType.ToUpper = "EFF" Then
            xSlideShow.TransitionNext()
        End If

        'VDO SLIDE 
        If Slides(CurrentSlide).Source.ToUpper.EndsWith("AVI") Then
            Try
                'Dim media_itm As MediaElement = FindName("MediaElementItem" + CStr(CurrentSlide))
                'media_itm.LoadedBehavior = MediaState.Manual
                Dim media_itm As WPFMediaKit.DirectShow.Controls.MediaUriElement = FindName("MediaElementItem" + CStr(CurrentSlide))
                media_itm.LoadedBehavior = WPFMediaKit.DirectShow.MediaPlayers.MediaState.Play
                If File.Exists(ContentLocalRoot + bDir + "\" + Slides(CurrentSlide).Source) Then
                    'If File.Exists(ContentLocalRoot + bDir + "\" + SlideFile(CurrentSlide)) Then
                    media_itm.BeginInit()
                    media_itm.Source = New Uri(ContentLocalRoot + bDir + "\" + Slides(CurrentSlide).Source)
                    'media_itm.Source = New Uri(ContentLocalRoot + bDir + "\" + SlideFile(CurrentSlide))
                    media_itm.Volume = 5 / 100
                    media_itm.EndInit()
                    'pic_loops = SlideDuration(CurrentSlide)
                    pic_loops = Slides(CurrentSlide).Duration
                    media_itm.Play()
                    SlidesTimer.Stop()
                Else
                    Me.Status = "MISSING AVI FILE"
                End If
            Catch ex As Exception
                Me.Status = "AVI PLAYBACK ERROR!"
                AddToLog("AVI PLAYBACK ERROR: " + ex.ToString)
            End Try
        End If

        'UNIFILL ANIMATION
        If Me.Slides(CurrentSlide).Mode = "UNIFILL" Then
            Dim ImageSlide As Image = Me.FindName("ImageSlide" + CStr(Me.bOrder) + "_" + CStr(CurrentSlide))
            Dim ScrollViewerImageSlide As ScrollViewer = Me.FindName("ScrollViewerImageSlide" + CStr(Me.bOrder) + "_" + CStr(CurrentSlide))
            If Not IsNothing(ImageSlide) Then
                ImageSlide.RenderTransform = Nothing
                If ImageSlide.ActualWidth > Me.bWidth Then
                    ScrollViewerImageSlide.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
                    ScrollViewerImageSlide.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
                    Dim trans1 As New TranslateTransform
                    ImageSlide.RenderTransform = trans1
                    Dim anim As New DoubleAnimation(0, (Me.bWidth - ImageSlide.ActualWidth), TimeSpan.FromSeconds(9))
                    anim.BeginTime = TimeSpan.FromSeconds(2)
                    anim.AutoReverse = True
                    anim.RepeatBehavior = RepeatBehavior.Forever
                    Dim ease1 As New ExponentialEase
                    ease1.EasingMode = EasingMode.EaseInOut
                    anim.EasingFunction = ease1
                    trans1.BeginAnimation(TranslateTransform.YProperty, Nothing)
                    trans1.BeginAnimation(TranslateTransform.XProperty, anim)
                End If

                If ImageSlide.ActualHeight > Me.bHeight Then
                    ScrollViewerImageSlide.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden
                    ScrollViewerImageSlide.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
                    Dim trans1 As New TranslateTransform
                    ImageSlide.RenderTransform = trans1
                    Dim anim As New DoubleAnimation(0, (Me.bHeight - ImageSlide.ActualHeight), TimeSpan.FromSeconds(9))
                    anim.BeginTime = TimeSpan.FromSeconds(2)
                    anim.AutoReverse = True
                    anim.RepeatBehavior = RepeatBehavior.Forever
                    Dim ease1 As New ExponentialEase
                    ease1.EasingMode = EasingMode.EaseInOut
                    anim.EasingFunction = ease1
                    trans1.BeginAnimation(TranslateTransform.XProperty, Nothing)
                    trans1.BeginAnimation(TranslateTransform.YProperty, anim)
                End If
            End If

            'If Image1.ActualHeight > ScrollViewerImage.ActualHeight Then
            '    ScrollViewerImage.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden
            '    ScrollViewerImage.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            '    Dim trans As New TranslateTransform
            '    Image1.RenderTransform = trans
            '    Dim anim As New DoubleAnimation(0, (ScrollViewerImage.ActualHeight - Image1.ActualHeight), TimeSpan.FromSeconds(5))
            '    anim.BeginTime = TimeSpan.FromSeconds(1)
            '    Dim ease As New ExponentialEase
            '    ease.EasingMode = EasingMode.EaseInOut
            '    anim.EasingFunction = ease
            '    trans.BeginAnimation(TranslateTransform.XProperty, Nothing)
            '    trans.BeginAnimation(TranslateTransform.YProperty, anim)
            'End If
        End If

        If Slides.Count <= 1 Then SlidesTimer.Stop()

    End Sub

    'S C R O L L E R  V I D E O  E N D S
    Dim pic_loops As Integer = 0
    Private Sub MediaScroller_MediaEnded()
        Try
            'Dim media_itm As MediaElement = FindName("MediaElementItem" + CStr(CurrentSlide))
            Dim media_itm As WPFMediaKit.DirectShow.Controls.MediaUriElement = FindName("MediaElementItem" + CStr(CurrentSlide))
            pic_loops -= 1
            If pic_loops > 0 Then
                media_itm.BeginInit()
                media_itm.Source = New Uri(ContentLocalRoot + bDir + "\" + Slides(CurrentSlide).Source)
                media_itm.EndInit()
                media_itm.Play()
            End If
            If pic_loops <= 0 Then
                SlidesTimer.Interval = New TimeSpan(0, 0, 0)
                SlidesTimer.Start()
            End If
        Catch ex As Exception
            Me.Status = "MEDIA PLAYBACK ERR: " + ex.ToString
        End Try
    End Sub

    'D I R  C O N T E N T S  S T A T U S
    Dim sync_file As String = ""
    Function SyncStatus() As String
        Dim compare_state As String = ""
        sync_file = ""
        'Try
        Dim source_files As String() = getFiles(ContentSourceRoot + bDir, "*.*", SearchOption.TopDirectoryOnly)
        'For i = 0 To source_files.Count - 1
        '    Dim file_info As New FileInfo(source_files(i))
        'Next
        Dim local_files As String() = getFiles(ContentLocalRoot + bDir, "*.*", SearchOption.TopDirectoryOnly)
        'For i = 0 To local_files.Count - 1
        '    Dim file_info As New FileInfo(local_files(i))
        'Next
        Dim samename_files_count As Integer = 0
        If Not IsNothing(source_files) And Not IsNothing(local_files) Then
            'COMPARE FILES COUNT
            If source_files.Count = local_files.Count Then
                'SAME COUNT -> COMPARE NAMES AND WR.TIME
                For i = 0 To source_files.Count - 1
                    Dim source_file_info As New FileInfo(source_files(i))
                    For j = 0 To local_files.Count - 1
                        Dim local_file_info As New FileInfo(local_files(j))
                        'IF SAME NAMES
                        If source_file_info.Name = local_file_info.Name Then
                            samename_files_count += 1
                            'COMPARE WR.TIME
                            If source_file_info.LastWriteTime = local_file_info.LastWriteTime Then
                                'SAME TIME
                                compare_state = "UTD"
                            Else
                                'DIFF TIME
                                Return "SYNC"
                                Exit Function
                            End If
                        End If
                    Next j
                Next i
                'HAVE DIFF NAMES
                If samename_files_count <> source_files.Count Then
                    Return "SYNC"
                    Exit Function
                End If
            Else
                'DIFFERENT COUNT

                'GET FILE NAMES FROM FOLDERS
                Dim source_files_info() As FileInfo
                Dim source_files_info_names() As String
                For i = 0 To source_files.Count - 1
                    ReDim Preserve source_files_info(i)
                    source_files_info(i) = New FileInfo(source_files(i))
                    ReDim Preserve source_files_info_names(i)
                    source_files_info_names(i) = source_files_info(i).Name
                Next

                Dim local_files_info() As FileInfo
                Dim local_files_info_names() As String
                For i = 0 To local_files.Count - 1
                    ReDim Preserve local_files_info(i)
                    local_files_info(i) = New FileInfo(local_files(i))
                    ReDim Preserve local_files_info_names(i)
                    local_files_info_names(i) = local_files_info(i).Name
                Next

                If source_files.Count > local_files.Count Then
                    'ADD NEW FILES
                    sync_file = "add"
                    'there are more files as cloud folder then on local station
                    'get new files list...
                    Dim add_filenames = source_files_info_names

                    'If Not IsNothing(local_files_info_names) Then add_filenames = source_files_info_names.Except(local_files_info_names) 'ERROR HERE !!! --- TO FIX
                    If Not IsNothing(local_files_info_names) Then add_filenames = source_files_info_names

                    For i = 0 To add_filenames.Count - 1
                        File.Copy(ContentSourceRoot + bDir + "\" + add_filenames(i), ContentLocalRoot + bDir + "\" + add_filenames(i), True)
                    Next
                    'MsgBox(add_filenames.Last.ToString)
                    Return "SYNC"
                    Exit Function
                Else
                    'REMOVE FILES
                    sync_file = "remove"
                    'there are more files at local station then at cloud, have to clean them
                    'get removed files list...
                    Dim del_filenames = local_files_info_names
                    Try
                        If Not IsNothing(source_files_info_names) Then del_filenames = local_files_info_names.Except(source_files_info_names)
                    Catch ex As Exception

                    End Try
                    'MsgBox(del_filenames.Last.ToString)
                    'delete files
                    For i = 0 To del_filenames.Count - 1
                        Try
                            File.Delete(ContentLocalRoot + bDir + "\" + del_filenames(i))
                        Catch ex As Exception
                            AddToLog("SYNC ERR: Cannot delete local file. Details: " + ex.Message)
                        End Try
                    Next
                    Return "UPD"
                End If
                'SyncStatus() '--- double check?
                Return "SYNC"
                Exit Function
            End If
        End If
        Return compare_state
        'Catch ex As Exception
        '    Return "ERROR" + ex.ToString
        'End Try
    End Function

    Public Function getFiles(ByVal SourceFolder As String, ByVal Filter As String, ByVal searchOption As System.IO.SearchOption) As String()
        If Directory.Exists(SourceFolder) Then
            Dim alFiles As ArrayList = New ArrayList() ' ArrayList will hold all file names
            Dim MultipleFilters() As String = Filter.Split("|") ' Create an array of filter string
            For Each FileFilter As String In MultipleFilters ' for each filter find mathing file names
                Dim files() As String
                Try
                    files = Directory.GetFiles(SourceFolder, FileFilter, searchOption)
                Catch ex As Exception
                    AddToLog("ERR getting files: " + ex.Message)
                    MsgBox("ERR getting files: " + ex.Message)
                End Try
                If Not IsNothing(files) Then alFiles.AddRange(files) ' add found file names to array list
            Next
            Return alFiles.ToArray(Type.GetType("System.String")) ' returns string array of relevant file names
        Else
            Return Nothing
        End If
    End Function

    'C O N T E N T  S Y NC
    Public Sub ContentDirsSync()
        If Not Directory.Exists(ContentLocalRoot + bDir) Then Directory.CreateDirectory(ContentLocalRoot + bDir)
        If Directory.Exists(ContentLocalRoot + bDir) And Directory.Exists(ContentSourceRoot + bDir) Then
            Try
                Directory.Delete(ContentLocalRoot + bDir, True)
                AddToLog("SYNC OK")
            Catch ex As Exception
                AddToLog("SYNC ERR: " + ex.ToString)
                Me.Status = "content sync err, cannot delete dir"
            End Try
            If Not IsNothing(ContentSourceRoot) Then CopyDirectory(ContentSourceRoot + bDir, ContentLocalRoot + bDir)
            Me.Status += " / DIR SYNC OK"
        End If
    End Sub

    'SYNC TIMER
    Dim sync_proc As Boolean = False
    Dim timelimit_status(99) As Boolean '99 = g-code, ahtung!
    Dim timelimit_statusbefore(99) As Boolean
    Dim timelimit_timestatus(99) As Boolean
    Dim timelimit_timestatusbefore(99) As Boolean

    Public Sub UpdateTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)

        'TIME LIMIT CHECK FOR SLIDES
        If Not IsNothing(Me.Slides_bak) Then
            For i = 0 To Me.Slides_bak.Count - 1
                With Me.Slides_bak(i)
                    'check if TimeLimit was enabled
                    timelimit_status(i) = .TimeLimit
                    If timelimit_status(i) <> timelimit_statusbefore(i) Then
                        If Not .TimeLimit Then
                            LoadContent() ' no limits - just load content as common
                        Else
                            If CheckDateTimeLimit(.FromTime, .ToTime) Then LoadContent() 'Else .ClearBlockContent()
                            'have limit enabled? then check if now is right time to load content, if not - clear stuff
                        End If
                        '... or relaod all block data?
                    Else
                        'status was not chnages, so have to check if time is come
                        If .TimeLimit Then
                            timelimit_timestatus(i) = CheckDateTimeLimit(.FromTime, .ToTime)
                            If timelimit_timestatus(i) <> timelimit_timestatusbefore(i) Then
                                If CheckDateTimeLimit(.FromTime, .ToTime) Then LoadContent() 'Else .ClearBlockContent()
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
            Dim sync_status As String = ""

            If Not sync_proc Then sync_status = SyncStatus()

            Me.Status = sync_status.ToString

            'INIT SYNC WATCHDOG
            If sync_status = "SYNC" Then
                sync_proc = True
                Me.Status = "UPDATING CONTENT ..."
            End If

            'START SYNC
            If sync_status = "SYNC" And sync_proc Then
                SlidesTimer.Stop()
                SlidesTimer.Interval = TimeSpan.FromSeconds(0.5)
                'STOP ALL MEDIA
                If Not IsNothing(Me.Slides) Then
                    For i = 0 To Slides.Count - 1
                        Dim media_itm As WPFMediaKit.DirectShow.Controls.MediaUriElement = FindName("MediaElementItem" + CStr(i))
                        If Not IsNothing(media_itm) Then
                            With media_itm
                                .LoadedBehavior = MediaState.Manual
                                .Stop()
                                .Close()
                                .LoadedBehavior = MediaState.Close
                                .Source = Nothing
                            End With
                        End If
                    Next
                End If

                ContentDirsSync()
                LoadSlidesXml()
                LoadContent()
                sync_proc = False
                Me.Status += " / UPD COMPLETE"
            End If
        End If

        'Catch ex As Exception
        'Me.Status = "UPDATING CONTENT ERROR"
        'AddToLog("UPDATING CONTENT ERROR: " + ex.ToString())
        'End Try
    End Sub

    'L O G
    Public Sub AddToLog(ByVal value As String)
        Try
            Dim log_dir As String = app_dir + "log\"
            Dim log_file As String = log_dir + bTitle + "_log.txt"
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


    '--- FOR SETUP MODE --->

    'C O N T E N T  B A C K  S Y NC  (KURA method, WTF is KURA? keep-update-rename-a... append?)
    Public Sub ContentDirsBackSync() 'As String
        Dim result As String = ""

        'FROM LOCAL --> TO CLOUD SOURCE
        Dim to_dir As String = Me.ContentSourceRoot + bDir
        Dim from_dir As String = Me.ContentLocalRoot + bDir

        If Not Directory.Exists(to_dir) Then
            Directory.CreateDirectory(to_dir)
        End If

        If Directory.Exists(from_dir) And Directory.Exists(to_dir) Then

            'Dim local_files As String() = getFiles(ContentLocalRoot + bDir, "*.*", SearchOption.TopDirectoryOnly)
            Dim from_files As String() = Directory.GetFiles(from_dir, "*.*", SearchOption.TopDirectoryOnly)
            Dim from_files_info() As FileInfo
            Dim from_files_info_name() As String
            Dim from_files_info_time() As String
            For i = 0 To from_files.Count - 1
                ReDim Preserve from_files_info(i)
                from_files_info(i) = New FileInfo(from_files(i))
                ReDim Preserve from_files_info_name(i)
                from_files_info_name(i) = from_files_info(i).Name
                ReDim Preserve from_files_info_time(i)
                from_files_info_time(i) = from_files_info(i).LastWriteTime
            Next

            'Dim source_files As String() = getFiles(ContentSourceRoot + bDir, "*.*", SearchOption.TopDirectoryOnly)
            Dim to_files As String() = Directory.GetFiles(to_dir, "*.*", SearchOption.TopDirectoryOnly)
            Dim to_files_info() As FileInfo
            Dim to_files_info_name() As String
            Dim to_files_info_time() As String
            For i = 0 To to_files.Count - 1
                ReDim Preserve to_files_info(i)
                to_files_info(i) = New FileInfo(to_files(i))
                ReDim Preserve to_files_info_name(i)
                to_files_info_name(i) = to_files_info(i).Name
                ReDim Preserve to_files_info_time(i)
                to_files_info_time(i) = to_files_info(i).LastWriteTime
            Next

            If Not IsNothing(from_files) And Not IsNothing(to_files) Then

                'COPY
                If from_files.Count >= to_files.Count And from_files.Count <> 0 Then
                    If to_files.Count <> 0 Then
                        Dim add_filenames = from_files_info_name.Except(to_files_info_name)
                        For i = 0 To add_filenames.Count - 1
                            File.Copy(from_dir + "\" + add_filenames(i), to_dir + "\" + add_filenames(i), True)
                        Next
                    Else
                        For i = 0 To from_files_info_name.Count - 1
                            File.Copy(from_dir + "\" + from_files_info_name(i), to_dir + "\" + from_files_info_name(i), True)
                        Next
                    End If
                End If

                'REMOVE
                If from_files.Count <= to_files.Count And to_files.Count <> 0 Then
                    If from_files.Count <> 0 Then
                        Dim del_filenames = to_files_info_name.Except(from_files_info_name)
                        For i = 0 To del_filenames.Count - 1
                            File.Delete(to_dir + "\" + del_filenames(i))
                        Next
                    Else
                        For i = 0 To to_files_info_name.Count - 1
                            File.Delete(to_dir + "\" + to_files_info_name(i))
                        Next
                    End If
                End If

                'UPDATE
                If from_files.Count = to_files.Count Then
                    For i = 0 To from_files.Count - 1
                        For j = 0 To to_files.Count - 1
                            If from_files_info_name(i) = to_files_info_name(j) Then
                                If from_files_info_time(i) <> to_files_info_time(j) Then
                                    If from_files_info_name(i).ToUpper <> "THUMBS.DB" Then _
                                        File.Copy(from_dir + "\" + from_files_info_name(i), to_dir + "\" + to_files_info_name(i), True)
                                End If
                            End If
                        Next j
                    Next i
                End If


            End If


        End If
        'Return someresult
    End Sub

    'F E E D S
    Dim FeedTimer As DispatcherTimer = New DispatcherTimer()
    Dim feed_itm As Integer = 0
    Dim feed_upd_tick As Integer = 0

    'I N I T  R S S
    Dim rss_contents1() As String
    Dim rss_contents2() As String
    Dim rss_contents3() As String

    Dim rss_media_url() As String
    Dim rss_media_images() As BitmapImage
    Dim rss_init As Boolean = False

    Dim bro As New System.Windows.Forms.WebBrowser

    Sub InitRSSFeed()
        FeedTimer.Stop()
        rss_contents1 = Nothing
        rss_contents2 = Nothing
        rss_contents3 = Nothing

        rss_media_url = Nothing
        rss_media_images = Nothing
        feed_itm = 0
        feed_upd_tick = 0
        feed_reload_preset = 100
        'xTextBlockMiddle.Text = "Init. RSS..."

        Dim RSS_URL As String = Me.bSource

        'NAV TO PAGE FOR UPDATE
        bro.Navigate(RSS_URL)
        AddHandler bro.DocumentCompleted, AddressOf bro_DocumentCompleted

        'Dim request As System.Net.HttpWebRequest = System.Net.HttpWebRequest.Create(RSS_URL)
        'request.KeepAlive = False
        'request.Timeout = 10000
        'Dim response As System.Net.HttpWebResponse = request.GetResponse()
        'Dim sr As System.IO.StreamReader = New System.IO.StreamReader(response.GetResponseStream())
        'Dim sourcecode As String = sr.ReadToEnd()
        'MsgBox(sourcecode)

    End Sub

    Sub bro_DocumentCompleted()
        Dim doc As System.Windows.Forms.HtmlDocument = bro.Document
        If Not IsNothing(doc.Body.InnerText) Then
            Dim rssdata As String = doc.Body.InnerText
            If rssdata.IndexOf("<") <> -1 Then
                rssdata = rssdata.Substring(rssdata.IndexOf("<"), rssdata.Length - 2) '! 2 is not good
            End If
            rssdata = rssdata.Replace("- ", "") 'why?
            rssdata = rssdata.Replace("&nbsp;", " ")
            rssdata = rssdata.Replace("&amp;", " and ")
            rssdata = rssdata.Replace("&", " and ")

            ReadSomeNodes(rssdata)
        End If
    End Sub

    Sub ReadSomeNodes(ByVal rssdata As String)
        Dim RSS_URL As String = Me.bSource

        Dim doc As New XmlDocument()
        Dim rss_title() As String
        Dim rss_text() As String

        Try
            doc.LoadXml(rssdata)

            Dim pos As Integer = 0
            Dim navigator As XPathNavigator = doc.CreateNavigator()

            'DEFAULT SELECTORS
            Dim selector1 As String = "title"
            Dim selector2 As String = "description"
            Dim selector3 As String = "pubdate"

            'SELECTORS BY TEXT AREAS
            'If Me.TopText.Text <> "" And Me.TopText.Text <> "feed" Then selector1 = Me.TopText.Text
            'If Me.MidText.Text <> "" And Me.MidText.Text <> "feed" Then selector2 = Me.MidText.Text
            'If Me.BtmText.Text <> "" And Me.BtmText.Text <> "feed" Then selector3 = Me.BtmText.Text

            'Try
            Dim nodes_title As XPathNodeIterator = navigator.Select("/rss/channel/item/" + selector1) 'title
            While nodes_title.MoveNext
                Dim node As XPathNavigator = nodes_title.Current
                Dim tmp As String = node.Value.Trim()
                tmp = tmp.Replace(ControlChars.CrLf, "")
                tmp = tmp.Replace(ControlChars.Lf, "")
                tmp = tmp.Replace(ControlChars.Cr, "")
                tmp = tmp.Replace(ControlChars.FormFeed, "")
                tmp = tmp.Replace(ControlChars.NewLine, "")
                ReDim Preserve rss_title(pos)
                rss_title(pos) = tmp
                pos += 1
            End While

            'CLEAR STUFF FOR TWITTER-RSS TITLE
            For i = 0 To pos - 1
                Dim tmp As String = rss_title(i)
                If tmp.IndexOf("#") >= 0 Then
                    tmp = tmp.Substring(tmp.IndexOf("#") + 1, tmp.Length - tmp.IndexOf("#") - 1)
                    rss_title(i) = tmp
                End If
                tmp = rss_title(i)
                If tmp.IndexOf("#") >= 0 Then
                    tmp = tmp.Substring(tmp.IndexOf("#") + 1, tmp.Length - tmp.IndexOf("#") - 1)
                    rss_title(i) = tmp
                End If
                tmp = rss_title(i)
                If tmp.IndexOf("@") >= 0 Then
                    tmp = tmp.Substring(tmp.IndexOf("@") + 1, tmp.Length - tmp.IndexOf("@") - 1)
                    rss_title(i) = tmp
                End If
                tmp = rss_title(i)
                If tmp.IndexOf("@") >= 0 Then
                    tmp = tmp.Substring(tmp.IndexOf("@") + 1, tmp.Length - tmp.IndexOf("@") - 1)
                    rss_title(i) = tmp
                End If
            Next

            'CLEAR TEXT - SYMBOLS REPLACEMENT
            pos = 0
            Dim nodes_descr As XPathNodeIterator = navigator.Select("/rss/channel/item/" + selector2) 'description
            While nodes_descr.MoveNext
                Dim node As XPathNavigator = nodes_descr.Current
                Dim tmp As String = node.Value.Trim()
                tmp = tmp.Replace("&nbsp;", " ")
                tmp = tmp.Replace("&ndash;", "-")
                tmp = tmp.Replace("&quot;", " ")
                tmp = tmp.Replace("&#39;", "`")
                tmp = tmp.Replace("&#039;", "`")
                tmp = tmp.Replace("and #xA0;", "")
                tmp = tmp.Replace("and #x2026;", "")
                tmp = tmp.Replace("and #x201D;", "")
                tmp = tmp.Replace("and #x3E;", "")
                tmp = tmp.Replace("and #xED;", "")
                tmp = tmp.Replace("and #x27;", "")
                tmp = tmp.Replace("and #x2019;", "")
                If tmp.Contains("<") And tmp.Contains(">") Then tmp = ""
                ReDim Preserve rss_text(pos)
                rss_text(pos) = tmp
                pos += 1
            End While



            'CLEAR TEXT FROM SYMBOLS
            If Not IsNothing(rss_text) Then
                For i = 0 To rss_text.Length - 1
                    'While Not rss_text(i).Contains("&#") 'WHILE NOT WORKING???
cleartext:
                    Dim start_ind As Integer = rss_text(i).IndexOf("&#")
                    Dim end_ind As Integer = -1
                    If start_ind >= 0 Then end_ind = rss_text(i).IndexOf(";", start_ind)
                    If start_ind >= 0 And end_ind >= 0 Then
                        rss_text(i) = rss_text(i).Remove(start_ind, end_ind - start_ind + 1)
                    End If
                    If rss_text(i).Contains("&#") Then GoTo cleartext
                    'End While
                Next
            End If

            'CLEAR TEXT FROM URL
            If Not IsNothing(rss_text) Then
                For i = 0 To rss_text.Length - 1
                    Dim start_ind As Integer = rss_text(i).IndexOf("http:")
                    Dim end_ind As Integer = -1
                    If start_ind >= 0 Then end_ind = rss_text(i).IndexOf(" ", start_ind)
                    If start_ind >= 0 And end_ind >= 0 Then
                        rss_text(i) = rss_text(i).Remove(start_ind, end_ind - start_ind + 1)
                    End If
                Next
                For i = 0 To rss_text.Length - 1
                    Dim start_ind As Integer = rss_text(i).IndexOf("http:")
                    Dim end_ind As Integer = -1
                    If start_ind >= 0 Then end_ind = rss_text(i).IndexOf("html", start_ind)
                    If start_ind >= 0 And end_ind >= 0 Then
                        rss_text(i) = rss_text(i).Remove(start_ind, end_ind + 3 - start_ind + 1)
                    End If
                Next
                For i = 0 To rss_text.Length - 1
                    Dim start_ind As Integer = rss_text(i).IndexOf("http:")
                    Dim end_ind As Integer = -1
                    If start_ind >= 0 Then end_ind = rss_text(i).IndexOf(vbCrLf, start_ind)
                    If start_ind >= 0 And end_ind >= 0 Then
                        rss_text(i) = rss_text(i).Remove(start_ind, end_ind - start_ind + 1)
                    End If
                Next
                For i = 0 To rss_text.Length - 1
                    Dim start_ind As Integer = rss_text(i).IndexOf("http:")
                    Dim end_ind As Integer = -1
                    If start_ind >= 0 Then end_ind = rss_text(i).IndexOf("/", start_ind)
                    If start_ind >= 0 And end_ind >= 0 Then
                        rss_text(i) = rss_text(i).Remove(start_ind, end_ind - start_ind + 1)
                    End If
                Next
            End If


            'GETTING IMG URL FROM MEDIA SECTION
            Dim media_pos As Integer = 0
            Dim ns_manager As XmlNamespaceManager = New XmlNamespaceManager(navigator.NameTable)
            ns_manager.AddNamespace("media", "http://search.yahoo.com/mrss/")

            'RSS common images
            Dim nodes_media_url As XPathNodeIterator = navigator.Select("/rss/channel/item/media:content", ns_manager)
            While nodes_media_url.MoveNext
                Dim node As XPathNavigator = nodes_media_url.Current
                Dim tmp As String = node.GetAttribute("url", "").ToString.Trim()
                ReDim Preserve rss_media_url(media_pos)
                rss_media_url(media_pos) = tmp
                media_pos += 1
            End While

            'RSS enclosure images
            If IsNothing(rss_media_url) Then
                nodes_media_url = navigator.Select("/rss/channel/item/enclosure", ns_manager)
                While nodes_media_url.MoveNext
                    Dim node As XPathNavigator = nodes_media_url.Current
                    Dim tmp As String = node.GetAttribute("url", "").ToString.Trim()
                    ReDim Preserve rss_media_url(media_pos)
                    rss_media_url(media_pos) = tmp
                    media_pos += 1
                End While
            End If

            'TWI images
            If IsNothing(rss_media_url) Then
                'nodes_media_url = navigator.Select("/rss/channel/item/description", ns_manager)
                'While nodes_media_url.MoveNext
                '    Dim node As XPathNavigator = nodes_media_url.Current
                '    Dim tmp As String = node.Value.Trim()
                '    If tmp.Contains("pic.twitter.com") Then
                '        tmp = tmp.Substring(tmp.IndexOf("pic.twitter.com"), tmp.Length - tmp.IndexOf("pic.twitter.com"))
                '        If tmp.Contains("and #x201D;") Then
                '            tmp = tmp.Substring(0, tmp.Length - tmp.IndexOf("and #x201D;"))
                '        End If
                '    End If
                '    ReDim Preserve rss_media_url(media_pos)
                '    rss_media_url(media_pos) = tmp
                '    media_pos += 1
                'End While
            End If

            'RSS TEXT OUTPUT
            'xTextBlockMiddle.Text = "Loading RSS feed..."
            For i = 0 To pos - 1
                ReDim Preserve rss_contents1(i)
                If Not IsNothing(rss_title) And Not IsNothing(rss_text) Then
                    If rss_text(i) <> "" Then rss_contents1(i) = rss_title(i) + " @ " + rss_text(i)
                    'DO NOT SHOW TITLE FOR www.twitrss.me SOURCE
                    If rss_text(i) <> "" And RSS_URL.Contains("twitrss.me") Then rss_contents1(i) = rss_text(i)

                    'PRELOAD IMAGES
                    If Not IsNothing(rss_media_url) Then
                        If rss_media_url(i) <> "" Then
                            Try
                                Dim BmpImgBg As New BitmapImage
                                With BmpImgBg
                                    .BeginInit()
                                    '.CreateOptions = BitmapCreateOptions.IgnoreImageCache
                                    .CacheOption = BitmapCacheOption.OnLoad
                                    .UriSource = New Uri(rss_media_url(i))
                                    .EndInit()
                                End With
                                ReDim Preserve rss_media_images(i)
                                rss_media_images(i) = BmpImgBg
                                'xTextBlockMiddle.Text = "RSS IMG: " + CStr(i) + "..."
                            Catch ex As Exception
                                'xTextBlockMiddle.Text = "Image load ERR..."
                                feed_reload_preset = 2
                            End Try
                        End If
                    End If
                Else
                    feed_reload_preset = 5
                End If
            Next i

            If Not IsNothing(rss_title) And Not IsNothing(rss_text) Then
                If Not rss_init Then
                    'RECOLOR
                    'xTextBlockMiddle.TextEffects.Add(tw_text_eff)
                    tw_text_eff.Foreground = Me.MidText.FrontColor
                    Dim sh_eff As New Effects.DropShadowEffect With {.Color = Me.MidText.BackColor.Color, .BlurRadius = 1, .ShadowDepth = 0}
                    'tw_text_eff.Transform = New ScaleTransform(0.75, 0.75)
                    'TIMER
                    AddHandler FeedTimer.Tick, AddressOf FeedTimer_Tick
                    FeedTimer.Interval = TimeSpan.FromSeconds(6 + Me.bOrder / 10)
                    'div10 - for have some minor delays between different blocks with rss data
                    FeedTimer.Start()
                    rss_init = True
                End If
            End If
            'xTextBlockMiddle.Text = "Starting..."

            'Catch exx As Exception
            'xTextBlockFeed.Text = "FEED LOAD ERR"
            'AddToLog("FEED LOAD ERR: " + exx.Message)
            'End Try

            FeedTimer.Start()
            FeedTimer_Tick(Nothing, Nothing)

        Catch ex As Exception
            AddToLog("FEED LOAD ERR: " + ex.Message)
        End Try

    End Sub

    'I N I T  T W I T
    Dim tweets() As String
    Dim tw_text_eff As New TextEffect
    Dim tw_init As Boolean = False

    Sub InitTwitterFeed()
        tweets = Nothing
        feed_itm = 0
        feed_upd_tick = 0
        feed_reload_preset = 100
        Dim tw As New TwitterAPI
        tw.AuthenticateWith("yhUzSRbwDGaKGvDsaLRCIg", "ldZuKv2GkeaGeDctgSQBVBAMnEzr3MrKtZPyF6RHuVM", "rnoydfn3bfYXZ0Id24vAkGb9ycsLLYDVyNBq3WN0", "CTEpJuwRMrohOFRmzXEn2N4kmyBUbptNzhqQbYq08")
        Try
            For Each tweet As TwitterStatus In tw.UserTimeline(Me.bSource) 'bbcnews, issimple1
                ReDim Preserve tweets(feed_itm)
                'CLEAN FROM URL
                Dim clean_text As String = tweet.Text
                Dim url_index As Integer = InStr(clean_text, "http")
                If url_index <> 0 Then
                    Dim http_addr As String = clean_text.Substring(url_index) + " "
                    Dim http_count As Integer = http_addr.IndexOf(" ")
                    clean_text = clean_text.Remove(url_index - 1, http_count + 1)
                End If
                'FINAL STRING
                tweets(feed_itm) = clean_text + " @ " + tweet.CreatedAt.TimeOfDay.ToString + " / " + tweet.CreatedAt.Date
                feed_itm += 1
            Next
        Catch ex As Exception
        End Try
        feed_itm = 0
        If Not IsNothing(tweets) Then
            If tweets.Count <> 0 Then
                xTextBlockMiddle.Text = "Loading tweets..."
                If Not tw_init Then
                    'TXT EFF
                    'xTextBlockMiddle.TextEffects.Add(tw_text_eff)
                    tw_text_eff.Foreground = Brushes.DarkGray
                    'tw_text_eff.Transform = New ScaleTransform(0.75, 0.75)
                    'TIMER INIT
                    AddHandler FeedTimer.Tick, AddressOf FeedTimer_Tick
                    FeedTimer.Interval = New TimeSpan(0, 0, 0, 7)
                    FeedTimer.Start()
                    tw_init = True
                End If
            End If
        Else
            feed_reload_preset = 5
        End If
    End Sub

    'F E E D  T I M E R
    Dim feed_reload_preset As Integer = 100

    Public Sub FeedTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
        'TWITTER FEED
        If Me.bType = "TWI" And Not IsNothing(tweets) Then
            If feed_itm = tweets.Count - 1 Then feed_itm = 0
            'RECOLOR TIME AND DATE
            If Not IsNothing(tweets(feed_itm)) Then
                tw_text_eff.PositionStart = tweets(feed_itm).IndexOf("@")
                tw_text_eff.PositionCount = tweets(feed_itm).Length - tweets(feed_itm).IndexOf("@")
            End If
            'UPDATE TEXT
            'xTextBlockFeed.FontSize = Me.MidText.TextSize
            xTextBlockMiddle.Text = tweets(feed_itm)
            'ANIMATE
            Dim anim As New DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5))
            xTextBlockMiddle.BeginAnimation(TextBlock.OpacityProperty, anim)
            'INC COUNTERS
            feed_itm += 1
            feed_upd_tick += 1
            'RELOAD AFTER TIMER_PRESET(sec)x100
            If feed_upd_tick = feed_reload_preset Then InitTwitterFeed()
        End If

        'RSS FEED
        If Me.bType = "RSS" And Not IsNothing(rss_contents1) Then

            Dim TextArea As TextBlock = xTextBlockMiddle
            If Me.TopText.Text = "feed" Then TextArea = xTextBlockTop
            If Me.MidText.Text = "feed" Then TextArea = xTextBlockMiddle
            If Me.BtmText.Text = "feed" Then TextArea = xTextBlockBottom

            TextArea.Text += "."
            If feed_itm = rss_contents1.Count - 1 Then feed_itm = 0
            'TEXT RECOLOR
            If Not IsNothing(rss_contents1(feed_itm)) Then
                If rss_contents1(feed_itm).Contains("@") Then
                    tw_text_eff.PositionStart = rss_contents1(feed_itm).IndexOf("@")
                    tw_text_eff.PositionCount = rss_contents1(feed_itm).Length - rss_contents1(feed_itm).IndexOf("@")
                End If
            End If
            'MEDIA IMAGES
            If Not IsNothing(rss_media_images) Then
                If Not IsNothing(rss_media_images(feed_itm)) Then xImageBlockBg.Source = rss_media_images(feed_itm)
                xImageBlockBg.Stretch = Stretch.UniformToFill
                xImageBlockBg.BeginAnimation(Image.OpacityProperty, New DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5)))
            End If

            'UPDATE TEXT
            'xViewboxFeed.Stretch = Stretch.None
            'xTextBlockFeed.FontSize = Me.MidText.TextSize
            TextArea.Text = rss_contents1(feed_itm)
            'xTextBlockFeed.UpdateLayout()
            'xViewboxFeed.Stretch = Stretch.Uniform

            'ANIMATE TEXT BLOCK
            Dim anim As New DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5))
            TextArea.BeginAnimation(TextBlock.OpacityProperty, anim)
            Dim trans As New TranslateTransform
            TextArea.RenderTransform = trans
            trans.BeginAnimation(TranslateTransform.YProperty, New DoubleAnimation(10, 0, TimeSpan.FromSeconds(0.25)))

            'INC COUNTERS
            feed_itm += 1
            feed_upd_tick += 1

            'RELOAD AFTER TIMER_PRESET(sec)x100
            If feed_upd_tick = feed_reload_preset Then InitRSSFeed()
        End If

    End Sub

    'TIMER BLOCK
    Dim TimeTimer As DispatcherTimer = New DispatcherTimer()
    Sub InitTimeBlock()
        AddHandler TimeTimer.Tick, AddressOf TimeTimer_Tick
        TimeTimer.Interval = TimeSpan.FromSeconds(1)
        TimeTimer.Start()

        Me.TopText.Text = DateTime.Now.DayOfWeek.ToString
        Me.MidText.Text = DateTime.Now.ToShortTimeString
        Me.BtmText.Text = DateTime.Now.ToShortDateString
    End Sub
    'TIME DATE UPD AND ANIM
    Dim prev_min As String
    Public Sub TimeTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)

        xTextBlockTop.Text = DateTime.Now.DayOfWeek.ToString
        xTextBlockMiddle.Text = DateTime.Now.ToShortTimeString
        xTextBlockBottom.Text = DateTime.Now.ToShortDateString

        'xTextBlockFeed.Text = DateTime.Now.ToString("HH:mm")
        If DateTime.Now.Minute.ToString <> prev_min Then
            Dim anim_time As New DoubleAnimation(0, -5, TimeSpan.FromSeconds(0.25))
            anim_time.AutoReverse = True
            Dim time_eff As New TextEffect
            time_eff.PositionStart = 3
            time_eff.PositionCount = 2
            Dim time_tr As New TranslateTransform
            time_eff.Transform = time_tr
            time_tr.BeginAnimation(TranslateTransform.YProperty, anim_time)
            xTextBlockMiddle.TextEffects.Add(time_eff)
        End If
        prev_min = DateTime.Now.Minute.ToString
    End Sub

    Public Function GetParamValueByIndex(ByVal index As Integer) As String
        Select Case index
            Case 0 : Return Me.bTitle
            Case 1 : Return Me.bDir

            Case 2 : Return Me.bLeft
            Case 3 : Return Me.bTop
            Case 4 : Return Me.bWidth
            Case 5 : Return Me.bHeight

            Case 6 : Return Me.bType
            Case 7 : Return Me.bSource
            Case 8 : Return Me.bDots.ToString
            Case 9 : Return Me.bMargin.ToString

            Case 10 : Return Me.TopText.Text
            Case 11 : Return Me.TopText.TextSize.ToString
            Case 12 : Return Me.TopText.FrontColor.ToString
            Case 13 : Return Me.TopText.BackColor.ToString
            Case 14 : Return Me.TopText.TextAlign

            Case 15 : Return Me.MidText.Text
            Case 16 : Return Me.MidText.TextSize.ToString
            Case 17 : Return Me.MidText.FrontColor.ToString
            Case 18 : Return Me.MidText.BackColor.ToString
            Case 19 : Return Me.MidText.TextAlign

            Case 20 : Return Me.BtmText.Text
            Case 21 : Return Me.BtmText.TextSize.ToString
            Case 22 : Return Me.BtmText.FrontColor.ToString
            Case 23 : Return Me.BtmText.BackColor.ToString
            Case 24 : Return Me.BtmText.TextAlign

            Case 25 : Return Me.bSimpleTouch.ToString

            Case 26 : Return Me.bTimeLimit.ToString
            Case 27 : Return Me.bFromTime.ToString
            Case 28 : Return Me.bToTime.ToString

            Case 29 : Return Me.bLinkTo

            Case Else : Return Nothing
        End Select
    End Function

    Public Sub SetParamValueByindex(ByVal index As Integer, ByVal new_value As Object)
        Select Case index
            Case 0 : Me.bTitle = new_value
            Case 1 : Me.bDir = new_value

            Case 2 : Me.bLeft = new_value
            Case 3 : Me.bTop = new_value
            Case 4 : Me.bWidth = new_value
            Case 5 : Me.bHeight = new_value

            Case 6 : Me.bType = new_value
            Case 7 : Me.bSource = new_value
            Case 8 : Me.bDots = new_value
            Case 9 : Me.bMargin = new_value

            Case 10 : Me.TopText.Text = new_value
            Case 11 : Me.TopText.TextSize = new_value
            Case 12 : Me.TopText.FrontColor = new_value
            Case 13 : Me.TopText.BackColor = new_value
            Case 14 : Me.TopText.TextAlign = new_value

            Case 15 : Me.MidText.Text = new_value
            Case 16 : Me.MidText.TextSize = new_value
            Case 17 : Me.MidText.FrontColor = new_value
            Case 18 : Me.MidText.BackColor = new_value
            Case 19 : Me.MidText.TextAlign = new_value

            Case 20 : Me.BtmText.Text = new_value
            Case 21 : Me.BtmText.TextSize = new_value
            Case 22 : Me.BtmText.FrontColor = new_value
            Case 23 : Me.BtmText.BackColor = new_value
            Case 24 : Me.BtmText.TextAlign = new_value

            Case 25 : Me.bSimpleTouch = new_value

            Case 26 : Me.bTimeLimit = new_value
            Case 27 : Me.bFromTime = new_value
            Case 28 : Me.bToTime = new_value

            Case 29 : Me.bLinkTo = new_value
        End Select
    End Sub

    Public Function GetSlideParamValueByIndex(ByVal index As Integer, ByVal sel_sl As Integer) As String
        With Me.Slides(sel_sl)
            Select Case index
                Case 0 : Return .Source
                Case 1 : Return .Title
                Case 2 : Return .Duration
                Case 3 : Return .Mode

                Case 4 : Return .TopText.Text
                Case 5 : Return .TopText.TextSize.ToString
                Case 6 : Return .TopText.FrontColor.ToString
                Case 7 : Return .TopText.BackColor.ToString
                Case 8 : Return .TopText.TextAlign

                Case 9 : Return .MidText.Text
                Case 10 : Return .MidText.TextSize.ToString
                Case 11 : Return .MidText.FrontColor.ToString
                Case 12 : Return .MidText.BackColor.ToString
                Case 13 : Return .MidText.TextAlign

                Case 14 : Return .BtmText.Text
                Case 15 : Return .BtmText.TextSize.ToString
                Case 16 : Return .BtmText.FrontColor.ToString
                Case 17 : Return .BtmText.BackColor.ToString
                Case 18 : Return .BtmText.TextAlign

                Case 19 : Return .TimeLimit.ToString
                Case 20 : Return .FromTime.ToString
                Case 21 : Return .ToTime.ToString

                Case 22 : Return .Location.ToString

                Case Else : Return Nothing
            End Select
        End With
    End Function

    Public Sub SetSlideParamValueByindex(ByVal index As Integer, ByVal new_value As Object, ByVal sel_sl As Integer)
        With Me.Slides(sel_sl)
            Select Case index
                Case 0 : .Source = new_value
                Case 1 : .Title = new_value
                Case 2 : .Duration = new_value
                Case 3 : .Mode = new_value

                Case 4 : .TopText.Text = new_value
                Case 5 : .TopText.TextSize = new_value
                Case 6 : .TopText.FrontColor = new_value
                Case 7 : .TopText.BackColor = new_value
                Case 8 : .TopText.TextAlign = new_value

                Case 9 : .MidText.Text = new_value
                Case 10 : .MidText.TextSize = new_value
                Case 11 : .MidText.FrontColor = new_value
                Case 12 : .MidText.BackColor = new_value
                Case 13 : .MidText.TextAlign = new_value

                Case 14 : .BtmText.Text = new_value
                Case 15 : .BtmText.TextSize = new_value
                Case 16 : .BtmText.FrontColor = new_value
                Case 17 : .BtmText.BackColor = new_value
                Case 18 : .BtmText.TextAlign = new_value

                Case 19 : .TimeLimit = new_value
                Case 20 : .FromTime = new_value
                Case 21 : .ToTime = new_value

                Case 22 : .Location = new_value
            End Select
        End With
    End Sub

End Class
