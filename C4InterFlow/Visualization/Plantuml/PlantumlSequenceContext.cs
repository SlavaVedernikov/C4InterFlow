using C4InterFlow.Commons.FileSystem;
using C4InterFlow.Visualization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C4InterFlow.Visualization.Plantuml
{
    public class PlantumlSequenceContext : PlantumlContext
    {
        protected override string SavePumlFiles(string outputDirectory, Diagram diagram, string path, string fileName)
        {
            try
            {
                PlantumlResources.LoadResources(outputDirectory);
                var filePath = Path.Combine(outputDirectory, path, fileName);
                Directory.CreateDirectory(Path.Combine(outputDirectory, path));
                File.WriteAllText(filePath, diagram.ToPumlSequenceString(UsingStandardLibraryBaseUrl));
                return filePath;
            }
            catch (Exception e)
            {
                throw new PlantumlException($"{nameof(PlantumlException)}: Could not save puml file.", e);
            }
        }
    }
}
