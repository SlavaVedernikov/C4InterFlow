using C4InterFlow.Structures;
using C4InterFlow.Structures.Interfaces;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using C4InterFlow.Commons;
using Serilog;
using C4InterFlow.Structures.Views;

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

                var fullAlias = token.Path;
                var label = token!["Label"]?.ToString() ?? Utils.GetLabel(alias.Split('.').Last()) ?? string.Empty;
                var description = token?["Description"]?.ToString() ?? string.Empty;
                var tagsToken = token?["Tags"];
                var tags = tagsToken?.Type == JTokenType.Array && tagsToken.All(x => x.Type == JTokenType.String) ? tagsToken.ToObject<string[]>() : new string[] { };
                var icon = token?["Icon"]?.ToString() ?? string.Empty;

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
                                    result = typeConstructor.Invoke(new object[] { fullAlias, label, description }) as T;
                                }
                            }
                        }

                        break;
                    case "Activities":
                        var activityFlow = new Flow(fullAlias);
                        var activityFlows = token!["Flows"]?.ToObject<List<Flow>>();
                        if (activityFlows != null)
                        {
                            activityFlows.ForEach(x => x.SetParents());
                            activityFlow.AddFlowsRange(activityFlows);
                        }


                        var activity = new Activity(fullAlias, activityFlows.ToArray(), ownerToken.Path, label);

                        result = activity as T;
                        break;
                    case "BusinessProcesses":
                        var activityTokens = token!["Activities"];

                        if (activityTokens != null)
                        {
                            var activities = new List<Activity>();
                            foreach(var activityToken in activityTokens)
                            {
                                if(activityToken.Type == JTokenType.Object)
                                {
                                    var activityActor = activityToken?["Actor"]?.ToString();
                                    var activityLabel = activityToken!["Label"]?.ToString();

                                    var activityItemFlows = activityToken!["Flows"]?.ToObject<List<Flow>>();
                                    if (activityItemFlows != null)
                                    {
                                        var activityItemFlow = new Flow();
                                        activityItemFlow.AddFlowsRange(activityItemFlows);
                                        if (activityActor == null)
                                        {
                                            var useFlows = activityItemFlow.GetUsesInterfaces();
                                            if (useFlows!.Length > 0)
                                            {
                                                activityActor = useFlows.First().Owner;
                                            }
                                        }
                                        activities.Add(new Activity(activityItemFlow.Flows.ToArray(), activityActor, activityLabel)); ;
                                    }
                                }
                                else if(activityToken.Type == JTokenType.String)
                                {
                                    var activityAlias = activityToken.ToString();

                                    if(activityAlias.Contains(".Interfaces."))
                                    {
                                        var activityInterface = GetInstance<Interface>(activityAlias);
                                        if(activityInterface != null)
                                        {
                                            activities.Add(new Activity(
                                                new Flow().Use(activityAlias),
                                                activityInterface.Owner,
                                                activityInterface.Name));
                                        }


                                    }
                                    else if (activityAlias.Contains(".Activities."))
                                    {
                                        var activityItem = GetInstance<Activity>(activityAlias);
                                        if (activityItem != null)
                                        {
                                            activities.Add(activityItem);
                                        }


                                    }
                                }
                            }

                            result = new BusinessProcess(activities, fullAlias, label)
                            {
                                Description = description
                            } as T;
                        }
                        break;
                    case "Interfaces":

                        var protocol = token?["Protocol"]?.ToString() ?? string.Empty;
                        var path = token?["Path"]?.ToString() ?? string.Empty;

                        var interfaceFlow = new Flow(fullAlias);
                        var interfaceFlows = token!["Flows"]?.ToObject<List<Flow>>();
                        if (interfaceFlows != null)
                        {
                            interfaceFlows.ForEach(x => x.SetParents());
                            interfaceFlow.AddFlowsRange(interfaceFlows);
                        }


                        var @interface = new Interface(ownerToken.Path, fullAlias, label)
                        {
                            Flow = interfaceFlow,
                            Protocol = protocol,
                            Path = path,
                            Description = description
                        };

                        
                        result = @interface as T;
                        break;
                    case "SoftwareSystems":
                        var softwareSystemsBoundaryName = token!["Boundary"]?.ToString() ?? string.Empty;
                        result = new SoftwareSystem(fullAlias, label, description)
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

                        if (!Enum.TryParse(containerTypeName, out ContainerType parsedContainerType) ||
                            string.IsNullOrWhiteSpace(containerTypeName))
                        {
                            parsedContainerType = ContainerType.None;
                        }

                        result = new Container(ownerToken.Path, fullAlias, label)
                        {
                            ContainerType = parsedContainerType,
                            Description = description,
                            Technology = containerTechnology,
                            Tags = tags ?? new string[] { },
                            Icon = icon
                        } as T;
                        break;
                    case "Components":
                        var componentTypeName = token!["ComponentType"]?.ToString() ?? string.Empty;
                        var componentTechnology = token?["Technology"]?.ToString() ?? string.Empty;
                        result = new Component(ownerToken.Path, fullAlias, label)
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
                        result = new Entity(ownerToken.Path, fullAlias, label, EntityType.None)
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

                        result = new StructureAttribute(fullAlias, label, value) as T;
                        break;
                    case "Views":
                        var scopes = token!["Scopes"]?.ToObject<string[]>();
                        var types = token!["Types"]?.ToObject<string[]>();
                        var levelsOfDetails = token!["LevelsOfDetails"]?.ToObject<string[]>();
                        var formats = token!["Formats"]?.ToObject<string[]>();
                        var interfaces = token!["Interfaces"]?.ToObject<string[]>();
                        var businessProcesses = token!["BusinessProcesses"]?.ToObject<string[]>();
                        var namespaces = token!["Namespaces"]?.ToObject<string[]>();
                        var aaCInputPaths = token!["AaCInputPaths"]?.ToObject<string[]>();
                        var expandUpstream = token!["ExpandUpstream"]?.ToObject<bool>();

                        var interfacesInputFile = token!["InterfacesInputFile"]?.ToObject<string>();
                        var outputDir = token!["OutputDir"]?.ToObject<string>();
                        var namePrefix = token!["NamePrefix"]?.ToObject<string>();
                        var outputSubDir = token!["OutputSubDir"]?.ToObject<string>();
                        var aaCReaderStrategy = token!["AaCReaderStrategy"]?.ToObject<string>();
                        var maxLineLabels = default(int?);
                        if (int.TryParse(token!["MaxLineLabels"]?.ToString(), out var maxLineLabelsValue))
                            maxLineLabels = maxLineLabelsValue;
                        
                        result = new View(fullAlias, label) { 
                            BusinessProcesses = businessProcesses,
                            Formats = formats,
                            Interfaces = interfaces,
                            LevelsOfDetails = levelsOfDetails,
                            MaxLineLabels = maxLineLabels,
                            Namespaces = namespaces,
                            Scopes = scopes,
                            Types = types,
                            ExpandUpstream = expandUpstream
                        } as T;
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
