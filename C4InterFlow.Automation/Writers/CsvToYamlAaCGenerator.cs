using C4InterFlow.Structures;

namespace C4InterFlow.Automation.Writers
{
    public class CsvToYamlAaCGenerator : CsvToJObjectAaCWriterStrategy
    {
        public override void Execute()
        {
            var architectureRootNamespaceSegments = ArchitectureRootNamespace.Split('.');
            var writer = CsvToYamlAaCWriter
                .WithCsvData(ArchitectureInputPath);


            writer.WithSoftwareSystems()
                    .ToList().ForEach(s =>
                    {

                        writer
                            .WithArchitectureRootNamespace(ArchitectureRootNamespace)
                            .WithSoftwareSystemsCollection();

                        var softwareSystemName = s.Alias;
                        writer.AddSoftwareSystem(softwareSystemName, s.GetBoundary(), s.Name);

                        s.WithInterfaces(writer.DataProvider).ToList().ForEach(i =>
                        {
                            writer.AddSoftwareSystemInterface(softwareSystemName, i.Alias.Split('.').Last(), i.Name);
                        });

                        s.WithContainers(writer.DataProvider).ToList().ForEach(c =>
                        {
                            var containerName = c.Alias.Split('.').Last();
                            writer.AddContainer(softwareSystemName, containerName, c.Type, c.Name);

                            c.WithInterfaces(writer.DataProvider).ToList().ForEach(i =>
                            {
                                writer.AddContainerInterface(softwareSystemName, containerName, i.Alias.Split('.').Last(), i.Name);
                            });
                        });

                        writer.WithSoftwareSystemInterfaceObjects(s.Alias)
                        .ToList().ForEach(x => x.AddFlowToSoftwareSystemInterfaceObject(
                            writer));

                        writer.WithContainerInterfaceObjects(s.Alias)
                        .ToList().ForEach(x => x.AddFlowToContainerInterfaceObject(
                            writer));

                        writer.WithSoftwareSystemObjects()
                        .ToList().ForEach(x => x.AddAttributesToSoftwareSystemObject(
                            writer));

                        writer.WriteArchitecture(Path.Combine(ArchitectureOutputPath, "SoftwareSystems"), s.Alias);

                    });

            writer.WithActors()
                    .ToList().ForEach(a =>
                    {
                        writer
                            .WithArchitectureRootNamespace(ArchitectureRootNamespace)
                            .WithActorsCollection();

                        if (!a.TryGetType(writer.DataProvider, out var type))
                        {
                            type = nameof(Person);
                        }

                        writer.AddActor(a.Alias, type, a.Name);

                        writer.WriteArchitecture(Path.Combine(ArchitectureOutputPath, "Actors"), a.Alias);
                    });

            writer.WithBusinessProcesses()
                .ToList().ForEach(b =>
                {
                    writer
                        .WithArchitectureRootNamespace(ArchitectureRootNamespace)
                        .WithBusinessProcessesCollection();

                    writer.AddBusinessProcess(b.Alias, b.Name);

                    writer.WriteArchitecture(Path.Combine(ArchitectureOutputPath, "BusinessProcesses"), b.Alias);
                });
        }
    }
}
