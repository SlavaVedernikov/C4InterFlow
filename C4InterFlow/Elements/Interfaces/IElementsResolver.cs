using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C4InterFlow.Elements.Interfaces
{
    public interface IElementsResolver
    {
        public IEnumerable<T> GetNestedInstances<T>(string? alias) where T : Structure;
        public T? GetInstance<T>(string? alias) where T : Structure;
        public IEnumerable<string> ResolveStructures(IEnumerable<string> structures);
        public IEnumerable<Interface> GetAllInterfaces();
    }
}
