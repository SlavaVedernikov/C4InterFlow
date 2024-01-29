using C4InterFlow.Elements.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace C4InterFlow.Elements
{
    public class NetElementsResolver : IElementsResolver
    {
        private static IList<string> _nonAssemblyPaths = new List<string>();
        private static Dictionary<string, object> _aliasToStructureMap = new Dictionary<string, object>();

        public T? GetInstance<T>(string alias) where T : class
        {
            if (string.IsNullOrEmpty(alias)) return default(T);

            if (_aliasToStructureMap.ContainsKey(alias))
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

        public Type? GetType(string alias)
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

        public IEnumerable<string> ResolveWildcardStructures(IEnumerable<string> structures)
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
    }
}
