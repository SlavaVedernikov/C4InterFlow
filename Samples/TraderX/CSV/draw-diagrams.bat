@echo off
:: Possible values: TRUE, FALSE
if not defined redraw-all set "redraw-all=TRUE"
:::::::::::::::::::::::::::::::

if not defined build-configuration set "build-configuration=Debug"
set "aac-root-namespace=TraderXExample"
set "cli-project-path=..\..\..\C4InterFlow.Cli\C4InterFlow.Cli.csproj"
set "cli-output-dir=..\..\..\C4InterFlow.Cli\bin\%build-configuration%\net6.0\win-x64"
set "cli-exe=C4InterFlow.Cli.exe"
set "diagrams-dir=.\Diagrams"
set "aac-reader-strategy=C4InterFlow.Automation.Readers.YamlAaCReaderStrategy,C4InterFlow.Automation"
set "aac-input-paths=.\Architecture\Yaml"

CALL :NormalizePath %cli-project-path%
SET "cli-project-path=%_NORMALIZED_PATH_%"
CALL :NormalizePath %cli-output-dir%
SET "cli-output-dir=%_NORMALIZED_PATH_%"
CALL :NormalizePath %aac-input-paths%
SET "aac-input-paths=%_NORMALIZED_PATH_%"
CALL :NormalizePath %diagrams-dir%
SET "diagrams-dir=%_NORMALIZED_PATH_%"

if "%ENABLE_LINE_DRAWING%"=="" (
    echo redraw-all          : %redraw-all%
    echo aac-root-namespace  : %aac-root-namespace%
    echo cli-output-dir      : %cli-output-dir%
    echo cli-exe             : %cli-exe%
    echo diagrams-dir        : %diagrams-dir%
    echo aac-reader-strategy : %aac-reader-strategy%
    echo aac-input-paths     : %aac-input-paths%
) else (
    @ECHO %ENABLE_LINE_DRAWING%lqqqqqqqqqqqqqqqqqqqqqqwqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqk
    @ECHO x %DISABLE_LINE_DRAWING%redraw-all          %ENABLE_LINE_DRAWING% x  %DISABLE_LINE_DRAWING%!redraw-all!%ENABLE_LINE_DRAWING%
    @ECHO x %DISABLE_LINE_DRAWING%aac-root-namespace  %ENABLE_LINE_DRAWING% x  %DISABLE_LINE_DRAWING%!aac-root-namespace!%ENABLE_LINE_DRAWING%
    @ECHO x %DISABLE_LINE_DRAWING%cli-output-dir      %ENABLE_LINE_DRAWING% x  %DISABLE_LINE_DRAWING%!cli-output-dir!%ENABLE_LINE_DRAWING%
    @ECHO x %DISABLE_LINE_DRAWING%cli-exe             %ENABLE_LINE_DRAWING% x  %DISABLE_LINE_DRAWING%!cli-exe!%ENABLE_LINE_DRAWING%
    @ECHO x %DISABLE_LINE_DRAWING%diagrams-dir        %ENABLE_LINE_DRAWING% x  %DISABLE_LINE_DRAWING%!diagrams-dir!%ENABLE_LINE_DRAWING%
    @ECHO x %DISABLE_LINE_DRAWING%aac-reader-strategy %ENABLE_LINE_DRAWING% x  %DISABLE_LINE_DRAWING%!aac-reader-strategy!%ENABLE_LINE_DRAWING%
    @ECHO x %DISABLE_LINE_DRAWING%aac-input-paths     %ENABLE_LINE_DRAWING% x  %DISABLE_LINE_DRAWING%!aac-input-paths!%ENABLE_LINE_DRAWING%
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

echo Draw Diagrams with '%aac-reader-strategy%' AaC reader strategy and '%aac-input-paths%' AaC input path
if NOT "%BATCH_TEST_MODE%"=="1" pause

echo Drawing Diagrams...
if %redraw-all%==TRUE (
%cli-output-dir%\%cli-exe% draw-diagrams --interfaces  %aac-root-namespace%.SoftwareSystems.*.Containers.*.Interfaces.* --business-processes %aac-root-namespace%.BusinessProcesses.* --levels-of-details context container --aac-reader-strategy "%aac-reader-strategy%" --aac-input-paths "%aac-input-paths%" --output-dir "%diagrams-dir%" --formats png svg
) else (
%cli-output-dir%\%cli-exe% draw-diagrams --interfaces  %aac-root-namespace%.SoftwareSystems.*.Containers.*.Interfaces.* --business-processes %aac-root-namespace%.BusinessProcesses.* --levels-of-details context container --aac-reader-strategy "%aac-reader-strategy%" --aac-input-paths "%aac-input-paths%" --output-dir "%diagrams-dir%" 
)
if NOT "%BATCH_TEST_MODE%"=="1" pause
@GOTO :end

:NormalizePath
@SET _NORMALIZED_PATH_=%~f1
@EXIT /B 0

:end  