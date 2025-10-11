# 1) crea una performance con capienza 5
$perf = [guid]::NewGuid()
Invoke-RestMethod -Method POST http://localhost:5108/inventory/performances `
  -ContentType application/json `
  -Body (@{ performanceId=$perf; capacity=5 } | ConvertTo-Json) | Out-Null

# 2) prepara le due chiamate concorrenti: quantity=3 e quantity=3
$uri = "http://localhost:5108/sales/reservations"
$body = (@{ performanceId=$perf; quantity=3 } | ConvertTo-Json)

$script = {
  param($u,$b)
  try {
    $r = Invoke-WebRequest -Uri $u -Method POST -ContentType 'application/json' -Body $b -TimeoutSec 60
    [int]$r.StatusCode
  } catch {
    # ritorna lo status code dell'errore (409 atteso su una delle due)
    [int]$_.Exception.Response.StatusCode.value__
  }
}

# 3) lancia in parallelo
$j1 = Start-Job -ScriptBlock $script -ArgumentList $uri,$body
$j2 = Start-Job -ScriptBlock $script -ArgumentList $uri,$body
$codes = @($j1,$j2 | Wait-Job | Receive-Job)

$codes
# atteso: uno 201, uno 409

# 4) controlla l'inventario
Invoke-RestMethod "http://localhost:5108/inventory/performances/$perf"
# atteso: Reserved=3, Sold=0, Capacity=5
