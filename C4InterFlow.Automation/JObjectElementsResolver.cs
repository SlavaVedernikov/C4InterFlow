using C4InterFlow.Elements.Interfaces;
using YamlDotNet.RepresentationModel;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;
using System.Text;
using C4InterFlow.Cli;

namespace C4InterFlow.Elements
{
    public class JObjectElementsResolver : IElementsResolver
    {
        private JObject? JsonObject { get; set; }

        public JObjectElementsResolver()
        {
        }
        public JObjectElementsResolver(JObject jsonObject)
        {
            JsonObject = jsonObject;
        }

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
