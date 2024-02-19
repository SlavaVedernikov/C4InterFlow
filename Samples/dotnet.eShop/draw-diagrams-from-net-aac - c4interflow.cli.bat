@echo off
set "build-configuration=Debug"
set "aac-root-namespace=dotnet.eShop.Architecture"
set "aac-project-path=.\%aac-root-namespace%\%aac-root-namespace%.csproj"
set "aac-output-dir=.\%aac-root-namespace%\bin\%build-configuration%\net6.0"
set "diagrams-dir=.\%aac-root-namespace%\Diagrams"
set "aac-reader-strategy=C4InterFlow.Automation.Readers.CSharpAaCReaderStrategy,C4InterFlow.Automation"
set "aac-input-paths=.\%aac-output-dir%\%aac-root-namespace%.dll"
:: set "cli-exe=..\..\C4InterFlow.Cli\bin\Debug\net6.0\C4InterFlow.Cli.exe"
set "cli-exe=..\..\C4InterFlow.Cli\bin\Release\net6.0\publish\win-x86\C4InterFlow.Cli.exe"

dotnet publish %aac-project-path% --configuration %build-configuration% --output %aac-output-dir%

echo Clearing diagrams...
pause
powershell.exe -Command "if (Test-Path '%diagrams-dir%\*') { Get-ChildItem -Path '%diagrams-dir%' -Recurse | Where-Object { $_.Extension -eq '.puml' } | ForEach-Object { Remove-Item -Path $_.FullName -Force } }"
:: powershell.exe -Command "if (Test-Path '%diagrams-dir%\*') { Remove-Item -Path '%diagrams-dir%\*' -Recurse -Force }"

echo Draw Diagrams with '%aac-reader-strategy%' AaC reader strategy and '%aac-input-paths%' AaC input path
pause

echo Drawing Diagrams...
%cli-exe% draw-diagrams --interfaces  %aac-root-namespace%.SoftwareSystems.*.Containers.*.Components.*.Interfaces.* --aac-reader-strategy "%aac-reader-strategy%" --aac-input-paths "%aac-input-paths%" --output-dir "%diagrams-dir%"

:: --formats png svg

pause   
