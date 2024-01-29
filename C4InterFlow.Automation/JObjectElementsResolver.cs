using C4InterFlow.Elements.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace C4InterFlow.Elements
{
    public class JObjectElementsResolver : IElementsResolver
    {
        public T? GetInstance<T>(string alias) where T : class
        {
            if (string.IsNullOrEmpty(alias)) return default(T);

            //TODO: Add logic 

            return default(T);
        }

        public Type? GetType(string alias)
        {
            Type? result = null;

            if (alias == null) return result;

            //TODO: Add logic 

            return result;
        }

        public IEnumerable<string> ResolveWildcardStructures(IEnumerable<string> structures)
        {
            var result = new List<string>();

            //TODO: Add logic

            return result;
        }
    }
}
