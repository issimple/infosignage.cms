Module CMSOnlyFunctions

    Public res_dict As New ResourceDictionary With {.Source = New Uri("iNFOSignage.CMS;component/Dictionary1.xaml", UriKind.RelativeOrAbsolute)}

    Class PluginWindow : Inherits Window
        Public ResultValue As String = ""
        Public XmlRoot As String = ""
        Dim inTimePluginSetupObj As inTimePluginSetup

        Public Sub New(ByVal _xml_root As String)
            With Me
                .XmlRoot = _xml_root
                '.Height = 480
                '.Width = 640
                .SizeToContent = Windows.SizeToContent.WidthAndHeight
                .WindowStartupLocation = WindowStartupLocation.CenterScreen
                .WindowStyle = WindowStyle.SingleBorderWindow
                .Title = "inTime - Schedule Display Plugin"
            End With

            Dim MainGrid As New Grid
            inTimePluginSetupObj = New inTimePluginSetup(XmlRoot)
            MainGrid.Children.Add(inTimePluginSetupObj)
            Me.AddChild(MainGrid)
        End Sub

        Public Sub Termainate() Handles Me.Closing
            If inTimePluginSetupObj.WasUpdated Then Me.ResultValue = "updated"
        End Sub
    End Class

End Module
