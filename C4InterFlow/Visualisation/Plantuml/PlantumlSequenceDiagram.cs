using System.Text;
using C4InterFlow.Visualisation.Plantuml.Enums;

namespace C4InterFlow.Visualisation.Plantuml
{
    public static class PlantumlSequenceDiagram
    {
        /// <summary>
        /// Create PUML content from Sequence Diagram
        /// </summary>
        /// <param name="diagram"></param>
        /// <returns></returns>
        public static string ToPumlSequenceString(this Diagram diagram, string diagramPath, SequenceDiagramStyle style) => diagram.ToPumlSequenceString(false, diagramPath, style);

        /// <summary>
        /// Create PUML content from Sequence Diagram
        /// </summary>
        /// <param name="diagram"></param>
        /// <param name="useStandardLibrary"></param>
        /// <returns></returns>
        public static string ToPumlSequenceString(this Diagram diagram, bool useStandardLibrary, string diagramPath, SequenceDiagramStyle style) => new StringBuilder()
                .BuildSequenceHeader(diagram, useStandardLibrary, diagramPath, style)
                .BuildSequenceBody(diagram, style)
                .BuildSequenceFooter(diagram, style)
                .ToString();

        private static StringBuilder BuildSequenceHeader(this StringBuilder stream, Diagram diagram, bool useStandardLibrary, string diagramPath, SequenceDiagramStyle style)
        {
            stream.AppendLine($"@startuml");

            if(style == SequenceDiagramStyle.C4)
            {
                var includePath = diagram.GetPumlFilePath(useStandardLibrary, diagramPath, true);
                stream.AppendLine($"!include {includePath}");
            }

            //TODO: Make diagram resolution configurable via diagram parameter.
            //stream.AppendLine("skinparam dpi 60");
            stream.AppendLine();


            if (!string.IsNullOrWhiteSpace(diagram.Title))
            {
                stream.AppendLine($"title {diagram.Title}");
                stream.AppendLine();
            }

            return stream;
        }

        private static StringBuilder BuildSequenceBody(this StringBuilder stream, Diagram diagram, SequenceDiagramStyle style)
        {
            var flowParticipants = diagram.Flow?.Flows?
                .Select(x => Utils.GetInstance<Structures.Structure>(x.Owner))
                .Where(x => x != null && !diagram.Structures.Any(s => s.Alias == x.Alias)).Distinct();

            if (flowParticipants != null)
            {
                foreach (var structure in flowParticipants)
                {
                    if(style == SequenceDiagramStyle.PlantUML)
                    {
                        stream.AppendLine(structure?.ToPumlSequenceString());
                    }
                    else if(style == SequenceDiagramStyle.C4)
                    {
                        stream.AppendLine(structure?.ToPumlString(BoundaryStyle.BoundaryEndClosed));
                    }
                }
            }

            foreach (var structure in diagram.Structures)
            {
                if (style == SequenceDiagramStyle.PlantUML)
                {
                    stream.AppendLine(structure?.ToPumlSequenceString());
                }
                else if (style == SequenceDiagramStyle.C4)
                {
                    stream.AppendLine(structure?.ToPumlString(BoundaryStyle.BoundaryEndClosed));
                }
            }

            stream.AppendLine();

            stream.AppendLine(diagram.Flow?.ToPumlSequenceString(style));

            stream.AppendLine();

            return stream;
        }

        private static StringBuilder BuildSequenceFooter(this StringBuilder stream, Diagram diagram, SequenceDiagramStyle style)
        {
            stream.AppendLine("@enduml");

            return stream;
        }
    }
}
