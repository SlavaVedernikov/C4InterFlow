using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class DiagramScopesOption
{
    public const string SOFTWARE_SYSTEMS = "software-systems";
    public const string SOFTWARE_SYSTEM = "software-system";
    public const string SOFTWARE_SYSTEM_INTERFACE = "software-system-interface";
    public const string CONTAINER = "container";
    public const string CONTAINER_INTERFACE = "container-interface";
    public const string COMPONENT = "component";
    public const string COMPONENT_INTERFACE = "component-interface";
    public const string BUSINESS_PROCESS = "business-process";
    public const string ALL_SCOPES = "*";
    public static Option<string[]> Get()
    {
        const string description = $"Scopes of Diagram(s) to draw.";

        var option = new Option<string[]>(new[] { "--scopes", "-s" }, description)
        {
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true
        };
        option.FromAmong(
            SOFTWARE_SYSTEMS,
            SOFTWARE_SYSTEM,
            SOFTWARE_SYSTEM_INTERFACE,
            CONTAINER,
            CONTAINER_INTERFACE,
            COMPONENT,
            COMPONENT_INTERFACE,
            BUSINESS_PROCESS,
            ALL_SCOPES);

        option.SetDefaultValue(new[] { ALL_SCOPES });
        return option;        
    }

    public static string[] GetAllScopes()
    {
        return new[] {
            SOFTWARE_SYSTEMS,
            SOFTWARE_SYSTEM,
            SOFTWARE_SYSTEM_INTERFACE,
            CONTAINER,
            CONTAINER_INTERFACE,
            COMPONENT,
            COMPONENT_INTERFACE,
            BUSINESS_PROCESS
        };
    }
}
