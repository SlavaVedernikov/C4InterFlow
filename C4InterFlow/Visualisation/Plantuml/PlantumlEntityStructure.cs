using C4InterFlow.Commons.Extensions;
using C4InterFlow.Structures;
using C4InterFlow.Structures.Boundaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace C4InterFlow.Visualisation.Plantuml
{
    public static class PlantumlEntityStructure
    {
        /// <summary>
        /// Default indentation tabsize
        /// </summary>
        internal static int TabSize => 4;

        public static string ToPumlObjectString(this Structure structure)
        => structure switch
        {
            Entity entity => entity.ToPumlObjectString(),
            PackageBoundary packageBoundary => packageBoundary.ToPumlObjectString(),
            _ => string.Empty
        };
        public static string ToPumlObjectString(this Entity structure)
        {
            return $"{GetTypeName(structure)} \"{structure.Label}\" as {structure.Alias.Replace('.', '_')} {GetStereotypeName(structure)}";
        }

        private static string GetTypeName(Entity structure)
        {
            var result = "class";

            switch (structure.EntityType)
            {
                case EntityType.Enum:
                    result = "enum";
                    break;
                case EntityType.Data:
                    result = "entity";
                    break;
                default:
                    break;
            }

            return result;
        }

        private static string GetStereotypeName(Entity structure)
        {
            var result = string.Empty;

            switch (structure.EntityType)
            {
                case EntityType.Data:
                    result = "Data";
                    break;
                case EntityType.Event:
                    result = "Event";
                    break;
                case EntityType.Query:
                    result = "Query";
                    break;
                case EntityType.Command:
                    result = "Command";
                    break;
                case EntityType.Message:
                    result = "Message";
                    break;
                default:
                    break;
            }

            return string.IsNullOrEmpty(result) ? result : $"<<{result}>>";
        }

        public static string ToPumlObjectString(this PackageBoundary boundary)
        {
            var stream = new StringBuilder();

            stream.AppendLine();
            stream.AppendLine($"package \"{boundary.Label}\"{{");
            foreach (var entity in boundary.Entities)
            {
                stream.AppendLine($"{string.Empty.PadLeft(TabSize)}{entity.ToPumlObjectString()}");
            }

            stream.AppendLine("}");

            return stream.ToString();
        }
    }
}
