using System.Diagnostics;
using System.Text;


namespace C4InterFlow.Diagrams.Plantuml;

public partial class PlantumlContext : IDisposable
{
    protected bool UsingStandardLibraryBaseUrl { get; set; }
    private bool GenerateDiagramImages { get; set; }
    private bool GenerateDiagramSvgImages { get; set; }
    private bool GenerateDiagramMdDocuments { get; set; }
    private string? PlantumlJarPath { get; set; }
    private ProcessStartInfo ProcessInfo { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    public PlantumlContext()
    {
        PlantumlJarPath = null;
        UsingStandardLibraryBaseUrl = false;
        GenerateDiagramImages = false;
        GenerateDiagramSvgImages = false;
        GenerateDiagramMdDocuments = false;

        ProcessInfo = new ProcessStartInfo
        {
            FileName = "java",
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };
    }

    /// <summary>
    /// The C4Sharp has embedded the current version of C4-PluntUML.
    /// But, if you want to use the C4-PlantUML up-to-date version from their repo,
    /// use this method
    /// </summary>
    /// <returns>PlantumlSession instance</returns>
    public PlantumlContext UseStandardLibraryBaseUrl()
    {
        UsingStandardLibraryBaseUrl = true;
        return this;
    }

    /// <summary>
    /// The C4Sharp will generate *.puml files of your diagram.
    /// Also, you could save the *.png files using this method
    /// </summary>
    /// <returns></returns>
    public PlantumlContext UseDiagramImageBuilder()
    {
        GenerateDiagramImages = true;
        return this;
    }

    /// <summary>
    /// The C4Sharp will generate *.puml files of your diagram.
    /// Also, you could save the *.svg files using this method
    /// </summary>
    /// <returns></returns>
    public PlantumlContext UseDiagramSvgImageBuilder()
    {
        GenerateDiagramSvgImages = true;
        return this;
    }

    /// <summary>
    /// The C4Sharp will generate *.puml files of your diagram.
    /// Also, you could save the *.md files using this method
    /// </summary>
    /// <returns></returns>
    public PlantumlContext UseDiagramMdDocumentBuilder()
    {
        GenerateDiagramMdDocuments = true;
        return this;
    }

    /// <summary>
    /// It creates a Puml file into the default directory "./c4"
    /// If the attribute of Session GenerateDiagramImages is true
    /// It generates png files of the diagram
    /// </summary>
    /// <param name="diagrams">C4 Diagrams</param>
    public void Export(Diagram diagram, string path, string fileName)
    {
        Export(Directory.GetCurrentDirectory(), diagram, path, fileName);
    }

    /// <summary>
    /// It creates a Puml file into the default directory "./c4"
    /// If the attribute of Session GenerateDiagramImages is true
    /// It generates png files of the diagram
    /// </summary>
    /// <param name="diagram">C4 Diagrams</param>
    /// <param name="outputDirectory">
    /// Full path of the directory
    /// <example>For windows.: C:\users\user\projects\</example>
    /// <example>For Unix.: users/user/projects/</example>
    /// </param>
    /// ReSharper disable once MemberCanBePrivate.Global
    public void Export(string outputDirectory, Diagram diagram, string path, string fileName)
    {       
        SavePumlFiles(outputDirectory, diagram, path, fileName);

        if (GenerateDiagramImages) SaveDiagramFiles(outputDirectory, path, fileName, "png");
        if (GenerateDiagramSvgImages) SaveDiagramFiles(outputDirectory, path, fileName, "svg");
        if (GenerateDiagramMdDocuments)
        {
            if(!GenerateDiagramImages) SaveDiagramFiles(outputDirectory, path, fileName, "png");
            SaveMdFiles(outputDirectory, path, fileName);
        }
    }
}

/// <summary>
/// PUML, SVG, PNG file utils
/// </summary>
public partial class PlantumlContext
{
    /// <summary>
    /// Save puml file. It's creates path if non exists.
    /// </summary>
    /// <param name="diagram">C4 Diagram</param>
    /// <param name="path">Output path</param>
    protected virtual string SavePumlFiles(string outputDirectory, Diagram diagram, string path, string fileName)
    {
        try
        {
            PlantumlResources.LoadResources(outputDirectory);
            var filePath = Path.Combine(outputDirectory, path, fileName);
            Directory.CreateDirectory(Path.Combine(outputDirectory, path));
            File.WriteAllText(filePath, diagram.ToPumlString(UsingStandardLibraryBaseUrl, path));
            return filePath;
        }
        catch (Exception e)
        {
            throw new PlantumlException($"{nameof(PlantumlException)}: Could not save puml file.", e);
        }
    }

