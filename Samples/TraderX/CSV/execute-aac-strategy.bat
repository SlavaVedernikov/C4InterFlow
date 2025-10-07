@echo off

if not defined aac-type set "aac-type=Json"

if not defined build-configuration set "build-configuration=Debug"
set "aac-root-namespace=TraderXExample"
set "aac-project-name=Architecture"
set "cli-project-path=..\..\..\C4InterFlow.Cli\C4InterFlow.Cli.csproj"
set "cli-output-dir=..\..\..\C4InterFlow.Cli\bin\%build-configuration%\net9.0"
set "cli-exe=C4InterFlow.Cli.exe"
set "aac-input-path=Architecture Catalogue - CSV Export"
set "aac-output-path=%aac-project-name%\%aac-type%"
set "aac-writer-strategy=C4InterFlow.Automation.Writers.CsvTo%aac-type%AaCGenerator,C4InterFlow.Automation"

if not %aac-type%==Json if not %aac-type%==Yaml (
    echo ERROR: 'aac-type' can only be set either to 'Json' or 'Yaml'. Edit script and re-run.
    pause
    goto end
)

CALL :NormalizePath %cli-project-path%
SET "cli-project-path=%_NORMALIZED_PATH_%"
CALL :NormalizePath %cli-project-path%
SET "cli-project-path=%_NORMALIZED_PATH_%"
CALL :NormalizePath %cli-output-dir%
SET "cli-output-dir=%_NORMALIZED_PATH_%"

if "%ENABLE_LINE_DRAWING%"=="" (
    echo build-configuration : %build-configuration%
    echo aac-root-namespace  : %aac-root-namespace%
    echo aac-input-path      : %aac-input-path%
    echo aac-output-path     : %aac-output-path%
    echo aac-writer-strategy : %aac-writer-strategy%
    echo cli-project-path    : %cli-project-path%
    echo cli-output-dir      : %cli-output-dir%
    echo cli-exe             : %cli-exe%
) else (
    @ECHO %ENABLE_LINE_DRAWING%lqqqqqqqqqqqqqqqqqqqqqwqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqk
    echo x %DISABLE_LINE_DRAWING%build-configuration %ENABLE_LINE_DRAWING%x %DISABLE_LINE_DRAWING%%build-configuration%%ENABLE_LINE_DRAWING%
    echo x %DISABLE_LINE_DRAWING%aac-root-namespace  %ENABLE_LINE_DRAWING%x %DISABLE_LINE_DRAWING%%aac-root-namespace%%ENABLE_LINE_DRAWING%
    echo x %DISABLE_LINE_DRAWING%aac-input-path      %ENABLE_LINE_DRAWING%x %DISABLE_LINE_DRAWING%%aac-input-path%%ENABLE_LINE_DRAWING%
    echo x %DISABLE_LINE_DRAWING%aac-output-path     %ENABLE_LINE_DRAWING%x %DISABLE_LINE_DRAWING%%aac-output-path%%ENABLE_LINE_DRAWING%
    echo x %DISABLE_LINE_DRAWING%aac-writer-strategy %ENABLE_LINE_DRAWING%x %DISABLE_LINE_DRAWING%%aac-writer-strategy%%ENABLE_LINE_DRAWING%
    echo x %DISABLE_LINE_DRAWING%cli-project-path    %ENABLE_LINE_DRAWING%x %DISABLE_LINE_DRAWING%%cli-project-path%%ENABLE_LINE_DRAWING%
    echo x %DISABLE_LINE_DRAWING%cli-output-dir      %ENABLE_LINE_DRAWING%x %DISABLE_LINE_DRAWING%%cli-output-dir%%ENABLE_LINE_DRAWING%
    echo x %DISABLE_LINE_DRAWING%cli-exe             %ENABLE_LINE_DRAWING%x %DISABLE_LINE_DRAWING%%cli-exe%%ENABLE_LINE_DRAWING%
    @ECHO mqqqqqqqqqqqqqqqqqqqqqvqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqj%DISABLE_LINE_DRAWING%%COLOR_RESET%
)


if NOT "%BATCH_TEST_MODE%"=="1" (
    echo Check the above settings.
    pause
)

:: Publish
echo Publishing...
dotnet publish %cli-project-path% --configuration %build-configuration% --output %cli-output-dir% --verbosity quiet


:: Clear AaC
echo Clearing AaC at '%aac-output-path%'...
powershell.exe -Command "if (Test-Path '%aac-output-path%\*') { Remove-Item -Path '%aac-output-path%\*' -Recurse -Force }"
echo AaC (%aac-output-path%) is cleared

:: Execute AaC Strategy
echo Executing AaC Strategy using '%aac-writer-strategy%' with '%aac-output-path%' output directory...
if NOT "%BATCH_TEST_MODE%"=="1" pause
%cli-output-dir%\%cli-exe% execute-aac-strategy --aac-root-namespace "%aac-root-namespace%" --aac-output-path "%aac-output-path%" --aac-writer-strategy "%aac-writer-strategy%" --params aac-input-path="%aac-input-path%"

if NOT "%BATCH_TEST_MODE%"=="1" pause
@GOTO :end

:NormalizePath
@SET _NORMALIZED_PATH_=%~f1
@EXIT /B 0

:end  