@echo off
if not defined cli-built call "%~dp0..\..\build.bat" || exit /b 1

:: execute-commands reads c4interflow.config.yaml from the current directory
pushd "%~dp0"
"%cli-exe%" execute-commands
set "exit-code=%ERRORLEVEL%"
popd

if NOT "%BATCH_TEST_MODE%"=="1" pause
exit /b %exit-code%
