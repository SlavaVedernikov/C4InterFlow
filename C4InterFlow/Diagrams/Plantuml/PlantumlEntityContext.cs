using C4InterFlow.Commons.FileSystem;
using C4InterFlow.Diagrams;

namespace C4InterFlow.Diagrams.Plantuml
{
    public class PlantumlEntityContext
    {
        private bool StandardLibraryBaseUrl { get; set; }

        public PlantumlEntityContext()
        {
            StandardLibraryBaseUrl = false;
        }

        public void Export(Diagram diagram, string path, string fileName)
        {
            Export(Directory.GetCurrentDirectory(), diagram, path, fileName);
        }

        public void Export(string outputDirectory, Diagram diagram, string path, string fileName)
        {
            SavePumlFiles(outputDirectory, diagram, path, fileName);
        }
        private void SavePumlFiles(string outputDirectory, Diagram diagram, string path, string fileName)
        {
            try
            {
                var filePath = Path.Combine(outputDirectory, path, fileName);
                Directory.CreateDirectory(Path.Combine(outputDirectory, path));
                File.WriteAllText(filePath, diagram.ToPumlObjectString(StandardLibraryBaseUrl));
            }
            catch (Exception e)
            {
                throw new PlantumlException($"{nameof(PlantumlException)}: Could not save puml file.", e);
            }
        }
    }
}
