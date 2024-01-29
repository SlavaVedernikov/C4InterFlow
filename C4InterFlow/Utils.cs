using System.Reflection;
using System.Text.Json;
using C4InterFlow.Cli;
using C4InterFlow.Elements;
using C4InterFlow.Elements.Interfaces;
using C4InterFlow.Elements.Relationships;

namespace C4InterFlow
{
    public class Utils
    {
        private static IList<string> _nonAssemblyPaths = new List<string>();
        private static Dictionary<string, object> _aliasToStructureMap = new Dictionary<string, object>();
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
            return CommandExecutionContext.CurrentArchitectureAsCodeReaderContext.GetType(alias);
        }

        public static T? GetInstance<T>(string alias) where T : class
        {
            return CommandExecutionContext.CurrentArchitectureAsCodeReaderContext.GetInstance<T>(alias);
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
    }
}
