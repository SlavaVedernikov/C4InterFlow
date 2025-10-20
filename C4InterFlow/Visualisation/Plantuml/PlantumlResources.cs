using C4InterFlow.Commons;
using C4InterFlow.Commons.FileSystem;
using Serilog;
using System.IO;

namespace C4InterFlow.Visualisation.Plantuml;

internal static class PlantumlResources
{
    private static readonly object _resourceLock = new();
    /// <summary>
    /// Load all C4_Plantuml files
    /// </summary>
    public static void LoadResources(string outputDirectory)
    {
        var local = Path.Join(outputDirectory, C4Directory.ResourcesDirectoryName);

        LoadResource(local, "C4.puml");
        LoadResource(local, "C4_Component.puml");
        LoadResource(local, "C4_Container.puml");
        LoadResource(local, "C4_Context.puml");
        LoadResource(local, "C4_Deployment.puml");
        LoadResource(local, "C4_Sequence.puml");
    }

    /// <summary>
    /// Load all C4_Plantuml files
    /// </summary>
    public static void LoadHtmlResources(string path)
    {
        LoadResource(path, "ds.js");
        LoadIconPngResource(path);
    }

    /// <summary>
    /// Load C4_Plantuml file
    /// </summary>
    /// <param name="resourcesPath"></param>
    /// <param name="resourceName"></param>
    /// <exception cref="C4FileException"></exception>
    private static void LoadResource(string resourcesPath, string resourceName)
    {
        var path = Path.Join(resourcesPath, resourceName);
        try
        {
            // Fast check (no lock) — avoids unnecessary locking
            if (File.Exists(path))
                return;

            lock (_resourceLock)
            {
                // Double-check inside the lock (critical section)
                if (File.Exists(path))
                    return;

                var stream = ResourceFile.ReadString(resourceName);
                Directory.CreateDirectory(resourcesPath);
                File.WriteAllText(path, stream);
            }

            throw new Exception("Test file access error");
        }
        catch (Exception e)
        {
            Log.Error("An exception occurred while the package tried loading the resource files '{Path}': {Error}", path, e);
        }
    }

    /// <summary>
    /// Get Stream from plantuml.jar file
    /// </summary>
    /// <returns>Stream</returns>
    /// <exception cref="PlantumlException"></exception>
    public static string LoadPlantumlJar()
    {
        try
        {
            const string resourceName = "plantuml.jar";
            var fileName = Path.GetTempFileName();

            LoadStream(fileName, resourceName);

            return fileName;
        }
        catch (Exception e)
        {
            throw new PlantumlException($"{nameof(PlantumlException)}: Could not load plantuml engine.", e);
        }
    }

    /// <summary>
    /// Load Icon PNG file
    /// </summary>
    /// <param name="resourcesPath"></param>
    /// <exception cref="C4FileException"></exception>
    private static void LoadIconPngResource(string resourcesPath)
    {
        try
        {
            const string resourceName = "icon.png";
            var path = Path.Join(resourcesPath, resourceName);

            if (File.Exists(path))
            {
                return;
            }

            LoadStream(path, resourceName);
        }
        catch (Exception e)
        {
            throw new PlantumlException($"{nameof(PlantumlException)}: Could not load plantuml engine.", e);
        }
    }

    private static void LoadStream(string path, string resourceName)
    {
        var stream = ResourceFile.ReadStream(resourceName) ?? throw new InvalidOperationException();
        using var file = new FileStream(path, FileMode.Create, FileAccess.Write);
        stream.CopyTo(file);
        stream.Flush();
    }
}
