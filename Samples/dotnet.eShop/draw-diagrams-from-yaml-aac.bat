@echo off
set "build-configuration=Debug"
set "aac-root-namespace=dotnet.eShop.Architecture"
set "cli-project-path=.\%aac-root-namespace%.Cli\%aac-root-namespace%.Cli.csproj"
set "cli-output-dir=.\%aac-root-namespace%.Cli\bin\%build-configuration%\net6.0"
set "cli-exe=%aac-root-namespace%.Cli.exe"
set "diagrams-dir=%aac-root-namespace%\Diagrams"
set "aac-reader-strategy=C4InterFlow.Automation.Readers.YamlAaCReaderStrategy,C4InterFlow.Automation"
set "aac-input-paths=%aac-root-namespace%\Yaml"

dotnet publish %cli-project-path% --configuration %build-configuration% --output %cli-output-dir%

echo Clearing diagrams...
:: pause
:: powershell.exe -Command "if (Test-Path '%diagrams-dir%\*') { Get-ChildItem -Path '%diagrams-dir%' -Recurse | Where-Object { $_.Extension -eq '.puml' } | ForEach-Object { Remove-Item -Path $_.FullName -Force } }"
powershell.exe -Command "if (Test-Path '%diagrams-dir%\*') { Remove-Item -Path '%diagrams-dir%\*' -Recurse -Force }"

echo Draw Diagrams with '%aac-reader-strategy%' AaC reader strategy and '%aac-input-paths%' AaC input path
:: pause

echo Drawing Diagrams...
%cli-output-dir%\%cli-exe% draw-diagrams --interfaces  %aac-root-namespace%.SoftwareSystems.*.Containers.*.Components.*.Interfaces.* --aac-reader-strategy "%aac-reader-strategy%" --aac-input-paths "%aac-input-paths%" --output-dir "%diagrams-dir%"

pause   