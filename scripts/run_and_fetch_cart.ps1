$port = 5005
$project = 'c:\Users\ASUS\Downloads\ProjectLTWNangCao-main\eCommerce.Web\eCommerce.Web.csproj'
$cwd = 'c:\Users\ASUS\Downloads\ProjectLTWNangCao-main'

$entry = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue | Select-Object -First 1
if ($entry) {
    try { Stop-Process -Id $entry.OwningProcess -Force -ErrorAction SilentlyContinue; Start-Sleep -s 1 } catch {}
}

Write-Output 'Starting dotnet run...'
Start-Process -FilePath 'dotnet' -ArgumentList @('run','--project',$project,'--','--urls',"http://localhost:$port") -WorkingDirectory $cwd -NoNewWindow -PassThru | Out-Null

# Wait for server
$tries = 0; $ok = $false
while ($tries -lt 30) {
    try {
        Invoke-WebRequest -Uri "http://localhost:$port/Cart" -UseBasicParsing -TimeoutSec 3 -ErrorAction Stop | Out-Null
        $ok = $true; break
    } catch {
        Start-Sleep -Seconds 1; $tries++
    }
}

if (-not $ok) { Write-Output 'Server did not start in time.'; exit 2 }

$sess = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$resp = Invoke-WebRequest -Uri "http://localhost:$port/Cart" -WebSession $sess -UseBasicParsing
$resp.Content | Out-File -FilePath "$cwd\scripts\cart_live_check.html" -Encoding utf8
Write-Output "WROTE:$cwd\scripts\cart_live_check.html"
$lines = $resp.Content -split "`n" | Where-Object { $_ -match 'Tạm tính|Giảm giá|Phí vận chuyển|Tổng cộng|data-test' }
Write-Output '--- Cart Summary Lines ---'
$lines | ForEach-Object { $_.Trim() }
