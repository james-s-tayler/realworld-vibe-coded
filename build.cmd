@echo off
setlocal

set BUILD_PROJECT=build\_build\_build.csproj
set BUILD_OUTPUT=build\_build\bin\Debug\_build.dll
set BUILD_EXECUTABLE=build\_build\bin\Debug\_build.exe

rem Check if build is up to date
set BUILD_NEEDED=0

rem Check if output exists
if not exist "%BUILD_OUTPUT%" if not exist "%BUILD_EXECUTABLE%" set BUILD_NEEDED=1

rem Check if project file is newer (basic check)
if exist "%BUILD_PROJECT%" if not exist "%BUILD_OUTPUT%" set BUILD_NEEDED=1

rem Build if necessary
if %BUILD_NEEDED%==1 (
    echo Building Nuke project...
    dotnet build "%BUILD_PROJECT%" --verbosity quiet
)

rem Run the built executable
if exist "%BUILD_EXECUTABLE%" (
    "%BUILD_EXECUTABLE%" %*
) else if exist "%BUILD_OUTPUT%" (
    dotnet "%BUILD_OUTPUT%" %*
) else (
    echo Error: Could not find built Nuke executable
    exit /b 1
)