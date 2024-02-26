using C4InterFlow.Visualisation.Plantuml.Enums;

namespace C4InterFlow.Visualisation.Plantuml
{
    public class PlantumlSequenceContext : PlantumlContext
    {
        private SequenceDiagramStyle Style {get; set;}
        public PlantumlSequenceContext() : this(SequenceDiagramStyle.PlantUML) { }

        public PlantumlSequenceContext(SequenceDiagramStyle style) {
            Style = style;
        }
        protected override string SavePumlFiles(string outputDirectory, Diagram diagram, string path, string fileName)
        {
            try
            {
                PlantumlResources.LoadResources(outputDirectory);
                var filePath = Path.Combine(outputDirectory, path, fileName);
                Directory.CreateDirectory(Path.Combine(outputDirectory, path));
                File.WriteAllText(filePath, diagram.ToPumlSequenceString(UsingStandardLibraryBaseUrl, path, Style));
                return filePath;
            }
            catch (Exception e)
            {
                throw new PlantumlException($"{nameof(PlantumlException)}: Could not save puml file.", e);
            }
        }
    }
}
