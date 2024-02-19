@echo off
set "aac-root-namespace=dotnet.eShop.Architecture"
set "diagrams-dir=%aac-root-namespace%\Diagrams"
set "aac-reader-strategy=C4InterFlow.Automation.Readers.YamlAaCReaderStrategy,C4InterFlow.Automation"
set "aac-input-paths=%aac-root-namespace%\Yaml"
:: set "cli-exe=..\..\C4InterFlow.Cli\bin\Debug\net6.0\C4InterFlow.Cli.exe"
set "cli-exe=..\..\C4InterFlow.Cli\bin\Release\net6.0\publish\win-x86\C4InterFlow.Cli.exe"

echo Clearing diagrams...
:: pause
powershell.exe -Command "if (Test-Path '%diagrams-dir%\*') { Get-ChildItem -Path '%diagrams-dir%' -Recurse | Where-Object { $_.Extension -eq '.puml' } | ForEach-Object { Remove-Item -Path $_.FullName -Force } }"
:: powershell.exe -Command "if (Test-Path '%diagrams-dir%\*') { Remove-Item -Path '%diagrams-dir%\*' -Recurse -Force }"

echo Draw Diagrams with '%aac-reader-strategy%' AaC reader strategy and '%aac-input-paths%' AaC input path
:: pause

echo Drawing Diagrams...
%cli-exe% draw-diagrams --interfaces  %aac-root-namespace%.SoftwareSystems.*.Containers.*.Components.*.Interfaces.* --aac-reader-strategy "%aac-reader-strategy%" --aac-input-paths "%aac-input-paths%" --output-dir "%diagrams-dir%"

pause   