namespace C4InterFlow.Structures
{
    public enum NotePosition
    {
        Left = 1,
        Righ = 2
    }
    public record Note
    {
        public string Text { get; init; }
        public NotePosition Position { get; init; }
        public string StructureAlias { get; init; }

        public Note(string text, string structureAlias, NotePosition position) => (Text, StructureAlias, Position) = (text, structureAlias, position);

    }
}
