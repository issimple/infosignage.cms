Module Timing

    'check time-date with preset, if match - then return True for play content
    'if no match - then skip content playback

    'applied for: locations, screens, layers(?) and blocks(!)

    'setup:
    'daily: any time / from 16:30 to 18:30
    '---- and period setup
    'date = from 2.03.14 to 5.03.14
    '---- and repeats setup
    'weekly: any day / mo,tu,we,...
    'dates: any date / 1,2,3,...
    'monthly: any month / jan, feb, mar,...
    'annualy: any year / 2014,2015,...

    'DATETIME FORMATS: 'msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx


    'ADD TO TEMPLATES:
    '<set id="time_limit_enabled" value="True" />
    '<set id="time_limit_from" value="09-00-00,2014-09-04" />
    '<set id="time_limit_to" value="11-00-00,2014-09-04" />

    'CHECK LIMITS
    Public Function CheckDateTimeLimit(ByVal from_limit As String, ByVal to_limit As String) As Boolean
        Dim DisplayContent As Boolean = False
        Dim from_flag As Integer = 0
        Dim to_flag As Integer = 0

        'FROM LIMIT EXIST
        If from_limit <> "" Then
            'FROM DATE-TIME STRING TO DATETIME
            Dim from_datetime() As String = from_limit.Split(",")
            Dim from_time() As String = from_datetime(0).Split("-")
            Dim from_date() As String = from_datetime(1).Split("-")
            Dim from_lim As New DateTime(from_date(0), from_date(1), from_date(2), from_time(0), from_time(1), from_time(2))
            'COMPARE
            from_flag = from_lim.CompareTo(DateTime.Now)
        End If

        'FROM LIMIT EXIST
        If to_limit <> "" Then
            'TO DATE-TIME STRING TO DATETIME
            Dim to_datetime() As String = to_limit.Split(",")
            Dim to_time() As String = to_datetime(0).Split("-")
            Dim to_date() As String = to_datetime(1).Split("-")
            Dim to_lim As New DateTime(to_date(0), to_date(1), to_date(2), to_time(0), to_time(1), to_time(2))
            'COMPARE
            to_flag = to_lim.CompareTo(DateTime.Now)
        End If

        'RESULT IF:
        '1- NO FROM LIMIT
        If from_flag = 0 And to_flag = 1 Then DisplayContent = True
        '2 - NO TO LIMIT
        If from_flag = -1 And to_flag = 0 Then DisplayContent = True
        '3 - BOTH LIMITS SET
        If from_flag = -1 And to_flag = 1 Then DisplayContent = True
        '4 - BOTH LIMITS NOT SET
        If from_flag = 0 And to_flag = 0 Then DisplayContent = True

        Return DisplayContent
    End Function

    Public Function CheckDateTimeRepeat() As Boolean
        Dim DisplayContent As Boolean = True

        'CHECK REPEAT CONDITIONS
        Dim repeat_datetime As String = "8-00-00,9-00-00,..."

        Return DisplayContent
    End Function

End Module
