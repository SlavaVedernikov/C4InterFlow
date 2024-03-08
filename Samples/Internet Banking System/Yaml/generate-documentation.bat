@echo off

set "build-configuration=Debug"
set "cli-project-path=..\..\..\C4InterFlow.Cli\C4InterFlow.Cli.csproj"
set "cli-output-dir=..\..\..\C4InterFlow.Cli\bin\%build-configuration%\net6.0\win-x64"
set "cli-exe=C4InterFlow.Cli.exe"
set "output-dir=.\Diagrams"
set "file-extension=md"
set "templates-dir=.\Templates"
set "aac-reader-strategy=C4InterFlow.Automation.Readers.YamlAaCReaderStrategy,C4InterFlow.Automation"
set "aac-input-paths=.\Architecture"

echo cli-output-dir: %cli-output-dir%
echo cli-exe: %cli-exe%
echo output-dir: %output-dir%
echo file-extension: %file-extension%
echo templates-dir: %templates-dir%
echo aac-reader-strategy: %aac-reader-strategy%
echo aac-input-paths: %aac-input-paths%

echo Check the above settings.
pause

:: Publish
echo Publishing...
:: pause
dotnet publish %cli-project-path% --configuration %build-configuration% --output %cli-output-dir%

echo Clearing documentation...
:: pause
powershell.exe -Command "if (Test-Path '%output-dir%\*') { Get-ChildItem -Path '%output-dir%' -Recurse | Where-Object { $_.Extension -eq '.%file-extension%' } | ForEach-Object { Remove-Item -Path $_.FullName -Force } }"

echo Generate '.%file-extension%' documentation files with templates at '%templates-dir%', '%aac-reader-strategy%' AaC reader strategy and '%aac-input-paths%' AaC input path
pause

echo Generating Documentation...
%cli-output-dir%\%cli-exe% generate-documentation --structures BigBankPlc.SoftwareSystems.* BigBankPlc.SoftwareSystems.*.Containers.* --templates-dir "%templates-dir%" --aac-reader-strategy "%aac-reader-strategy%" --aac-input-paths "%aac-input-paths%" --output-dir "%output-dir%" --file-extension "%file-extension%"
pause 
:end  