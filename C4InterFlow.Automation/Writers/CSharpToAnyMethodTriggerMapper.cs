using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace C4InterFlow.Automation.Writers
{
    public delegate void CSharpToAnyMethodTriggerMapper(MethodDeclarationSyntax methodDeclarationSyntax, SemanticModel semanticModel, IList<string> usesList, CSharpToAnyArchitectureAsCodeWriter writer);
}
