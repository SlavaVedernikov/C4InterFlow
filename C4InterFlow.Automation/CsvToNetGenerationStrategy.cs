namespace C4InterFlow.Automation
{
    public class CsvToNetGenerationStrategy : CsvToNetArchitectureAsCodeStrategy
    {
        public override void Execute()
        {
            var addSystemClassAction = "add System Class";
            var addSystemInterfaceClassAction = "add System Interface Class";

            var architectureRootNamespaceSegments = ArchitectureRootNamespace.Split('.');
            var generationWriter = CsvToNetArchitectureAsCodeWriter
                .WithCsvData(ArchitectureInputPath)
                .WithArchitectureRootNamespace(ArchitectureRootNamespace)
                .WithArchitectureProject(ArchitectureOutputPath);

            generationWriter.WithSoftwareSystems()
                    .ToList().ForEach(s => {
                        generationWriter
                        .AddSoftwareSystemClass(name: s.Alias, label: s.Name);
                        
                        s.WithInterfaces(generationWriter).ToList().ForEach(i => {
                            generationWriter.AddSoftwareSystemInterfaceClass(i);
                        });

                        s.WithContainers(generationWriter).ToList().ForEach(c => {
                            generationWriter.AddContainerClass(s.Alias, c.Alias.Split('.').Last(), c.Type, c.Name);

                            c.WithInterfaces(generationWriter).ToList().ForEach(i =>
                            {
                                generationWriter.AddContainerInterfaceClass(i);
                            });
                        });

                        generationWriter.WithSoftwareSystemInterfaceClasses(s.Alias, true)
                        .ToList().ForEach(x => x.AddFlowToSoftwareSystemInterfaceClass(
                            generationWriter));

                        generationWriter.WithContainerInterfaceClasses()
                        .ToList().ForEach(x => x.AddFlowToContainerInterfaceClass(
                            generationWriter));

                    });
            
        }
    }
}
