# Contributing to C4InterFlow

## As a Developer

C4InterFlow is developed on dotnet core, and so far development has been done on a Windows machine with Visual Studio.
Editor files have been put in place to enforce consistency where applicable.

### Environment Prerequisites

* Windows environment
  * All testing scripts are currently utilizing Windows Batch files.
* Visual Studio (able to support dotnet 6 or later)
* Java JRE
  * Necessary to execute the `plantuml.jar` file to render diagrams

## As a tester

The [Samples](./Samples/) folder contains all valid use cases for the project.
The focus of the samples is on a business domain rather than a specific technology or format.

There are `draw-diagrams.bat` scripts for each sample that can be run individually.
This is a great way to work on an isolated domain at a time.
But there is also a [`draw-diagrams.bat`](./Samples/draw-diagrams.bat) in the Samples directory that runs all the samples under the same conditions.
This is the best way to perform what effectively is a regression test.

## All Others