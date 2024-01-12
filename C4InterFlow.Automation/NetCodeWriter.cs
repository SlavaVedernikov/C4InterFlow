using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace C4InterFlow.Automation
{
    public class NetCodeWriter : ICodeWriter
    {
        internal const string ROOT_ARCHITECTURE_NAMESPACE = "C4InterFlow";
        private static string GetSoftwareSystemAlias(string architectureNamespace, string softwareSystemName)
        {
            if (string.IsNullOrEmpty(architectureNamespace) || string.IsNullOrEmpty(softwareSystemName)) return string.Empty;

            return $"{architectureNamespace}.SoftwareSystems.{softwareSystemName}";
        }

        private static string GetActorAlias(string architectureNamespace, string actorName)
        {
            if (string.IsNullOrEmpty(architectureNamespace) || string.IsNullOrEmpty(actorName)) return string.Empty;

            return $"{architectureNamespace}.Actors.{actorName}";
        }
        private static string GetContainerAlias(string architectureNamespace, string softwareSystemName, string containerName)
        {
            if (string.IsNullOrEmpty(architectureNamespace) || string.IsNullOrEmpty(softwareSystemName) || string.IsNullOrEmpty(containerName)) return string.Empty;

            return $"{architectureNamespace}.SoftwareSystems.{softwareSystemName}.Containers.{containerName}";
        }

        private static string GetComponentAlias(string architectureNamespace, string softwareSystemName, string containerName, string componentName)
        {
            if (string.IsNullOrEmpty(softwareSystemName) || string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(componentName)) return string.Empty;

            return $"{architectureNamespace}.SoftwareSystems.{softwareSystemName}.Containers.{containerName}.Components.{componentName}";
        }

        public static string GetEntityAlias(string architectureNamespace, string softwareSystemName, string containerName, string entityName)
        {
            if (string.IsNullOrEmpty(softwareSystemName) || string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(entityName)) return string.Empty;

            return $"{architectureNamespace}.SoftwareSystems.{softwareSystemName}.Containers.{containerName}.Entities.{entityName}";
        }

        private static string GetComponentInterfaceAlias(string componentAlias, string interfaceName)
        {
            if (string.IsNullOrEmpty(componentAlias) || string.IsNullOrEmpty(interfaceName)) return string.Empty;

            return $"{componentAlias}.Interfaces.{interfaceName}";
        }

        private string GetContainerInterfaceAlias(string architectureNamespace, string softwareSystemName, string containerName, string interfaceName)
        {
            if (string.IsNullOrEmpty(softwareSystemName) || string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(interfaceName)) return string.Empty;

            return $"{architectureNamespace}.SoftwareSystems.{softwareSystemName}.Containers.{containerName}.Interfaces.{interfaceName}";
        }

        private string GetSoftwareSystemInterfaceAlias(string architectureNamespace, string softwareSystemName, string interfaceName)
        {
            if (string.IsNullOrEmpty(architectureNamespace) || string.IsNullOrEmpty(softwareSystemName) || string.IsNullOrEmpty(interfaceName)) return string.Empty;

            return $"{architectureNamespace}.SoftwareSystems.{softwareSystemName}.Interfaces.{interfaceName}";
        }

        private string EnsureDoubleQuotes(string text)
        {
            if (string.IsNullOrEmpty(text)) return "\"\"";

            var result = text;

            if (!result.StartsWith("\""))
                result = $"\"{result}";

            if (!result.EndsWith("\""))
                result = $"{result}\"";

            return result;

        }

        private string GetName(string text)
        {
            var result = string.Empty;
            if (!string.IsNullOrEmpty(text))
            {
                result = text.Replace(" ", string.Empty);
            }

            return result;
        }

        public static string? GetLabel(string? text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            return Regex.Replace(text.Replace("\"", string.Empty), "((?<=[a-z])[A-Z]|[A-Z](?=[a-z]))", " $1").Trim();
        }
        public string GetContainerCode(string architectureNamespace, string softwareSystemName, string containerName, string label, string? type = null, string? description = null, string? technology = null, string? boundary = null)
        {
            var softwareSystemAlias = GetSoftwareSystemAlias(architectureNamespace, softwareSystemName);
            return $@"
    public partial class {GetName(softwareSystemName)}
    {{
        public partial class Containers
        {{
            public partial class {GetName(containerName)} : IContainerInstance
            {{
                public const string ALIAS = {$"\"{GetContainerAlias(architectureNamespace, softwareSystemName, containerName)}\""};

                public static Container Instance => new Container(
                    {softwareSystemAlias}.ALIAS, ALIAS, {EnsureDoubleQuotes(label)})
                {{
                    ContainerType = ContainerType.{(!string.IsNullOrEmpty(type) ? type : "None")},
                    Description = {(!string.IsNullOrEmpty(description) ? EnsureDoubleQuotes(description) : "\"\"")},
                    Technology = {(!string.IsNullOrEmpty(technology) ? EnsureDoubleQuotes(technology) : "\"\"")},
                    Boundary = Boundary.{(!string.IsNullOrEmpty(boundary) ? boundary : "Internal")}
                }};

                public partial class Components
                {{ }}

                public partial class Interfaces
                {{ }}

                public partial class Entities
                {{ }}
            }}
        }}
    }}
";
        }

        public string GetSoftwareSystemsCodeHeader(string architectureNamespace)
        {
            var result = new StringBuilder();

            result.AppendLine(@"// <auto-generated/>");
            result.AppendLine($"using {ROOT_ARCHITECTURE_NAMESPACE}.Elements;");
            result.AppendLine($"using {ROOT_ARCHITECTURE_NAMESPACE}.Elements.Interfaces;");
            result.AppendLine($"using {ROOT_ARCHITECTURE_NAMESPACE}.Elements.Relationships;");
            result.AppendLine();
            result.AppendLine($"namespace {architectureNamespace}.SoftwareSystems");
            result.AppendLine("{");

            return result.ToString();
        }

        public string GetActorsCodeHeader(string architectureNamespace)
        {
            var result = new StringBuilder();

            result.AppendLine(@"// <auto-generated/>");
            result.AppendLine($"using {ROOT_ARCHITECTURE_NAMESPACE}.Elements;");
            result.AppendLine($"using {ROOT_ARCHITECTURE_NAMESPACE}.Elements.Interfaces;");
            result.AppendLine();
            result.AppendLine($"namespace {architectureNamespace}.Actors");
            result.AppendLine("{");

            return result.ToString();
        }

        public string GetBusinessProcessesCodeHeader(string architectureNamespace)
        {
            var result = new StringBuilder();

            result.AppendLine(@"// <auto-generated/>");
            result.AppendLine($"using {ROOT_ARCHITECTURE_NAMESPACE};");
            result.AppendLine($"using {ROOT_ARCHITECTURE_NAMESPACE}.Elements;");
            result.AppendLine($"using {ROOT_ARCHITECTURE_NAMESPACE}.Elements.Interfaces;");
            result.AppendLine();
            result.AppendLine($"namespace {architectureNamespace}.BusinessProcesses");
            result.AppendLine("{");

            return result.ToString();
        }

        public string GetSoftwareSystemCode(string architectureNamespace, string name, string label, string? description = null, string? boundary = null)
        {
            return $@"
    public partial class {GetName(name)} : ISoftwareSystemInstance
    {{
        public const string ALIAS = {$"\"{GetSoftwareSystemAlias(architectureNamespace, name)}\""};

        public static SoftwareSystem Instance => new SoftwareSystem(
            ALIAS, {EnsureDoubleQuotes(label)})
        {{
            Description = {(!string.IsNullOrEmpty(description) ? EnsureDoubleQuotes(description) : "\"\"")},
            Boundary = Boundary.{(!string.IsNullOrEmpty(boundary) ? boundary : "Internal")}
        }};

        public partial class Containers
        {{ }}

        public partial class Interfaces
        {{ }}
    }}
";
        }

        public string GetActorCode(string architectureNamespace, string type, string name, string label, string? description = null)
        {
            return $@"
    public class {GetName(name)} : I{type}Instance
    {{
        public const string ALIAS = {$"\"{GetActorAlias(architectureNamespace, name)}\""};

        public static {type} Instance => new {type}(
            ALIAS, {EnsureDoubleQuotes(label)})
        {{
            Description = {(description != null ? description : "\"\"")},
        }};
    }}
";
        }

        public string GetComponentCode(string architectureNamespace, string softwareSystemName, string containerName, string name, string label, string componentType = "None", string? description = null, string? technology = null)
        {
            var containerAlias = GetContainerAlias(architectureNamespace, softwareSystemName, containerName);
            return $@"
    public partial class {GetName(softwareSystemName)}
    {{
        public partial class Containers
        {{
            public partial class {GetName(containerName)}
            {{
                public partial class Components
                {{
                    public partial class {GetName(name)} : IComponentInstance
                    {{
                        public const string ALIAS = {$"\"{GetComponentAlias(architectureNamespace, softwareSystemName, containerName, name)}\""};

                        public static Component Instance => new Component(
                            {containerAlias}.ALIAS, ALIAS, {EnsureDoubleQuotes(label)})
                        {{
                            ComponentType = ComponentType.{componentType},
                            Description = {(!string.IsNullOrEmpty(description) ? EnsureDoubleQuotes(description) : "\"\"")},
                            Technology = {(!string.IsNullOrEmpty(technology) ? EnsureDoubleQuotes(technology) : "\"\"")}
                        }};

                        public partial class Interfaces
                        {{ }}
                    }}
                }}
            }}
        }}
    }}
";
        }

        public string GetEntityCode(string architectureNamespace, string softwareSystemName, string containerName, string name, string label, string? type = null, string? description = null, string[]? composedOfMany = null, string[]? composedOfOne = null, string? extends = null)
        {
            var containerAlias = GetContainerAlias(architectureNamespace, softwareSystemName, containerName);
            return $@"
    public partial class {GetName(softwareSystemName)}
    {{
        public partial class Containers
        {{
            public partial class {GetName(containerName)}
            {{
                public partial class Entities
                {{
                    public partial class {GetName(name)} : IEntityInstance
                    {{
                        public const string ALIAS = {$"\"{GetEntityAlias(architectureNamespace, softwareSystemName, containerName, name)}\""};

                        public static Entity Instance => new Entity(
                            {containerAlias}.ALIAS, ALIAS, {EnsureDoubleQuotes(label)}, {(type != null ? type : "EntityType.None")})
                        {{
                            Description = {(!string.IsNullOrEmpty(description) ? EnsureDoubleQuotes(description) : "\"\"")},
                            ComposedOfMany = new string[] {{{(composedOfMany != null ? string.Join(", ", composedOfMany.Select(x => EnsureDoubleQuotes(x))) : " ")}}},
                            ComposedOfOne = new string[] {{{(composedOfOne != null ? string.Join(", ", composedOfOne.Select(x => EnsureDoubleQuotes(x))) : " ")}}},
                            Extends = {(!string.IsNullOrEmpty(extends) ? EnsureDoubleQuotes(extends) : "\"\"")},
                        }};
                    }}
                }}
            }}
        }}
    }}
 ";
        }

        public string GetComponentInterfaceCode(string architectureNamespace, string softwareSystemName, string containerName, string componentName, string name, string label, string? description = null, string? protocol = null, string? path = null, bool? isPrivate = null, string? uses = null, string? input = null, string? inputTemplate = null, string? output = null, string? outputTemplate = null)
        {
            var componentAlias = GetComponentAlias(architectureNamespace, softwareSystemName, containerName, componentName);
            return $@"
    public partial class {GetName(softwareSystemName)}
    {{
        public partial class Containers
        {{
            public partial class {containerName}
            {{
                public partial class Components
                {{
                    public partial class {componentName}
                    {{
                        public partial class Interfaces
                        {{
                            public partial class {GetName(name)} : IInterfaceInstance
                            {{
                                public const string ALIAS = {$"\"{GetComponentInterfaceAlias(componentAlias, name)}\""};

                                public static Interface Instance => new Interface(
                                    {componentAlias}.ALIAS, ALIAS, {EnsureDoubleQuotes(label)})
                                {{
                                    Description = {(description != null ? EnsureDoubleQuotes(description) : "\"\"")},
                                    Path = {(path != null ? EnsureDoubleQuotes(path) : "\"\"")},
                                    IsPrivate = {(isPrivate != null ? isPrivate.ToString().ToLower() : "false")},
                                    Protocol =  {(protocol != null ? EnsureDoubleQuotes(protocol) : "\"\"")},
                                    Flow = new Flow(ALIAS),
                                    Input = {(input != null ? input : "\"\"")},
                                    InputTemplate = {(inputTemplate != null ? inputTemplate : "\"\"")},
                                    Output = {(output != null ? output : "\"\"")},
                                    OutputTemplate = {(outputTemplate != null ? outputTemplate : "\"\"")}
                                }};
                            }}
                        }}
                    }}
                }}
            }}
        }}
    }}
";
        }

        public string GetContainerInterfaceCode(string architectureNamespace, string softwareSystemName, string containerName, string name, string label, string? description = null, string? protocol = null, string? uses = null, string? input = null, string? inputTemplate = null, string? output = null, string? outputTemplate = null)
        {
            var containerAlias = GetContainerAlias(architectureNamespace, softwareSystemName, containerName);
            return $@"
    public partial class {GetName(softwareSystemName)}
    {{
        public partial class Containers
        {{
            public partial class {containerName}
            {{
                public partial class Interfaces
                {{
                    public partial class {GetName(name)} : IInterfaceInstance
                    {{
                        public const string ALIAS = {$"\"{GetContainerInterfaceAlias(architectureNamespace, softwareSystemName, containerName, name)}\""};

                        public static Interface Instance => new Interface(
                            {containerAlias}.ALIAS, ALIAS, {EnsureDoubleQuotes(label)})
                        {{
                            Description = {(description != null ? EnsureDoubleQuotes(description) : "\"\"")},
                            Flow = new Flow(ALIAS),
                            Protocol = {(protocol != null ? EnsureDoubleQuotes(protocol) : "\"\"")},
                            Input = {(input != null ? input : "\"\"")},
                            InputTemplate = {(inputTemplate != null ? inputTemplate : "\"\"")},
                            Output = {(output != null ? output : "\"\"")},
                            OutputTemplate = {(outputTemplate != null ? outputTemplate : "\"\"")}
                        }};
                    }}
                }}
            }}
        }}
    }}
";
        }

        public string GetSoftwareSystemInterfaceCode(string architectureNamespace, string softwareSystemName, string name, string label, string? description = null, string? protocol = null, string? uses = null, string? input = null, string? inputTemplate = null, string? output = null, string? outputTemplate = null)
        {
            var softwareSystemAlias = GetSoftwareSystemAlias(architectureNamespace, softwareSystemName);
            return $@"
    public partial class {softwareSystemName}
    {{
        public partial class Interfaces
        {{
            public partial class {GetName(name)} : IInterfaceInstance
            {{
                public const string ALIAS = {$"\"{GetSoftwareSystemInterfaceAlias(architectureNamespace, softwareSystemName, name)}\""};

                public static Interface Instance => new Interface(
                    {softwareSystemAlias}.ALIAS, ALIAS, {EnsureDoubleQuotes(label)})
                {{
                    Description = {(description != null ? EnsureDoubleQuotes(description) : "\"\"")},
                    Flow = new Flow(ALIAS),
                    Protocol = {(protocol != null ? EnsureDoubleQuotes(protocol) : "\"\"")},
                    Input = {(input != null ? input : "\"\"")},
                    InputTemplate = {(inputTemplate != null ? inputTemplate : "\"\"")},
                    Output = {(output != null ? output : "\"\"")},
                    OutputTemplate = {(outputTemplate != null ? outputTemplate : "\"\"")}
                }};
            }}
        }}
    }}
";
        }

        public string GetFlowHeader()
        {
            return $"new Flow(ALIAS)";
        }

        public string GetLoopFlowCode(string condition)
        {
            return $"\t.Loop(@\"{GetFormattedParams(condition)}\")";
        }

        public string GetEndLoopFlowCode()
        {
            return "\t.EndLoop()";
        }

        public string GetIfFlowCode(string condition)
        {
            return $"\t.If(@\"{GetFormattedParams(condition)}\")";
        }

        public string GetEndIfFlowCode()
        {
            return "\t.EndIf()";
        }

        public string GetElseIfFlowCode(string condition)
        {
            return $"\t.ElseIf(@\"{GetFormattedParams(condition)}\")";
        }

        public string GetElseFlowCode()
        {
            return "\t.Else()";
        }

        public string GetReturnFlowCode(string? expression = null)
        {
            return $"\t.Return(@\"{GetFormattedParams(expression ?? string.Empty)}\")";
        }

        public string GetTryFlowCode()
        {
            return $"\t.Try()";
        }

        public string GetEndTryFlowCode()
        {
            return "\t.EndTry()";
        }

        public string GetCatchFlowCode(string? exception = null)
        {
            return $"\t\t.Catch(@\"{GetFormattedParams(exception ?? string.Empty)}\")";
        }

        public string GetEndCatchFlowCode()
        {
            return "\t\t.EndCatch()";
        }

        public string GetFinallyFlowCode()
        {
            return $"\t\t.Finally()";
        }

        public string GetEndFinallyFlowCode()
        {
            return "\t\t.EndFinally()";
        }

        public string GetThrowExceptionFlowCode(string? exception = null)
        {
            return $"\t.ThrowException(@\"{GetFormattedParams(exception ?? string.Empty)}\")";
        }

        private string GetFormattedParams(string @params)
        {
            return @params.Replace(Environment.NewLine, "\\n").Replace("\"", "\"\"");
        }

        public string GetUseFlowCode(string alias)
        {
            return $"\t.Use(\"{alias}\")";
        }

        public string GetBusinessProcessStartCode(string architectureNamespace, string name, string label, string? description = null)
        {
            return $@"
    public class {GetName(name)} : IBusinessProcessInstance
    {{
        public static BusinessProcess  Instance => new BusinessProcess(new BusinessActivity[]
        {{
            ";
        }

        public string GetBusinessProcessEndCode(string architectureNamespace, string name, string label, string? description = null)
        {
            return $@"
        }}, {(label != null ? EnsureDoubleQuotes(label) : "\"\"")});
    }}
";
        }

        public string GetBusinessActivityCode(string name, string actor, string[] uses, string? description = null)
        {
            return $@"
            new BusinessActivity(new Flow(""{actor}"")
                {string.Join($"{Environment.NewLine}\t\t\t\t", uses.Select(x => $".Use(\"{x}\")").ToArray())},
                {(name != null ? EnsureDoubleQuotes(name) : "\"\"")}),
";
        }
    }
}
