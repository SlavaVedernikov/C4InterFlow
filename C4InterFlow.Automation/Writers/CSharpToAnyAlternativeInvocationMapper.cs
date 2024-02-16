using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace C4InterFlow.Automation.Writers
{
    public delegate void CSharpToAnyAlternativeInvocationMapper(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, IList<string> usesList, CSharpToAnyArchitectureAsCodeWriter writer, Dictionary<string, object>? args);

    public record NetToAnyAlternativeInvocationMapperConfig
    {
        public CSharpToAnyAlternativeInvocationMapper Mapper { get; init; }
        public Dictionary<string, object> Args { get; init; }
    }
}
