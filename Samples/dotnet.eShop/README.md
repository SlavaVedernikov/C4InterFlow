
# E-Shop sample
This a sample with AaC and Diagrams generated by C4InterFlow from the forked source code of the [dotnet.eShop - sample .NET application ](https://github.com/SlavaVedernikov/DotNetEShop)

It is using the following C4InterFlow toolchain flow.

![C4InterFlow - toolchain flow](C4InterFlow%20-%20toolchain%20flow.png)

## Architecture as Code (AaC)
AaC is generated from C# source code
- [C# AaC](https://github.com/SlavaVedernikov/C4InterFlow/tree/master/Samples/dotnet.eShop/DotNetEShop/SoftwareSystems)
- [YAML AaC](https://github.com/SlavaVedernikov/C4InterFlow/tree/master/Samples/dotnet.eShop/DotNetEShop/Yaml/SoftwareSystems)

## Diagrams
Diagrams (generated and tested with both **C#** and **YAML** AaC) for all **Scope**, **Level of Details**, **Types** and **Formats**.
- [Diagram files](https://github.com/SlavaVedernikov/C4InterFlow/tree/master/Samples/dotnet.eShop/DotNetEShop/Diagrams)

## Try it locally

1. Clone [E-Shop sample .NET application ](https://github.com/SlavaVedernikov/DotNetEShop) repo
    1. Remember your local E-Shop repo directory, you will need it later.
1. Clone [C4InterFlow](https://github.com/SlavaVedernikov/C4InterFlow) repo
1. Open `Samples\dotnet.eShop` sub-directory under the directory where C4InterFlow repo was cloned to in the previous step
1. Edit `execute-aac-strategy.bat` file as follows:
    1. Replace `C:\Data\Projects\C4InterFlow\eShop-main` in `set "net-sourcecode-dir=C:\Data\Projects\C4InterFlow\eShop-main\src"` with the path to the directory where you cloned the E-Shop repo to
    1. Choose one of the following settings
        1. To generate Architecture as Code in **C#**, set `aac-type` to `CSharp` i.e. `set "aac-type=CSharp"`
        1. To generate Architecture as Code in **YAML**, set `aac-type` to `Yaml` i.e. `set "aac-type=Yaml"`
    1. Save the changes
1. Double-click on `execute-aac-strategy.bat` file to run it and make sure it completed successfully
1. Edit `draw-diagrams.bat` file as follows:
    1. Choose one of the following settings
        1. To re-draw **all diagrams** (i.e. `.puml`, `.png` and `.svg` formats), set `redraw-all` to `TRUE` i.e. `set "redraw-all=TRUE"`
            1. **NOTE**: this may take several minutes
        1. To re-draw **only diagrams** in `.puml` format, set `redraw-all` to `FALSE` i.e. `set "redraw-all=FALSE"`
    1. Choose one of the following settings
        1. To generate diagrams from **C#** Architecture as Code, set `aac-type` to `CSharp` i.e. `set "aac-type=CSharp"`
        1. To generate diagrams from **YAML** Architecture as Code, set `aac-type` to `Yaml` i.e. `set "aac-type=Yaml"`
    1. Save the changes
1. Double-click on `draw-diagrams.bat` file to run it and make sure it completed successfully
