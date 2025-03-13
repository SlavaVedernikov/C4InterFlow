# 1.0.0

* Added Use Flow Expression validation for YAML AaC
* Added support for partial Interface Aliases in Use Flows of YAML AaC
* Made sure all diagrams have consistent and complete titles
* Simplified Flows representation in YAML and JSON
* Added JsonAaCReaderStrategy

# 1.6.0

* Fixed Interface inference issue
* Added support for viewing diagrams in GitHub pages
* Added support for Tags to YAML AaC
* Added YAML and JSON syntax validation

# 1.7.0

* Added support for Namespace sub-directories on the file system
* Added `namespace` scopes option
* Added namespaces to the diagrams files system path
* Added logging
* Added shortcuts for AaC reader strategies
* Added support for recursive descent JSON Path syntax to dotnet resolver

## 1.8.0

* Fixed the issue with nested Invocation Expressions when generating AaC from CSharp
* Changed the way Protocol and Tags are coded in PlantUML
* Changed CSharp AaC generation to remove ALIAS field
* Simplified C# AaC by making it strongly-typed throughout
* Removed type names repetitions in C# AaC definitions

## 2.0.0
* Added support for icons
* Added `--max-line-labels` option to `draw-diagrams` command
* Added views for drawing custom diagrams
* Added JSON schema
* Made `Actor` optional for Business Process `Activity`
* Removed `Flow` element from `Interface` and Business Process `Activity` structures. Use `Flows` array instead. (**Braking change**)