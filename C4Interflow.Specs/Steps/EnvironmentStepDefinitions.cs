using System.Reflection;
using C4InterFlow.Automation.Readers;
using TechTalk.SpecFlow;


[assembly: Parallelize(Scope = ExecutionScope.ClassLevel)]

namespace C4Interflow.Specs.Steps
{
    [Binding]
    public sealed class EnvironmentStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly TestContext _testContext;

        public EnvironmentStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
        {
            _scenarioContext = scenarioContext;
            _testContext = testContext;
        }

        [BeforeScenario]
        public void UpdateScenarioRun(ITestRunnerManager testRunnerManager)
        {
            _scenarioContext.SetTempPath(Path.Combine(Path.GetTempPath(), Assembly.GetExecutingAssembly().GetName().Name,
                    Guid.NewGuid().ToString("N")));
        }

        [AfterScenario]
        public void AfterScenario()
        {
            var exitCode = _scenarioContext.GetCliExitCode() ?? int.MinValue;
            Assert.AreEqual(0, exitCode, "The command exited with a non-zero value.");
        }

        [Given(@"the '(.+)' command")]
        public void ForCommand(string command)
        {
            var driver = new CliDriver(m => _testContext.WriteLine($"[DRIVER]: {m}"));
            driver.ForCommand(command);
            _scenarioContext.SetDriver(driver);
        }

        [Given(@"the '(.+)' example")]
        public void GivenTheExample(string example)
        {
            var driver = _scenarioContext.GetDriver();
            driver.WorkingDirectory =
                Path.GetFullPath("../../../../../Samples", Assembly.GetExecutingAssembly().Location);
            driver.SampleRootName = example;
        }

        [Given(@"the path '(.+)'")]
        public void GivenThePath(string path)
        {
            _scenarioContext.GetDriver().WithAaCInputPath(path);
        }

        [Given(@"the reader strategy is '(.+)'")]
        public void GivenTheReaderStrategy(string strategy)
        {
            var driver = _scenarioContext.GetDriver();
            switch (strategy)
            {
                case "Yaml":
                    driver.WithAaCReaderStrategy(typeof(YamlAaCReaderStrategy));
                    break;
                case "Json":
                    driver.WithAaCReaderStrategy(typeof(JsonAaCReaderStrategy));
                    break;
                default:
                    Assert.Fail($"Unknown strategy: '{strategy}'");
                    break;
            }
        }

        [Given(@"the interfaces are '(.*)'")]
        public void GivenTheInterfacesAre(string interfaceQuery)
        {
            _scenarioContext.GetDriver().WithInterfaces(interfaceQuery);
        }

        [Given(@"the business processes are '(.*)'")]
        public void GivenTheBusinessProcessesAre(string businessQuery)
        {
            var driver = _scenarioContext.GetDriver();
            driver.WithBusinessProcesses(businessQuery);
        }


        [Given(@"the level of details is '(.*)'")]
        public void GivenTheLevelOfDetailsIs(string levelOfDetails)
        {
            var driver = _scenarioContext.GetDriver();
            driver.WithLevelOfDetails(levelOfDetails);
        }

        [Given(@"send the output to '(.*)'")]
        public void GivenSendTheOutputTo(string outputPath)
        {
            var driver = _scenarioContext.GetDriver();
            if (!Path.IsPathRooted(outputPath))
            {
                outputPath = Path.Combine(_scenarioContext.GetTempPath(), outputPath);
            }

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            driver.OutputTo(outputPath);
        }

        [When(@"invoking the commandline for those arguments")]
        public void WhenInvokingTheCommandlineForThoseArguments()
        {
            var couldAdd = _scenarioContext.SetCliExitCode(_scenarioContext.GetDriver().BuildAndInvoke().Result);
            Assert.IsTrue(couldAdd, "Failed to add the CLI exit code to the scenario. Likely because it already exists.");
        }

        [Then(@"all files under '(.*)' should match example path '(.*)'")]
        public void ThenAllFilesUnderShouldMatchExamplePath(string actualPath, string expectedPath)
        {
            if (!Path.IsPathRooted(actualPath))
            {
                actualPath = Path.Combine(_scenarioContext.GetTempPath(), actualPath);
            }

            expectedPath = Path.Combine(_scenarioContext.GetDriver().WorkingDirectory, expectedPath);

            var getFileList = (string path) => Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                .Select(p => new { FullPath = p, RelativePath = Path.GetRelativePath(path, p) }).ToArray();

            _testContext.WriteLine($"...Expected Path: {expectedPath}");
            _testContext.WriteLine($"...Actual Path:   {actualPath}");
            var expectedFiles = getFileList(expectedPath);
            var actualFiles = getFileList(actualPath);

            Assert.AreNotEqual(0, actualFiles.Length, $"No files were generated under '{actualPath}'.");

            // We cannot compare the raw file count because what's in the Sample directory will always have more files.
            // Instead, we have to compare based on the Relative paths.

            var actualRelativePaths = actualFiles.ToDictionary(x => x.RelativePath, x => x);
            var expectedRelativePaths = expectedFiles.ToDictionary(x => x.RelativePath, x => x);

            var extraFiles = actualRelativePaths.Keys.Except(expectedRelativePaths.Keys).ToArray();
            Assert.AreEqual(0, extraFiles.Length, "There are extra files in the output.");

            foreach (var kvp in actualRelativePaths)
            {
                var actualFullPath = kvp.Value.FullPath;
                var expectedFullPath = expectedRelativePaths[kvp.Key].FullPath;

                var actualContents = File.ReadAllText(actualFullPath);
                var expectedContents = File.ReadAllText(expectedFullPath);
                Assert.AreEqual(expectedContents, actualContents, $"The contents of '{actualFullPath}' did not match the contents of '{expectedFullPath}'");
            }
        }
    }
}

