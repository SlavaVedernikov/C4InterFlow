using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace C4InterFlow.Automation
{
    public delegate void NetToAnyAlternativeInvocationMapper(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, IList<string> usesList, NetToAnyArchitectureAsCodeWriter writer, Dictionary<string, object>? args);

    public record NetToAnyAlternativeInvocationMapperConfig
    {
        public NetToAnyAlternativeInvocationMapper Mapper {get; init; }
        public Dictionary<string, object> Args { get; init; }
    }
}
