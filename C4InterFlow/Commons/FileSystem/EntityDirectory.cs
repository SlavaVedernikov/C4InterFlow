namespace C4InterFlow.Commons.FileSystem;

/// <summary>
/// Manipulate the C4 folder and their resources
/// </summary>
internal static class EntityDirectory
{
    /// <summary>
    /// Default Resource Folder Name
    /// </summary>
    public static string GetResourcesFolderName(string diagramPath)
    {
        var diagramPathSegments = diagramPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var parentDirs = string.Join(string.Empty,
            Enumerable.Repeat($"..{Path.DirectorySeparatorChar}", diagramPathSegments.Length));
        return Path.Combine(parentDirs.TrimEnd(Path.DirectorySeparatorChar), ".c4s");
    }
}