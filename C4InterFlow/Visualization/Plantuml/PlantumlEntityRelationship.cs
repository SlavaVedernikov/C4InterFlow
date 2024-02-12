using C4InterFlow.Elements.Relationships;

namespace C4InterFlow.Visualization.Plantuml
{
    public static class PlantumlEntityRelationship
    {
        public static string ToPumlObjectString(this Relationship relationship)
        {
            return $"{relationship.From.Replace('.', '_')} {GetRelationshipSymbol(relationship)} {relationship.To.Replace('.', '_')} : {relationship.Label}";
            //return $"{relationship.From} {GetRelationshipSymbol(relationship)} {relationship.To} : {relationship.Label}";
            //return $"{relationship.From.Split('.').Last()} {GetRelashionshipSymbol(relationship)} {relationship.To.Split('.').Last()} : {relationship.Label}";
        }

        private static string GetRelationshipSymbol(Relationship relationship)
        {
            var result = relationship.Direction == Direction.Forward ? ">" : "<";

            if (!string.IsNullOrEmpty(relationship.Protocol))
            {
                switch (relationship.Protocol)
                {
                    case "composition":
                        result = "*";
                        break;
                    case "extension":
                        result = relationship.Direction == Direction.Forward ? "|>" : "<|";
                        break;
                    case "aggregation":
                        result = "o";
                        break;
                    default:
                        break;
                }
            }

            return relationship.Direction == Direction.Forward ? $"--{result}" : $"{result}--";
        }
    }
}
