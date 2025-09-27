@echo off
setlocal

set BUILD_SOLUTION=Ops\TaskRunner\Nuke.sln
set BUILD_OUTPUT=Ops\TaskRunner\_build\bin\Debug\_build.dll
set BUILD_EXECUTABLE=Ops\TaskRunner\_build\bin\Debug\_build.exe

rem Check if build is up to date
set BUILD_NEEDED=0

rem Check if output exists
if not exist "%BUILD_OUTPUT%" if not exist "%BUILD_EXECUTABLE%" set BUILD_NEEDED=1

rem Check if solution file is newer (basic check)
if exist "%BUILD_SOLUTION%" if not exist "%BUILD_OUTPUT%" set BUILD_NEEDED=1

rem Build if necessary
if %BUILD_NEEDED%==1 (
    echo Building Nuke solution...
    dotnet build "%BUILD_SOLUTION%" --verbosity quiet
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