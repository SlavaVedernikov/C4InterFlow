using C4InterFlow.Structures;
using C4InterFlow.Structures.Relationships;
using Microsoft.Build.Framework;
using Microsoft.CodeAnalysis.Operations;
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

namespace C4InterFlow.Automation.Writers
{
    public class YamlCodeWriter : JObjectBuilder, ICodeWriter
    {
        private const string INDENTATION_UNIT = "  ";

        private string CurrectFlowIndentation { get; set; }

        public static string? GetLabel(string? text)
        {
            return AnyCodeWriter.GetLabel(text);
        }

        private void IncreaseFlowIndentation()
        {
            CurrectFlowIndentation += INDENTATION_UNIT;
        }

        private void DecreaseFlowIndentation()
        {
            CurrectFlowIndentation = CurrectFlowIndentation.Substring(0, CurrectFlowIndentation.Length - INDENTATION_UNIT.Length);
        }

        public override string GetCode(JObject jObject)
        {
            var json = JsonConvert.SerializeObject(jObject, Formatting.Indented);
            var jsonObject = JsonConvert.DeserializeObject<ExpandoObject>(json);
            var result = new SerializerBuilder().Build().Serialize(jsonObject);

            return result;
        }

        public string GetContainerCode(string architectureNamespace, string softwareSystemName, string name, string label, string? type = null, string? description = null, string? technology = null, string? boundary = null)
        {
            return GetCode(BuildContainerObject(architectureNamespace, softwareSystemName, name, label, type, description, technology, boundary));
        }

        public string GetSoftwareSystemCode(string architectureNamespace, string name, string label, string? description = null, string? boundary = null)
        {
            return GetCode(BuildSoftwareSystemObject(architectureNamespace, name, label, description, boundary));
        }

        public string GetActorCode(string architectureNamespace, string type, string name, string label, string? description = null)
        {
            return GetCode(BuildActorObject(architectureNamespace, type, name, label, description));
        }

        public string GetComponentCode(string architectureNamespace, string softwareSystemName, string containerName, string name, string label, string componentType = "None", string? description = null, string? technology = null)
        {
            return GetCode(BuildComponentObject(architectureNamespace, softwareSystemName, containerName, name, label, componentType, description, technology));
        }

        public string GetEntityCode(string architectureNamespace, string softwareSystemName, string containerName, string name, string label, string? type = null, string? description = null, string[]? composedOfMany = null, string[]? composedOfOne = null, string? extends = null)
        {
            return GetCode(BuildEntityObject(architectureNamespace, softwareSystemName, containerName, name, label, type, description, composedOfMany, composedOfOne, extends));
        }

        public string GetComponentInterfaceCode(string architectureNamespace, string softwareSystemName, string containerName, string componentName, string name, string label, string? description = null, string? protocol = null, string? path = null, bool? isPrivate = null, string? uses = null, string? input = null, string? inputTemplate = null, string? output = null, string? outputTemplate = null)
        {
            return GetCode(BuildComponentInterfaceObject(architectureNamespace, softwareSystemName, containerName, componentName, name, label, description, protocol, path, isPrivate, uses, input, inputTemplate, output, outputTemplate));
        }

        public string GetContainerInterfaceCode(string architectureNamespace, string softwareSystemName, string containerName, string name, string label, string? description = null, string? protocol = null, string? uses = null, string? input = null, string? inputTemplate = null, string? output = null, string? outputTemplate = null)
        {
            return GetCode(BuildContainerInterfaceObject(architectureNamespace, softwareSystemName, containerName, name, label, description, protocol, uses, input, inputTemplate, output, outputTemplate));
        }

        public string GetSoftwareSystemInterfaceCode(string architectureNamespace, string softwareSystemName, string name, string label, string? description = null, string? protocol = null, string? uses = null, string? input = null, string? inputTemplate = null, string? output = null, string? outputTemplate = null)
        {
            return GetCode(BuildSoftwareSystemInterfaceObject(architectureNamespace, softwareSystemName, name, label, description, protocol, uses, input, inputTemplate, output, outputTemplate));
        }

        public string GetFlowCode()
        {
            CurrectFlowIndentation = string.Empty;

            var result = new StringBuilder();

            result.AppendLine("Flow:");

            IncreaseFlowIndentation();
            result.AppendLine($"{CurrectFlowIndentation}Flows:");
            IncreaseFlowIndentation();

            return result.ToString();
        }

        public string GetLoopFlowCode(string condition)
        {
            var result = new StringBuilder();

            result.AppendLine($"{CurrectFlowIndentation}- Type: Loop");

            IncreaseFlowIndentation();
            result.AppendLine($"{CurrectFlowIndentation}Expression: \"{GetFormattedParams(condition)}\"");
            result.AppendLine($"{CurrectFlowIndentation}Flows:");
            IncreaseFlowIndentation();

            return result.ToString();
        }

        public string GetEndLoopFlowCode()
        {
            DecreaseFlowIndentation();
            DecreaseFlowIndentation();
            return string.Empty;
        }

