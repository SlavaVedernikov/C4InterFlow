using C4PlusSharp.Automation;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Text.RegularExpressions;

namespace dotnet.eShop.Architecture.Cli
{
    internal class Utils
    {
        public const string ARG_SOFTWARE_SYSTEM_NAME = "softwareSystemName";
        public const string ARG_INVOCATION_INTERFACES = "invocationInterfaces";
        public const string ARG_INVOCATION_RECEIVER_TYPE_NAME = "invocationReceiverTypeName";
        public const string ARG_COMPONENT_NAME = "componentName";
        public const string ARG_CONTAINER_NAME = "containerName";

        internal static readonly string[] RedisDatabaseInterfaces = new string[]
        {
            "KeyDeleteAsync",
            "StringGetLeaseAsync",
            "StringSetAsync"
        };

        internal static readonly string[] DbContextEntityInterfaces = new string[]
        {
            "ToListAsync",
            "FirstOrDefaultAsync",
            "SingleOrDefault",
            "AnyAsync",
            "FindAsync",
            "AsQueryable",
            "Add",
            "Remove"
        };

        internal static readonly string[] DbContextInterfaces = new string[]
        {
            "SaveChangesAsync"
        };

        private static bool TryGetArgument<T>(Dictionary<string, object>? args, string key, out T arg)
        {
            arg = default(T);
            var value = default(object);
            var result = args?.TryGetValue(key, out value);

            if (result == true && value is T castValue)
            {
                arg = castValue;
                result = !EqualityComparer<T>.Default.Equals(arg, default(T));
            }

            if(result != true)
            {
                Console.WriteLine($"Cannot get value for '{key}' argument.");
            }

            return result == true;
        }

        public static void MapTypeInterfacesInvocation(InvocationExpressionSyntax invocation, SemanticModel semanticModel, IList<string> usesAliases, NetToNetArchitectureAsCodeWriter writer, Dictionary<string, object>? args)
        {
            if (!TryGetArgument(args, ARG_SOFTWARE_SYSTEM_NAME, out string softwareSystemName)) return;
            if (!TryGetArgument(args, ARG_INVOCATION_INTERFACES, out string[] invocationInterfaces)) return;
            if (!TryGetArgument(args, ARG_INVOCATION_RECEIVER_TYPE_NAME, out string invocationReceiverTypeName)) return;
            if (!TryGetArgument(args, ARG_COMPONENT_NAME, out string componentName)) return;
            if (!TryGetArgument(args, ARG_CONTAINER_NAME, out string containerName)) return;


            var memberIdentifierNameSyntax = default(IdentifierNameSyntax);
            var memberGenericNameSyntax = invocation.DescendantNodes().OfType<GenericNameSyntax>()
                .Where(x => invocationInterfaces.Contains(x.Identifier.Text)).FirstOrDefault();

            if (memberGenericNameSyntax == null)
            {
                memberIdentifierNameSyntax = invocation.DescendantNodes().OfType<IdentifierNameSyntax>().Skip(1)
                     .Where(x => invocationInterfaces.Contains(x.Identifier.Text)).FirstOrDefault();
            }
            var invocationReceiverSyntax = invocation.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
            var invocationReceiverType = invocationReceiverSyntax != null ? semanticModel.GetTypeInfo(invocationReceiverSyntax).Type : null;

            if (invocationReceiverType?.ToDisplayString() == invocationReceiverTypeName)
            {
                var componentInterfaceFilePath = @$"^{(softwareSystemName == null ? ".*" : $@".*\\SoftwareSystems\\{softwareSystemName}")}\\Containers\\{containerName}\\Components\\{componentName}\\Interfaces\\{memberIdentifierNameSyntax?.Identifier.Text ?? memberGenericNameSyntax?.Identifier.Text}\.cs$";
                var interfaceAliasValue = writer.WithComponentInterface(componentInterfaceFilePath)?.GetAliasFieldValue();

                if (!string.IsNullOrEmpty(interfaceAliasValue))
                {
                    usesAliases.Add(interfaceAliasValue);
                }
            }
        }

