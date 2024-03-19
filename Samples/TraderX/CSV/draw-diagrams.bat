@echo off
:: Possible values: TRUE, FALSE
set "redraw-all=FALSE"
:::::::::::::::::::::::::::::::

set "build-configuration=Debug"
set "aac-root-namespace=TraderX"
set "cli-project-path=..\..\..\C4InterFlow.Cli\C4InterFlow.Cli.csproj"
set "cli-output-dir=..\..\..\C4InterFlow.Cli\bin\%build-configuration%\net6.0\win-x64"
set "cli-exe=C4InterFlow.Cli.exe"
set "diagrams-dir=.\Diagrams"
set "aac-reader-strategy=C4InterFlow.Automation.Readers.YamlAaCReaderStrategy,C4InterFlow.Automation"
set "aac-input-paths=.\Architecture\Yaml"

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

:: Publish
echo Publishing...
:: pause
dotnet publish %cli-project-path% --configuration %build-configuration% --output %cli-output-dir%

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
%cli-output-dir%\%cli-exe% draw-diagrams --interfaces  %aac-root-namespace%.SoftwareSystems.*.Containers.*.Interfaces.* --business-processes %aac-root-namespace%.BusinessProcesses.* --levels-of-details context container --aac-reader-strategy "%aac-reader-strategy%" --aac-input-paths "%aac-input-paths%" --output-dir "%diagrams-dir%" --formats png svg
) else (
%cli-output-dir%\%cli-exe% draw-diagrams --interfaces  %aac-root-namespace%.SoftwareSystems.*.Containers.*.Interfaces.* --business-processes %aac-root-namespace%.BusinessProcesses.* --levels-of-details context container --aac-reader-strategy "%aac-reader-strategy%" --aac-input-paths "%aac-input-paths%" --output-dir "%diagrams-dir%" 
)
pause 
:end  