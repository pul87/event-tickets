# capienza 5
$perf2 = [guid]::NewGuid()
Invoke-RestMethod -Method POST http://localhost:5000/inventory/performances `
  -ContentType application/json `
  -Body (@{ performanceId=$perf2; capacity=5 } | ConvertTo-Json) | Out-Null

$uri = "http://localhost:5000/sales/reservations"
$body = (@{ performanceId=$perf2; quantity=1 } | ConvertTo-Json)

$script = {
  param($u,$b)
  try {
    $r = Invoke-WebRequest -Uri $u -Method POST -ContentType 'application/json' -Body $b -TimeoutSec 60
    [int]$r.StatusCode
  } catch {
    [int]$_.Exception.Response.StatusCode.value__
  }
}

$jobs = 1..10 | ForEach-Object { Start-Job -ScriptBlock $script -ArgumentList $uri,$body }
$codes = $jobs | Wait-Job | Receive-Job
$success = ($codes | Where-Object { $_ -eq 201 }).Count
$conflict = ($codes | Where-Object { $_ -eq 409 }).Count
"success: $success, conflict: $conflict, all: $($codes.Count)"

Invoke-RestMethod "http://localhost:5000/inventory/performances/$perf2"
# atteso: Reserved = 5
