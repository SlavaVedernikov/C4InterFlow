using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace C4InterFlow.Automation
{
    public delegate void NetToNetMethodTriggerMapper(MethodDeclarationSyntax methodDeclarationSyntax, SemanticModel semanticModel, IList<string> usesList, NetToNetArchitectureAsCodeWriter writer);
}
