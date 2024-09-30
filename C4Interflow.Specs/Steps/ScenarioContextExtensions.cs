using TechTalk.SpecFlow;

namespace C4Interflow.Specs.Steps
{
    internal static class ScenarioContextExtensions
    {
        private const string CLI_EXIT_CODE_KEY = "_exit_code_";
        private const string DRIVER_KEY = "_driver_";
        private const string TEMP_PATH_KEY = "_temp_path_";

        public static CliDriver GetDriver(this ScenarioContext scenarioContext) => scenarioContext.Get<CliDriver>(DRIVER_KEY);

        public static bool SetDriver(this ScenarioContext scenarioContext, CliDriver driver)
        {
            return scenarioContext.TryAdd(DRIVER_KEY, driver);
        }

        public static string GetTempPath(this ScenarioContext scenarioContext) =>
            scenarioContext.Get<string>(TEMP_PATH_KEY);

        public static bool SetTempPath(this ScenarioContext scenarioContext, string tempPath) =>
            scenarioContext.TryAdd(TEMP_PATH_KEY, tempPath);

        public static int? GetCliExitCode(this ScenarioContext scenarioContext) =>
            (int?)scenarioContext.GetValueOrDefault(CLI_EXIT_CODE_KEY, null);

        public static bool SetCliExitCode(this ScenarioContext scenarioContext, int exitCode) =>
            scenarioContext.TryAdd(CLI_EXIT_CODE_KEY, exitCode);
    }
}