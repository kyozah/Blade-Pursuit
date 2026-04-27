$csv = "d:\unity\Blade-Pursuit\Test_Execution_Report.csv"
$xlsx = "d:\unity\Blade-Pursuit\Test_Execution_Report.xlsx"

# Tạo Excel application object
$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false

# Tạo workbook mới
$workbook = $excel.Workbooks.Add()
$worksheet = $workbook.Sheets.Item(1)
$worksheet.Name = "Test Cases"

# Import CSV data
$data = Import-Csv -Path $csv

# Thêm header
$headers = @("Test Case ID", "Test Case Title", "Expected Result", "Actual Result (Code)", "Run Type", "Tested By", "Test Step Detail", "Status")
for ($i = 0; $i -lt $headers.Count; $i++) {
    $cell = $worksheet.Cells.Item(1, $i + 1)
    $cell.Value = $headers[$i]
    $cell.Font.Bold = $true
    $cell.Interior.ColorIndex = 5
    $cell.Font.Color = 16777215
}

# Thêm data
$row = 2
foreach ($item in $data) {
    $worksheet.Cells.Item($row, 1) = $item."Test Case ID"
    $worksheet.Cells.Item($row, 2) = $item."Test Case Title"
    $worksheet.Cells.Item($row, 3) = $item."Expected Result"
    $worksheet.Cells.Item($row, 4) = $item."Actual Result (Code)"
    $worksheet.Cells.Item($row, 5) = $item."Run Type"
    $worksheet.Cells.Item($row, 6) = $item."Tested By"
    $worksheet.Cells.Item($row, 7) = $item."Test Step Detail"
    $worksheet.Cells.Item($row, 8) = $item."Status"
    
    # Định dạng theo Status
    if ($item.Status -eq "Passed") {
        $worksheet.Rows.Item($row).Interior.ColorIndex = 35
    } else {
        $worksheet.Rows.Item($row).Interior.ColorIndex = 3
    }
    
    $row++
}

# Điều chỉnh độ rộng cột
$worksheet.Columns.Item(1).ColumnWidth = 12
$worksheet.Columns.Item(2).ColumnWidth = 35
$worksheet.Columns.Item(3).ColumnWidth = 25
$worksheet.Columns.Item(4).ColumnWidth = 50
$worksheet.Columns.Item(5).ColumnWidth = 12
$worksheet.Columns.Item(6).ColumnWidth = 12
$worksheet.Columns.Item(7).ColumnWidth = 40
$worksheet.Columns.Item(8).ColumnWidth = 12

# Lưu file
$workbook.SaveAs($xlsx)
$excel.Quit()

Write-Host "Excel file created successfully: $xlsx"
Write-Host "Total test cases: $(($data | Measure-Object).Count)"
