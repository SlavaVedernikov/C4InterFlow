using C4InterFlow.Structures;

namespace C4InterFlow.Automation.Writers
{
    public class CsvToJsonGenerationStrategy : CsvToJsonArchitectureAsCodeStrategy
    {
        public override void Execute()
        {
            var addSystemClassAction = "add System Class";
            var addSystemInterfaceClassAction = "add System Interface Class";

            var architectureRootNamespaceSegments = ArchitectureRootNamespace.Split('.');
            var writer = CsvToJsonArchitectureAsCodeWriter
                .WithCsvData(ArchitectureInputPath);


            writer.WithSoftwareSystems()
                    .ToList().ForEach(s =>
                    {

                        writer
                            .WithArchitectureRootNamespace(ArchitectureRootNamespace)
                            .WithSoftwareSystemsCollection()
                            .WithArchitectureOutputPath(Path.Combine(ArchitectureOutputPath, "SoftwareSystems", $"{s.Alias}.json"));


                        writer.AddSoftwareSystemObject(s.Alias, s.GetBoundary(), s.Name);

                        s.WithInterfaces(writer).ToList().ForEach(i =>
                        {
                            writer.AddSoftwareSystemInterfaceObject(i);
                        });

                        s.WithContainers(writer).ToList().ForEach(c =>
                        {
                            writer.AddContainerObject(s.Alias, c.Alias.Split('.').Last(), c.Type, c.Name);

                            c.WithInterfaces(writer).ToList().ForEach(i =>
                            {
                                writer.AddContainerInterfaceObject(i);
                            });
                        });

                        writer.WithSoftwareSystemInterfaceObjects(s.Alias)
                        .ToList().ForEach(x => x.AddFlowToSoftwareSystemInterfaceObject(
                            writer));

                        writer.WithContainerInterfaceObjects(s.Alias)
                        .ToList().ForEach(x => x.AddFlowToContainerInterfaceObject(
                            writer));

                        writer.WriteArchitecture();

                    });

            writer.WithActors()
                    .ToList().ForEach(a =>
                    {
                        writer
                            .WithArchitectureRootNamespace(ArchitectureRootNamespace)
                            .WithActorsCollection()
                            .WithArchitectureOutputPath(Path.Combine(ArchitectureOutputPath, "Actors", $"{a.Alias}.json"));

                        if (!a.TryGetType(writer, out var type))
                        {
                            type = nameof(Person);
                        }

                        writer.AddActorObject(a.Alias, type, a.Name);

                        writer.WriteArchitecture();
                    });

            writer.WithBusinessProcesses()
                .ToList().ForEach(b =>
                {
                    writer
                        .WithArchitectureRootNamespace(ArchitectureRootNamespace)
                        .WithBusinessProcessesCollection()
                        .WithArchitectureOutputPath(Path.Combine(ArchitectureOutputPath, "BusinessProcessess", $"{b.Alias}.json"));

                    writer.AddBusinessProcessObject(b.Alias, b.WithBusinessActivities(writer).ToArray(), b.Name);

                    writer.WriteArchitecture();
                });
        }
    }
}
