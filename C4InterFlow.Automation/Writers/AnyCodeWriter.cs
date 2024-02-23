using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace C4InterFlow.Automation.Writers
{
    public class AnyCodeWriter
    {
        internal static readonly string SoftwareSystemInterfacePattern = @"^(?!.*\.(Containers|Components)\.).*\.Interfaces\.\w+$";
        internal static readonly string  ContainerInterfacePattern = @"^.*\.Containers\.\w+\.Interfaces\.\w+$";
        internal static string GetSoftwareSystemAlias(string architectureNamespace, string softwareSystemName)
        {
            if (string.IsNullOrEmpty(architectureNamespace) || string.IsNullOrEmpty(softwareSystemName)) return string.Empty;

            return $"{architectureNamespace}.SoftwareSystems.{softwareSystemName}";
        }

        internal static string GetActorAlias(string architectureNamespace, string actorName)
        {
            if (string.IsNullOrEmpty(architectureNamespace) || string.IsNullOrEmpty(actorName)) return string.Empty;

            return $"{architectureNamespace}.Actors.{actorName}";
        }

        internal static string GetBusinessProcessAlias(string architectureNamespace, string businessProcessName)
        {
            if (string.IsNullOrEmpty(architectureNamespace) || string.IsNullOrEmpty(businessProcessName)) return string.Empty;

            return $"{architectureNamespace}.BusinessProcesses.{businessProcessName}";
        }

        internal static string GetContainerAlias(string architectureNamespace, string softwareSystemName, string containerName)
        {
            if (string.IsNullOrEmpty(architectureNamespace) || string.IsNullOrEmpty(softwareSystemName) || string.IsNullOrEmpty(containerName)) return string.Empty;

            return $"{architectureNamespace}.SoftwareSystems.{softwareSystemName}.Containers.{containerName}";
        }

        internal static string GetComponentAlias(string architectureNamespace, string softwareSystemName, string containerName, string componentName)
        {
            if (string.IsNullOrEmpty(softwareSystemName) || string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(componentName)) return string.Empty;

            return $"{architectureNamespace}.SoftwareSystems.{softwareSystemName}.Containers.{containerName}.Components.{componentName}";
        }

        internal static string GetEntityAlias(string architectureNamespace, string softwareSystemName, string containerName, string entityName)
        {
            if (string.IsNullOrEmpty(softwareSystemName) || string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(entityName)) return string.Empty;

            return $"{architectureNamespace}.SoftwareSystems.{softwareSystemName}.Containers.{containerName}.Entities.{entityName}";
        }

        internal static string GetComponentInterfaceAlias(string componentAlias, string interfaceName)
        {
            if (string.IsNullOrEmpty(componentAlias) || string.IsNullOrEmpty(interfaceName)) return string.Empty;

            return $"{componentAlias}.Interfaces.{interfaceName}";
        }

        internal static string GetContainerInterfaceAlias(string architectureNamespace, string softwareSystemName, string containerName, string interfaceName)
        {
            if (string.IsNullOrEmpty(softwareSystemName) || string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(interfaceName)) return string.Empty;

            return $"{architectureNamespace}.SoftwareSystems.{softwareSystemName}.Containers.{containerName}.Interfaces.{interfaceName}";
        }

        internal static string GetSoftwareSystemInterfaceAlias(string architectureNamespace, string softwareSystemName, string interfaceName)
        {
            if (string.IsNullOrEmpty(architectureNamespace) || string.IsNullOrEmpty(softwareSystemName) || string.IsNullOrEmpty(interfaceName)) return string.Empty;

            return $"{architectureNamespace}.SoftwareSystems.{softwareSystemName}.Interfaces.{interfaceName}";
        }

        internal static string EnsureDoubleQuotes(string text)
        {
            if (string.IsNullOrEmpty(text)) return "\"\"";

            var result = text;

            if (!result.StartsWith("\""))
                result = $"\"{result}";

            if (!result.EndsWith("\""))
                result = $"{result}\"";

            return result;

        }

        internal static string GetName(string text)
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
            return Utils.GetLabel(text);
        }

        private static string GetFormattedParams(string @params)
        {
            return @params.Replace(Environment.NewLine, "\\n").Replace("\"", "\"\"");
        }
    }
}
