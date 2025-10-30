$port=5005
$project='c:\Users\ASUS\Downloads\ProjectLTWNangCao-main\eCommerce.Web\eCommerce.Web.csproj'
$cwd='c:\Users\ASUS\Downloads\ProjectLTWNangCao-main'

$exists = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
if (-not $exists) {
    Write-Output 'Starting app...'
    Start-Process -FilePath 'dotnet' -ArgumentList @('run','--project',$project,'--','--urls',"http://localhost:$port") -WorkingDirectory $cwd -NoNewWindow -PassThru | Out-Null
    Start-Sleep -Seconds 4
} else {
    Write-Output "Port $port already in use."
}

$tries = 0
$ok = $false
while ($tries -lt 20) {
    try {
        Invoke-WebRequest -Uri "http://localhost:$port/" -UseBasicParsing -TimeoutSec 3 -ErrorAction Stop | Out-Null
        $ok = $true
        break
    } catch {
        Start-Sleep -Seconds 1
        $tries++
    }
}
if (-not $ok) { Write-Output 'Server did not start in time.'; exit 2 }

$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession

# Get index page to obtain antiforgery token for add-to-cart form
$index = Invoke-WebRequest -Uri "http://localhost:$port/" -WebSession $session -UseBasicParsing
$token = $null
if ($index.Content -match 'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"') { $token = $matches[1] }
 

if (-not $token) { Write-Output 'Could not find antiforgery token on Index page; trying Cart page.'; $index = Invoke-WebRequest -Uri "http://localhost:$port/Cart" -WebSession $session -UseBasicParsing; if ($index.Content -match 'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"') { $token = $matches[1] } }

if (-not $token) { Write-Output 'No antiforgery token found; cannot POST.'; exit 3 }

# Add productId=1
$addBody = @{ __RequestVerificationToken = $token; productId = 1; qty = 1 }
$add = Invoke-WebRequest -Uri "http://localhost:$port/Cart?handler=Add" -Method Post -WebSession $session -Body $addBody -UseBasicParsing -ErrorAction SilentlyContinue
Write-Output (("AddStatus:{0}" -f ($add.StatusCode -as [string])) -as [string])

# Get cart and refresh token for subsequent forms
$cart = Invoke-WebRequest -Uri "http://localhost:$port/Cart" -WebSession $session -UseBasicParsing
if ($cart.Content -match 'Laptop Gaming Pro X') { Write-Output 'CartContainsProduct:1' } else { Write-Output 'CartContainsProduct:0' }
if ($cart.Content -match 'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"') { $token = $matches[1] }

# Apply voucher
$applyBody = @{ __RequestVerificationToken = $token; VoucherCode = 'WELCOME10' }
$apply = Invoke-WebRequest -Uri "http://localhost:$port/Cart?handler=ApplyVoucher" -Method Post -WebSession $session -Body $applyBody -UseBasicParsing -ErrorAction SilentlyContinue
Write-Output (("ApplyStatus:{0}" -f ($apply.StatusCode -as [string])) -as [string])

# Calculate shipping (Province: Hồ Chí Minh)
$cart = Invoke-WebRequest -Uri "http://localhost:$port/Cart" -WebSession $session -UseBasicParsing
if ($cart.Content -match 'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"') { $token = $matches[1] }
$calcBody = @{ __RequestVerificationToken = $token; Country = 'Vietnam'; Province = 'Hồ Chí Minh' }
$calc = Invoke-WebRequest -Uri "http://localhost:$port/Cart?handler=CalculateShipping" -Method Post -WebSession $session -Body $calcBody -UseBasicParsing -ErrorAction SilentlyContinue
Write-Output (("CalcStatus:{0}" -f ($calc.StatusCode -as [string])) -as [string])

$cart2 = Invoke-WebRequest -Uri "http://localhost:$port/Cart" -WebSession $session -UseBasicParsing
$lines = $cart2.Content -split "`n" | Where-Object { $_ -match 'Tạm tính|Giảm giá|Phí vận chuyển|Tổng cộng' }
Write-Output '--- Cart Summary Lines ---'
$lines | ForEach-Object { $_.Trim() }
