@echo off
:: Publishes the C4InterFlow CLI and sets 'cli-exe' to its full path.
:: Skips publishing if it was already done in this session (e.g. when called from Samples\draw-diagrams.bat).
if not defined build-configuration set "build-configuration=Debug"
set "cli-dir=%~dp0..\C4InterFlow"
set "cli-output-dir=%cli-dir%\bin\%build-configuration%\net10.0\win-x64"
set "cli-exe=%cli-output-dir%\C4InterFlow.exe"

if "%cli-built%"=="%build-configuration%" exit /b 0

echo Publishing...
dotnet publish "%cli-dir%\C4InterFlow.csproj" --configuration %build-configuration% --output "%cli-output-dir%" --verbosity quiet || exit /b 1
set "cli-built=%build-configuration%"
