using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C4InterFlow.Structures.Interfaces
{
    public interface IStructuresResolver
    {
        public IEnumerable<T> GetNestedInstances<T>(string? alias) where T : Structure;
        public T? GetInstance<T>(string? alias) where T : Structure;
        public IEnumerable<string> ResolveStructures(IEnumerable<string> structures);
        public IEnumerable<Interface> GetAllInterfaces();
        public void Validate(out IEnumerable<ValidationError> errors);
    }
}