    /// <summary>
    /// Execute plantuml.jar
    /// </summary>
    /// <param name="path">puml files path</param>
    /// <param name="generatedImageFormat">specifies the format of the generated images</param>
    /// <exception cref="PlantumlException"></exception>
    private void SaveDiagramFiles(string outputDirectory, string path, string fileName, string generatedImageFormat)
    {
        try
        {
            PlantumlResources.LoadResources(outputDirectory);
            PlantumlJarPath ??= PlantumlResources.LoadPlantumlJar();

            var directory = new DirectoryInfo(Path.Combine(outputDirectory, path)).FullName;
            var filePath = Path.Combine(directory, fileName);

            if (string.IsNullOrEmpty(directory))
            {
                throw new PlantumlException($"{nameof(PlantumlException)}: puml file not found.");
            }

            var results = new StringBuilder();

            var jar = CalculateJarCommand(UsingStandardLibraryBaseUrl, generatedImageFormat, directory);

            // Get the JAVA_OPTS environment variable
            var javaOpts = Environment.GetEnvironmentVariable("JAVA_OPTS");

            ProcessInfo.Arguments = $"{(!string.IsNullOrEmpty(javaOpts) ? $"{javaOpts} " : string.Empty)}{jar} \"{filePath}\"";
            ProcessInfo.RedirectStandardOutput = true;
            ProcessInfo.StandardOutputEncoding = Encoding.UTF8;

            var process = new Process { StartInfo = ProcessInfo };

            process.OutputDataReceived += (_, args) =>
            {
                results.AppendLine(args.Data);
            };
                
            
            process.Start();
            process.WaitForExit();
        }
        catch (Exception e)
        {
            throw new PlantumlException($"{nameof(PlantumlException)}: puml file not found.", e);
        }
    }

    /// <summary>
    /// Save puml file. It's creates path if non exists.
    /// </summary>
    /// <param name="diagram">C4 Diagram</param>
    /// <param name="path">Output path</param>
    private void SaveMdFiles(string outputDirectory, string path, string fileName)
    {
        try
        {
            var directory = new DirectoryInfo(Path.Combine(outputDirectory, path)).FullName;
            var filePath = Path.Combine(directory, fileName);

            if (string.IsNullOrEmpty(directory))
            {
                throw new PlantumlException($"{nameof(PlantumlException)}: png file not found.");
            }

            var mdFilePath = filePath.Replace(".puml", ".md");
            var pngFileName = Uri.EscapeDataString(fileName.Replace(".puml", ".png"));
            var title = $"{fileName.Replace(".puml", string.Empty)}";
            var alt = $"{path.Replace(@"\", " - ")} - {title}";

            using (StreamWriter writer = new StreamWriter(mdFilePath))
            {
                writer.WriteLine($"# {title}");
                writer.WriteLine($"![{alt}](./{pngFileName})");
            }
        }
        catch (Exception e)
        {
            throw new PlantumlException($"{nameof(PlantumlException)}: Could not save md file.", e);
        }
    }

    private string CalculateJarCommand(bool useStandardLibrary, string generatedImageFormat, string directory)
    {
        const string includeLocalFilesArg = "-DRELATIVE_INCLUDE=\".\"";

        var resourcesOriginArg = useStandardLibrary ? string.Empty : includeLocalFilesArg;
        var imageFormatOutputArg = string.IsNullOrWhiteSpace(generatedImageFormat)
            ? string.Empty
            : $"-t{generatedImageFormat}";

        return
            $"-jar \"{PlantumlJarPath}\" {resourcesOriginArg} {imageFormatOutputArg} -Playout=smetana -o \"{directory}\" -charset UTF-8";
    }

    /// <summary>
    /// Clear Plantuml Resource
    /// </summary>
    public void Dispose()
    {
        try
        {
            if (PlantumlJarPath is not null)
            {
                File.Delete(PlantumlJarPath);
            }
        }
        catch
        {
            // ignored
        }
    }
}