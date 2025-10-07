@echo off
:: Possible values: TRUE, FALSE
if not defined redraw-all set "redraw-all=FALSE"
:::::::::::::::::::::::::::::::

if not defined build-configuration set "build-configuration=Debug"
set "aac-root-namespace=ToDoAppExample"
set "cli-project-path=..\..\..\..\..\C4InterFlow.Cli\C4InterFlow.Cli.csproj"
set "cli-output-dir=..\..\..\..\..\C4InterFlow.Cli\bin\%build-configuration%\net9.0\win-x64"
set "cli-exe=C4InterFlow.Cli.exe"
set "diagrams-dir=..\..\Diagrams\Future"
set "aac-reader-strategy=C4InterFlow.Automation.Readers.YamlAaCReaderStrategy,C4InterFlow.Automation"
set "aac-input-path-current=..\..\Architecture\Current"
set "aac-input-path-future=..\..\Architecture\Future"

CALL :NormalizePath %cli-project-path%
SET "cli-project-path=%_NORMALIZED_PATH_%"
CALL :NormalizePath %cli-output-dir%
SET "cli-output-dir=%_NORMALIZED_PATH_%"
CALL :NormalizePath %aac-input-path-current%
SET "aac-input-path-current=%_NORMALIZED_PATH_%"
CALL :NormalizePath %aac-input-path-future%
SET "aac-input-path-future=%_NORMALIZED_PATH_%"
CALL :NormalizePath %diagrams-dir%
SET "diagrams-dir=%_NORMALIZED_PATH_%"

if "%ENABLE_LINE_DRAWING%"=="" (
    echo redraw-all                 : %redraw-all%
    echo aac-root-namespace         : %aac-root-namespace%
    echo cli-project-path           : %cli-project-path%
    echo cli-output-dir             : %cli-output-dir%
    echo cli-exe                    : %cli-exe%
    echo diagrams-dir               : %diagrams-dir%
    echo aac-reader-strategy        : %aac-reader-strategy%
    echo aac-input-path-current     : %aac-input-path-current%
    echo aac-input-path-future      : %aac-input-path-future%
) else (
    @ECHO %ENABLE_LINE_DRAWING%lqqqqqqqqqqqqqqqqqqqqqqwqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqk
    @ECHO x %DISABLE_LINE_DRAWING%redraw-all                 %ENABLE_LINE_DRAWING% x  %DISABLE_LINE_DRAWING%!redraw-all!%ENABLE_LINE_DRAWING%
    @ECHO x %DISABLE_LINE_DRAWING%aac-root-namespace         %ENABLE_LINE_DRAWING% x  %DISABLE_LINE_DRAWING%!aac-root-namespace!%ENABLE_LINE_DRAWING%
    @ECHO x %DISABLE_LINE_DRAWING%cli-project-path           %ENABLE_LINE_DRAWING% x  %DISABLE_LINE_DRAWING%!cli-project-path!%ENABLE_LINE_DRAWING%
    @ECHO x %DISABLE_LINE_DRAWING%cli-output-dir             %ENABLE_LINE_DRAWING% x  %DISABLE_LINE_DRAWING%!cli-output-dir!%ENABLE_LINE_DRAWING%
    @ECHO x %DISABLE_LINE_DRAWING%cli-exe                    %ENABLE_LINE_DRAWING% x  %DISABLE_LINE_DRAWING%!cli-exe!%ENABLE_LINE_DRAWING%
    @ECHO x %DISABLE_LINE_DRAWING%diagrams-dir               %ENABLE_LINE_DRAWING% x  %DISABLE_LINE_DRAWING%!diagrams-dir!%ENABLE_LINE_DRAWING%
    @ECHO x %DISABLE_LINE_DRAWING%aac-reader-strategy        %ENABLE_LINE_DRAWING% x  %DISABLE_LINE_DRAWING%!aac-reader-strategy!%ENABLE_LINE_DRAWING%
    @ECHO x %DISABLE_LINE_DRAWING%aac-input-path-current     %ENABLE_LINE_DRAWING% x  %DISABLE_LINE_DRAWING%!aac-input-path-current!%ENABLE_LINE_DRAWING%
    @ECHO x %DISABLE_LINE_DRAWING%aac-input-path-future      %ENABLE_LINE_DRAWING% x  %DISABLE_LINE_DRAWING%!aac-input-path-future!%ENABLE_LINE_DRAWING%
    @ECHO mqqqqqqqqqqqqqqqqqqqqqqvqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqj%DISABLE_LINE_DRAWING%%COLOR_RESET%
)

if not %redraw-all%==TRUE if not %redraw-all%==FALSE (
    echo ERROR: 'redraw-all' can only be set either to 'TRUE' or 'FALSE'. Edit script and re-run.
    pause
    goto end
)

if NOT "%BATCH_TEST_MODE%"=="1" (
    echo Check the above settings.
    pause
)

:: Publish
echo Publishing...
:: pause
dotnet publish %cli-project-path% --configuration %build-configuration% --output %cli-output-dir% --verbosity quiet

echo Clearing diagrams...
:: pause
if %redraw-all%==TRUE (
powershell.exe -Command "if (Test-Path '%diagrams-dir%\*') { Remove-Item -Path '%diagrams-dir%\*' -Recurse -Force }"
) else (
powershell.exe -Command "if (Test-Path '%diagrams-dir%\*') { Get-ChildItem -Path '%diagrams-dir%' -Recurse | Where-Object { $_.Extension -eq '.puml' } | ForEach-Object { Remove-Item -Path $_.FullName -Force } }"
)

echo Draw Diagrams with '%aac-reader-strategy%' AaC reader strategy and '%aac-input-path-current%' and '%aac-input-path-future%' AaC input paths
if NOT "%BATCH_TEST_MODE%"=="1" pause

echo Drawing Diagrams...
if %redraw-all%==TRUE (
%cli-output-dir%\%cli-exe% draw-diagrams --interfaces  %aac-root-namespace%.SoftwareSystems.*.Containers.*.Interfaces.* --levels-of-details container --aac-reader-strategy "%aac-reader-strategy%" --aac-input-paths "%aac-input-path-current%" "%aac-input-path-future%" --output-dir "%diagrams-dir%" --formats svg
) else (
%cli-output-dir%\%cli-exe% draw-diagrams --interfaces  %aac-root-namespace%.SoftwareSystems.*.Containers.*.Interfaces.* --levels-of-details container --aac-reader-strategy "%aac-reader-strategy%" --aac-input-paths "%aac-input-path-current%" "%aac-input-path-future%" --output-dir "%diagrams-dir%" 
)
if NOT "%BATCH_TEST_MODE%"=="1" pause
@GOTO :end

:NormalizePath
@SET _NORMALIZED_PATH_=%~f1
@EXIT /B 0

:end  