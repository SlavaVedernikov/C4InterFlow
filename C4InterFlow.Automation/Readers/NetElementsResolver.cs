using C4InterFlow.Structures;
using C4InterFlow.Structures.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using static C4InterFlow.Automation.Writers.CsvToAnyAaCWriter;

namespace C4InterFlow.Automation.Readers
{
    public class NetElementsResolver : IElementsResolver
    {
        private static IList<string> _nonAssemblyPaths = new List<string>();
        private static ConcurrentDictionary<string, object> _aliasToStructureMap = new ConcurrentDictionary<string, object>();

        public NetElementsResolver() { }

        private string[]? ArchitectureInputPaths { get; set; }
        private IEnumerable<Assembly> ArchitectureAssemblies  { get; set; }
        public NetElementsResolver(string[] architectureInputPaths)
        {
            ArchitectureInputPaths = architectureInputPaths;
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
                    foreach (var item in assemblies)
                    {
                        result = Type.GetType($"{path}, {item.FullName}");
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

        public IEnumerable<string> ResolveStructures(IEnumerable<string> structures)
        {
            var result = new List<string>();

            if (structures == null) return result;

            foreach (var item in structures)
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
                                    foreach (var nestedType in type.GetNestedTypes())
                                    {
                                        newTypes.Add(nestedType.FullName.Replace("+", "."));
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
                            newTypes.AddRange(
                                GetTypes(segmentItem)
                                    .Select(x => x.FullName.Replace("+", ".")));
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

            if (ArchitectureAssemblies == null) return result;

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
            if (ArchitectureInputPaths == null || ArchitectureInputPaths.Length == 0) return new List<Assembly>();

            var paths = new List<string>();

            foreach (var path in ArchitectureInputPaths)
            {
                paths.AddRange(Directory.GetFiles(Directory.GetCurrentDirectory(), path, SearchOption.TopDirectoryOnly));
            }

            foreach(var path in paths.Distinct())
            {
                try
                {
                    AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Failed to load an assembly from path '{path}': {ex.Message}");
                }
            }
            return paths.Select(AssemblyLoadContext.Default.LoadFromAssemblyPath);
        }

        private IEnumerable<Type> GetTypes(string @namespace)
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
