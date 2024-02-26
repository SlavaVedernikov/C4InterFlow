using System.Text;

namespace C4InterFlow.Visualisation.Plantuml
{
    public static class PlantumlEntityDiagram
    {
        public static string ToPumlObjectString(this Diagram diagram) => diagram.ToPumlObjectString(false);

        /// <summary>
        /// Create PUML content from Object Diagram
        /// </summary>
        /// <param name="diagram"></param>
        /// <param name="useStandardLibrary"></param>
        /// <returns></returns>
        public static string ToPumlObjectString(this Diagram diagram, bool useStandardLibrary) => new StringBuilder()
                .BuildObjectHeader(diagram, useStandardLibrary)
                .BuildObjectBody(diagram)
                .BuildObjectFooter(diagram)
                .ToString();

        private static StringBuilder BuildObjectHeader(this StringBuilder stream, Diagram diagram, bool useStandardLibrary)
        {
            stream.AppendLine($"@startuml");
            stream.AppendLine();

            return stream;
        }

        private static StringBuilder BuildObjectBody(this StringBuilder stream, Diagram diagram)
        {
            foreach (var structure in diagram.Structures)
            {
                stream.AppendLine(structure.ToPumlObjectString());
            }

            stream.AppendLine();

            foreach (var relationship in diagram.Relationships)
            {
                stream.AppendLine(relationship.ToPumlObjectString());
            }

            stream.AppendLine();

            return stream;
        }

        private static StringBuilder BuildObjectFooter(this StringBuilder stream, Diagram diagram)
        {
            stream.AppendLine("@enduml");

            return stream;
        }
    }
}
