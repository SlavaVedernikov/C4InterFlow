namespace C4InterFlow.Commons.FileSystem;

/// <summary>
/// Manipulate the C4 folder and their resources
/// </summary>
internal static class C4Directory
{
    public static string ResourcesDirectoryName => ".c4s";

    /// <summary>
    /// Default Resource Folder Name
    /// </summary>
    public static string GetRelativeResourcesDirectoryPath(string diagramPath)
    {
        if (string.IsNullOrEmpty(diagramPath))
        {
            return ResourcesDirectoryName;
        }
        else
        {
            var diagramPathSegments = diagramPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var parentDirs = string.Join(string.Empty,
                Enumerable.Repeat($"..{Path.DirectorySeparatorChar}", diagramPathSegments.Length));
            return Path.Combine(parentDirs.TrimEnd(Path.DirectorySeparatorChar), ResourcesDirectoryName);
        }
    }
}