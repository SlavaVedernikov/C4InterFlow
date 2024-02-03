using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C4InterFlow.Elements.Interfaces
{
    public interface IElementsResolver
    {
        public Type? GetType(string alias);
        public T? GetInstance<T>(string? alias) where T : class;
        public IEnumerable<string> ResolveWildcardStructures(IEnumerable<string> structures);
        public IEnumerable<Interface> GetAllInterfaces();
    }
}
