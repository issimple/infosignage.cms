
Imports Net.SourceForge.Koogra.Excel
Imports System.Data

Public Class ExcelImporter

    Public xlsSheets() As String
    Public xlsSheetNames() As String
    Public xlsRowsCount() As Long
    Public xlsColsCount() As Integer
    Private Const xlsHeaderRowsCount = 0
    Private Const xlsLeftColsGap = 0

    Function GetDataTable(ByRef wb As Workbook, ByRef sheet As Integer) As DataTable
        Dim ws As Worksheet
        ws = wb.Sheets.ElementAt(sheet - 1)
        'GET SHEET NAMES
        ReDim Preserve xlsSheetNames(sheet - 1)
        xlsSheetNames(sheet - 1) = ws.Name
        'GET ROWS DATA
        Dim row As Row
        row = ws.Rows.Item(0)
        Dim dt As New DataTable
        For i = 0 To CInt(ws.Rows.Item(1).Cells.MaxCol)
            dt.Columns.Add()
        Next i
        Dim dr As DataRow
        For i = 0 To CInt(ws.Rows.MaxRow)
            dr = dt.NewRow()
            For j = 0 To dt.Columns.Count - 1
                If Not IsNothing(ws.Rows.Item(i)) Then
                    If Not IsNothing(ws.Rows.Item(i).Cells.Item(j)) Then
                        dr(j) = ws.Rows.Item(i).Cells.Item(j).Value
                    Else
                        dr(j) = Nothing
                    End If
                Else
                    dr(j) = Nothing
                End If
            Next j
            dt.Rows.Add(dr)
        Next i
        GetDataTable = dt
        ws = Nothing
        dt = Nothing
    End Function

    Public Sub xlsImport(ByRef xls_file As String, ByRef xlsSheetsArray()(,) As String)
        Dim i As Integer
        Dim TabIndex As Integer
        If Not My.Computer.FileSystem.FileExists(xls_file) Then Exit Sub
        Dim wb As New Workbook(xls_file)
        Dim sheet_count As Integer = wb.Sheets.Count

        TabIndex = 0
        ReDim xlsSheets(TabIndex)
        ReDim xlsSheetsArray(TabIndex + 1)
        ReDim xlsRowsCount(sheet_count)
        ReDim xlsColsCount(sheet_count)

        For i = 1 To sheet_count
            xlsSheets(TabIndex) = wb.Sheets.ElementAt(i - 1).Name
            Dim dt As DataTable
            dt = GetDataTable(wb, i)
            xlsColsCount(i) = dt.Columns.Count
            xlsRowsCount(i) = dt.Rows.Count
            For ii = 0 To xlsRowsCount(i) - 1
                If dt.Rows(ii)(1).ToString = "" Then
                    xlsRowsCount(i) = ii
                    Exit For
                End If
            Next ii
            ReDim Preserve xlsSheetsArray(TabIndex + 1)(xlsRowsCount(i), xlsColsCount(i))
            For ii = 0 To xlsRowsCount(i) - 1
                For jj = 0 To xlsColsCount(i) - 1
                    If Not IsDBNull(dt.Rows(ii)(jj)) Then
                        xlsSheetsArray(TabIndex + 1)(ii + 1, jj + 1) = dt.Rows(ii)(jj)
                    End If
                Next jj
            Next ii
            TabIndex += 1
            ReDim Preserve xlsSheets(TabIndex)
            ReDim Preserve xlsSheetsArray(TabIndex + 1)
        Next i

        wb = Nothing
        GC.Collect()
    End Sub

End Class