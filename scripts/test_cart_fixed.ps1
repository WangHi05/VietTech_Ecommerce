$port = 5005
$project = 'c:\Users\ASUS\Downloads\ProjectLTWNangCao-main\eCommerce.Web\eCommerce.Web.csproj'
$cwd = 'c:\Users\ASUS\Downloads\ProjectLTWNangCao-main'

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
# Log Set-Cookie from the response and current session cookies
Write-Output "Add Response Set-Cookie: $($add.Headers['Set-Cookie'])"
Write-Output 'Cookies after Add:'
$session.Cookies.GetCookies((New-Object System.Uri("http://localhost:$port"))) | ForEach-Object { Write-Output ("  {0}={1}; Path={2}; Domain={3}; Expires={4}" -f $_.Name, $_.Value, $_.Path, $_.Domain, $_.Expires) }

# Get cart and refresh token for subsequent forms
 $cart = Invoke-WebRequest -Uri "http://localhost:$port/Cart" -WebSession $session -UseBasicParsing
Write-Output (("Cart GET Status:{0}" -f ($cart.StatusCode -as [string])) -as [string])
Write-Output 'Cookies after Cart GET:'
$session.Cookies.GetCookies((New-Object System.Uri("http://localhost:$port"))) | ForEach-Object { Write-Output ("  {0}={1}; Path={2}; Domain={3}; Expires={4}" -f $_.Name, $_.Value, $_.Path, $_.Domain, $_.Expires) }
if ($cart.Content -match 'Laptop Gaming Pro X') { Write-Output 'CartContainsProduct:1' } else { Write-Output 'CartContainsProduct:0' }
if ($cart.Content -match 'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"') { $token = $matches[1] }

# Apply voucher
$applyBody = @{ __RequestVerificationToken = $token; VoucherCode = 'WELCOME10' }
 $apply = Invoke-WebRequest -Uri "http://localhost:$port/Cart?handler=ApplyVoucher" -Method Post -WebSession $session -Body $applyBody -UseBasicParsing -ErrorAction SilentlyContinue
Write-Output (("ApplyStatus:{0}" -f ($apply.StatusCode -as [string])) -as [string])
Write-Output "Apply Response Set-Cookie: $($apply.Headers['Set-Cookie'])"
Write-Output 'Cookies after Apply:'
$session.Cookies.GetCookies((New-Object System.Uri("http://localhost:$port"))) | ForEach-Object { Write-Output ("  {0}={1}; Path={2}; Domain={3}; Expires={4}" -f $_.Name, $_.Value, $_.Path, $_.Domain, $_.Expires) }

# Calculate shipping (Province: Hồ Chí Minh)
 $cart = Invoke-WebRequest -Uri "http://localhost:$port/Cart" -WebSession $session -UseBasicParsing
Write-Output (("Cart GET2 Status:{0}" -f ($cart.StatusCode -as [string])) -as [string])
Write-Output 'Cookies after Cart GET2:'
$session.Cookies.GetCookies((New-Object System.Uri("http://localhost:$port"))) | ForEach-Object { Write-Output ("  {0}={1}; Path={2}; Domain={3}; Expires={4}" -f $_.Name, $_.Value, $_.Path, $_.Domain, $_.Expires) }
if ($cart.Content -match 'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"') { $token = $matches[1] }
$calcBody = @{ __RequestVerificationToken = $token; Country = 'Vietnam'; Province = 'Hồ Chí Minh' }
$calc = Invoke-WebRequest -Uri "http://localhost:$port/Cart?handler=CalculateShipping" -Method Post -WebSession $session -Body $calcBody -UseBasicParsing -ErrorAction SilentlyContinue
Write-Output (("CalcStatus:{0}" -f ($calc.StatusCode -as [string])) -as [string])
Write-Output "Calc Response Set-Cookie: $($calc.Headers['Set-Cookie'])"
Write-Output 'Cookies after Calc:'
$session.Cookies.GetCookies((New-Object System.Uri("http://localhost:$port"))) | ForEach-Object { Write-Output ("  {0}={1}; Path={2}; Domain={3}; Expires={4}" -f $_.Name, $_.Value, $_.Path, $_.Domain, $_.Expires) }

$cart2 = Invoke-WebRequest -Uri "http://localhost:$port/Cart" -WebSession $session -UseBasicParsing
# save the final cart HTML that this test session sees for inspection
$cart2.Content | Out-File -FilePath "$cwd\scripts\cart_session_dump.html" -Encoding utf8
Write-Output "WROTE_SESSION_DUMP:$cwd\scripts\cart_session_dump.html"
$lines = $cart2.Content -split "`n" | Where-Object { $_ -match 'Tạm tính|Giảm giá|Phí vận chuyển|Tổng cộng|summary-row|summary' }
Write-Output '--- Cart Summary Lines ---'
$lines | ForEach-Object { $_.Trim() }
