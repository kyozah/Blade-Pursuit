# Simple CSV to Excel conversion using Excel Application
$csvPath = "d:\unity\Blade-Pursuit\Test_Execution_Report.csv"
$excelPath = "d:\unity\Blade-Pursuit\Test_Execution_Report.xlsx"

# Remove old file
if (Test-Path $excelPath) {
    Remove-Item $excelPath -Force
}

# Create Excel COM object
$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false

Try {
    # Create new workbook
    $workbook = $excel.Workbooks.Add()
    $sheet = $workbook.Sheets.Item(1)
    
    # Import CSV data
    $data = Import-Csv $csvPath
    
    # Get column headers
    $headers = $data[0].PSObject.Properties.Name
    
    # Write headers (row 1)
    $col = 1
    foreach ($header in $headers) {
        $sheet.Cells.Item(1, $col) = $header
        $sheet.Cells.Item(1, $col).Font.Bold = $true
        $sheet.Cells.Item(1, $col).Interior.Color = 4472541  # Blue
        $sheet.Cells.Item(1, $col).Font.Color = 16777215   # White
        $col++
    }
    
    # Write data rows
    $row = 2
    foreach ($record in $data) {
        $col = 1
        foreach ($header in $headers) {
            $cellValue = $record.$header
            $sheet.Cells.Item($row, $col) = $cellValue
            
            # Color Status column
            if ($header -eq "Status") {
                if ($cellValue -eq "Passed") {
                    $sheet.Cells.Item($row, $col).Interior.Color = 13561798  # Light green
                }
            }
            $col++
        }
        $row++
    }
    
    # Auto-fit all columns
    $sheet.UsedRange.Columns.AutoFit() | Out-Null
    
    # Save workbook
    $workbook.SaveAs($excelPath, -4143)  # -4143 = xlOpenXMLWorkbook
    
    Write-Host "Excel file created successfully: $excelPath"
    Write-Host "Total test cases: $($data.Count)"
    
} Catch {
    Write-Host "Error: $_"
} Finally {
    $workbook.Close($false)
    $excel.Quit()
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($excel) | Out-Null
    [GC]::Collect()
}
