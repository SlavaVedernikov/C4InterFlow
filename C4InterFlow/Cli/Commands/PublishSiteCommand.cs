using System.CommandLine;
using System.Diagnostics;
using C4InterFlow.Cli.Commands.Options;
using Newtonsoft.Json;

namespace C4InterFlow.Cli.Commands;

public class PublishSiteCommand : Command
{
    private const string COMMAND_NAME = "publish-site";
    public PublishSiteCommand() : base(COMMAND_NAME,
        "Publishes Diagrams and Documentation")
    {
        var siteSourceDirectoryOption = SiteSourceDirectoryOption.Get();
        var outputDirectoryOption = OutputDirectoryOption.Get();
        var batchFileOption = BatchFileOption.Get();
        var siteBuildDirectoryOption = SiteBuildDirectoryOption.Get();
        var diagramFormatsOption = DiagramFormatsOption.Get();

        AddOption(siteSourceDirectoryOption);
        AddOption(outputDirectoryOption);
        AddOption(batchFileOption);
        AddOption(siteBuildDirectoryOption);

        this.SetHandler(async (siteSourceDirectory, outputDirectory, batchFile, siteBuildDirectory, diagramFormats) =>
            {
                await Execute(siteSourceDirectory, outputDirectory, batchFile, siteBuildDirectory, diagramFormats);
            },
            siteSourceDirectoryOption, outputDirectoryOption, batchFileOption, siteBuildDirectoryOption, diagramFormatsOption);
    }

    private static async Task<int> Execute(string siteSourceDirectory, string outputDirectory, string? batchFile = null, string? siteBuildDirectory = null, string[]? diagramFormats = null)
    {

        diagramFormats = diagramFormats?.Length > 0  ? diagramFormats : DiagramFormatsOption.GetAllDiagramFormats();
        batchFile = batchFile ?? "build.bat";
        siteBuildDirectory = siteBuildDirectory ?? "build";
        try
        {
            Console.WriteLine($"'{COMMAND_NAME}' command is executing...");

            var sitemap = new
            {
                urlset = BuildDirectoryMap(outputDirectory, outputDirectory, diagramFormats.Select(x => $".{x}").ToArray())
            };

            string json = JsonConvert.SerializeObject(sitemap, Formatting.Indented);
            File.WriteAllText(Path.Join(outputDirectory, "sitemap.json"), json);


            RunBatchFile(Path.Join(siteSourceDirectory, batchFile.Replace(siteSourceDirectory, "").TrimStart('\\')));

            CopyFilesRecursively(Path.Join(siteSourceDirectory, siteBuildDirectory.Replace(siteSourceDirectory, "").TrimStart('\\')), outputDirectory);

            Console.WriteLine($"'{COMMAND_NAME}' command completed.");
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"'{COMMAND_NAME}' command failed with exception(s) '{e.Message}'{(e.InnerException != null ? $", '{e.InnerException}'" : string.Empty)}.");
            return 1;
        }
    }

    private static List<object> BuildDirectoryMap(string directory, string rootPath, string[] fileExtensions)
    {
        List<object> map = new List<object>();
        foreach (string subDirectory in Directory.GetDirectories(directory))
        {
            string label = Path.GetFileName(subDirectory);
            var directoryItem = new
            {
                label = label,
                loc = subDirectory.Replace(rootPath, "").TrimStart('\\').Replace("\\", "/"),
                urlset = BuildDirectoryMap(subDirectory, rootPath, fileExtensions),
                types = new HashSet<string>(),
                levelsOfDetails = new HashSet<string>(),
                formats = new HashSet<string>()
            };

            AddFileDetails(subDirectory, fileExtensions, directoryItem.types, directoryItem.levelsOfDetails, directoryItem.formats);
            map.Add(directoryItem);
        }
        return map;
    }

    private static void AddFileDetails(string directory, string[] fileExtensions, HashSet<string> types, HashSet<string> levelsOfDetails, HashSet<string> formats)
    {
        foreach (var file in Directory.GetFiles(directory))
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string extension = Path.GetExtension(file);

            // Check if file matches expected formats
            if (fileExtensions.Contains(extension))
            {
                formats.Add(extension.Substring(1));
                string[] parts = fileName.Split('-');
                if (parts.Length == 2)
                {
                    levelsOfDetails.Add(parts[0].Trim());
                    types.Add(parts[1].Trim());
                }
            }
        }
    }

    private static void RunBatchFile(string batchFile)
    {
        string currentDirectory = Directory.GetCurrentDirectory();

        // Change the current directory to the .bat file's directory
        Directory.SetCurrentDirectory(Path.GetDirectoryName(batchFile));

        Process process = new Process();

        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = $"/c \"{batchFile}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = !process.StartInfo.UseShellExecute;
        process.StartInfo.RedirectStandardError = !process.StartInfo.UseShellExecute;

        process.Start();

        var output = string.Empty;
        var error = string.Empty;

        if (!process.StartInfo.UseShellExecute)
        {
            //output = process.StandardOutput.ReadToEnd();
            //error = process.StandardError.ReadToEnd();
        }

        process.WaitForExit();

        if(!string.IsNullOrEmpty(output))
        {
            Console.WriteLine("Batch File Output: " + output);
        }
        
        if(!string.IsNullOrEmpty(error))
        {
            Console.WriteLine("Batch File Error: " + error);
        }
        
        Console.WriteLine("Batch File execution exited with code: " + process.ExitCode.ToString());

        // Change the current directory back to the original one
        Directory.SetCurrentDirectory(currentDirectory);
    }

    static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        // Check if the target directory exists, if not, create it.
        if (!Directory.Exists(targetPath))
        {
            Directory.CreateDirectory(targetPath);
        }

        // Copy each file into the new directory.
        foreach (string filePath in Directory.GetFiles(sourcePath))
        {
            string fileName = Path.GetFileName(filePath);
            string destFile = Path.Combine(targetPath, fileName);
            File.Copy(filePath, destFile, true);
        }

        // Copy each subdirectory using recursion.
        foreach (string directoryPath in Directory.GetDirectories(sourcePath))
        {
            string dirName = Path.GetFileName(directoryPath);
            if (!Directory.Exists(Path.Combine(targetPath, dirName)))
            {
                Directory.CreateDirectory(Path.Combine(targetPath, dirName));
            }
            CopyFilesRecursively(directoryPath, Path.Combine(targetPath, dirName));
        }
    }

}
