using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace C4InterFlow.Automation
{
    public delegate void NetToAnyMethodTriggerMapper(MethodDeclarationSyntax methodDeclarationSyntax, SemanticModel semanticModel, IList<string> usesList, NetToAnyArchitectureAsCodeWriter writer);
}