        public static void MapDbContextEntityInvocation(InvocationExpressionSyntax invocation, SemanticModel semanticModel, IList<string> usesAliases, NetToNetArchitectureAsCodeWriter writer, Dictionary<string, object>? args)
        {
            if (!TryGetArgument(args, ARG_SOFTWARE_SYSTEM_NAME, out string softwareSystemName)) return;
            if (!TryGetArgument(args, ARG_INVOCATION_INTERFACES, out string[] invocationInterfaces)) return;
            if (!TryGetArgument(args, ARG_CONTAINER_NAME, out string containerName)) return;

            var expressionSegments = invocation.DescendantNodes().OfType<IdentifierNameSyntax>().ToArray();
            var memberSyntax = expressionSegments.FirstOrDefault(x => invocationInterfaces.Contains(x.Identifier.Text));
            var invocationReceiverSyntax = expressionSegments.FirstOrDefault();
            var invocationReceiverType = semanticModel.GetTypeInfo(invocationReceiverSyntax).Type;

            var patterns = new string[]
            {
                "Services"
            };

            if (invocationReceiverType != null && 
                patterns.Any(x => Regex.IsMatch(invocationReceiverType.Name, x)) &&
                expressionSegments.Count() > 3 &&
                expressionSegments?.Any(x => x.Identifier.Text == "Context") == true)
            {
                var entitySyntax = expressionSegments[2];
                var componentName = invocationReceiverType.Name.Replace("Services", "Context");
                var interfaceAliasValue = writer.WithComponentInterface(@$"^{(softwareSystemName == null ? ".*" : $@".*\\SoftwareSystems\\{softwareSystemName}")}\\Containers\\{containerName}\\Components\\{componentName}\\Interfaces\\{entitySyntax}{memberSyntax}\.cs$")?.GetAliasFieldValue();

                if (!string.IsNullOrEmpty(interfaceAliasValue))
                {
                    usesAliases.Add(interfaceAliasValue);
                }
            }
        }

        public static void MapDbContextInvocation(InvocationExpressionSyntax invocation, SemanticModel semanticModel, IList<string> usesAliases, NetToNetArchitectureAsCodeWriter writer, Dictionary<string, object>? args)
        {
            if (!TryGetArgument(args, ARG_SOFTWARE_SYSTEM_NAME, out string softwareSystemName)) return;
            if (!TryGetArgument(args, ARG_INVOCATION_INTERFACES, out string[] invocationInterfaces)) return;
            if (!TryGetArgument(args, ARG_CONTAINER_NAME, out string containerName)) return;

            var expressionSegments = invocation.DescendantNodes().OfType<IdentifierNameSyntax>().ToArray();
            var memberSyntax = expressionSegments.FirstOrDefault(x => invocationInterfaces.Contains(x.Identifier.Text));
            var invocationReceiverSyntax = expressionSegments.FirstOrDefault();
            var invocationReceiverType = semanticModel.GetTypeInfo(invocationReceiverSyntax).Type;

            var patterns = new string[]
            {
                "Services"
            };

            if (invocationReceiverType != null && 
                patterns.Any(x => Regex.IsMatch(invocationReceiverType.Name, x)) &&
                expressionSegments.Count() == 3 &&
                expressionSegments?.Any(x => x.Identifier.Text == "Context") == true)
            {
                var componentName = invocationReceiverType.Name.Replace("Services", "Context");
                var interfaceAliasValue = writer.WithComponentInterface(@$"^{(softwareSystemName == null ? ".*" : $@".*\\SoftwareSystems\\{softwareSystemName}")}\\Containers\\{containerName}\\Components\\{componentName}\\Interfaces\\{memberSyntax}\.cs$")?.GetAliasFieldValue();

                if (!string.IsNullOrEmpty(interfaceAliasValue))
                {
                    usesAliases.Add(interfaceAliasValue);
                }
            }
        }
    }
}
