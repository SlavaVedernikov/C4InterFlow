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
        private JObject? rootJObject { get; set; }

        public JObjectElementsResolver()
        {
            rootJObject = null;
        }
        public JObjectElementsResolver(JObject rootJObject)
        {
            this.rootJObject = rootJObject;
        }

        public T? GetInstance<T>(string? alias) where T : class
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

        public IEnumerable<Interface> GetAllInterfaces()
        {
            var result = new List<Interface>();

            //TODO: Add logic

            return result;
        }
    }
}
