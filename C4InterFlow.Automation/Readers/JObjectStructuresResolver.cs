using C4InterFlow.Structures;
using C4InterFlow.Structures.Interfaces;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Reflection;

namespace C4InterFlow.Automation.Readers
{
    public class JObjectStructuresResolver : IStructuresResolver
    {
        private static ConcurrentDictionary<string, object> _aliasToStructureMap = new ConcurrentDictionary<string, object>();
        private JObject? RootJObject { get; init; }

        public JObjectStructuresResolver()
        {
            RootJObject = null;
        }
        public JObjectStructuresResolver(JObject rootJObject)
        {
            RootJObject = rootJObject;
        }

        public T? GetInstance<T>(string? alias) where T : Structure
        {
            var result = default(T);

            if (string.IsNullOrEmpty(alias)) return result;

            if (_aliasToStructureMap.ContainsKey(alias))
            {
                return _aliasToStructureMap[alias] as T;
            }

            var token = RootJObject?.SelectToken(alias);

            if (token == null) return result;

            var collectionToken = token?.Parent?.Parent;
            var ownerToken = collectionToken?.Parent?.Parent;

            if (collectionToken == null || ownerToken == null) return result;

            var label = token?["Label"]?.ToString() ?? Utils.GetLabel(alias.Split('.').Last()) ?? string.Empty;

            switch (collectionToken.Path.Split('.').Last())
            {
                case "Actors":
                    var typeName = token?["Type"]?.ToString();

                    if (!string.IsNullOrEmpty(typeName))
                    {
                        var type = Type.GetType($"{nameof(C4InterFlow)}.{nameof(Structures)}.{typeName},{nameof(C4InterFlow)}");

                        if (type != null)
                        {
                            var typeConstructor = type.GetConstructor(new[] { typeof(string), typeof(string) });

                            if (typeConstructor != null)
                            {
                                result = typeConstructor.Invoke(new object[] { alias, label }) as T;
                            }
                        }
                    }

                    break;
                case "BusinessProcesses":
                    var activities = token?["Activities"]?.ToObject<Activity[]>();

                    if (activities != null)
                    {
                        result = new BusinessProcess(activities, label) as T;
                    }
                    break;
                case "Interfaces":
                    var interfaceFlow = token?["Flow"]?.ToObject<Flow>();
                    if (interfaceFlow != null)
                    {
                        interfaceFlow.Owner = alias;
                    }
                    else
                    {
                        interfaceFlow = new Flow(alias);
                    }

                    result = new Interface(ownerToken.Path, alias, label)
                    {
                        Flow = interfaceFlow
                    } as T;
                    break;
                case "SoftwareSystems":
                    var softwareSystemsBoundaryName = token?["Boundary"]?.ToString();
                    result = new SoftwareSystem(alias, label)
                    {
                        Boundary = !string.IsNullOrEmpty(softwareSystemsBoundaryName) &&
                        Enum.TryParse(softwareSystemsBoundaryName, out Boundary softwareSystemsBoundary) ?
                        softwareSystemsBoundary : Boundary.Internal
                    } as T;
                    break;
                case "Containers":
                    var containerTypeName = token?["ContainerType"]?.ToString();
                    result = new Container(ownerToken.Path, alias, label)
                    {
                        ContainerType = !string.IsNullOrEmpty(containerTypeName) &&
                        Enum.TryParse(containerTypeName, out ContainerType containerType) ?
                        containerType : ContainerType.None
                    } as T;
                    break;
                case "Components":
                    var componentTypeName = token?["ComponentType"]?.ToString();
                    result = new Component(ownerToken.Path, alias, label)
                    {
                        ComponentType = !string.IsNullOrEmpty(componentTypeName) &&
                        Enum.TryParse(componentTypeName, out ComponentType componentType) ?
                        componentType : ComponentType.None
                    } as T;
                    break;
                case "Entities":
                    result = new Entity(ownerToken.Path, alias, label, EntityType.None) as T;
                    break;
                default:
                    break;
            }

            if (result != null && !_aliasToStructureMap.ContainsKey(alias))
            {
                _aliasToStructureMap.TryAdd(alias, result);
            }

            return result;
        }

        public IEnumerable<string> ResolveStructures(IEnumerable<string> structures)
        {
            var result = new List<string>();

            if (structures == null) return result;

            foreach (var item in structures)
            {
                if (item.Contains(".*"))
                {
                    Console.WriteLine($"Resolving wildcard Structures for '{item}'.");
                }

                result.AddRange(RootJObject.SelectTokens(item).Select(x => x.Path));
            }

            return result;
        }

        public IEnumerable<Interface> GetAllInterfaces()
        {
            var result = new List<Interface>();

            result.AddRange(GetInterfaces(RootJObject.SelectTokens($"..Interfaces.*")));

            return result;
        }

        private IEnumerable<Interface> GetInterfaces(IEnumerable<JToken> tokens)
        {
            var result = new List<Interface>();

            foreach (var token in tokens)
            {
                var interfaceInstance = GetInstance<Interface>(token.Path);
                if (interfaceInstance != null)
                {
                    result.Add(interfaceInstance);
                }
            }
            return result;
        }

        public IEnumerable<T> GetNestedInstances<T>(string? alias) where T : Structure
        {
            var result = new List<T>();

            if (string.IsNullOrEmpty(alias)) return result;

            var token = RootJObject?.SelectToken(alias);

            if (token == null) return result;

            foreach (var item in token.Children())
            {
                var instance = GetInstance<T>(item.Path);

                if (instance != null)
                {
                    result.Add(instance);
                }
            }

            return result;
        }
    }
}
