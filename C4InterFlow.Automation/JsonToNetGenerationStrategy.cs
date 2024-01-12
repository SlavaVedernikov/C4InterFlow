namespace C4InterFlow.Automation
{
    public class JsonToNetGenerationStrategy : JsonToNetArchitectureAsCodeStrategy
    {
        public override void Execute()
        {
            var addSystemClassAction = "add System Class";
            var addSystemInterfaceClassAction = "add System Interface Class";

            var architectureRootNamespaceSegments = ArchitectureRootNamespace.Split('.');
            var generationWriter = JsonToNetArchitectureAsCodeWriter
                .WithJsonData(ArchitectureInputPath)
                .WithArchitectureRootNamespace(ArchitectureRootNamespace)
                .WithArchitectureProject(ArchitectureOutputPath);

            generationWriter.WithSoftwareSystems()
                    .ToList().ForEach(s => {
                        var softwareSystemName = s.Path.Split('.').Last();
                        generationWriter
                        .AddSoftwareSystemClass(softwareSystemName);
                        
                        s.WithInterfaces().ToList().ForEach(i => {
                            generationWriter.AddSoftwareSystemInterfaceClass(i);
                        });

                        s.WithContainers().ToList().ForEach(c => {
                            generationWriter.AddContainerClass(softwareSystemName, c.Path.Split('.').Last(), c.Property("Type")?.Value?.ToString());

                            c.WithInterfaces().ToList().ForEach(i =>
                            {
                                generationWriter.AddContainerInterfaceClass(i);
                            });
                        });

                        generationWriter.WithSoftwareSystemInterfaceClasses(softwareSystemName, true)
                        .ToList().ForEach(x => x.AddFlowToSoftwareSystemInterfaceClass(
                            generationWriter));

                        generationWriter.WithContainerInterfaceClasses()
                        .ToList().ForEach(x => x.AddFlowToContainerInterfaceClass(
                            generationWriter));

                    });
            
        }
    }
}
