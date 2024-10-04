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
using C4InterFlow.Commons;
using Serilog;
using static C4InterFlow.Automation.Writers.CsvToAnyAaCWriter;

namespace C4InterFlow.Automation.Readers
{
    public class NetStructuresResolver : IStructuresResolver
    {
        private static ConcurrentDictionary<string, object> _aliasToStructureMap = new();

        public NetStructuresResolver()
        {
        }

        private string[]? ArchitectureInputPaths { get; set; }
        private IEnumerable<Assembly> ArchitectureAssemblies { get; set; }

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

            if (type != null)
            {
                var instanceObject = Activator.CreateInstance(type);
                var propertyInfo = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Instance);

                var result = propertyInfo?.GetValue(instanceObject) as T;

                if (result != null && !_aliasToStructureMap.ContainsKey(alias))
                {
                    _aliasToStructureMap.TryAdd(alias, result);
                }

                return result;
            }
            else
            {
                return default(T);
            }
        }

        private Type? GetType(string? alias)
        {
            Type? result = null;

            if (alias == null || ArchitectureAssemblies.Any() == false) return result;

            return ArchitectureAssemblies
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(x => NamespaceMatchesPattern(x.FullName?.Replace('+', '.'), alias));
        }

        public IEnumerable<string> ResolveStructures(IEnumerable<string>? aliases)
        {
            var result = new List<string>();

            foreach (var item in aliases ?? Enumerable.Empty<string>())
            {
                var locatedTypeNames = ArchitectureAssemblies.SelectMany(x => x.GetTypes())
                    .Where(x => x.FullName != null)
                    .Select(x => x.FullName!.Replace('+', '.'));

                var locatedTypes = locatedTypeNames
                    .Where(x => NamespaceMatchesPattern(x, item));

                Log.Debug("Founded types: {Types} for an alias: {Alias}", result, item);


                result.AddRange(locatedTypes.Distinct());
            }

            return result.Distinct();
        }

        public IEnumerable<Interface> GetAllInterfaces()
        {
            var result = new List<Interface>();

            var types = GetAllTypesOfInterface<IInterfaceInstance>();

            foreach (var type in types)
            {
                var instanceObject = Activator.CreateInstance(type);

                if (type?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Instance)
                        ?.GetValue(instanceObject) is Interface interfaceInstance)
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
                paths.AddRange(Directory.GetFiles(Directory.GetCurrentDirectory(), path,
                    SearchOption.TopDirectoryOnly));
                paths.AddRange(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, path,
                    SearchOption.TopDirectoryOnly));
            }

            foreach (var path in paths.Distinct())
            {
                try
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);

                    if (assembly != null)
                    {
                        result.Add(assembly);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to load an assembly from path {AssemblyPath}: {Error}", path, ex.Message);
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

            if(parentType != null)
            {
                var nestedTypes = parentType?.GetNestedTypes();

                if (nestedTypes != null)
                {
                    foreach (var type in nestedTypes)
                    {
                        var instanceObject = Activator.CreateInstance(type);
                        var instance = type?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Instance)
                            ?.GetValue(instanceObject) as T;

                        if (instance != null)
                        {
                            result.Add(instance);
                        }
                    }
                }
            }

            return result;

        }

        private bool NamespaceMatchesPattern(string? namespaceName, string pattern)
        {
            if (string.IsNullOrEmpty(namespaceName))
                return false;

            // Remove leading dots from pattern and split into parts
            var patternParts = pattern.TrimStart('.').Split('.');

            // Split the namespace into parts
            var namespaceParts = namespaceName.Split('.');

            // Ensure that the namespace is long enough to match the pattern
            if (namespaceParts.Length < patternParts.Length)
                return false;

            // Check if the namespace ends with the pattern
            for (int i = 0; i < patternParts.Length; i++)
            {
                var patternPart = patternParts[patternParts.Length - 1 - i];
                var namespacePart = namespaceParts[namespaceParts.Length - 1 - i];

                if (patternPart != "*" && patternPart != namespacePart)
                {
                    return false;
                }
            }

            return true;
        }

        public void Validate(out IEnumerable<LogMessage> errors)
        {
            //TODO: Implement Use Flow Expressions validation
            errors = new List<LogMessage>();
        }
    }
}