:: Push the current directory so we can drop back to it after
@pushd

@setlocal enableextensions enabledelayedexpansion

@CALL :SetESC
@CALL :SetColors
@SET /A _TIMEOUT_=10

@IF NOT DEFINED net-sourcecode-dir SET "net-sourcecode-dir=C:\Data\Projects\C4InterFlow\eShop-main"

@SET /P "eshop_path=Where is the dotnet EShop repo cloned to (default: '%net-sourcecode-dir%')?"
@IF DEFINED eshop_path SET "net-sourcecode-dir=%eshop_path%"
@IF NOT EXIST "%net-sourcecode-dir%\eShop.sln" (
    ECHO "Could not find the 'eShop.sln' file under '%net-sourcecode-dir%'!"
    EXIT 1
)

@ECHO Make a choice for the following parameters. The default value will be chosen after %_TIMEOUT_% seconds of no input.
@ECHO.

@CHOICE /c DR /T %_TIMEOUT_% /D D /M "Build Configuration - [D]ebug or [R]elease (default: Debug)"
@IF %ERRORLEVEL% EQU 1 SET "build-configuration=Debug"
@IF %ERRORLEVEL% EQU 2 SET "build-configuration=Release"


@CHOICE /c YN /T %_TIMEOUT_% /D Y /M "Stop execution if files change "
@IF %ERRORLEVEL% EQU 1 SET "stop_on_modified=1"
@IF %ERRORLEVEL% EQU 2 SET "stop_on_modified=0"


@SET ALL_ARGS=%*
@IF NOT DEFINED ALL_ARGS SET "ALL_ARGS="

@SET SCRIPT="%~dpnx0"
@SET "SCRIPT_DIRECTORY=%~dp0"

:: Assume the script is being run interactively
@SET SCRIPT_IS_INTERACTIVE=1

@SET BATCH_TEST_MODE=1

:: Search for the script's name in the command's command line
:: if the script exists there, it likely was executed by Explorer rather than the command line
:: Inspired by https://stackoverflow.com/a/9422268
@ECHO %cmdcmdline% | FIND /i %SCRIPT% >nul
@IF NOT errorlevel 1 SET SCRIPT_IS_INTERACTIVE=0
:: when executing this from Powershell, it runs the same command as if it is coming from Explorer
:: So, circumvent that by checking for something that's different about Powershell...
:: See https://github.com/PowerShell/PowerShell/issues/9797 for more context
@ECHO "%PATHEXT%" | FIND /i ".CPL" >nul
@IF NOT errorlevel 1 SET SCRIPT_IS_INTERACTIVE=1


@SET _OUTPUT_CHANGED_=0

@SET "aac-type=Yaml"
::@CALL :ExecuteAaCStrategy "dotnet EShop (%aac-type%)" "dotnet.eShop"
@IF "%_OUTPUT_CHANGED_%"=="1" CALL :ReportOutputChanged

@SET "aac-type=CSharp"
::@CALL :ExecuteAaCStrategy "dotnet EShop (%aac-type%)" "dotnet.eShop"
@IF "%_OUTPUT_CHANGED_%"=="1" CALL :ReportOutputChanged

@SET "aac-type=Json"
@CALL :ExecuteAaCStrategy "Internet Banking System (%aac-type%)" "Internet Banking System\CSV"
@IF "%_OUTPUT_CHANGED_%"=="1" CALL :ReportOutputChanged

@SET "aac-type=Yaml"
@CALL :ExecuteAaCStrategy "Internet Banking System (%aac-type%)" "Internet Banking System\CSV"
@IF "%_OUTPUT_CHANGED_%"=="1" CALL :ReportOutputChanged

@SET "aac-type=Json"
@CALL :ExecuteAaCStrategy "TraderX (%aac-type%)" "TraderX\CSV"
@IF "%_OUTPUT_CHANGED_%"=="1" CALL :ReportOutputChanged

@SET "aac-type=Yaml"
@CALL :ExecuteAaCStrategy "TraderX (%aac-type%)" "TraderX\CSV"
@IF "%_OUTPUT_CHANGED_%"=="1" CALL :ReportOutputChanged



@GOTO :EXIT




:SetESC
@for /F "tokens=1,2 delims=#" %%a in ('"prompt #$H#$E# & echo on & for %%b in (1) do rem"') do @(
  set ESC=%%b
  exit /B 0
)
@exit /B 0

:SetColors
@SET "COLOR_RESET=%ESC%[0m"
@SET "COLOR_FG_RED=%ESC%[91m"
@SET "COLOR_FG_GREEN=%ESC%[92m"
@SET "COLOR_FG_CYAN=%ESC%[94m"
@SET "ENABLE_LINE_DRAWING=%ESC%(0"
@SET "DISABLE_LINE_DRAWING=%ESC%(B"
@exit /B 0

:ExecuteAaCStrategy
:: Pad the name with enough whitespace to trim after
@SET "_SAMPLE_NAME_=%1                                                            "
@SET "_DIRECTORY_=%2"

:: Because the parameters will likely have spaces in them, they are passed in quotes.
:: So strip the quotes off because Batch files will pass the quotes as part of the parameter.
@SET "_SAMPLE_NAME_=%_SAMPLE_NAME_:"=%"
@SET "_DIRECTORY_=%_DIRECTORY_:"=%"

@ECHO.
:: See https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences?redirectedfrom=MSDN#designate-character-set to understand the DEC Line Drawing mode
@ECHO %COLOR_FG_GREEN%%ENABLE_LINE_DRAWING%lqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqk
@ECHO x    %DISABLE_LINE_DRAWING%!_SAMPLE_NAME_:~0,70!%ENABLE_LINE_DRAWING%    x
@ECHO mqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqj%DISABLE_LINE_DRAWING%%COLOR_RESET%
@CD %SCRIPT_DIRECTORY%!_DIRECTORY_!
@CALL "%SCRIPT_DIRECTORY%!_DIRECTORY_!\execute-aac-strategy.bat"
FOR /f "tokens=*" %%a in ('git status --short --porcelain --untracked-files=normal -- "%aac-output-path%"') DO SET _OUTPUT_CHANGED_=1
@exit /B 0

:ReportOutputChanged
@ECHO %COLOR_FG_RED%The files in '%aac-output-path%' has changed !!!!!%COLOR_RESET%
@IF "%stop_on_modified%"=="1" (
    @GOTO :EXIT
) else (
    @EXIT /B 0
)

:EXIT
:: Pause only if the script was opened from Explorer
@IF "%SCRIPT_IS_INTERACTIVE%"=="0" PAUSE
@ENDLOCAL
@POPD