using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using C4InterFlow.Automation;
using C4InterFlow.Cli;
using C4InterFlow.Elements;
using C4InterFlow.Elements.Interfaces;
using C4InterFlow.Elements.Relationships;

namespace C4InterFlow
{
    public class Utils
    {
        public class ExternalSystem : ISoftwareSystemInstance
        {
            public const string ALIAS = $"{nameof(C4InterFlow)}.{nameof(Utils)}.{nameof(ExternalSystem)}";
            public static SoftwareSystem Instance => new SoftwareSystem(ALIAS, "External")
            {
                Boundary = Boundary.External
            };

            public class Interfaces
            {
                public class ExternalInterface : IInterfaceInstance
                {
                    public const string ALIAS = $"{nameof(C4InterFlow)}.{nameof(Utils)}.{nameof(ExternalSystem)}.{nameof(Interfaces)}.{nameof(ExternalInterface)}";
                    public static Interface Instance => new Interface(
                    ExternalSystem.ALIAS,
                    ALIAS,
                    "External");
                }
            }
        }

        public static Type? GetType(string alias)
        {
            return ArchitectureAsCodeReaderContext.Strategy.GetType(alias);
        }

        public static T? GetInstance<T>(string alias) where T : class
        {
            return ArchitectureAsCodeReaderContext.Strategy.GetInstance<T>(alias);
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

            return Regex.Replace(Regex.Replace(text.Replace("\"", string.Empty), "((?<=[a-z])[A-Z]|A-Z)", " $1"), "((?<=[a-zA-Z])[0-9]|(?<=[0-9])[a-zA-Z])", " $1").Trim();
        }
    }
}
