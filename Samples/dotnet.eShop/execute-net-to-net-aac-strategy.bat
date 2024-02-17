@echo off
set "build-configuration=Debug"
set "aac-root-namespace=dotnet.eShop.Architecture"
set "cli-project-path=.\%aac-root-namespace%.Cli\%aac-root-namespace%.Cli.csproj"
set "cli-output-dir=.\%aac-root-namespace%.Cli\bin\%build-configuration%\net6.0"
set "cli-exe=%aac-root-namespace%.Cli.exe"
set "net-sourcecode-dir=C:\Data\Projects\C4InterFlow\eShop-main\src"

echo build-configuration: %build-configuration%
echo aac-root-namespace: %aac-root-namespace%
echo cli-project-path: %cli-project-path%
echo cli-output-dir: %cli-output-dir%
echo cli-exe: %cli-exe%
echo net-sourcecode-dir: %net-sourcecode-dir%

:: Publish
echo Publishing...
:: pause
dotnet publish %cli-project-path% --configuration %build-configuration% --output %cli-output-dir%

:: Clear AaC
echo Clearing AaC...
:: pause
powershell.exe -Command "if (Test-Path '%aac-root-namespace%\SoftwareSystems\*') { Remove-Item -Path '%aac-root-namespace%\SoftwareSystems\*' -Recurse -Force }"
powershell.exe -Command "if (Test-Path '%aac-root-namespace%\Actors\*') { Remove-Item -Path '%aac-root-namespace%\Actors\*' -Recurse -Force }"
powershell.exe -Command "if (Test-Path '%aac-root-namespace%\BusinessProcesses\*') { Remove-Item -Path '%aac-root-namespace%\BusinessProcesses\*' -Recurse -Force }"

:: Execute AaC Strategy
echo Executing AaC Strategy...
:: pause
%cli-output-dir%\%cli-exe% execute-aac-strategy --aac-root-namespace "%aac-root-namespace%" --aac-output-path "%aac-root-namespace%\%aac-root-namespace%.csproj" --aac-writer-strategy "%aac-root-namespace%.Cli.CSharpToCSharpBasketApiAaCGenerator, %aac-root-namespace%.Cli" --aac-params software-system-source-path="%net-sourcecode-dir%\Basket.API\Basket.API.csproj" --aac-params software-system-name="BasketApi"

%cli-output-dir%\%cli-exe% execute-aac-strategy --aac-root-namespace "%aac-root-namespace%" --aac-output-path "%aac-root-namespace%\%aac-root-namespace%.csproj" --aac-writer-strategy "%aac-root-namespace%.Cli.CSharpToCSharpCatalogApiAaCGenerator, %aac-root-namespace%.Cli" --aac-params software-system-source-path="%net-sourcecode-dir%\Catalog.API\Catalog.API.csproj" --aac-params software-system-name="CatalogApi"

pause
