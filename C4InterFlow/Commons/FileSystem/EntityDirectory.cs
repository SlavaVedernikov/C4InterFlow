namespace C4InterFlow.Commons.FileSystem;

/// <summary>
/// Manipulate the C4 folder and their resourcers
/// </summary>
internal static class EntityDirectory
{
    /// <summary>
    /// Default Resource Folder Name
    /// </summary>
    public static string GetResourcesFolderName(string diagramPath)
    {
        var diagramPathSegments = diagramPath.Split(@"\");

        return Path.Join(string.Join("", Enumerable.Repeat(@"..\", diagramPathSegments.Length)), ".c4s");
    }
}
