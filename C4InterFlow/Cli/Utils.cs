using C4InterFlow.Automation;
using C4InterFlow.Automation.Readers;
using C4InterFlow.Structures;
using Serilog;
using System.Text.RegularExpressions;

namespace C4InterFlow.Cli
{
    public class Utils
    {
        
        public static IEnumerable<string> ResolveStructures(IEnumerable<string> structures)
        {
            return AaCReaderContext.Strategy.ResolveStructures(structures);
        }

        public static IEnumerable<Interface> GetAllInterfaces()
        {
            return AaCReaderContext.Strategy.GetAllInterfaces();
        }

        public static IEnumerable<string> ReadLines(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Log.Warning("Could not read lines from a file. The file {Path} does not exist", filePath);

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

        public static void WriteLines(List<string> items, string filePath, bool append = false)
        {
            string directoryPath = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, append))
                {
                    foreach (string item in items)
                    {
                        writer.WriteLine(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning("Could not write lines to a file. An error occurred: {Error}", ex.Message);
            }
        }

        public static void SetArchitectureAsCodeReaderContext(string[] architectureAsCodeInputPaths, string[] viewsInputPaths = null)
        {
            try
            {
                AaCReaderContext.SetCurrentStrategy(new YamlAaCReaderStrategy(), architectureAsCodeInputPaths, viewsInputPaths, new Dictionary<string, string>());
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to set Architecture As Code Reader Context {Error}", $"{e.Message}");
            }
        }

        public static string ToKebabCase(string value)
        {
            // Add hyphens before each uppercase letter that has a lowercase letter following it,
            // then convert the entire string to lowercase.
            var result = Regex.Replace(value, @"([a-z0-9])([A-Z])", "$1-$2").ToLower();
            return result;
        }
    }
}
