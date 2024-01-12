namespace C4InterFlow.Automation
{
    public abstract class NetToAnyArchitectureAsCodeStrategy : ArchitectureAsCodeStrategy
    {
        public const string SOFTWARE_SYSTEM_SOURCE_PATH = "software-system-source-path";
        public const string SOFTWARE_SYSTEM_NAME = "software-system-name";
        public string SoftwareSystemSourcePath { get; private set; }
        public string SoftwareSystemName { get; private set; }
        private bool _isInitialised = false;
        public override bool IsInitialised {
            get
            {
                return _isInitialised && base.IsInitialised;
            }
        }

        public override (string name, bool isRequired)[] GetParameterDefinitions()
        {
            return new[] { 
                (SOFTWARE_SYSTEM_SOURCE_PATH, true), 
                (SOFTWARE_SYSTEM_NAME, true)};
        }
        public override void Initialise(string architectureRootNamespace, string? architectureOutputPath, Dictionary<string, string>? parameters)
        {
            if(parameters == null)
            {
                throw new ArgumentException($"The following arguments where expected, but were not provided: {string.Join(", ", GetParameterDefinitions().Select(x => $"'{x.name}' ({(x.isRequired ? "required" : "optional")})"))}");
            }

            if (!parameters.TryGetValue(SOFTWARE_SYSTEM_SOURCE_PATH, out var softwareSystemSourcePath))
            {
                throw new ArgumentException($"The '{SOFTWARE_SYSTEM_SOURCE_PATH}' argument is required.");
            }

            if (!parameters.TryGetValue(SOFTWARE_SYSTEM_NAME, out var softwareSystemName))
            {
                throw new ArgumentException($"The '{SOFTWARE_SYSTEM_NAME}' argument is required.");
            }

            SoftwareSystemSourcePath = softwareSystemSourcePath;
            SoftwareSystemName = softwareSystemName;
            base.Initialise(architectureRootNamespace, architectureOutputPath, null);
            _isInitialised = true;
        }
        
    }
}
