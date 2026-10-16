param(
    [string]$GodotPath = $env:GODOT_EXE
)

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path

if (-not $GodotPath) {
    $command = Get-Command godot -ErrorAction SilentlyContinue
    if ($command) { $GodotPath = $command.Source }
}

if (-not $GodotPath -or -not (Test-Path -LiteralPath $GodotPath -PathType Leaf)) {
    throw 'Set GODOT_EXE to the Godot 4.7.2 .NET console executable, or pass -GodotPath.'
}

$version = (& $GodotPath --version)
if ($LASTEXITCODE -ne 0 -or $version -notmatch '^4\.7\.2\.stable\.mono\.') {
    throw "Expected Godot 4.7.2 .NET; found: $version"
}

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
