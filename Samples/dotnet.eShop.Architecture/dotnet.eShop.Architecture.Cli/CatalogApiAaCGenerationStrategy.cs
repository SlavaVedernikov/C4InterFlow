using C4InterFlow.Automation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet.eShop.Architecture.Cli
{
    public class CatalogApiAaCGenerationStrategy : NetToNetArchitectureAsCodeStrategy
    {
        public override void Execute()
        {
            var addComponentClassAction = "add Component Class";
            var addComponentInterfaceClassAction = "add Component Interface Class";
            var addEntityClassAction = "add Entity Class";

            var writer = new NetToNetArchitectureAsCodeWriter(SoftwareSystemSourcePath, ArchitectureRootNamespace);

            writer.WithArchitectureProject(ArchitectureOutputPath)
                .AddSoftwareSystem(SoftwareSystemName);

            var projectName = string.Empty;
            var containerName = string.Empty;
            var componentName = string.Empty;

            projectName = "Catalog.API";
            containerName = "Api";

            writer.WithSoftwareSystemProject(projectName)
                .AddContainer(SoftwareSystemName, containerName);

            Console.WriteLine($"Adding {containerName} Container, Component and Interface classes for '{projectName}' project...");
            writer.WithDocuments().Where(d => d.FilePath.Contains(@"\Apis\") && d.Name.EndsWith("Api.cs"))
                //.WithConfirmation(addComponentClassAction)
                .ToList().ForEach(d =>
                {
                    d.WithClasses().Where(c => c.Identifier.Text.EndsWith("Api"))
                    //.WithConfirmation(addComponentClassAction)
                    .ToList().ForEach(c =>
                    {
                        c.AddComponentClass(SoftwareSystemName, containerName, writer)
                        .WithMethods()
                        //.WithConfirmation(addComponentInterfaceClassAction)
                        .ToList().ForEach(m =>
                            m.AddComponentInterfaceClass(
                                SoftwareSystemName,
                                containerName,
                                c.Identifier.Text,
                                writer));
                    });
                });

            containerName = "Infrastructure";

            Console.WriteLine($"Adding {containerName} Container, Component and Interface classes for '{projectName}' project...");
            writer.AddContainer(SoftwareSystemName, containerName);

            writer.WithDocuments().Where(d => d.FilePath.Contains(@"\Infrastructure\") && d.Name.EndsWith("Context.cs"))
                //.WithConfirmation(addComponentClassAction)
                .ToList().ForEach(d =>
                {
                    d.WithClasses().Where(c => c.Identifier.Text.EndsWith("Context"))
                    //.WithConfirmation(addComponentClassAction)
                    .ToList().ForEach(c =>
                    {
                        c.AddComponentClass(SoftwareSystemName, containerName, writer)
                        .WithProperties()
                        //.WithConfirmation(addComponentInterfaceClassAction)
                        .ToList().ForEach(p =>
                            p.AddComponentInterfaceClass(
                                SoftwareSystemName,
                                containerName,
                                c.Identifier.Text,
                                writer,
                                Utils.DbContextEntityInterfaces));

                        foreach (var @interface in Utils.DbContextInterfaces)
                        {
                            writer.AddComponentInterface(
                                SoftwareSystemName,
                                containerName,
                                c.Identifier.Text,
                                @interface);
                        }

                    });
                });


            Console.WriteLine($"Adding Software System Type Mappings...");
            AddSoftwareSystemTypeMapping(writer);

            Console.WriteLine($"Updating Flow property in all Interface classes...");
            writer.WithComponentInterfaces(true).ToList()
                .ForEach(x => x.AddFlowToComponentInterfaceClass(
                    writer, null,
                    new NetToNetAlternativeInvocationMapperConfig[]
                    {
                        new NetToNetAlternativeInvocationMapperConfig() {
                            Mapper = Utils.MapDbContextEntityInvocation,
                            Args = new Dictionary<string, object>() {
                                { Utils.ARG_SOFTWARE_SYSTEM_NAME, SoftwareSystemName },
                                { Utils.ARG_INVOCATION_INTERFACES, Utils.DbContextEntityInterfaces },
                                { Utils.ARG_CONTAINER_NAME, "Infrastructure" }
                            }
                        },
                        new NetToNetAlternativeInvocationMapperConfig() {
                            Mapper = Utils.MapDbContextInvocation,
                            Args = new Dictionary<string, object>() {
                                { Utils.ARG_SOFTWARE_SYSTEM_NAME, SoftwareSystemName },
                                { Utils.ARG_INVOCATION_INTERFACES, Utils.DbContextInterfaces },
                                { Utils.ARG_CONTAINER_NAME, "Infrastructure" }
                            }
                        }
                    }));
        }

        private void AddSoftwareSystemTypeMapping(NetToNetArchitectureAsCodeWriter writer)
        {
            var softwareSystemRootNamespace = "eShop.Catalog.API";
            var services = new Dictionary<string, string>
            {
                
            };


            foreach (var service in services)
            {
                writer.AddSoftwareSystemTypeMapping(service.Key, service.Value);
            }
        }

    }
}
