using C4InterFlow.Automation;
using C4InterFlow.Structures;
using C4InterFlow.Structures.Interfaces;
using System.Reflection;
using System.Runtime.Loader;
using Serilog;

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

        public static void SetArchitectureAsCodeReaderContext(string[] architectureAsCodeInputPaths, string architectureAsCodeReaderStrategyType)
        {
            Type strategyType = Type.GetType(architectureAsCodeReaderStrategyType);

            if (strategyType == null)
            {
                throw new ArgumentException($"Cannot load AaC Reader Strategy Type '{architectureAsCodeReaderStrategyType}'");
            }
            object strategyTypeInstance = Activator.CreateInstance(strategyType);
            var strategyInstance = strategyTypeInstance as IAaCReaderStrategy;

            if (strategyInstance == null)
            {
                throw new ArgumentException($"'{architectureAsCodeReaderStrategyType}' is not a valid Architecture As Code Reader Strategy type.");
            }

            AaCReaderContext.SetCurrentStrategy(strategyInstance, architectureAsCodeInputPaths, new Dictionary<string, string>());
        }
    }
}
