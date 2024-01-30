using C4InterFlow.Elements.Interfaces;
using C4InterFlow.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C4InterFlow.Automation
{
    public interface IArchitectureAsCodeReaderStrategy : IElementsResolver
    {
        string GetComponentInterfaceAlias();
        string GetComponentInterfaceAlias(string filePath);
        IEnumerable<Interface> GetAllInterfaces();
        (string name, bool isRequired)[] GetParameterDefinitions();
        void Initialise(string? architectureInputPath, Dictionary<string, string>? parameters);
        bool IsInitialised { get; }
    }

    public abstract class ArchitectureAsCodeReaderStrategy<ElementsResolverType> : IArchitectureAsCodeReaderStrategy where ElementsResolverType : IElementsResolver, new()
    {
        protected string? ArchitectureInputPath { get; private set; }
        private bool _isInitialised = false;
        public virtual bool IsInitialised
        {
            get
            {
                return _isInitialised;
            }
        }
        public abstract string GetComponentInterfaceAlias();
        public abstract string GetComponentInterfaceAlias(string filePath);
        public abstract IEnumerable<Interface> GetAllInterfaces();
        protected abstract ElementsResolverType ElementsResolver { get; }
        public virtual (string name, bool isRequired)[] GetParameterDefinitions()
        {
            return new (string name, bool isRequired)[] { };
        }
        public virtual void Initialise(string? architectureInputPath, Dictionary<string, string>? parameters)
        {
            ArchitectureInputPath = architectureInputPath;
            _isInitialised = true;
        }
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
