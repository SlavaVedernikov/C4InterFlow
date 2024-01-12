using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace C4InterFlow.Automation
{
    public delegate void NetToNetAlternativeInvocationMapper(InvocationExpressionSyntax invocationExpressionSyntax, SemanticModel semanticModel, IList<string> usesList, NetToNetArchitectureAsCodeWriter writer, Dictionary<string, object>? args);

    public record NetToNetAlternativeInvocationMapperConfig
    {
        public NetToNetAlternativeInvocationMapper Mapper {get; init; }
        public Dictionary<string, object> Args { get; init; }
    }
}
