# Helper: call Unity Pipeline HTTP API (works even when CLI ↔ package versions mismatch).
param(
    [Parameter(Mandatory = $true)][string]$Command,
    [hashtable]$Parameters = @{},
    [string]$ProjectPath = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [switch]$Job
)

$descPath = Join-Path $ProjectPath "Library\Pipeline\.unity-pipeline-port"
if (-not (Test-Path $descPath)) {
    throw "Pipeline descriptor not found at $descPath. Open the project in Unity with Pipeline installed."
}

$desc = Get-Content $descPath -Raw | ConvertFrom-Json
$bodyObj = @{ command = $Command; parameters = $Parameters }
if ($Job) { $bodyObj.job = $true }
$body = $bodyObj | ConvertTo-Json -Compress -Depth 8
$tmp = Join-Path $env:TEMP ("unity-exec-{0}.json" -f [guid]::NewGuid().ToString("n"))
[System.IO.File]::WriteAllText($tmp, $body)

try {
    $url = "http://127.0.0.1:$($desc.port)/api/exec"
    $raw = & curl.exe -s -X POST `
        -H "Authorization: Bearer $($desc.evalToken)" `
        -H "Content-Type: application/json" `
        --data-binary "@$tmp" `
        $url
    $raw
}
finally {
    Remove-Item $tmp -ErrorAction SilentlyContinue
}
