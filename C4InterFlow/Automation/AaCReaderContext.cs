﻿namespace C4InterFlow.Automation
{
    public class AaCReaderContext
    {
        private static IAaCReaderStrategy? _strategy;

        public static bool HasStrategy
        {
            get
            {
                return _strategy != null && _strategy.IsInitialised;
            }
        }
        public static IAaCReaderStrategy Strategy
        {
            get
            {
                if (_strategy == null || !_strategy.IsInitialised)
                {
                    throw new InvalidOperationException("Architecture As Code Reader Strategy was not set or not initialised.");
                }

                return _strategy;
            }
        }

        public static void SetCurrentStrategy(IAaCReaderStrategy strategy, string[]? architectureInputPaths, string[]? viewsInputPaths, Dictionary<string, string>? parameters)
        {
            _strategy = strategy;
            var missingRequiredParameters = strategy.GetParameterDefinitions().Where(x =>
                x.isRequired &&
                (parameters != null ? !parameters.Keys.Contains(x.name) || string.IsNullOrEmpty(parameters[x.name]) : false));

            if (missingRequiredParameters.Any())
            {
                throw new ArgumentException($"The following required arguments where expected, but were not provided: {string.Join(", ", missingRequiredParameters.Select(x => $"'{x.name}'"))}");
            }

            _strategy.Initialise(architectureInputPaths, viewsInputPaths,parameters);
        }
    }
}
