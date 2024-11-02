using System;
using System.Collections.Concurrent;

namespace C4InterFlow.Cli
{

    public sealed class Context
    {
        // Singleton instance
        private static readonly Lazy<Context> _instance = new Lazy<Context>(() => new Context());

        // ConcurrentDictionary to hold context data
        private readonly ConcurrentDictionary<string, object> _current;

        // Private constructor to prevent instantiation from outside
        private Context()
        {
            _current = new ConcurrentDictionary<string, object>();
        }

        // Property to access the singleton instance
        public static Context Instance => _instance.Value;

        // Property to access the ConcurrentDictionary
        public ConcurrentDictionary<string, object> Current => _current;

        // Example methods for convenience
        public void AddOrUpdate(string key, object value) => _current.AddOrUpdate(key, value, (k, oldValue) => value);

        public bool TryGetValue(string key, out object value) => _current.TryGetValue(key, out value);

        public bool Remove(string key) => _current.TryRemove(key, out _);

        public object GetOrAdd(string key, Func<string, object> valueFactory) => _current.GetOrAdd(key, valueFactory);
    }
}
