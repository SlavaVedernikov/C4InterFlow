
# C4InterFlow.Automation
`C4InterFlow.Automation` package contains implementations for some Architecture as Code (AaC) Reader/Writer Strategies.
  
Below is the list of the key out-of-the-box Strategies:
- Reader Strategies in `C4InterFlow.Automation.Readers` namespace
  - `CSharpAaCReaderStrategy`: Reads AaC expressed in C#
  - `YamlAaCReaderStrategy`: Reads AaC expressed in YAML
- Writer Strategies in `C4InterFlow.Automation.Writers` namespace
  - `CSharpToCSharpAaCWriterStrategy` (abstract): Must be extended by a Custom AaC Generator to interpret C# source code
	- Uses `CSharpToCSharpAaCWriter`
	  - Extends `CSharpToAnyAaCWriter`, which can analyse C# code
	  - Writes AaC in C#
  - `CSharpToYamlAaCWriterStrategy` (abstract): Must be extended by a Custom AaC Generator to interpret C# source code
	- Uses `CSharpToYamlAaCWriter`
	  - Extends `CSharpToAnyAaCWriter`, which can analyse C# code
	  - Writes AaC in YAML
  - `CsvToCSharpAaCWriterStrategy` (abstract)
	  - Extended by `CsvToCSharpAaCGenerator`
		- Uses `CsvToCSharpAaCWriter`
		  - Extends `CsvToAnyAaCWriter`, which understands CSV Architecture Catalogue
		  - Writes AaC in C#
  - `CsvToJObjectAaCWriterStrategy` (abstract)
	- Extended by `CsvToJsonAaCGenerator`
	  - Uses `CsvToJsonAaCWriter`
	    - Extends `CsvToAnyAaCWriter`, which understands CSV Architecture Catalogue
	    - Writes AaC in JSON
	- Extended by `CsvToYamlAaCGenerator`
	  - Uses `CsvToYamlAaCWriter`
		- Extends `CsvToAnyAaCWriter`, which understands CSV Architecture Catalogue
	    - Writes AaC in YAML