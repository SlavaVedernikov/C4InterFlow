@echo off
:: Possible values: TRUE, FALSE
if not defined redraw-all set "redraw-all=TRUE"
:::::::::::::::::::::::::::::::

if not defined build-configuration set "build-configuration=Debug"
set "aac-root-namespace=ToDoAppExample"
set "aac-project-name=ToDoAppExample"
set "cli-project-path=.\%aac-project-name%.Cli\%aac-project-name%.Cli.csproj"
set "cli-output-dir=.\%aac-project-name%.Cli\bin\%build-configuration%\net6.0"
set "cli-exe=%aac-project-name%.Cli.exe"
set "diagrams-dir=.\Diagrams"
set "aac-reader-strategy=C4InterFlow.Automation.Readers.CSharpAaCReaderStrategy,C4InterFlow.Automation"
set "aac-input-paths=%aac-project-name%.dll"

call :NormalizePath %cli-project-path%
set "cli-project-path=%_NORMALIZED_PATH_%"
call :NormalizePath %cli-output-dir%
set "cli-output-dir=%_NORMALIZED_PATH_%"

call :NormalizePath %diagrams-dir%
set "diagrams-dir=%_NORMALIZED_PATH_%"

echo redraw-all          : %redraw-all%
echo build-configuration : %build-configuration%
echo aac-root-namespace  : %aac-root-namespace%
echo aac-project-name    : %aac-project-name%
echo cli-project-path    : %cli-project-path%
echo cli-output-dir      : %cli-output-dir%
echo cli-exe             : %cli-exe%
echo diagrams-dir        : %diagrams-dir%
echo aac-reader-strategy : %aac-reader-strategy%
echo aac-input-paths     : %aac-input-paths%

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
dotnet publish %cli-project-path% --configuration %build-configuration% --output %cli-output-dir% --verbosity quiet --property:WarningLevel=0

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
%cli-output-dir%\%cli-exe% draw-diagrams --interfaces  %aac-root-namespace%.SoftwareSystems.*.Containers.*.Interfaces.* --levels-of-details container --aac-reader-strategy "%aac-reader-strategy%" --aac-input-paths "%aac-input-paths%" --output-dir "%diagrams-dir%" --formats png svg
) else (
%cli-output-dir%\%cli-exe% draw-diagrams --interfaces  %aac-root-namespace%.SoftwareSystems.*.Containers.*.Interfaces.* --levels-of-details container --aac-reader-strategy "%aac-reader-strategy%" --aac-input-paths "%aac-input-paths%" --output-dir "%diagrams-dir%" 
)
if NOT "%BATCH_TEST_MODE%"=="1" pause
@GOTO :end

:NormalizePath
@set _NORMALIZED_PATH_=%~f1
@EXIT /B 0

:end  