        public string GetIfFlowCode(string condition)
        {
            var result = new StringBuilder();

            result.AppendLine($"{CurrectFlowIndentation}- Type: If");

            IncreaseFlowIndentation();
            result.AppendLine($"{CurrectFlowIndentation}Expression: \"{GetFormattedParams(condition)}\"");
            result.AppendLine($"{CurrectFlowIndentation}Flows:");
            IncreaseFlowIndentation();

            return result.ToString();
        }

        public string GetEndIfFlowCode()
        {
            DecreaseFlowIndentation();
            DecreaseFlowIndentation();
            return string.Empty;
        }

        public string GetElseIfFlowCode(string condition)
        {
            var result = new StringBuilder();

            result.AppendLine($"{CurrectFlowIndentation}- Type: ElseIf");

            IncreaseFlowIndentation();
            result.AppendLine($"{CurrectFlowIndentation}Expression: \"{GetFormattedParams(condition)}\"");
            result.AppendLine($"{CurrectFlowIndentation}Flows:");
            IncreaseFlowIndentation();

            return result.ToString();
        }

        public string GetEndElseIfFlowCode()
        {
            DecreaseFlowIndentation();
            DecreaseFlowIndentation();
            return string.Empty;
        }

        public string GetElseFlowCode()
        {
            var result = new StringBuilder();

            result.AppendLine($"{CurrectFlowIndentation}- Type: Else");

            IncreaseFlowIndentation();
            result.AppendLine($"{CurrectFlowIndentation}Flows:");
            IncreaseFlowIndentation();

            return result.ToString();
        }

        public string GetEndElseFlowCode()
        {
            DecreaseFlowIndentation();
            DecreaseFlowIndentation();
            return string.Empty;
        }

        public string GetReturnFlowCode(string? expression = null)
        {
            var result = new StringBuilder();

            result.AppendLine($"{CurrectFlowIndentation}- Type: Return");

            IncreaseFlowIndentation();
            result.AppendLine($"{CurrectFlowIndentation}Expression: \"{GetFormattedParams(expression ?? string.Empty)}\"");

            DecreaseFlowIndentation();

            return result.ToString();
        }

        public string GetTryFlowCode()
        {
            var result = new StringBuilder();

            result.AppendLine($"{CurrectFlowIndentation}- Type: Try");

            IncreaseFlowIndentation();
            result.AppendLine($"{CurrectFlowIndentation}Flows:");
            IncreaseFlowIndentation();

            return result.ToString();
        }

        public string GetEndTryFlowCode()
        {
            DecreaseFlowIndentation();
            DecreaseFlowIndentation();
            return string.Empty;
        }

        public string GetCatchFlowCode(string? exception = null)
        {
            var result = new StringBuilder();

            result.AppendLine($"{CurrectFlowIndentation}- Type: Catch");

            IncreaseFlowIndentation();
            result.AppendLine($"{CurrectFlowIndentation}Expression: \"{GetFormattedParams(exception ?? string.Empty)}\"");
            result.AppendLine($"{CurrectFlowIndentation}Flows:");
            IncreaseFlowIndentation();

            return result.ToString();
        }

        public string GetEndCatchFlowCode()
        {
            DecreaseFlowIndentation();
            DecreaseFlowIndentation();
            return string.Empty;
        }

        public string GetFinallyFlowCode()
        {
            var result = new StringBuilder();

            result.AppendLine($"{CurrectFlowIndentation}- Type: Finally");

            IncreaseFlowIndentation();
            result.AppendLine($"{CurrectFlowIndentation}Flows:");
            IncreaseFlowIndentation();

            return result.ToString();
        }

        public string GetEndFinallyFlowCode()
        {
            DecreaseFlowIndentation();
            DecreaseFlowIndentation();
            return string.Empty;
        }

        public string GetThrowExceptionFlowCode(string? exception = null)
        {
            var result = new StringBuilder();

            result.AppendLine($"{CurrectFlowIndentation}- Type: ThrowException");

            IncreaseFlowIndentation();
            result.AppendLine($"{CurrectFlowIndentation}Expression: \"{GetFormattedParams(exception ?? string.Empty)}\"");

            DecreaseFlowIndentation();

            return result.ToString();
        }

        private string GetFormattedParams(string @params)
        {
            return @params.Replace(Environment.NewLine, "\\n").Replace("\"", "\"\"");
        }

        public string GetUseFlowCode(string alias)
        {
            var result = new StringBuilder();

            result.AppendLine($"{CurrectFlowIndentation}- Type: Use");

            IncreaseFlowIndentation();
            result.AppendLine($"{CurrectFlowIndentation}Expression: \"{alias}\"");

            DecreaseFlowIndentation();

            return result.ToString();
        }

        public string GetBusinessProcessCode(string architectureNamespace, string name, string label, string businessActivitiesCode, string? description = null)
        {
            var deserializedData = new DeserializerBuilder().Build().Deserialize<List<Dictionary<string, object>>>(businessActivitiesCode);
            var businessActivities = JArray.FromObject(deserializedData);
            return GetCode(BuildBusinessProcessObject(architectureNamespace, name, label, businessActivities, description));
        }

        public string GetBusinessActivityCode(string name, string actor, string[] uses, string? description = null)
        {
            return GetCode(BuildBusinessActivityObject(name, actor, uses));
        }
    }
}
