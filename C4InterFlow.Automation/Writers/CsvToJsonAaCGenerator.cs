using C4InterFlow.Structures;

namespace C4InterFlow.Automation.Writers
{
    public class CsvToJsonAaCGenerator : CsvToJObjectAaCWriterStrategy
    {
        public override void Execute()
        {
            var architectureRootNamespaceSegments = ArchitectureRootNamespace.Split('.');
            var writer = CsvToJsonAaCWriter
                .WithCsvData(ArchitectureInputPath);


            writer.WithSoftwareSystems()
                    .ToList().ForEach(s =>
                    {

                        writer
                            .WithArchitectureRootNamespace(ArchitectureRootNamespace)
                            .WithSoftwareSystemsCollection();

                        var softwareSystemName = s.Alias;
                        writer.AddSoftwareSystem(softwareSystemName, s.GetBoundary(), s.Name, s.Description);

                        s.WithInterfaces(writer.DataProvider).ToList().ForEach(i =>
                        {
                            writer.AddSoftwareSystemInterface(softwareSystemName, i.Alias.Split('.').Last(), i.Name, i.Description, protocol: i.Protocol);
                        });

                        s.WithContainers(writer.DataProvider).ToList().ForEach(c =>
                        {
                            var containerName = c.Alias.Split('.').Last();
                            writer.AddContainer(softwareSystemName, containerName, c.Type, c.Technology, c.Name, c.Description);

                            c.WithInterfaces(writer.DataProvider).ToList().ForEach(i =>
                            {
                                writer.AddContainerInterface(softwareSystemName, containerName, i.Alias.Split('.').Last(), i.Name, i.Description, protocol: i.Protocol);
                            });
                        });

                        writer.WithSoftwareSystemInterfaceObjects(s.Alias)
                        .ToList().ForEach(x => x.AddFlowToSoftwareSystemInterfaceObject(
                            writer));

                        writer.WithContainerInterfaceObjects(s.Alias)
                        .ToList().ForEach(x => x.AddFlowToContainerInterfaceObject(
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
