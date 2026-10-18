param(
    [switch]$Server,
    [string]$Connect = '127.0.0.1',
    [ValidateRange(1024, 65535)][int]$Port = 24567,
    [string]$GodotPath
)

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$localGodot = Join-Path $projectRoot 'tools\godot\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe'
if (-not $GodotPath -and (Test-Path -LiteralPath $localGodot -PathType Leaf)) {
    $GodotPath = $localGodot
}
if (-not $GodotPath -and $env:GODOT_EXE) {
    $GodotPath = $env:GODOT_EXE
}
if (-not $GodotPath -or -not (Test-Path -LiteralPath $GodotPath -PathType Leaf)) {
    throw 'Godot 4.7.2 .NET was not found. Supply -GodotPath or set GODOT_EXE.'
}

if ($Server) {
    & $GodotPath --headless --path $projectRoot -- --server "--port=$Port"
} else {
    & $GodotPath --path $projectRoot -- "--connect=$Connect" "--port=$Port"
}
if ($LASTEXITCODE -ne 0) { throw "Godot network process exited with code $LASTEXITCODE." }
