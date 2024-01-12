using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Text;
using Newtonsoft.Json.Linq;

namespace C4InterFlow.Automation
{
    public static class JsonToNetExtensions
    {
        public static ClassDeclarationSyntax AddFlowToSoftwareSystemInterfaceClass(this ClassDeclarationSyntax classDeclaration,
            JsonToNetArchitectureAsCodeWriter writer)
        {
            var architectureWorkspace = writer.ArchitectureWorkspace;
            var architectureClassSyntaxTree = classDeclaration.SyntaxTree;
            var architectureClassRoot = architectureClassSyntaxTree.GetRoot();
            var architectureProject = architectureWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == architectureClassSyntaxTree.FilePath));
            var architectureCompilation = architectureProject.GetCompilationAsync().Result;
            var architectureSemanticModel = architectureCompilation.GetSemanticModel(architectureClassSyntaxTree);

            var softwareSystemInterface = writer.SoftwareSystemInterfaceClassFileNameMap.GetValueOrDefault(architectureClassSyntaxTree.FilePath);

            if (softwareSystemInterface == null) return classDeclaration;

            var flowCode = new StringBuilder().AppendLine($"new Flow(ALIAS)");
            softwareSystemInterface.WithUses()
                .ToList().ForEach(i =>
                {
                    var usesInterfaceAlis = i.Property("Params")?.Value?.ToString();
                    if (!string.IsNullOrEmpty(usesInterfaceAlis))
                    {
                        flowCode.AppendLine($"\t.Use(\"{usesInterfaceAlis}\")");
                    }
                });


            var flowSyntaxNode = architectureClassRoot.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .First(x => x.Left is IdentifierNameSyntax ins && ins.Identifier.Text == "Flow");

            var leadingTrivia = flowSyntaxNode.GetLeadingTrivia();
            if (flowSyntaxNode != null)
            {
                var newFlowSyntaxNode =
                    SyntaxFactory.ParseExpression($"{leadingTrivia}Flow = {string.Join($"{Environment.NewLine}{leadingTrivia}", flowCode.ToString().Split(Environment.NewLine).Where(x => !string.IsNullOrEmpty(x)))}");

                architectureClassRoot = architectureClassRoot.ReplaceNode(flowSyntaxNode, newFlowSyntaxNode);

                var document = architectureWorkspace.CurrentSolution.GetDocument(architectureClassSyntaxTree);
                document.ApplyChanges(architectureClassRoot);
            }


            return classDeclaration;
        }

        public static ClassDeclarationSyntax AddFlowToContainerInterfaceClass(this ClassDeclarationSyntax classDeclaration,
            JsonToNetArchitectureAsCodeWriter writer)
        {
            var architectureWorkspace = writer.ArchitectureWorkspace;
            var architectureClassSyntaxTree = classDeclaration.SyntaxTree;
            var architectureClassRoot = architectureClassSyntaxTree.GetRoot();
            var architectureProject = architectureWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == architectureClassSyntaxTree.FilePath));
            var architectureCompilation = architectureProject.GetCompilationAsync().Result;
            var architectureSemanticModel = architectureCompilation.GetSemanticModel(architectureClassSyntaxTree);

            var containerInterface = writer.ContainerInterfaceClassFileNameMap.GetValueOrDefault(architectureClassSyntaxTree.FilePath);

            if (containerInterface == null) return classDeclaration;

            var flowCode = new StringBuilder().AppendLine($"new Flow(ALIAS)");
            containerInterface.WithUses()
                .ToList().ForEach(i =>
                {
                    var usesInterfaceAlis = i.Property("Params")?.Value?.ToString();
                    if (!string.IsNullOrEmpty(usesInterfaceAlis))
                    {
                        flowCode.AppendLine($"\t.Use(\"{usesInterfaceAlis}\")");
                    }
                });


            var flowSyntaxNode = architectureClassRoot.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .First(x => x.Left is IdentifierNameSyntax ins && ins.Identifier.Text == "Flow");

            var leadingTrivia = flowSyntaxNode.GetLeadingTrivia();
            if (flowSyntaxNode != null)
            {
                var newFlowSyntaxNode =
                    SyntaxFactory.ParseExpression($"{leadingTrivia}Flow = {string.Join($"{Environment.NewLine}{leadingTrivia}", flowCode.ToString().Split(Environment.NewLine).Where(x => !string.IsNullOrEmpty(x)))}");

                architectureClassRoot = architectureClassRoot.ReplaceNode(flowSyntaxNode, newFlowSyntaxNode);

                var document = architectureWorkspace.CurrentSolution.GetDocument(architectureClassSyntaxTree);
                document.ApplyChanges(architectureClassRoot);
            }


            return classDeclaration;
        }

        public static IEnumerable<JObject> WithUses(this JObject interfaceObject)
        {
            var flows = interfaceObject.SelectToken("Flows") as JArray;
            if (flows != null)
            {
                return flows.Select(x => x as JObject);
            }

            return new List<JObject>();
        }

        public static IEnumerable<JObject> WithInterfaces(this JObject interfaceOwnerObject)
        {
            return interfaceOwnerObject.SelectTokens("Interfaces.*").Select(x => x as JObject);
        }

        public static IEnumerable<JObject> WithContainers(this JObject interfaceOwnerObject)
        {
            return interfaceOwnerObject.SelectTokens("Containers.*").Select(x => x as JObject);
        }
    }
}
