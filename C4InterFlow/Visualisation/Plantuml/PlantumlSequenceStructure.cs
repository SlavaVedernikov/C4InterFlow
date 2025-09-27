using C4InterFlow.Structures;
using C4InterFlow.Structures.Boundaries;
using System.Text;

namespace C4InterFlow.Visualisation.Plantuml
{
    public static class PlantumlSequenceStructure
    {
        /// <summary>
        /// Default indentation tabsize
        /// </summary>
        internal static int TabSize => 4;
        public static string ToPumlSequenceString(this Structure structure)
        {
            if (structure is SoftwareSystemBoundary softwareSystemBoundary)
            {
                var stream = new StringBuilder();

                stream.AppendLine();
                stream.AppendLine($"box \"{structure.Label}\" #White");
                foreach (var item in softwareSystemBoundary.Structures)
                {
                    stream.AppendLine($"{string.Empty.PadLeft(TabSize)}{item.ToPumlSequenceString()}");
                }

                stream.AppendLine("end box");

                return stream.ToString();
            }
            else if (structure is ContainerBoundary containerBoundary)
            {
                var stream = new StringBuilder();

                stream.AppendLine();
                stream.AppendLine($"box \"{structure.Label}\" #White");
                foreach (var item in containerBoundary.Components)
                {
                    stream.AppendLine($"{string.Empty.PadLeft(TabSize)}{item.ToPumlSequenceString()}");
                }

                stream.AppendLine("end box");

                return stream.ToString();
            }
            else
            {
                var participantKeyWord = "participant";

                if (structure is Container container)
                {
                    switch (container.ContainerType)
                    {
                        case ContainerType.Database:
                            participantKeyWord = "database";
                            break;
                        case ContainerType.Queue:
                        case ContainerType.Topic:
                            participantKeyWord = "queue";
                            break;
                        default:
                            break;
                    }
                }

                return $"{participantKeyWord} \"{structure.Label}\" as {structure.Alias}";
            }
        }
    }
}
