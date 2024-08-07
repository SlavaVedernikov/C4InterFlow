@echo off
:: Possible values: TRUE, FALSE
set "redraw-all=TRUE"
:::::::::::::::::::::::::::::::

set "aac-root-namespace=ToDoAppExample"
set "cli-exe=C:\\C4InterFlow.Cli.exe"
set "diagrams-dir=.\Diagrams"
set "aac-reader-strategy=C4InterFlow.Automation.Readers.YamlAaCReaderStrategy,C4InterFlow.Automation"
set "aac-input-paths=.\Architecture"

echo redraw-all: %redraw-all%
echo aac-root-namespace: %aac-root-namespace%
echo cli-output-dir: %cli-output-dir%
echo cli-exe: %cli-exe%
echo diagrams-dir: %diagrams-dir%
echo aac-reader-strategy: %aac-reader-strategy%
echo aac-input-paths: %aac-input-paths%

if not %redraw-all%==TRUE if not %redraw-all%==FALSE (
    echo ERROR: 'redraw-all' can only be set either to 'TRUE' or 'FALSE'. Edit script and re-run.
    pause
    goto end
)

echo Check the above settings.
pause

echo Clearing diagrams...
:: pause
if %redraw-all%==TRUE (
powershell.exe -Command "if (Test-Path '%diagrams-dir%\*') { Remove-Item -Path '%diagrams-dir%\*' -Recurse -Force }"
) else (
powershell.exe -Command "if (Test-Path '%diagrams-dir%\*') { Get-ChildItem -Path '%diagrams-dir%' -Recurse | Where-Object { $_.Extension -eq '.puml' } | ForEach-Object { Remove-Item -Path $_.FullName -Force } }"
)

echo Draw Diagrams with '%aac-reader-strategy%' AaC reader strategy and '%aac-input-paths%' AaC input path
pause

echo Drawing Diagrams...
if %redraw-all%==TRUE (
%cli-exe% draw-diagrams --interfaces  %aac-root-namespace%.SoftwareSystems.*.Containers.*.Interfaces.* --levels-of-details context container --aac-reader-strategy "%aac-reader-strategy%" --aac-input-paths "%aac-input-paths%" --output-dir "%diagrams-dir%" --formats png svg
) else (
%cli-exe% draw-diagrams --interfaces  %aac-root-namespace%.SoftwareSystems.*.Containers.*.Interfaces.* --levels-of-details context container --aac-reader-strategy "%aac-reader-strategy%" --aac-input-paths "%aac-input-paths%" --output-dir "%diagrams-dir%" 
)
pause 
:end  