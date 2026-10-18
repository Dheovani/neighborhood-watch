param([Parameter(Mandatory)][string]$GodotPath)

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$logRoot = Join-Path $projectRoot '.godot\network-smoke'
New-Item -ItemType Directory -Force -Path $logRoot | Out-Null
$port = Get-Random -Minimum 25000 -Maximum 35000
$processes = @()

function Start-HiddenGodot([string]$name, [string[]]$arguments) {
    $stdout = Join-Path $logRoot "$name.out"
    $stderr = Join-Path $logRoot "$name.err"
    $process = Start-Process -FilePath $GodotPath -ArgumentList $arguments -WorkingDirectory $projectRoot -WindowStyle Hidden -RedirectStandardOutput $stdout -RedirectStandardError $stderr -PassThru
    $script:processes += $process
    return $process
}

function Assert-Output([string]$name, [string]$expected) {
    $stdout = Join-Path $logRoot "$name.out"
    $stderr = Join-Path $logRoot "$name.err"
    if ((Get-Item -LiteralPath $stderr).Length -gt 0) {
        throw "$name wrote errors: $(Get-Content -LiteralPath $stderr -Raw)"
    }
    if (-not (Select-String -LiteralPath $stdout -Pattern $expected -Quiet)) {
        throw "$name did not report '$expected': $(Get-Content -LiteralPath $stdout -Raw)"
    }
}

try {
    $server = Start-HiddenGodot 'server' @('--headless', '--path', $projectRoot, '--', '--server', "--port=$port")
    $ready = $false
    for ($attempt = 0; $attempt -lt 50; $attempt++) {
        Start-Sleep -Milliseconds 100
        if ($server.HasExited) { throw 'Network server exited before clients connected.' }
        if (Select-String -LiteralPath (Join-Path $logRoot 'server.out') -Pattern 'Network server ready' -Quiet) {
            $ready = $true
            break
        }
    }
    if (-not $ready) { throw 'Network server was not ready within 5 seconds.' }

    $actor = Start-HiddenGodot 'actor' @('--headless', '--path', $projectRoot, '--script', 'res://tests/network_client_smoke.gd', '--', '--connect=127.0.0.1', "--port=$port", '--smoke-role=actor')
    $actorJoined = $false
    for ($attempt = 0; $attempt -lt 50; $attempt++) {
        Start-Sleep -Milliseconds 100
        if (Select-String -LiteralPath (Join-Path $logRoot 'server.out') -Pattern 'Server spawned peer' -Quiet) {
            $actorJoined = $true
            break
        }
    }
    if (-not $actorJoined) { throw 'Actor did not connect within 5 seconds.' }
    $observer = Start-HiddenGodot 'observer' @('--headless', '--path', $projectRoot, '--script', 'res://tests/network_client_smoke.gd', '--', '--connect=127.0.0.1', "--port=$port", '--smoke-role=observer')

    if (-not $actor.WaitForExit(20000) -or -not $observer.WaitForExit(20000)) {
        throw 'Network clients did not finish within 20 seconds.'
    }
    Assert-Output 'actor' 'M04 actor smoke passed'
    Assert-Output 'observer' 'M04 observer smoke passed'
    Assert-Output 'server' 'Server infraction #2'
    $serverOutput = Get-Content -LiteralPath (Join-Path $logRoot 'server.out') -Raw
    if ($serverOutput -match 'Server infraction #3') {
        throw 'Repeated use of a one-time spot created a duplicate infraction.'
    }
    Write-Host 'M04 network smoke passed: two clients, server movement, validated and rejected actions, spontaneous infraction, shared state, duplicate protection.'
}
finally {
    foreach ($process in $processes) {
        if (-not $process.HasExited) {
            Stop-Process -Id $process.Id -Force
        }
    }
}
