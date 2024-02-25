@echo off

set "build-configuration=Debug"
set "aac-root-namespace=BigBankPlc"
set "aac-project-name=Architecture"
set "cli-project-path=..\..\..\C4InterFlow.Cli\C4InterFlow.Cli.csproj"
set "cli-output-dir=..\..\..\C4InterFlow.Cli\bin\%build-configuration%\net6.0"
set "cli-exe=C4InterFlow.Cli.exe"
set "aac-input-path=%aac-project-name%\Csv"

echo build-configuration: %build-configuration%
echo aac-root-namespace: %aac-root-namespace%
echo aac-input-path: %aac-input-path%
echo aac-output-path: %aac-output-path%
echo cli-project-path: %cli-project-path%
echo cli-output-dir: %cli-output-dir%
echo cli-exe: %cli-exe%


echo Check the above settings.
pause
:: Publish
echo Publishing...
:: pause
dotnet publish %cli-project-path% --configuration %build-configuration% --output %cli-output-dir%

set "aac-output-path=%aac-project-name%\Json"
set "aac-writer-strategy=C4InterFlow.Automation.Writers.CsvToJsonAaCGenerator,C4InterFlow.Automation"

:: Clear AaC
echo Clearing AaC at '%aac-output-path%'...
:: pause

powershell.exe -Command "if (Test-Path '%aac-project-name%\Yaml\*') { Remove-Item -Path '%aac-project-name%\Yaml\*' -Recurse -Force }"

echo AaC is cleared
:: pause

:: Execute AaC Strategy
echo Executing AaC Strategy using '%aac-writer-strategy%' with '%aac-output-path%' output directory...
pause
%cli-output-dir%\%cli-exe% execute-aac-strategy --aac-root-namespace "%aac-root-namespace%" --aac-output-path "%aac-output-path%" --aac-writer-strategy "%aac-writer-strategy%" --params aac-input-path="%aac-input-path%"

set "aac-output-path=%aac-project-name%\Yaml"
set "aac-writer-strategy=C4InterFlow.Automation.Writers.CsvToYamlAaCGenerator,C4InterFlow.Automation"

:: Clear AaC
echo Clearing AaC at '%aac-output-path%'...
:: pause

powershell.exe -Command "if (Test-Path '%aac-project-name%\Yaml\*') { Remove-Item -Path '%aac-project-name%\Yaml\*' -Recurse -Force }"

echo AaC is cleared
:: pause

:: Execute AaC Strategy
echo Executing AaC Strategy using '%aac-writer-strategy%' with '%aac-output-path%' output directory...
pause
%cli-output-dir%\%cli-exe% execute-aac-strategy --aac-root-namespace "%aac-root-namespace%" --aac-output-path "%aac-output-path%" --aac-writer-strategy "%aac-writer-strategy%" --params aac-input-path="%aac-input-path%"

pause