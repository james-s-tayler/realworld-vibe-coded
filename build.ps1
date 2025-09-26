[CmdletBinding()]
Param(
    [string]$Script = "build.cake",
    [string]$Target,
    [string]$Configuration,
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [string]$Verbosity,
    [switch]$ShowDescription,
    [switch]$DryRun,
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$ScriptArgs
)

# Cake build script for Windows PowerShell

$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
$TOOLS_DIR = Join-Path $PSScriptRoot "tools"
$CAKE_VERSION = "4.0.0"
$CAKE_DLL = Join-Path $TOOLS_DIR "Cake.dll"

# Make sure tools folder exists
if (!(Test-Path $TOOLS_DIR)) {
    Write-Verbose "Creating tools directory..."
    New-Item -Path $TOOLS_DIR -Type Directory | out-null
}

###########################################################################
# INSTALL CAKE
###########################################################################

try {
    $InstalledVersion = &dotnet-cake --version 2>&1
    $CAKE_INSTALLED = $LASTEXITCODE -eq 0
} catch {
    $CAKE_INSTALLED = $false
}

if (!$CAKE_INSTALLED -or $InstalledVersion -ne $CAKE_VERSION) {
    Write-Host "Installing Cake $CAKE_VERSION..." -ForegroundColor Green
    &dotnet tool install --global Cake.Tool --version $CAKE_VERSION
    if ($LASTEXITCODE -ne 0) {
        Throw "An error occurred while installing Cake."
    }
}

###########################################################################
# RUN BUILD SCRIPT
###########################################################################

Write-Host "Running build script..." -ForegroundColor Green

# Build Cake arguments
$cakeArguments = @()
if ($Target) { $cakeArguments += "--target=$Target" }
if ($Configuration) { $cakeArguments += "--configuration=$Configuration" }
if ($Verbosity) { $cakeArguments += "--verbosity=$Verbosity" }
if ($ShowDescription) { $cakeArguments += "--showdescription" }
if ($DryRun) { $cakeArguments += "--dryrun" }
$cakeArguments += $ScriptArgs

&dotnet-cake $Script $cakeArguments
exit $LASTEXITCODE