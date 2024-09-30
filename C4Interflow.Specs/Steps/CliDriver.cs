namespace C4Interflow.Specs.Steps
{
    public class CliDriver
    {
        public CliDriver(Action<string>? logger = null)
        {
            _cliArgs = [];
            _logger = logger ?? (m => { });
        }

        private readonly List<string> _cliArgs;
        private readonly Action<string> _logger;
        private string? _workingDirectory;

        public string SampleRootName { get; set; }

        public string WorkingDirectory
        {
            get => _workingDirectory ?? Directory.GetCurrentDirectory();
            set => _workingDirectory = value;
        }

        public async Task<int> BuildAndInvoke()
        {
            var args = _cliArgs.ToArray();
            _logger(string.Join(" ", args));
            return await C4InterFlow.Cli.Program.Main(args);
        }

        public void WithAaCInputPath(string path)
        {
            _cliArgs.Add("--aac-input-paths");
            _cliArgs.Add(Path.Join(WorkingDirectory, SampleRootName, path));
        }

        public void WithAaCReaderStrategy(Type readerType)
        {
            _cliArgs.Add("--aac-reader-strategy");
            _cliArgs.Add(readerType.AssemblyQualifiedName);
        }

        public void WithInterfaces(string interfaceQuery)
        {
            _cliArgs.Add("--interfaces");
            _cliArgs.AddRange(interfaceQuery.Split(' '));
        }

        public void WithBusinessProcesses(string businessQuery)
        {
            _cliArgs.Add("--business-processes");
            _cliArgs.AddRange(businessQuery.Split(' '));
        }

        public void WithLevelOfDetails(string levelOfDetails)
        {
            _cliArgs.Add("--levels-of-details");
            _cliArgs.AddRange(levelOfDetails.Split(' '));
        }

        public void OutputTo(string outputPath)
        {
            _cliArgs.Add("--output-dir");
            _cliArgs.Add(outputPath);
        }

        public void ForCommand(string rootCommand)
        {
            _cliArgs.Clear();
            _cliArgs.Add(rootCommand);
        }
    }
}