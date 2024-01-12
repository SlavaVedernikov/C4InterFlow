namespace C4InterFlow.Automation
{
    public abstract class JsonToNetArchitectureAsCodeStrategy : ArchitectureAsCodeStrategy
    {
        //TODO: Rename as AAC_INPUT_PATH
        public const string AAC_INPUT_PATH = "aac-input-path";
        public string ArchitectureInputPath { get; private set; }
        private bool _isInitialised = false;
        public override bool IsInitialised
        {
            get
            {
                return _isInitialised && base.IsInitialised;
            }
        }

        public override (string name, bool isRequired)[] GetParameterDefinitions()
        {
            return new[] {
                (AAC_INPUT_PATH, true)
            };
        }
        public override void Initialise(string architectureRootNamespace, string? architectureProjectPath, Dictionary<string, string>? parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentException($"The following arguments where expected, but were not provided: {string.Join(", ", GetParameterDefinitions().Select(x => $"'{x.name}' ({(x.isRequired ? "required" : "optional")})"))}");
            }

            if (!parameters.TryGetValue(AAC_INPUT_PATH, out var softwareSystemSolutionPath))
            {
                throw new ArgumentException($"The '{AAC_INPUT_PATH}' argument is required.");
            }

            ArchitectureInputPath = softwareSystemSolutionPath;

            base.Initialise(architectureRootNamespace, architectureProjectPath, null);
            _isInitialised = true;
        }
    }
}