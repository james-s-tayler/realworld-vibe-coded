param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]] $Arguments
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$NukeRootDir = $ScriptDir
$NukeExe = "$ScriptDir/build/bin/Debug/_build.dll"

if (-not (Test-Path $NukeExe)) {
    Write-Output "Building Nuke build project..."
    dotnet build "$ScriptDir/build/_build.csproj" --nologo -c Debug --verbosity minimal
}

& dotnet $NukeExe $Arguments
exit $LASTEXITCODE