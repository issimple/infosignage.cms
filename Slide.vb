Public Class Slide
    'XML DATA
    Public Source As String
    Public Title As String
    Public Duration As Integer
    Public Mode As String
    Public TopText As TextInfo
    Public MidText As TextInfo
    Public BtmText As TextInfo
    'TIMING - LIMITS MODE
    Public TimeLimit As Boolean = False
    Public FromTime As String = ""
    Public ToTime As String = ""
    'LOCATION
    Public Location As String = ""

    'SYSTEM
    Public Order As Integer

    Public Sub New(ByVal stg() As String)
        Me.Source = stg(0)
        Me.Title = stg(1)
        Me.Duration = stg(2)
        Me.Mode = stg(3)
        'TEXT AREAS
        Me.TopText = New TextInfo(stg(4), stg(5), stg(6), stg(7), stg(8))
        Me.MidText = New TextInfo(stg(9), stg(10), stg(11), stg(12), stg(13))
        Me.BtmText = New TextInfo(stg(14), stg(15), stg(16), stg(17), stg(18))
        'TIMING
        Me.TimeLimit = stg(19)
        Me.FromTime = stg(20)
        Me.ToTime = stg(21)
        'LOCATION
        Me.Location = stg(22)
    End Sub

End Class