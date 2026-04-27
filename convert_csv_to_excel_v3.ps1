# Alternative: Open CSV, copy all, paste to Excel
$csvPath = "d:\unity\Blade-Pursuit\Test_Execution_Report.csv"
$excelPath = "d:\unity\Blade-Pursuit\Test_Execution_Report.xlsx"

# Remove old file
if (Test-Path $excelPath) {
    Remove-Item $excelPath -Force
}

# Create Excel
$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false

Try {
    # Open CSV as a workbook
    $workbook = $excel.Workbooks.Open($csvPath, 3)  # 3 = Tab-delimited CSV
    
    # Save as Excel
    $workbook.SaveAs($excelPath, -4143)  # -4143 = xlOpenXMLWorkbook (.xlsx)
    
    Write-Host "Excel file created successfully: $excelPath"
    
} Catch {
    Write-Host "Error: $_"
} Finally {
    if ($workbook) {
        $workbook.Close($false)
    }
    $excel.Quit()
    [System.GC]::Collect()
    [System.GC]::WaitForPendingFinalizers()
}
