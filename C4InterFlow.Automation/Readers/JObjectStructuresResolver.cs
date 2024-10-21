using C4InterFlow.Structures;
using C4InterFlow.Structures.Interfaces;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using C4InterFlow.Commons;
using Serilog;

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
            CompletePartialUseFlowExpressions();
        }

        public T? GetInstance<T>(string? alias) where T : Structure
        {
            var result = default(T);

            if (string.IsNullOrEmpty(alias)) return result;

            if (_aliasToStructureMap.ContainsKey(alias))
            {
                return _aliasToStructureMap[alias] as T;
            }

            try
            {
                var token = RootJObject?.SelectToken(alias);

                if (token == null) return result;

                var collectionToken = token?.Parent?.Parent;
                var ownerToken = collectionToken?.Parent?.Parent;

                if (collectionToken == null || ownerToken == null) return result;

                var label = token!["Label"]?.ToString() ?? Utils.GetLabel(alias.Split('.').Last()) ?? string.Empty;
                var description = token?["Description"]?.ToString() ?? string.Empty;
                var tagsToken = token?["Tags"];
                var tags = tagsToken?.Type == JTokenType.Array && tagsToken.All(x => x.Type == JTokenType.String) ? tagsToken.ToObject<string[]>() : new string[] { };
                var icon = token?["Icon"]?.ToString() ?? string.Empty;

                token!["Tags"]?.ToObject<string[]>();
                switch (collectionToken.Path.Split('.').Last())
                {
                    case "Actors":
                        var typeName = token!["Type"]?.ToString();

                        if (!string.IsNullOrEmpty(typeName))
                        {
                            var type = Type.GetType($"{nameof(C4InterFlow)}.{nameof(Structures)}.{typeName},{nameof(C4InterFlow)}");

                            if (type != null)
                            {
                                var typeConstructor = type.GetConstructor(new[] { typeof(string), typeof(string), typeof(string) });

                                if (typeConstructor != null)
                                {
                                    result = typeConstructor.Invoke(new object[] { alias, label, description }) as T;
                                }
                            }
                        }

                        break;
                    case "BusinessProcesses":
                        var activities = token!["Activities"]?.ToObject<Activity[]>();

                        if (activities != null)
                        {
                            result = new BusinessProcess(activities, alias, label)
                            {
                                Description = description
                            } as T;
                        }
                        break;
                    case "Interfaces":
                        var interfaceFlow = token!["Flow"]?.ToObject<Flow>();
                        var protocol = token?["Protocol"]?.ToString() ?? string.Empty;
                        var path = token?["Path"]?.ToString() ?? string.Empty;
                        if (interfaceFlow != null)
                        {
                            interfaceFlow.Owner = alias;
                        }
                        else
                        {
                            interfaceFlow = new Flow(alias);
                            var interfaceFlows = token!["Flows"]?.ToObject<List<Flow>>();
                            if (interfaceFlows != null)
                            {
                                interfaceFlow.AddFlowsRange(interfaceFlows);
                            }
                        }

                        result = new Interface(ownerToken.Path, alias, label)
                        {
                            Flow = interfaceFlow,
                            Protocol = protocol,
                            Path = path,
                            Description = description
                        } as T;
                        break;
                    case "SoftwareSystems":
                        var softwareSystemsBoundaryName = token!["Boundary"]?.ToString() ?? string.Empty;
                        result = new SoftwareSystem(alias, label, description)
                        {
                            Boundary = !string.IsNullOrEmpty(softwareSystemsBoundaryName) &&
                                Enum.TryParse(softwareSystemsBoundaryName, out Boundary softwareSystemsBoundary) ?
                                softwareSystemsBoundary : Boundary.Internal,
                            Tags = tags ?? new string[] { },
                            Icon = icon

                        } as T;
                        break;
                    case "Containers":
                        var containerTypeName = token!["ContainerType"]?.ToString() ?? string.Empty;
                        var containerTechnology = token!["Technology"]?.ToString() ?? string.Empty;
                        result = new Container(ownerToken.Path, alias, label)
                        {
                            ContainerType = !string.IsNullOrEmpty(containerTypeName) &&
                                Enum.TryParse(containerTypeName, out ContainerType containerType) ?
                                containerType : ContainerType.None,
                            Description = description,
                            Technology = containerTechnology,
                            Tags = tags ?? new string[] { },
                            Icon = icon
                        } as T;
                        break;
                    case "Components":
                        var componentTypeName = token!["ComponentType"]?.ToString() ?? string.Empty;
                        var componentTechnology = token?["Technology"]?.ToString() ?? string.Empty;
                        result = new Component(ownerToken.Path, alias, label)
                        {
                            ComponentType = !string.IsNullOrEmpty(componentTypeName) &&
                                Enum.TryParse(componentTypeName, out ComponentType componentType) ?
                                componentType : ComponentType.None,
                            Description = description,
                            Technology = componentTechnology,
                            Tags = tags ?? new string[] { },
                            Icon = icon
                        } as T;
                        break;
                    case "Entities":
                        result = new Entity(ownerToken.Path, alias, label, EntityType.None)
                        {
                            Description = description
                        } as T;
                        break;
                    case "Attributes":
                        var valueToken = token!["Value"];
                        object? value = null;

                        if (valueToken != null)
                        {
                            if (valueToken.Type == JTokenType.Object)
                            {
                                value = valueToken.ToObject<Dictionary<string, object>>();
                            }
                            else if (valueToken.Type == JTokenType.Array)
                            {
                                value = valueToken.ToObject<Dictionary<string, object>[]>();
                            }
                            else
                            {
                                value = valueToken.ToString();
                            }
                        }

                        result = new StructureAttribute(alias, label, value) as T;
                        break;
                    default:
                        break;
                }

                if (result != null && !_aliasToStructureMap.ContainsKey(alias))
                {
                    _aliasToStructureMap.TryAdd(alias, result);
                }

                
            }
            catch
            {
                Log.Error("Failed to resolve alias {Alias}", alias);
                throw;
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
                    Log.Information("Resolving wildcard Structure for {Structure}", item);
                }

                result.AddRange(RootJObject.SelectTokens(item)
                    .Select(x => x.Path)
                    .Where(x => !x.StartsWith($"{nameof(C4InterFlow)}.{nameof(SoftwareSystems)}")));
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

        public void CompletePartialUseFlowExpressions()
        {
            var interfaces = RootJObject.SelectTokens("..Interfaces.*");

            foreach (var @interface in interfaces)
            {
                var useFlows = @interface.SelectTokens("..[?(@.Type=='Use')]");

                foreach (var useFlow in useFlows)
                {
                    var usesInterfaceAlias = useFlow["Expression"].ToString();
                    var usesInterface = RootJObject.SelectToken(usesInterfaceAlias);

                    if (usesInterface == null)
                    {
                        var parent = @interface;
                        do
                        {
                            parent = parent.Parent;
                            usesInterface = parent.SelectToken(usesInterfaceAlias);
                        } while (usesInterface == null && parent.Parent != null);

                        if(usesInterface != null)
                        {
                            useFlow["Expression"] = usesInterface.Path;
                        }
                    }
                }
            }
        }

        public void Validate(out IEnumerable<LogMessage> errors)
        {
            var errorsInternal = new List<LogMessage>();
            
            var interfaces = RootJObject.SelectTokens("..Interfaces.*");

            foreach (var @interface in interfaces)
            {
                var useFlows = @interface.SelectTokens("..[?(@.Type=='Use')]");

                foreach(var useFlow in  useFlows)
                {
                    var usesInterfaceAlias = useFlow["Expression"].ToString();
                    var usesInterface = RootJObject.SelectToken(usesInterfaceAlias);

                    if (usesInterface == null)
                    {
                        var error = new LogMessage(
                            "Cannot resolve Interface {InterfaceAlias} referenced in Use Flow(s) of {Interface} Interface.", usesInterfaceAlias, @interface.Path);
                        errorsInternal.Add(error);
                    }
                }
            }

            var activitiesTokens = RootJObject.SelectTokens("..Activities");

            foreach (var activities in activitiesTokens)
            {
                foreach(var activity in activities)
                {
                    var useFlows = activity.SelectTokens("..[?(@.Type=='Use')]");

                    foreach (var useFlow in useFlows)
                    {
                        var usesInterfaceAlias = useFlow["Expression"].ToString();
                        var usesInterface = RootJObject.SelectToken(usesInterfaceAlias);

                        if (usesInterface == null)
                        {
                            var error = new LogMessage(
                                "Cannot resolve Interface {InterfaceAlias} referenced in Use Flow(s) of {Activity} Activity.", usesInterfaceAlias, $"{activity.Path} - {activity["Label"]}");
                            errorsInternal.Add(error);
                        }
                    }
                }
                
            }

            errors = errorsInternal;
        }
    }
}
