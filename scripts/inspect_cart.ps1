$uri='http://localhost:5005/Cart'
$c=(Invoke-WebRequest -Uri $uri -UseBasicParsing).Content
$needle='cart-summary'
if ($c.IndexOf($needle) -ge 0) {
    $i=$c.IndexOf($needle)
    Write-Output "Found '$needle' at index $i"
    $start=[Math]::Max(0,$i-200)
    $len=[Math]::Min(800,$c.Length - $start)
    Write-Output ($c.Substring($start,$len))
} else {
    Write-Output "'$needle' not found. Searching for summary-row or 'Tạm tính'..."
    if ($c.IndexOf('summary-row') -ge 0) { $i=$c.IndexOf('summary-row'); Write-Output "Found summary-row at $i"; $start=[Math]::Max(0,$i-200); $len=[Math]::Min(800,$c.Length - $start); Write-Output ($c.Substring($start,$len)) }
    elseif ($c.IndexOf('Tạm tính') -ge 0) { $i=$c.IndexOf('Tạm tính'); Write-Output "Found Tạm tính at $i"; $start=[Math]::Max(0,$i-200); $len=[Math]::Min(800,$c.Length - $start); Write-Output ($c.Substring($start,$len)) }
    else { Write-Output 'No summary markers found; dumping tail (last 800 chars):'; $start=[Math]::Max(0,$c.Length-800); Write-Output ($c.Substring($start,[Math]::Min(800,$c.Length))) }
}
