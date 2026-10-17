param(
    [string]$GodotPath,
    [switch]$Play
)

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$localRoot = Join-Path $projectRoot 'tools\godot'
$localGodot = Join-Path $localRoot 'Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe'

if ($PSBoundParameters.ContainsKey('GodotPath') -and
    -not (Test-Path -LiteralPath $GodotPath -PathType Leaf)) {
    throw "Godot executable not found at -GodotPath: $GodotPath"
}

if (-not $GodotPath -and (Test-Path -LiteralPath $localGodot -PathType Leaf)) {
    $GodotPath = $localGodot
}

if (-not $GodotPath -and $env:GODOT_EXE -and
    (Test-Path -LiteralPath $env:GODOT_EXE -PathType Leaf)) {
    $GodotPath = $env:GODOT_EXE
}

if (-not $GodotPath) {
    $cachedGodot = Join-Path $env:TEMP 'neighborhood-watch-godot-4.7.2\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe'
    if (Test-Path -LiteralPath $cachedGodot -PathType Leaf) {
        $GodotPath = $cachedGodot
    }
}

if (-not $GodotPath) {
    $command = Get-Command godot,godot4 -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($command) { $GodotPath = $command.Source }
}

if (-not $GodotPath) {
    $zipPath = Join-Path $env:USERPROFILE 'Downloads\Godot_v4.7.2-stable_mono_win64.zip'
    if (Test-Path -LiteralPath $zipPath -PathType Leaf) {
        Write-Host "Extracting Godot 4.7.2 .NET into $localRoot"
        Expand-Archive -LiteralPath $zipPath -DestinationPath $localRoot -Force
        $GodotPath = $localGodot
    }
}

if (-not $GodotPath -or -not (Test-Path -LiteralPath $GodotPath -PathType Leaf)) {
    throw 'Godot 4.7.2 .NET was not found. Put its ZIP in Downloads, extract it to tools/godot, or pass -GodotPath.'
}

$version = (& $GodotPath --version)
if ($LASTEXITCODE -ne 0 -or $version -notmatch '^4\.7\.2\.stable\.mono\.') {
    throw "Expected Godot 4.7.2 .NET; found: $version"
}
Write-Host "Using Godot: $GodotPath"

$localPackages = Join-Path (Split-Path -Parent $GodotPath) 'GodotSharp\Tools\nupkgs'
if (Test-Path -LiteralPath $localPackages) {
    dotnet restore (Join-Path $projectRoot 'NeighborhoodWatch.csproj') --source $localPackages
} else {
    dotnet restore (Join-Path $projectRoot 'NeighborhoodWatch.csproj')
}
if ($LASTEXITCODE -ne 0) { throw 'dotnet restore failed.' }

dotnet build (Join-Path $projectRoot 'NeighborhoodWatch.csproj') --no-restore
if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed.' }

& $GodotPath --headless --path $projectRoot --editor --import --quit
if ($LASTEXITCODE -ne 0) { throw 'Godot import failed.' }

& $GodotPath --headless --path $projectRoot --quit-after 20
if ($LASTEXITCODE -ne 0) { throw 'Godot scene launch failed.' }

& $GodotPath --headless --path $projectRoot --script 'res://tests/movement_smoke.gd'
if ($LASTEXITCODE -ne 0) { throw 'Movement smoke test failed.' }

Write-Host 'M01 validation passed: build, import, scene launch, and movement smoke.'

if ($Play) {
    & $GodotPath --path $projectRoot
    if ($LASTEXITCODE -ne 0) { throw 'Visible game launch failed.' }
}
