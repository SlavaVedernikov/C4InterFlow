Feature: Drawing Diagrams
	Test out different combinations of drawing diagrams

Scenario: Verify Examples
	Given the 'draw-diagrams' command
	  And the '<Example>' example
	  And the path '<Path>'
	  And the reader strategy is '<ReaderStrategy>'
	  And the interfaces are '<Interfaces>'
	  And the business processes are '<BusinessProcesses>'
	  And the level of details is '<LevelOfDetails>'
	  And send the output to '<OutputPath>'
	When invoking the commandline for those arguments
	Then all files under '<OutputPath>' should match example path '<ApprovedOutPath>'

	Examples:
	  | Description             | Example                 | Path                  | ReaderStrategy | Interfaces                                           | BusinessProcesses              | LevelOfDetails    | OutputPath     | ApprovedOutPath |
	  | Banking System via Yaml | Internet Banking System | CSV\Architecture\Yaml | Yaml           | BigBankPlc.SoftwareSystems.*.Containers.*.Interfaces | BigBankPlc.BusinessProcesses.* | context container | _bankingYaml      | Internet Banking System\CSV\Diagrams |
	  | Banking System via Json | Internet Banking System | CSV\Architecture\Json | Json           | BigBankPlc.SoftwareSystems.*.Containers.*.Interfaces | BigBankPlc.BusinessProcesses.* | context container | _bankingJson      | Internet Banking System\CSV\Diagrams |
	  | ECommerce via Yaml      | E-Commerce Platform     | Yaml\Architecture     | Yaml           | ECommercePlatform.*.*.SoftwareSystems.*.Interfaces.* ECommercePlatform.*.*.SoftwareSystems.*.Containers.*.Interfaces.* | ECommercePlatform.BusinessProcesses.* | context container | _ecommerceYaml | E-Commerce Platform\Yaml\Diagrams |
	  | TraderX via Json        | TraderX                 | CSV\Architecture\Json | Json           | TraderXExample.SoftwareSystems.*.Containers.*.Interfaces.* | TraderXExample.BusinessProcesses.* | context container | _traderXJson | TraderX\CSV\Diagrams |
	  | TraderX via Yaml        | TraderX                 | CSV\Architecture\Yaml | Yaml           | TraderXExample.SoftwareSystems.*.Containers.*.Interfaces.* | TraderXExample.BusinessProcesses.* | context container | _traderXYaml | TraderX\CSV\Diagrams |
	  | ToDoApp via Yaml        | ToDoApp                 | Yaml\Architecture     | Yaml           | ToDoAppExample.SoftwareSystems.*.Containers.*.Interfaces.* ToDoAppExample.SoftwareSystems.*.Interfaces.* | | container | _todoYaml | ToDoApp\Yaml\Diagrams |
