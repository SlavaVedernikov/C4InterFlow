﻿namespace C4InterFlow.Automation
{
    public class ArchitectureAsCodeReaderContext
    {
        private static IArchitectureAsCodeReaderStrategy? _strategy;

        public static IArchitectureAsCodeReaderStrategy Strategy
        {
            get
            {
                if (_strategy == null || !_strategy.IsInitialised)
                {
                    throw new InvalidOperationException("Architecture As Code Reader Strategy was not set.");
                }

                return _strategy;
            }
        }

        public static void SetCurrentStrategy(IArchitectureAsCodeReaderStrategy strategy, string? architectureInputPath, Dictionary<string, string>? parameters)
        {
            _strategy = strategy;
            var missingRequiredParameters = strategy.GetParameterDefinitions().Where(x =>
                x.isRequired &&
                (parameters != null ? !parameters.Keys.Contains(x.name) || string.IsNullOrEmpty(parameters[x.name]) : false));

            if (missingRequiredParameters.Any())
            {
                throw new ArgumentException($"The following required arguments where expected, but were not provided: {string.Join(", ", missingRequiredParameters.Select(x => $"'{x.name}'"))}");
            }

            _strategy.Initialise(architectureInputPath, parameters);
        }
    }
}