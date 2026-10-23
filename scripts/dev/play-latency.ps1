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
$visibleGodot = $GodotPath -replace '_console\.exe$', '.exe'
if (-not (Test-Path -LiteralPath $visibleGodot -PathType Leaf)) { $visibleGodot = $GodotPath }
$conditioner = Join-Path $projectRoot 'tests\NetworkConditions\bin\Release\net8.0\NetworkConditions.exe'
$logRoot = Join-Path $projectRoot '.godot\play-latency'
New-Item -ItemType Directory -Force -Path $logRoot | Out-Null
$serverPort = Get-Random -Minimum 25000 -Maximum 34997
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

try {
    Start-HiddenProcess 'server' $GodotPath @('--headless', '--path', $projectRoot, '--', '--server', "--port=$serverPort") | Out-Null
    Wait-ForText 'server' 'Network server ready'
    Start-HiddenProcess 'first-proxy' $conditioner @("$($serverPort + 1)", "$serverPort", "$OneWayDelayMs", "$DropEvery") | Out-Null
    Start-HiddenProcess 'second-proxy' $conditioner @("$($serverPort + 2)", "$serverPort", "$OneWayDelayMs", "$DropEvery") | Out-Null
    Wait-ForText 'first-proxy' 'Network conditioner ready'
    Wait-ForText 'second-proxy' 'Network conditioner ready'

    $first = Start-Process -FilePath $visibleGodot -ArgumentList @('--path', $projectRoot, '--',
        '--connect=127.0.0.1', "--port=$($serverPort + 1)") -WorkingDirectory $projectRoot -WindowStyle Normal -PassThru
    $processes += $first
    Wait-ForText 'server' 'Server spawned peer'
    $second = Start-Process -FilePath $visibleGodot -ArgumentList @('--path', $projectRoot, '--',
        '--connect=127.0.0.1', "--port=$($serverPort + 2)") -WorkingDirectory $projectRoot -WindowStyle Normal -PassThru
    $processes += $second
    $lossDescription = if ($DropEvery -eq 0) { 'no packet loss' } else { "1/$DropEvery packet loss" }
    Write-Host "Two clients are running with $($OneWayDelayMs * 2) ms nominal RTT and $lossDescription. Move both players, use a mission spot, then close both windows."
    $first.WaitForExit()
    $second.WaitForExit()
}
finally {
    foreach ($process in $processes) {
        if (-not $process.HasExited) { Stop-Process -Id $process.Id -Force }
    }
}
