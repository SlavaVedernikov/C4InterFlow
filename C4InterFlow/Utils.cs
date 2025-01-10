using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using C4InterFlow.Automation;
using C4InterFlow.Cli;
using C4InterFlow.Structures;
using C4InterFlow.Structures.Interfaces;
using C4InterFlow.Structures.Relationships;
using C4InterFlow.Structures.Views;
using Serilog;
using static System.Net.Mime.MediaTypeNames;

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
            T? result = AaCReaderContext.Strategy.GetInstance<T>(alias);

            if(result == null)
            {
                Log.Debug("Could not get the instance for alias '{Alias}'", alias);
            }
            return result;
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

        public static string GetLabelFromAlias(string alias)
        {
            return GetLabel(alias.Split(".").Last());
        }

        public static string GetPathFromAlias(string alias)
        {
            return Path.Join(alias.Split('.').Select(x => GetLabel(x)).ToArray());
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

        public static string GetStructureAlias<T>()
        {
            return GetStructureAlias(typeof(T));
        }

        public static string GetStructureAlias(Type type)
        {
            return type.FullName?.Replace('+', '.') ?? string.Empty;
        }

        public static string GetSoftwareSystemAlias(string alias)
        {
            var match = Regex.Match(alias, @"^(.*?)(?:\.Interfaces|\.Containers)");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        public static string GetInterfaceOwnerAlias(string alias)
        {
            var match = Regex.Match(alias, @"^(.*?)(?:\.Interfaces)");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        public static bool TryGetNamespaceAlias(string alias, out string? namespaceAlias)
        {
            namespaceAlias = null;

            int index = alias.IndexOf(".SoftwareSystems.");

            if (index < 0)
            {
                index = alias.IndexOf(".BusinessProcesses.");
            }

            if (index < 0)
            {
                index = alias.IndexOf(".Actors.");
            }

            if (index < 0)
            {
                index = alias.IndexOf(".Views.");
            }

            if (index >= 1)
            {
                namespaceAlias = alias.Substring(0, index);
            }
            //TODO: Disallow these tokens without at least one level of Namespace
            else if(!alias.StartsWith("SoftwareSystems.") &&
                !alias.StartsWith("BusinessProcesses.") &&
                !alias.StartsWith("Actors.") &&
                !alias.StartsWith("Views."))
            {
                namespaceAlias = alias;
            }

            return namespaceAlias != null;
        }
    }
}
