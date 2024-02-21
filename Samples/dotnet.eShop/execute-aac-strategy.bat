@echo off

:: Set net-sourcecode-dir to the directory path where E-Shop repo was cloned to
set "net-sourcecode-dir=C:\Data\Projects\C4InterFlow\eShop-main"
:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

:: Possible values: CSharp, Yaml
set "aac-type=Yaml"
::::::::::::::::::::::::::::::::

set "build-configuration=Debug"
set "aac-root-namespace=dotnet.eShop.Architecture"
set "aac-project-name=dotnet.eShop.Architecture"
set "cli-project-path=.\%aac-project-name%.Cli\%aac-project-name%.Cli.csproj"
set "cli-output-dir=.\%aac-project-name%.Cli\bin\%build-configuration%\net6.0"
set "cli-exe=%aac-project-name%.Cli.exe"

if %aac-type%==CSharp set "aac-output-path=%aac-project-name%\%aac-project-name%.csproj"
if %aac-type%==Yaml set "aac-output-path=%aac-project-name%\Yaml"

echo aac-type: %aac-type%
echo net-sourcecode-dir: %net-sourcecode-dir%
echo build-configuration: %build-configuration%
echo aac-root-namespace: %aac-root-namespace%
echo aac-project-name: %aac-project-name%
echo cli-project-path: %cli-project-path%
echo cli-output-dir: %cli-output-dir%
echo cli-exe: %cli-exe%

if not %aac-type%==CSharp if not %aac-type%==Yaml (
    echo ERROR: 'aac-type' can only be set either to 'CSharp' or 'Yaml'. Edit script and re-run.
    pause
    goto end
)

if not exist %net-sourcecode-dir% (
    echo ERROR: net-sourcecode-dir is set to '%net-sourcecode-dir%', but directory '%net-sourcecode-dir%' does not exist. Edit script and re-run.
    pause
    goto end
)

echo Check the above settings.
pause
:: Publish
echo Publishing...
:: pause
dotnet publish %cli-project-path% --configuration %build-configuration% --output %cli-output-dir%

:: Clear AaC
echo Clearing AaC...
:: pause
if %aac-type%==CSharp (
powershell.exe -Command "if (Test-Path '%aac-project-name%\SoftwareSystems\*') { Remove-Item -Path '%aac-project-name%\SoftwareSystems\*' -Recurse -Force }"
powershell.exe -Command "if (Test-Path '%aac-project-name%\Actors\*') { Remove-Item -Path '%aac-project-name%\Actors\*' -Recurse -Force }"
powershell.exe -Command "if (Test-Path '%aac-project-name%\BusinessProcesses\*') { Remove-Item -Path '%aac-project-name%\BusinessProcesses\*' -Recurse -Force }"
)
if %aac-type%==Yaml (
powershell.exe -Command "if (Test-Path '%aac-project-name%\Yaml\*') { Remove-Item -Path '%aac-project-name%\Yaml\*' -Recurse -Force }"
)
echo AaC is cleared
:: pause

:: Execute AaC Strategy
echo Executing AaC Strategy...

%cli-output-dir%\%cli-exe% execute-aac-strategy --aac-root-namespace "%aac-root-namespace%" --aac-output-path "%aac-output-path%" --aac-writer-strategy "%aac-project-name%.Cli.CSharpTo%aac-type%BasketApiAaCGenerator, %aac-project-name%.Cli" --aac-params software-system-source-path="%net-sourcecode-dir%\src\Basket.API\Basket.API.csproj" --aac-params software-system-name="BasketApi"

%cli-output-dir%\%cli-exe% execute-aac-strategy --aac-root-namespace "%aac-root-namespace%" --aac-output-path "%aac-output-path%" --aac-writer-strategy "%aac-project-name%.Cli.CSharpTo%aac-type%CatalogApiAaCGenerator, %aac-project-name%.Cli" --aac-params software-system-source-path="%net-sourcecode-dir%\src\Catalog.API\Catalog.API.csproj" --aac-params software-system-name="CatalogApi"

pause

:end