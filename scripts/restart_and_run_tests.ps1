$port = 5005
$project = 'c:\Users\ASUS\Downloads\ProjectLTWNangCao-main\eCommerce.Web\eCommerce.Web.csproj'
$cwd = 'c:\Users\ASUS\Downloads\ProjectLTWNangCao-main'
$entry = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue | Select-Object -First 1
if ($entry) {
    $ownPid = $entry.OwningProcess
    Write-Output "Stopping existing process listening on port $port (PID $ownPid)"
    try { Stop-Process -Id $ownPid -Force -ErrorAction SilentlyContinue; Start-Sleep -s 2 } catch { }
} else { Write-Output "No existing process on port $port" }

Write-Output 'Starting new dotnet run process...'
Start-Process -FilePath 'dotnet' -ArgumentList @('run','--project',$project,'--','--urls',"http://localhost:$port") -WorkingDirectory $cwd -NoNewWindow -PassThru | Out-Null
Start-Sleep -s 4

# Wait for server
$tries = 0; $ok = $false
while ($tries -lt 30) {
    try { Invoke-WebRequest -Uri "http://localhost:$port/" -UseBasicParsing -TimeoutSec 3 -ErrorAction Stop | Out-Null; $ok = $true; break } catch { Start-Sleep -s 1; $tries++ }
}
if (-not $ok) { Write-Output 'Server did not start in time.'; exit 2 }

Write-Output 'Running test script...'
# Invoke the fixed test script by relative path from the repository root
& "$cwd\scripts\test_cart_fixed.ps1"
