using Serilog;

namespace C4InterFlow.Automation
{
    public class AaCWriterContext
    {
        private AaCWriterStrategy Strategy { get; set; }

        public AaCWriterContext(AaCWriterStrategy strategy, string architectureRootNamespace, string? architectureOutputPath, Dictionary<string, string>? parameters)
        {
            Strategy = strategy;
            var missingRequiredParameters = strategy.GetParameterDefinitions().Where(x => 
                x.isRequired && 
                (parameters != null ? !parameters.Keys.Contains(x.name) || string.IsNullOrEmpty(parameters[x.name]) : false));

            if (missingRequiredParameters.Any())
            {
                throw new ArgumentException($"The following required arguments where expected, but were not provided: {string.Join(", ", missingRequiredParameters.Select(x => $"'{x.name}'"))}");
            }

            Strategy.Initialise(architectureRootNamespace, architectureOutputPath, parameters);
        }

        public void ExecuteStrategy()
        {
            if(!Strategy.IsInitialised)
            {
                throw new Exception("Strategy cannot be executed as it either was not initialised or failed to initialise.");
            }
            Strategy.Execute();

            Log.Information("Strategy execution completed");
        }
    }
}
