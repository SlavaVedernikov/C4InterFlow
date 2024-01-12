using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Text;

namespace C4InterFlow.Automation
{
    public static class CsvToNetExtensions
    {
        public static ClassDeclarationSyntax AddFlowToSoftwareSystemInterfaceClass(this ClassDeclarationSyntax classDeclaration,
            CsvToNetArchitectureAsCodeWriter writer)
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
            softwareSystemInterface.WithUses(writer)
                .ToList().ForEach(i => {
                    if(!string.IsNullOrEmpty(i.UsesInterfaceAlias))
                    {
                        var usesSoftwareSystem = i.UsesInterfaceAlias.Split('.').First();
                        flowCode.AppendLine($"\t.Use(\"{writer.ArchitectureNamespace}.SoftwareSystems.{i.UsesInterfaceAlias}\")");
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
            CsvToNetArchitectureAsCodeWriter writer)
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
            containerInterface.WithUses(writer)
                .ToList().ForEach(i => {
                    if (!string.IsNullOrEmpty(i.UsesContainerInterfaceAlias))
                    {
                        flowCode.AppendLine($"\t.Use(\"{writer.ArchitectureNamespace}.SoftwareSystems.{i.UsesContainerInterfaceAlias}\")");
                    }
                    else if(!string.IsNullOrEmpty(i.UsesSoftwareSystemInterfaceAlias))
                    {
                        flowCode.AppendLine($"\t.Use(\"{writer.ArchitectureNamespace}.SoftwareSystems.{i.UsesSoftwareSystemInterfaceAlias}\")");
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
    }
}
