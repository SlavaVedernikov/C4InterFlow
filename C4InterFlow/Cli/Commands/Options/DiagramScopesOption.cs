using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class DiagramScopesOption
{
    public const string ALL_SOFTWARE_SYSTEMS = "all-software-systems";
    public const string NAMESPACE = "namespace";
    public const string NAMESPACE_SOFTWARE_SYSTEMS = "namespace-software-systems";
    public const string SOFTWARE_SYSTEM = "software-system";
    public const string SOFTWARE_SYSTEM_INTERFACE = "software-system-interface";
    public const string CONTAINER = "container";
    public const string CONTAINER_INTERFACE = "container-interface";
    public const string COMPONENT = "component";
    public const string COMPONENT_INTERFACE = "component-interface";
    public const string BUSINESS_PROCESS = "business-process";
    public const string ACTIVITY = "activity";
    public const string ALL_SCOPES = "*";
    public const string AUTO = "auto";

    public static Option<string[]> Get()
    {
        const string description = $"Scopes of Diagram(s) to draw.";

        var option = new Option<string[]>(new[] { "--scopes", "-s" }, description)
        {
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true
        };
        option.FromAmong(
            ALL_SOFTWARE_SYSTEMS,
            NAMESPACE,
            NAMESPACE_SOFTWARE_SYSTEMS,
            SOFTWARE_SYSTEM,
            SOFTWARE_SYSTEM_INTERFACE,
            CONTAINER,
            CONTAINER_INTERFACE,
            COMPONENT,
            COMPONENT_INTERFACE,
            BUSINESS_PROCESS,
            ACTIVITY,
            ALL_SCOPES,
            AUTO);

        option.SetDefaultValue(new[] { ALL_SCOPES });
        return option;        
    }

    public static string[] GetAllValues()
    {
        return new[] {
            ALL_SOFTWARE_SYSTEMS,
            NAMESPACE,
            NAMESPACE_SOFTWARE_SYSTEMS,
            SOFTWARE_SYSTEM,
            SOFTWARE_SYSTEM_INTERFACE,
            CONTAINER,
            CONTAINER_INTERFACE,
            COMPONENT,
            COMPONENT_INTERFACE,
            BUSINESS_PROCESS,
            ACTIVITY,
            AUTO
        };
    }
}
