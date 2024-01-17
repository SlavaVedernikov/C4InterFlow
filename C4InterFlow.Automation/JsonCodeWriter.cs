using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace C4InterFlow.Automation
{
    public class JsonCodeWriter : ICodeWriter
    {
        public enum OutputFormat
        {
            Json = 1,
            Yaml = 2
        }

        protected OutputFormat Format { get; private set; }
        public JsonCodeWriter(OutputFormat format)
        {
            Format = format;
        }
        public static string? GetLabel(string? text)
        {
            return AnyCodeWriter.GetLabel(text);
        }

        public string GetCode(JObject jObject)
        {
            var result = string.Empty;

            if (Format == OutputFormat.Json)
            {
                result = JsonConvert.SerializeObject(jObject, Formatting.Indented);
            }
            else if (Format == OutputFormat.Yaml)
            {
                var serializer = new SerializerBuilder().Build();
                result = serializer.Serialize(jObject);
            }

            return result;
        }

        public string GetContainerCode(string architectureNamespace, string softwareSystemName, string containerName, string label, string? type = null, string? description = null, string? technology = null, string? boundary = null)
        {
            var softwareSystemAlias = AnyCodeWriter.GetSoftwareSystemAlias(architectureNamespace, softwareSystemName);
            return $@"
    public partial class {AnyCodeWriter.GetName(softwareSystemName)}
    {{
        public partial class Containers
        {{
            public partial class {AnyCodeWriter.GetName(containerName)} : IContainerInstance
            {{
                public const string ALIAS = {$"\"{AnyCodeWriter.GetContainerAlias(architectureNamespace, softwareSystemName, containerName)}\""};

                public static Container Instance => new Container(
                    {softwareSystemAlias}.ALIAS, ALIAS, {AnyCodeWriter.EnsureDoubleQuotes(label)})
                {{
                    ContainerType = ContainerType.{(!string.IsNullOrEmpty(type) ? type : "None")},
                    Description = {(!string.IsNullOrEmpty(description) ? AnyCodeWriter.EnsureDoubleQuotes(description) : "\"\"")},
                    Technology = {(!string.IsNullOrEmpty(technology) ? AnyCodeWriter.EnsureDoubleQuotes(technology) : "\"\"")},
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
            var architectureNamespaceSegments = architectureNamespace.Split('.');
            var result = new JObject();

            foreach (var segment in architectureNamespaceSegments)
            {
                var segmentObject = new JObject();
                result.Add(segment, segmentObject);
                result = segmentObject;
            }

            result.Add("SoftwareSystems", new JObject());

            return GetCode(result);
        }

        public string GetActorsCodeHeader(string architectureNamespace)
        {
            var result = new StringBuilder();

            result.AppendLine($"namespace {architectureNamespace}.Actors");
            result.AppendLine("{");

            return result.ToString();
        }

        private string GetBusinessProcessesCodeHeader(string architectureNamespace)
        {
            var result = new StringBuilder();

            result.AppendLine($"namespace {architectureNamespace}.BusinessProcesses");
            result.AppendLine("{");

            return result.ToString();
        }

        public string GetSoftwareSystemCode(string architectureNamespace, string name, string label, string? description = null, string? boundary = null)
        {
            return $@"
    public partial class {AnyCodeWriter.GetName(name)} : ISoftwareSystemInstance
    {{
        public const string ALIAS = {$"\"{AnyCodeWriter.GetSoftwareSystemAlias(architectureNamespace, name)}\""};

        public static SoftwareSystem Instance => new SoftwareSystem(
            ALIAS, {AnyCodeWriter.EnsureDoubleQuotes(label)})
        {{
            Description = {(!string.IsNullOrEmpty(description) ? AnyCodeWriter.EnsureDoubleQuotes(description) : "\"\"")},
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
    public class {AnyCodeWriter.GetName(name)} : I{type}Instance
    {{
        public const string ALIAS = {$"\"{AnyCodeWriter.GetActorAlias(architectureNamespace, name)}\""};

        public static {type} Instance => new {type}(
            ALIAS, {AnyCodeWriter.EnsureDoubleQuotes(label)})
        {{
            Description = {(description != null ? description : "\"\"")},
        }};
    }}
";
        }

        public string GetComponentCode(string architectureNamespace, string softwareSystemName, string containerName, string name, string label, string componentType = "None", string? description = null, string? technology = null)
        {
            var containerAlias = AnyCodeWriter.GetContainerAlias(architectureNamespace, softwareSystemName, containerName);
            return $@"
    public partial class {AnyCodeWriter.GetName(softwareSystemName)}
    {{
        public partial class Containers
        {{
            public partial class {AnyCodeWriter.GetName(containerName)}
            {{
                public partial class Components
                {{
                    public partial class {AnyCodeWriter.GetName(name)} : IComponentInstance
                    {{
                        public const string ALIAS = {$"\"{AnyCodeWriter.GetComponentAlias(architectureNamespace, softwareSystemName, containerName, name)}\""};

                        public static Component Instance => new Component(
                            {containerAlias}.ALIAS, ALIAS, {AnyCodeWriter.EnsureDoubleQuotes(label)})
                        {{
                            ComponentType = ComponentType.{componentType},
                            Description = {(!string.IsNullOrEmpty(description) ? AnyCodeWriter.EnsureDoubleQuotes(description) : "\"\"")},
                            Technology = {(!string.IsNullOrEmpty(technology) ? AnyCodeWriter.EnsureDoubleQuotes(technology) : "\"\"")}
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
            var containerAlias = AnyCodeWriter.GetContainerAlias(architectureNamespace, softwareSystemName, containerName);
            return $@"
    public partial class {AnyCodeWriter.GetName(softwareSystemName)}
    {{
        public partial class Containers
        {{
            public partial class {AnyCodeWriter.GetName(containerName)}
            {{
                public partial class Entities
                {{
                    public partial class {AnyCodeWriter.GetName(name)} : IEntityInstance
                    {{
                        public const string ALIAS = {$"\"{AnyCodeWriter.GetEntityAlias(architectureNamespace, softwareSystemName, containerName, name)}\""};

                        public static Entity Instance => new Entity(
                            {containerAlias}.ALIAS, ALIAS, {AnyCodeWriter.EnsureDoubleQuotes(label)}, {(type != null ? type : "EntityType.None")})
                        {{
                            Description = {(!string.IsNullOrEmpty(description) ? AnyCodeWriter.EnsureDoubleQuotes(description) : "\"\"")},
                            ComposedOfMany = new string[] {{{(composedOfMany != null ? string.Join(", ", composedOfMany.Select(x => AnyCodeWriter.EnsureDoubleQuotes(x))) : " ")}}},
                            ComposedOfOne = new string[] {{{(composedOfOne != null ? string.Join(", ", composedOfOne.Select(x => AnyCodeWriter.EnsureDoubleQuotes(x))) : " ")}}},
                            Extends = {(!string.IsNullOrEmpty(extends) ? AnyCodeWriter.EnsureDoubleQuotes(extends) : "\"\"")},
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
            var componentAlias = AnyCodeWriter.GetComponentAlias(architectureNamespace, softwareSystemName, containerName, componentName);
            return $@"
    public partial class {AnyCodeWriter.GetName(softwareSystemName)}
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
                            public partial class {AnyCodeWriter.GetName(name)} : IInterfaceInstance
                            {{
                                public const string ALIAS = {$"\"{AnyCodeWriter.GetComponentInterfaceAlias(componentAlias, name)}\""};

                                public static Interface Instance => new Interface(
                                    {componentAlias}.ALIAS, ALIAS, {AnyCodeWriter.EnsureDoubleQuotes(label)})
                                {{
                                    Description = {(description != null ? AnyCodeWriter.EnsureDoubleQuotes(description) : "\"\"")},
                                    Path = {(path != null ? AnyCodeWriter.EnsureDoubleQuotes(path) : "\"\"")},
                                    IsPrivate = {(isPrivate != null ? isPrivate.ToString().ToLower() : "false")},
                                    Protocol =  {(protocol != null ? AnyCodeWriter.EnsureDoubleQuotes(protocol) : "\"\"")},
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
            var containerAlias = AnyCodeWriter.GetContainerAlias(architectureNamespace, softwareSystemName, containerName);
            return $@"
    public partial class {AnyCodeWriter.GetName(softwareSystemName)}
    {{
        public partial class Containers
        {{
            public partial class {containerName}
            {{
                public partial class Interfaces
                {{
                    public partial class {AnyCodeWriter.GetName(name)} : IInterfaceInstance
                    {{
                        public const string ALIAS = {$"\"{AnyCodeWriter.GetContainerInterfaceAlias(architectureNamespace, softwareSystemName, containerName, name)}\""};

                        public static Interface Instance => new Interface(
                            {containerAlias}.ALIAS, ALIAS, {AnyCodeWriter.EnsureDoubleQuotes(label)})
                        {{
                            Description = {(description != null ? AnyCodeWriter.EnsureDoubleQuotes(description) : "\"\"")},
                            Flow = new Flow(ALIAS),
                            Protocol = {(protocol != null ? AnyCodeWriter.EnsureDoubleQuotes(protocol) : "\"\"")},
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
            var softwareSystemAlias = AnyCodeWriter.GetSoftwareSystemAlias(architectureNamespace, softwareSystemName);
            return $@"
    public partial class {softwareSystemName}
    {{
        public partial class Interfaces
        {{
            public partial class {AnyCodeWriter.GetName(name)} : IInterfaceInstance
            {{
                public const string ALIAS = {$"\"{AnyCodeWriter.GetSoftwareSystemInterfaceAlias(architectureNamespace, softwareSystemName, name)}\""};

                public static Interface Instance => new Interface(
                    {softwareSystemAlias}.ALIAS, ALIAS, {AnyCodeWriter.EnsureDoubleQuotes(label)})
                {{
                    Description = {(description != null ? AnyCodeWriter.EnsureDoubleQuotes(description) : "\"\"")},
                    Flow = new Flow(ALIAS),
                    Protocol = {(protocol != null ? AnyCodeWriter.EnsureDoubleQuotes(protocol) : "\"\"")},
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

        public string GetBusinessProcessCode(string architectureNamespace, string name, string label, string businessActivitiesCode, string? description = null)
        {
            var result = new StringBuilder(GetSoftwareSystemsCodeHeader(architectureNamespace));

            result.Append($@"
    public class {AnyCodeWriter.GetName(name)} : IBusinessProcessInstance
    {{
        public static BusinessProcess  Instance => new BusinessProcess(new BusinessActivity[]
        {{
            {businessActivitiesCode}
        }}, {(label != null ? AnyCodeWriter.EnsureDoubleQuotes(label) : "\"\"")});
    }}
");
            result.AppendLine("}");

            return result.ToString();
        }

        public string GetBusinessActivityCode(string name, string actor, string[] uses, string? description = null)
        {
            return $@"
            new BusinessActivity(new Flow(""{actor}"")
                {string.Join($"{Environment.NewLine}\t\t\t\t", uses.Select(x => $".Use(\"{x}\")").ToArray())},
                {(name != null ? AnyCodeWriter.EnsureDoubleQuotes(name) : "\"\"")}),
";
        }
    }
}
