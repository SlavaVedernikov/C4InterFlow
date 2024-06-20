using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using C4InterFlow.Automation;
using C4InterFlow.Cli;
using C4InterFlow.Structures;
using C4InterFlow.Structures.Interfaces;
using C4InterFlow.Structures.Relationships;

namespace C4InterFlow
{
    public class Utils
    {
        public static IEnumerable<T> GetNestedInstances<T>(string? alias) where T : Structure
        {
            return AaCReaderContext.Strategy.GetNestedInstances<T>(alias);
        }

        public static T? GetInstance<T>(string? alias) where T : Structure
        {
            return AaCReaderContext.Strategy.GetInstance<T>(alias);
        }

        public static T Clone<T>(T source)
        {
            var result = default(T);

            if (source != null)
            {
                var json = JsonSerializer.Serialize(source);
                result = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions() { IncludeFields = true});
            }

            return result;
        }

        public static string? GetLabel(string? text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            return Regex.Replace(Regex.Replace(Regex.Replace(text.Replace("\"", string.Empty), "([A-Z]+)([A-Z][a-z])", "$1 $2"), "((?<=[a-z])[A-Z]|A-Z)", " $1"), "((?<=[a-zA-Z])[0-9]|(?<=[0-9])[a-zA-Z])", " $1").Trim();
        }

        public static string GetContainerAlias(string alias)
        {
            var match = Regex.Match(alias, @"(.*\.SoftwareSystems\.[^.]+\.Containers\.[^.]+)");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        public static string GetSoftwareSystemAlias(string alias)
        {
            var match = Regex.Match(alias, @"^(.*?)(?:\.Interfaces|\.Containers)");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        public static bool TryGetNamespaceAlias(string alias, out string? namespaceAlias)
        {
            namespaceAlias = null;

            int index = alias.IndexOf("SoftwareSystems");

            if (index < 0)
            {
                index = alias.IndexOf("BusinessProcesses");
            }

            if (index < 0)
            {
                index = alias.IndexOf("Actors");
            }

            if (index >= 0)
            {
                namespaceAlias = alias.Substring(0, index - 1);
            }
            else
            {
                namespaceAlias = alias;
            }

            return namespaceAlias != null;
        }
    }
}
