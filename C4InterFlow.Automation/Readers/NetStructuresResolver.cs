using C4InterFlow.Structures;
using C4InterFlow.Structures.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using static C4InterFlow.Automation.Writers.CsvToAnyAaCWriter;

namespace C4InterFlow.Automation.Readers
{
    public class NetStructuresResolver : IStructuresResolver
    {
        private static ConcurrentDictionary<string, object> _aliasToStructureMap = new ConcurrentDictionary<string, object>();

        public NetStructuresResolver()
        {
        
        }

        private string[]? ArchitectureInputPaths { get; set; }
        private IEnumerable<Assembly> ArchitectureAssemblies  { get; set; }
        public NetStructuresResolver(string[] architectureInputPaths)
        {
            var paths = new List<string>();
            paths.AddRange(architectureInputPaths);
            paths.Add($"{nameof(C4InterFlow)}.dll");

            ArchitectureInputPaths = paths.ToArray();

            ArchitectureAssemblies = LoadArchitectureAssemblies();
        }

        public T? GetInstance<T>(string? alias) where T : Structure
        {
            if (string.IsNullOrEmpty(alias)) return default;

            if (_aliasToStructureMap.ContainsKey(alias))
            {
                return _aliasToStructureMap[alias] as T;
            }

            var type = GetType(alias);

            var result = type?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null, null) as T;

            if (result != null && !_aliasToStructureMap.ContainsKey(alias))
            {
                _aliasToStructureMap.TryAdd(alias, result);
            }

            return result;
        }

        private Type? GetType(string alias)
        {
            Type? result = null;

            if (alias == null) return result;

            if(ArchitectureAssemblies?.Any() == false) return result;

            var path = string.Empty;

            foreach (var segment in alias.Split('.'))
            {
                if (!string.IsNullOrEmpty(path)) path += ".";

                path += segment;

                if (result == null)
                {
                    foreach (var item in ArchitectureAssemblies)
                    {
                        result = item.GetType(path);
                        if (result != null)
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

        public IEnumerable<string> ResolveStructures(IEnumerable<string> aliases)
        {
            var result = new List<string>();

            if (aliases == null) return result;

            foreach (var item in aliases)
            {
                var segments = item.Split(".*");
                if (segments.Length == 1)
                {
                    result.Add(item);
                }
                else
                {
                    Console.WriteLine($"Resolving wildcard Structures for '{item}'.");
                    var types = new List<string>();
                    var supersededTypes = new List<string>();
                    foreach (var segmentItem in segments)
                    {
                        var newTypes = new List<string>();
                        if (string.IsNullOrEmpty(segmentItem))
                        {
                            break;
                        }

                        if (types.Count == 0 || types.Count > 0 && segmentItem.StartsWith("."))
                        {
                            if (types.Count > 0)
                            {
                                foreach (var typeItem in types)
                                {
                                    supersededTypes.Add(typeItem);
                                    var newType = GetType(typeItem + segmentItem);
                                    if (newType != null)
                                    {
                                        if (segments.Last().Equals(segmentItem))
                                        {
                                            newTypes.Add(typeItem + segmentItem);
                                        }
                                        else
                                        {
                                            foreach (var nestedType in newType.GetNestedTypes())
                                            {
                                                newTypes.Add(nestedType.FullName.Replace("+", "."));
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var type = GetType(segmentItem);
                                if (type != null)
                                {
                                    var nestedTypes = type.GetNestedTypes();

                                    if (nestedTypes?.Any() == true)
                                    {
                                        foreach (var nestedType in nestedTypes)
                                        {
                                            newTypes.Add(nestedType.FullName.Replace("+", "."));
                                        }
                                    }
                                    else
                                    {
                                        newTypes.Add(type.FullName.Replace("+", "."));
                                    }
                                    
                                }
                            }
                        }
                        else
                        {
                            types.RemoveAll(x => !x.EndsWith(segmentItem));
                        }

                        if (newTypes.Count == 0 && !segmentItem.StartsWith("."))
                        {
                            GetTypes(segmentItem)
                                .Select(x => x.FullName.Replace("+", "."))
                                .ToList()
                                .ForEach(x => newTypes.Add(x));
                        }

                        types.AddRange(newTypes);
                        types.RemoveAll(x => supersededTypes.Contains(x));
                    }


                    result.AddRange(types);
                }

            }

            return result.Distinct();
        }

        public IEnumerable<Interface> GetAllInterfaces()
        {
            var result = new List<Interface>();

            var interfaceClasses = GetAllTypesOfInterface<IInterfaceInstance>();

            foreach (var interfaceClass in interfaceClasses)
            {
                var interfaceInstance = interfaceClass?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null, null) as Interface;

                if (interfaceInstance != null)
                {
                    result.Add(interfaceInstance);
                }
            }

            return result;
        }
        private IEnumerable<Type> GetAllTypesOfInterface<T>()
        {
            var result = new List<Type>();

            if (ArchitectureAssemblies?.Any() == false) return result;

            foreach (var assembly in ArchitectureAssemblies)
            {
                result.AddRange(assembly
                .GetTypes()
                .Where(type => typeof(T).IsAssignableFrom(type) && !type.IsInterface));
            }
            return result;
        }

        private IEnumerable<Assembly> LoadArchitectureAssemblies()
        {
            var result = new List<Assembly>();

           

            if (ArchitectureInputPaths?.Any() == false) return new List<Assembly>();

            

            var paths = new List<string>();

            foreach (var path in ArchitectureInputPaths)
            {
                paths.AddRange(Directory.GetFiles(Directory.GetCurrentDirectory(), path, SearchOption.TopDirectoryOnly));
                paths.AddRange(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, path, SearchOption.TopDirectoryOnly));
            }

            foreach (var path in paths.Distinct())
            {
                try
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);

                    if(assembly != null)
                    {
                        result.Add(assembly);
                    }
                    
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Failed to load an assembly from path '{path}': {ex.Message}");
                }
            }
            return result;
        }

        private IEnumerable<Type> GetTypes(string @namespace)
        {
            var result = new List<Type>();

            if (@namespace == null) return result;

            if (ArchitectureAssemblies?.Any() == false) return result;

            var path = string.Empty;

            foreach (var segment in @namespace.Split('.'))
            {
                if (!string.IsNullOrEmpty(path)) path += ".";

                path += segment;

                foreach (var item in ArchitectureAssemblies)
                {
                    result.AddRange(item.GetTypes().Where(x =>
                    x.Namespace == @namespace && !x.IsNested));
                }
            }

            return result.Distinct();
        }

        public IEnumerable<T> GetNestedInstances<T>(string? alias) where T : Structure
        {
            var result = new List<T>();

            if (string.IsNullOrEmpty(alias)) return result;

            var parentType = GetType(alias);
            var nestedTypes = parentType?.GetNestedTypes();

            if (nestedTypes != null)
            {
                foreach (var type in nestedTypes)
                {
                    var instance = type?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null, null) as T;
                    if (instance != null)
                    {
                        result.Add(instance);
                    }
                }
            }

            return result;
        }
    }
}
