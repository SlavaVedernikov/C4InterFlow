using C4InterFlow.Elements.Interfaces;
using C4InterFlow.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C4InterFlow.Automation
{
    public interface IArchitectureAsCodeReaderContext : IElementsResolver
    {
        string GetComponentInterfaceAlias();
        string GetComponentInterfaceAlias(string filePath);

        IEnumerable<Interface> GetAllInterfaces();
    }

    public abstract class ArchitectureAsCodeReaderContext<ElementsResolverType> : IArchitectureAsCodeReaderContext where ElementsResolverType : IElementsResolver, new()
    {
        public abstract string GetComponentInterfaceAlias();
        public abstract string GetComponentInterfaceAlias(string filePath);
        public abstract IEnumerable<Interface> GetAllInterfaces();
        private ElementsResolverType ElementsResolver { get => new ElementsResolverType(); }
        public Type? GetType(string alias)
        {
            return ElementsResolver.GetType(alias);
        }

        public T? GetInstance<T>(string alias) where T : class
        {
            return ElementsResolver.GetInstance<T>(alias);
        }

        public IEnumerable<string> ResolveWildcardStructures(IEnumerable<string> structures)
        {
            return ElementsResolver.ResolveWildcardStructures(structures);
        }
    }
}
