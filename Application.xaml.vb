Imports System.Windows.Forms
Imports System.Drawing

Class Application

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.
    Protected Overrides Sub OnStartup(e As StartupEventArgs)
        MyBase.OnStartup(e)
        Dim w1 As New WindowCMS() 'MainWindow
        w1.WindowState = WindowState.Maximized
        w1.WindowStartupLocation = WindowStartupLocation.Manual
        'Dim screens() As Screen = Screen.AllScreens
        'If screens.Count = 2 Then
        '    Dim r2 As Rectangle = screens(1).WorkingArea
        '    w1.Top = r2.Top
        '    w1.Left = r2.Left
        'End If
        w1.Show()
        'w1.WindowState = WindowState.Maximized
    End Sub
End Class
