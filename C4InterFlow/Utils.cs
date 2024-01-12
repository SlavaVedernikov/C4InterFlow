using System.Reflection;
using System.Text.Json;
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
            Type? result = null;

            if (alias == null) return result;

            var path = string.Empty;

            var assemblies = new List<Assembly>();

            

            foreach (var segment in alias.Split('.'))
            {
                Assembly? assembly = null;

                if (!string.IsNullOrEmpty(path)) path += ".";

                path += segment;

                if (!_nonAssemblyPaths.Contains(path))
                {
                    try
                    {
                        assembly = AppDomain.CurrentDomain.Load(new AssemblyName(path));
                        assemblies.Add(assembly);
                    }
                    catch
                    {
                        _nonAssemblyPaths.Add(path);
                    }
                }

                if (result == null && assemblies.Count > 0)
                {
                    foreach(var item in assemblies)
                    {
                        result = Type.GetType($"{path}, {item.FullName}");
                        if(result != null)
                        {
                            break;
                        }
                    }
                    continue;
                }

                if (result != null)
                {
                    result = result.GetNestedType(segment);
                }
            }

            return result;
        }

        public static IEnumerable<Type> GetTypes(string @namespace)
        {
            var result = new List<Type>();

            if (@namespace == null) return result;

            var path = string.Empty;

            var assemblies = new List<Assembly>();

            foreach (var segment in @namespace.Split('.'))
            {
                Assembly? assembly = null;

                if (!string.IsNullOrEmpty(path)) path += ".";

                path += segment;

                if (!_nonAssemblyPaths.Contains(path))
                {
                    try
                    {
                        assembly = AppDomain.CurrentDomain.Load(new AssemblyName(path));
                        assemblies.Add(assembly);
                    }
                    catch
                    {
                        _nonAssemblyPaths.Add(path);
                    }
                }

                if (assemblies.Count > 0)
                {
                    foreach (var item in assemblies)
                    {
                        result.AddRange(item.GetTypes().Where(x =>
                        x.Namespace == @namespace && !x.IsNested));
                    }
                }
            }

            return result.Distinct();
        }
        public static T? GetInstance<T>(string alias) where T : class
        {
            if (string.IsNullOrEmpty(alias)) return default(T);

            if(_aliasToStructureMap.ContainsKey(alias))
            {
                return _aliasToStructureMap[alias] as T;
            }

            var type = GetType(alias);

            var result = type?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null, null) as T;

            if (result != null && !_aliasToStructureMap.ContainsKey(alias))
            {
                _aliasToStructureMap.Add(alias, result);
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
    }
}
