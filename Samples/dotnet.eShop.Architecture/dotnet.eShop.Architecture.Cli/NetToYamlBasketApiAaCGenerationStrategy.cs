using C4InterFlow.Automation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet.eShop.Architecture.Cli
{
    public class NetToYamlBasketApiAaCGenerationStrategy : NetToYamlArchitectureAsCodeStrategy
    {
        public override void Execute()
        {
            var addComponentClassAction = "add Component Class";
            var addComponentInterfaceClassAction = "add Component Interface Class";
            var addEntityClassAction = "add Entity Class";

            var writer = new NetToYamlArchitectureAsCodeWriter(SoftwareSystemSourcePath, ArchitectureRootNamespace, ArchitectureOutputPath);

            writer.AddSoftwareSystem(SoftwareSystemName);

            var projectName = string.Empty;
            var containerName = string.Empty;
            var componentName = string.Empty;

            projectName = "Basket.API";
            containerName = "Grpc";

            writer.WithSoftwareSystemProject(projectName)
                .AddContainer(SoftwareSystemName, containerName);

            Console.WriteLine($"Adding {containerName} Container, Component and Interface classes for '{projectName}' project...");
            writer.WithDocuments().Where(d => d.FilePath.Contains(@"\Grpc\") && d.Name.EndsWith("Service.cs"))
                //.WithConfirmation(addComponentClassAction)
                .ToList().ForEach(d =>
                {
                    d.WithClasses().Where(c => c.Identifier.Text.EndsWith("Service"))
                    //.WithConfirmation(addComponentClassAction)
                    .ToList().ForEach(c =>
                    {
                        c.AddComponentYamlFile(SoftwareSystemName, containerName, writer)
                        .WithMethods()
                        //.WithConfirmation(addComponentInterfaceClassAction)
                        .ToList().ForEach(m =>
                            m.AddComponentInterfaceYamlFile(
                                SoftwareSystemName,
                                containerName,
                                c.Identifier.Text,
                                writer));
                    });
                });

            containerName = "Data";

            writer.AddContainer(SoftwareSystemName, containerName);

            Console.WriteLine($"Adding {containerName} Container, Component and Interface classes for '{projectName}' project...");
            writer.WithDocuments().Where(d => d.FilePath.Contains(@"\Repositories\") && d.Name.EndsWith("Repository.cs"))
                //.WithConfirmation(addComponentClassAction)
                .ToList().ForEach(d =>
                {
                    d.WithClasses().Where(c => c.Identifier.Text.EndsWith("Repository"))
                    //.WithConfirmation(addComponentClassAction)
                    .ToList().ForEach(c =>
                    {
                        c.AddComponentYamlFile(SoftwareSystemName, containerName, writer)
                        .WithMethods()
                        //.WithConfirmation(addComponentInterfaceClassAction)
                        .ToList().ForEach(m =>
                            m.AddComponentInterfaceYamlFile(
                                SoftwareSystemName,
                                containerName,
                                c.Identifier.Text,
                                writer));
                    });
                });

            componentName = "RedisDatabase";

            writer.AddComponent(SoftwareSystemName, containerName, componentName);

            foreach (var @interface in Utils.RedisDatabaseInterfaces)
            {
                writer.AddComponentInterface(SoftwareSystemName, containerName, componentName, @interface);
            }

            Console.WriteLine($"Adding Software System Type Mappings...");
            AddSoftwareSystemTypeMapping(writer);

            Console.WriteLine($"Updating Flow property in all Interface classes...");
            /*
            writer.WithComponentInterfaces(true).ToList()
                .ForEach(x => x.AddFlowToComponentInterfaceClass(
                    writer, null,
                    new NetToNetAlternativeInvocationMapperConfig[]
                    {
                        new NetToNetAlternativeInvocationMapperConfig() {
                            Mapper = Utils.MapTypeInterfacesInvocation,
                            Args = new Dictionary<string, object>() { 
                                { Utils.ARG_SOFTWARE_SYSTEM_NAME, SoftwareSystemName },
                                { Utils.ARG_INVOCATION_INTERFACES, Utils.RedisDatabaseInterfaces },
                                { Utils.ARG_INVOCATION_RECEIVER_TYPE_NAME, "StackExchange.Redis.IDatabase" },
                                { Utils.ARG_CONTAINER_NAME, "Data" },
                                { Utils.ARG_COMPONENT_NAME, "RedisDatabase" },
                            }
                        }
                    }));
            */
        }

        private void AddSoftwareSystemTypeMapping(NetToYamlArchitectureAsCodeWriter writer)
        {
            var softwareSystemRootNamespace = "eShop.Basket.API";
            var services = new Dictionary<string, string>
            {
                { $"{softwareSystemRootNamespace}.Grpc.BasketService", $"{softwareSystemRootNamespace}.Grpc.BasketService" },
                { $"{softwareSystemRootNamespace}.Repositories.IBasketRepository", $"{softwareSystemRootNamespace}.Repositories.RedisBasketRepository" }
            };


            foreach (var service in services)
            {
                writer.AddSoftwareSystemTypeMapping(service.Key, service.Value);
            }
        }

    }
}
