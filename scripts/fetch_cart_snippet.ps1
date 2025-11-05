$uri="http://localhost:5005/Cart"
$c=(Invoke-WebRequest -Uri $uri -UseBasicParsing).Content
$lines = $c -split "`n"
$matches = $lines | Where-Object { $_ -match 'Tạm tính|Giảm giá|Phí vận chuyển|Tổng cộng|summary-row|summary' }
if ($matches) { $matches | ForEach-Object { $_.Trim() } } else { Write-Output 'No matches found'; Write-Output '--- DUMP START ---'; $lines[0..40] | ForEach-Object { $_ }; Write-Output '--- DUMP END ---' }
