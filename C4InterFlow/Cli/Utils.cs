using C4InterFlow.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using static C4InterFlow.Utils.ExternalSystem;

namespace C4InterFlow.Cli
{
    public class Utils
    {
        public static IEnumerable<string> ResolveWildcardStructures(IEnumerable<string> structures)
        {
            var result = new List<string>();

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

                        if(types.Count == 0 || types.Count > 0 && segmentItem.StartsWith("."))
                        {
                            if (types.Count > 0)
                            {
                                foreach (var typeItem in types)
                                {
                                    supersededTypes.Add(typeItem);
                                    var newType = C4InterFlow.Utils.GetType(typeItem + segmentItem);
                                    if (newType != null)
                                    {
                                        foreach (var nestedType in newType.GetNestedTypes())
                                        {
                                            newTypes.Add(nestedType.FullName.Replace("+", "."));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var type = C4InterFlow.Utils.GetType(segmentItem);
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
                                C4InterFlow.Utils.GetTypes(segmentItem)
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

        public static IEnumerable<Type> GetAllTypesOfInterface<T>()
        {
            var result = new List<Type>();

            var assemblies = GetAssemblies();
            foreach (var assembly in assemblies)
            {
                result.AddRange(assembly
                .GetTypes()
                .Where(type => typeof(T).IsAssignableFrom(type) && !type.IsInterface));
            }
            return result;
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            var paths = Directory.GetFiles(AppContext.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly);
            //TODO: Review this logic. Consider uising inclusion logic instead of exclusion.
            return paths
            .Where(x => { var assembly = x.Split("\\").Last(); return new[] { "C4InterFlow", "System.", "Microsoft." }.All(y => !assembly.StartsWith(y)); })
            .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath);
        }

        public static IEnumerable<string> ReadLines(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Could not read lines from a file. The file {filePath} does not exist.");
                yield break;
            }

            using (StreamReader sr = new StreamReader(filePath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public static void WriteLines(List<string> items, string filePath)
        {
            string directoryPath = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (string item in items)
                    {
                        writer.WriteLine(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not write lines to a file. An error occurred: {ex.Message}");
            }
        }
    }
}
