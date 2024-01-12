namespace C4InterFlow.Automation
{
    public abstract class ArchitectureAsCodeStrategy
    {
        protected string ArchitectureRootNamespace { get; private set; }
        protected string? ArchitectureOutputPath { get; private set; }
        private bool _isInitialised = false;
        public virtual bool IsInitialised
        {
            get
            {
                return _isInitialised;
            }
        }

        public virtual (string name, bool isRequired)[] GetParameterDefinitions()
        {
            return new (string name, bool isRequired)[] { };
        }
        public abstract void Execute();
        public virtual void Initialise(string architectureRootNamespace, string? architectureOutputPath, Dictionary<string, string>? parameters)
        {
            ArchitectureRootNamespace = architectureRootNamespace;
            ArchitectureOutputPath = architectureOutputPath;
            _isInitialised = true;
        }
    }
}
