namespace C4InterFlow.Automation
{
    public class CsvToJsonGenerationStrategy : CsvToJsonArchitectureAsCodeStrategy
    {
        public override void Execute()
        {
            var addSystemClassAction = "add System Class";
            var addSystemInterfaceClassAction = "add System Interface Class";

            var architectureRootNamespaceSegments = ArchitectureRootNamespace.Split('.');
            var generationWriter = CsvToJsonArchitectureAsCodeWriter
                .WithCsvData(ArchitectureInputPath)
                //.WithArchitectureRootNamespace(ArchitectureRootNamespace)
                //.WithArchitectureOutputPath(Path.Combine(ArchitectureOutputPath, $"{ArchitectureRootNamespace}.json"))
                ;

            generationWriter.WithSoftwareSystems()
                    .ToList().ForEach(s => {
                        generationWriter
                        .WithArchitectureRootNamespace(ArchitectureRootNamespace)
                        .WithArchitectureOutputPath(Path.Combine(ArchitectureOutputPath, $"{ArchitectureRootNamespace}.{s.Alias}.json"))
                        .AddSoftwareSystemObject(s.Alias);
                        
                        s.WithInterfaces(generationWriter).ToList().ForEach(i => {
                            generationWriter.AddSoftwareSystemInterfaceObject(i);
                        });

                        s.WithContainers(generationWriter).ToList().ForEach(c => {
                            generationWriter.AddContainerObject(s.Alias, c.Alias.Split('.').Last(), c.Type);

                            c.WithInterfaces(generationWriter).ToList().ForEach(i =>
                            {
                                generationWriter.AddContainerInterfaceObject(i);
                            });
                        });

                        generationWriter.WithSoftwareSystemInterfaceObjects(s.Alias)
                        .ToList().ForEach(x => x.AddFlowToSoftwareSystemInterfaceObject(
                            generationWriter));

                        generationWriter.WithContainerInterfaceObjects(s.Alias)
                        .ToList().ForEach(x => x.AddFlowToContainerInterfaceObject(
                            generationWriter));

                        generationWriter.WriteArchitecture();

                    });

            //generationWriter.WriteArchitecture();


        }
    }
}
