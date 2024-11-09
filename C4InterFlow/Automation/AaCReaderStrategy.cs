using C4InterFlow.Structures.Interfaces;
using C4InterFlow.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C4InterFlow.Commons;

namespace C4InterFlow.Automation
{
    public interface IAaCReaderStrategy : IStructuresResolver
    {
        string GetComponentInterfaceAlias();
        string GetComponentInterfaceAlias(string filePath);
        (string name, bool isRequired)[] GetParameterDefinitions();
        void Initialise(string[]? architectureInputPaths, string[]? viewsInputPaths, Dictionary<string, string>? parameters);
        bool IsInitialised { get; }
    }

    public abstract class AaCReaderStrategy<ElementsResolverType> : IAaCReaderStrategy where ElementsResolverType : IStructuresResolver, new()
    {
        protected string[]? ArchitectureInputPaths { get; private set; }
        protected string[]? ViewsInputPaths { get; private set; }
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
        public abstract ElementsResolverType ElementsResolver { get; }
        public virtual (string name, bool isRequired)[] GetParameterDefinitions()
        {
            return new (string name, bool isRequired)[] { };
        }
        public virtual void Initialise(string[]? architectureInputPaths, string[]? viewsInputPaths, Dictionary<string, string>? parameters)
        {
            ArchitectureInputPaths = architectureInputPaths;
            ViewsInputPaths = viewsInputPaths;
            _isInitialised = true;
        }

        public T? GetInstance<T>(string alias) where T : Structure
        {
            return ElementsResolver.GetInstance<T>(alias);
        }

        public IEnumerable<string> ResolveStructures(IEnumerable<string> structures)
        {
            return ElementsResolver.ResolveStructures(structures);
        }

        public IEnumerable<Interface> GetAllInterfaces()
        {
            return ElementsResolver.GetAllInterfaces();
        }

        public IEnumerable<T> GetNestedInstances<T>(string? alias) where T : Structure
        {
            return ElementsResolver.GetNestedInstances<T>(alias);
        }

        public void Validate(out IEnumerable<LogMessage> errors)
        {
            ElementsResolver.Validate(out errors);
        }
    }
}
