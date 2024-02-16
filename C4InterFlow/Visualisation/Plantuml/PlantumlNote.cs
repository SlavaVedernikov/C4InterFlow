using C4InterFlow.Structures;

namespace C4InterFlow.Visualisation.Plantuml
{
    /// <summary>
    /// Parser Note to PlantUML
    /// </summary>
    internal static class PlantumlNote
    {
        /// <summary>
        /// Create PUML content from Note
        /// </summary>
        /// <param name="noye"></param>
        /// <returns></returns>        
        public static string ToPumlString(this Note note)
        {
            return $"note {(note.Position == NotePosition.Left ? "left" : note.Position == NotePosition.Righ ? "right" : "left")} of {note.StructureAlias} \n {note.Text} \n end note";
        }
    }
}
