Feature: Execute Architect-as-Code (AaC) Strategy
  Take some input and generate the output for drawing diagrams.

Scenario: Verify Examples
    Given the 'execute-aac-strategy' command
      And the '<Example>' example
      And the parameter of 'aac-input-path' set to Example '<Path>'
      And the writer strategy is '<WriterStrategy>'
      And the AaC root namespace of '<RootNamespace>'
      And send the AaC output to '<OutputPath>'
    When invoking the commandline for those arguments
    Then all files under '<OutputPath>' should match example path '<ApprovedOutPath>'

    Examples:
      | Description             | Example                 | RootNamespace | Path                                    | WriterStrategy | OutputPath       | ApprovedOutPath                           |
      | Banking System via Yaml | Internet Banking System | BigBankPlc    | CSV\Architecture Catalogue - CSV Export | CsvToYaml      | _aac_bankingYaml | Internet Banking System\Yaml\Architecture |
