using C4InterFlow.Elements;

namespace C4InterFlow.Automation
{
    public class CsvToNetGenerationStrategy : CsvToNetArchitectureAsCodeStrategy
    {
        public override void Execute()
        {
            var addSystemClassAction = "add System Class";
            var addSystemInterfaceClassAction = "add System Interface Class";

            var architectureRootNamespaceSegments = ArchitectureRootNamespace.Split('.');
            var writer = CsvToNetArchitectureAsCodeWriter
                .WithCsvData(ArchitectureInputPath)
                .WithArchitectureRootNamespace(ArchitectureRootNamespace)
                .WithArchitectureProject(ArchitectureOutputPath);

            writer.WithSoftwareSystems()
                    .ToList().ForEach(s => {
                        writer
                        .AddSoftwareSystemClass(name: s.Alias, boundary: s.GetBoundary(), label: s.Name);
                        
                        s.WithInterfaces(writer).ToList().ForEach(i => {
                            writer.AddSoftwareSystemInterfaceClass(i);
                        });

                        s.WithContainers(writer).ToList().ForEach(c => {
                            writer.AddContainerClass(s.Alias, c.Alias.Split('.').Last(), c.Type, c.Name);

                            c.WithInterfaces(writer).ToList().ForEach(i =>
                            {
                                writer.AddContainerInterfaceClass(i);
                            });
                        });

                        writer.WithSoftwareSystemInterfaceClasses(s.Alias, true)
                        .ToList().ForEach(x => x.AddFlowToSoftwareSystemInterfaceClass(
                            writer));

                        writer.WithContainerInterfaceClasses()
                        .ToList().ForEach(x => x.AddFlowToContainerInterfaceClass(
                            writer));

                    });

            writer.WithActors()
                    .ToList().ForEach(a =>
                    {
                        if(!a.TryGetType(writer, out var type))
                        {
                            type = nameof(Person);
                        }
                        writer.AddActorClass(a.Alias, type, a.Name);
                    });

            writer.WithBusinessProcesses()
                .ToList().ForEach(b => writer.AddBusinessProcessClass(b.Alias, b.WithBusinessActivities(writer).ToArray()));
        }
    }
}
