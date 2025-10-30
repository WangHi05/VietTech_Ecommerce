$port = 5005
$project = 'c:\Users\ASUS\Downloads\ProjectLTWNangCao-main\eCommerce.Web\eCommerce.Web.csproj'
$cwd = 'c:\Users\ASUS\Downloads\ProjectLTWNangCao-main'

$entry = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue | Select-Object -First 1
if ($entry) {
    try { Stop-Process -Id $entry.OwningProcess -Force -ErrorAction SilentlyContinue; Start-Sleep -s 1 } catch {}
}

Write-Output 'Starting dotnet run (background)...'
Start-Process -FilePath 'dotnet' -ArgumentList @('run','--project',$project,'--','--urls',"http://localhost:$port") -WorkingDirectory $cwd -NoNewWindow -PassThru | Out-Null

# Wait for server readiness
$tries = 0; $ok = $false
while ($tries -lt 30) {
    try {
        Invoke-WebRequest -Uri "http://localhost:$port/" -UseBasicParsing -TimeoutSec 3 -ErrorAction Stop | Out-Null
        $ok = $true; break
    } catch {
        Start-Sleep -Seconds 1; $tries++
    }
}

if (-not $ok) { Write-Output 'Server did not start in time.'; exit 1 }
Write-Output "Server started at http://localhost:$port"
