using C4InterFlow.Structures.Relationships;
using C4InterFlow.Visualisation.Plantuml.Enums;

namespace C4InterFlow.Visualisation.Plantuml
{
    public static class PlantumlSequenceRelationship
    {
        public static string ToPumlSequenceString(this Relationship relationship, SequenceDiagramStyle style)
        {
            if(style == SequenceDiagramStyle.PlantUML)
            {
                return $"{relationship.From} {(relationship.Direction == Direction.Forward ? "->" : "<-")} {relationship.To} : {relationship.Label}{(!string.IsNullOrEmpty(relationship.Protocol) ? $"\\n[{relationship.Protocol}]" : string.Empty)}";
            }
            else if(style == SequenceDiagramStyle.C4)
            {
                return relationship.ToPumlString();
            }
            else
            {
                return string.Empty;
            }

        }
    }
}
