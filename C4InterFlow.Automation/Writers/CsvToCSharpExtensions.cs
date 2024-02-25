using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Text;

namespace C4InterFlow.Automation.Writers
{
    public static class CsvToCSharpExtensions
    {
        public static ClassDeclarationSyntax AddFlowToSoftwareSystemInterfaceClass(this ClassDeclarationSyntax classDeclaration,
            CsvToCSharpAaCWriter writer)
        {
            var architectureWorkspace = writer.ArchitectureWorkspace;
            var architectureClassSyntaxTree = classDeclaration.SyntaxTree;
            var architectureClassRoot = architectureClassSyntaxTree.GetRoot();
            var architectureProject = architectureWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == architectureClassSyntaxTree.FilePath));
            var architectureCompilation = architectureProject.GetCompilationAsync().Result;
            var architectureSemanticModel = architectureCompilation.GetSemanticModel(architectureClassSyntaxTree);

            var softwareSystemInterface = writer.SoftwareSystemInterfaceAaCPathToCsvRecordMap.GetValueOrDefault(architectureClassSyntaxTree.FilePath);

            if (softwareSystemInterface == null) return classDeclaration;

            var flowCode = new StringBuilder().AppendLine($"new Flow(ALIAS)");
            softwareSystemInterface.WithUses(writer.DataProvider)
                .ToList().ForEach(x =>
                {
                    var hasCondition = !string.IsNullOrEmpty(x.Condition);

                    if (hasCondition)
                    {
                        flowCode.AppendLine($"\t.If(\"{x.Condition}\")");
                    }

                    if (!string.IsNullOrEmpty(x.UsesSoftwareSystemInterface))
                    {
                        flowCode.AppendLine($"{(hasCondition ? "\t" : string.Empty)}\t.Use(\"{writer.ArchitectureNamespace}.SoftwareSystems.{x.UsesSoftwareSystemInterface}\")");
                    }
                    else if (!string.IsNullOrEmpty(x.UsesContainerInterface))
                    {
                        flowCode.AppendLine($"{(hasCondition ? "\t" : string.Empty)}\t.Use(\"{writer.ArchitectureNamespace}.SoftwareSystems.{x.UsesContainerInterface}\")");
                    }

                    if (hasCondition)
                    {
                        flowCode.AppendLine($"\t.EndIf()");
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
            CsvToCSharpAaCWriter writer)
        {
            var architectureWorkspace = writer.ArchitectureWorkspace;
            var architectureClassSyntaxTree = classDeclaration.SyntaxTree;
            var architectureClassRoot = architectureClassSyntaxTree.GetRoot();
            var architectureProject = architectureWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == architectureClassSyntaxTree.FilePath));
            var architectureCompilation = architectureProject.GetCompilationAsync().Result;
            var architectureSemanticModel = architectureCompilation.GetSemanticModel(architectureClassSyntaxTree);

            var containerInterface = writer.ContainerInterfaceAaCPathToCsvRecordMap.GetValueOrDefault(architectureClassSyntaxTree.FilePath);

            if (containerInterface == null) return classDeclaration;

            var flowCode = new StringBuilder().AppendLine($"new Flow(ALIAS)");
            containerInterface.WithUses(writer.DataProvider)
                .ToList().ForEach(x =>
                {
                    var hasCondition = !string.IsNullOrEmpty(x.Condition);

                    if (hasCondition)
                    {
                        flowCode.AppendLine($"\t.If(\"{x.Condition}\")");
                    }

                    if (!string.IsNullOrEmpty(x.UsesContainerInterface))
                    {
                        flowCode.AppendLine($"{(hasCondition ? "\t" : string.Empty)}\t.Use(\"{writer.ArchitectureNamespace}.SoftwareSystems.{x.UsesContainerInterface}\")");
                    }
                    else if (!string.IsNullOrEmpty(x.UsesSoftwareSystemInterface))
                    {
                        flowCode.AppendLine($"{(hasCondition ? "\t" : string.Empty)}\t.Use(\"{writer.ArchitectureNamespace}.SoftwareSystems.{x.UsesSoftwareSystemInterface}\")");
                    }

                    if (hasCondition)
                    {
                        flowCode.AppendLine($"\t.EndIf()");
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
