param(
    [string]$GodotPath,
    [ValidateRange(0, 1000)][int]$OneWayDelayMs = 75,
    [ValidateRange(0, 1000)][int]$DropEvery = 50
)

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$localGodot = Join-Path $projectRoot 'tools\godot\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe'
if (-not $GodotPath -and (Test-Path -LiteralPath $localGodot -PathType Leaf)) { $GodotPath = $localGodot }
if (-not $GodotPath -and $env:GODOT_EXE) { $GodotPath = $env:GODOT_EXE }
if (-not $GodotPath -or -not (Test-Path -LiteralPath $GodotPath -PathType Leaf)) {
    throw 'Godot 4.7.2 .NET was not found. Set GODOT_EXE or pass -GodotPath.'
}
$logRoot = Join-Path $projectRoot '.godot\latency-smoke'
New-Item -ItemType Directory -Force -Path $logRoot | Out-Null
$serverPort = Get-Random -Minimum 25000 -Maximum 34997
$conditioner = Join-Path $projectRoot 'tests\NetworkConditions\bin\Release\net8.0\NetworkConditions.exe'
$processes = @()

dotnet build (Join-Path $projectRoot 'tests\NetworkConditions\NetworkConditions.csproj') --configuration Release --warnaserror
if ($LASTEXITCODE -ne 0) { throw 'Network conditioner build failed.' }

function Start-HiddenProcess([string]$name, [string]$file, [string[]]$arguments) {
    $process = Start-Process -FilePath $file -ArgumentList $arguments -WorkingDirectory $projectRoot -WindowStyle Hidden `
        -RedirectStandardOutput (Join-Path $logRoot "$name.out") -RedirectStandardError (Join-Path $logRoot "$name.err") -PassThru
    $script:processes += $process
    return $process
}

function Wait-ForText([string]$name, [string]$pattern) {
    $output = Join-Path $logRoot "$name.out"
    for ($attempt = 0; $attempt -lt 50; $attempt++) {
        Start-Sleep -Milliseconds 100
        if (Select-String -LiteralPath $output -Pattern $pattern -Quiet) { return }
    }
    throw "$name did not report '$pattern' within 5 seconds."
}

function Assert-Result([string]$name, [string]$pattern) {
    $output = Join-Path $logRoot "$name.out"
    $errors = Join-Path $logRoot "$name.err"
    if ((Get-Item -LiteralPath $errors).Length -gt 0) {
        throw "$name wrote errors: $(Get-Content -LiteralPath $errors -Raw)"
    }
    if (-not (Select-String -LiteralPath $output -Pattern $pattern -Quiet)) {
        throw "$name did not report '$pattern': $(Get-Content -LiteralPath $output -Raw)"
    }
}

try {
    $server = Start-HiddenProcess 'server' $GodotPath @('--headless', '--path', $projectRoot, '--', '--server', "--port=$serverPort")
    Wait-ForText 'server' 'Network server ready'
    $actorProxy = Start-HiddenProcess 'actor-proxy' $conditioner @("$($serverPort + 1)", "$serverPort", "$OneWayDelayMs", "$DropEvery")
    $observerProxy = Start-HiddenProcess 'observer-proxy' $conditioner @("$($serverPort + 2)", "$serverPort", "$OneWayDelayMs", "$DropEvery")
    Wait-ForText 'actor-proxy' 'Network conditioner ready'
    Wait-ForText 'observer-proxy' 'Network conditioner ready'

    $actor = Start-HiddenProcess 'actor' $GodotPath @('--headless', '--path', $projectRoot,
        '--script', 'res://tests/network_client_smoke.gd', '--', '--connect=127.0.0.1',
        "--port=$($serverPort + 1)", '--smoke-role=actor')
    Wait-ForText 'server' 'Server spawned peer'
    $observer = Start-HiddenProcess 'observer' $GodotPath @('--headless', '--path', $projectRoot,
        '--script', 'res://tests/network_client_smoke.gd', '--', '--connect=127.0.0.1',
        "--port=$($serverPort + 2)", '--smoke-role=observer')

    if (-not $actor.WaitForExit(30000) -or -not $observer.WaitForExit(30000)) {
        throw 'Conditioned clients did not finish within 30 seconds.'
    }
    Assert-Result 'actor' 'M04 actor smoke passed'
    Assert-Result 'observer' 'M04 observer smoke passed'
    Assert-Result 'server' 'Server infraction #3'
    if ($DropEvery -gt 0) {
        $dropped = (Select-String -LiteralPath (Join-Path $logRoot 'actor-proxy.out'),
            (Join-Path $logRoot 'observer-proxy.out') -Pattern 'Dropped .* packet' -Quiet)
        if (-not $dropped) { throw 'No packets were dropped during the conditioned test.' }
    }
    $lossDescription = if ($DropEvery -eq 0) { 'no packet loss' } else { "1/$DropEvery packet loss" }
    Write-Host "M04 conditioned network smoke passed: $($OneWayDelayMs * 2) ms nominal RTT and $lossDescription."
}
finally {
    foreach ($process in $processes) {
        if (-not $process.HasExited) { Stop-Process -Id $process.Id -Force }
    }
}
