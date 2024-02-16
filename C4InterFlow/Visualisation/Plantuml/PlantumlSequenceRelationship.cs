using C4InterFlow.Structures.Relationships;

namespace C4InterFlow.Visualisation.Plantuml
{
    public static class PlantumlSequenceRelationship
    {
        public static string ToPumlSequenceString(this Relationship relationship)
        {
            return $"{relationship.From} {(relationship.Direction == Direction.Forward ? "->" : "<-")} {relationship.To} : {relationship.Label}{(!string.IsNullOrEmpty(relationship.Protocol) ? $" ({relationship.Protocol})" : string.Empty)}";
        }
    }
}
