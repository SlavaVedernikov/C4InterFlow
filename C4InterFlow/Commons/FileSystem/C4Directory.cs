namespace C4InterFlow.Commons.FileSystem;

/// <summary>
/// Manipulate the C4 folder and their resourcers
/// </summary>
internal static class C4Directory
{
    public static string ResourcesDirectoryName => ".c4s";
    /// <summary>
    /// Default Resource Folder Name
    /// </summary>
    public static string GetRelativeResourcesDirectoryPath(string diagramPath)
    {
        var diagramPathSegments = diagramPath.Split(@"\");

        return Path.Join(string.Join("", Enumerable.Repeat(@"..\", diagramPathSegments.Length)), ResourcesDirectoryName);
    }
}
