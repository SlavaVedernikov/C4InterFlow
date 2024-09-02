@echo off
:: Possible values: TRUE, FALSE
set "redraw-all=TRUE"
:::::::::::::::::::::::::::::::

:: Possible values: CSharp, Yaml
set "aac-type=Yaml"
::::::::::::::::::::::::::::::::

set "build-configuration=Debug"
set "aac-root-namespace=DotNetEShop"
set "aac-project-name=DotNetEShop"
set "cli-project-path=.\%aac-project-name%.Cli\%aac-project-name%.Cli.csproj"
set "cli-output-dir=.\%aac-project-name%.Cli\bin\%build-configuration%\net6.0"
set "cli-exe=%aac-project-name%.Cli.exe"
set "diagrams-dir=%aac-project-name%\Diagrams"
set "aac-reader-strategy=C4InterFlow.Automation.Readers.%aac-type%AaCReaderStrategy,C4InterFlow.Automation"

if %aac-type%==CSharp set "aac-input-paths=%aac-project-name%.dll"
if %aac-type%==Yaml set "aac-input-paths=%aac-project-name%\Yaml"

echo redraw-all: %redraw-all%
echo aac-type: %aac-type%
echo build-configuration: %build-configuration%
echo aac-root-namespace: %aac-root-namespace%
echo aac-project-name: %aac-project-name%
echo cli-project-path: %cli-project-path%
echo cli-output-dir: %cli-output-dir%
echo cli-exe: %cli-exe%
echo diagrams-dir: %diagrams-dir%
echo aac-reader-strategy: %aac-reader-strategy%
echo aac-input-paths: %aac-input-paths%

if not %aac-type%==CSharp if not %aac-type%==Yaml (
    echo ERROR: 'aac-type' can only be set either to 'CSharp' or 'Yaml'. Edit script and re-run.
    pause
    goto end
)

if not %redraw-all%==TRUE if not %redraw-all%==FALSE (
    echo ERROR: 'redraw-all' can only be set either to 'TRUE' or 'FALSE'. Edit script and re-run.
    pause
    goto end
)

echo Check the above settings.
pause

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
%cli-output-dir%\%cli-exe% draw-diagrams --interfaces  %aac-root-namespace%.SoftwareSystems.*.Containers.*.Components.*.Interfaces.* --aac-reader-strategy "%aac-reader-strategy%" --aac-input-paths "%aac-input-paths%" --output-dir "%diagrams-dir%" --formats png svg
) else (
%cli-output-dir%\%cli-exe% draw-diagrams --interfaces  %aac-root-namespace%.SoftwareSystems.*.Containers.*.Components.*.Interfaces.* --aac-reader-strategy "%aac-reader-strategy%" --aac-input-paths "%aac-input-paths%" --output-dir "%diagrams-dir%" 
)
pause 
:end  