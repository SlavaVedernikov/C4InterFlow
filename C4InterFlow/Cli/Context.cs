using System;
using System.Collections.Concurrent;
using System.Threading;

namespace C4InterFlow.Cli
{
    public sealed class Context
    {
        // Thread-local storage for each thread's unique instance
        private static readonly ThreadLocal<Context> _threadInstance = new ThreadLocal<Context>(() => new Context());

        // ConcurrentDictionary to hold context data for each thread
        private readonly ConcurrentDictionary<string, object> _current;

        // Private constructor to prevent instantiation from outside
        private Context()
        {
            _current = new ConcurrentDictionary<string, object>();
        }

        // Property to access the thread-specific singleton instance
        public static Context Instance => _threadInstance.Value;

        // Property to access the ConcurrentDictionary
        public ConcurrentDictionary<string, object> Current => _current;

        // Example methods for convenience
        public void AddOrUpdate(string key, object value) => _current.AddOrUpdate(key, value, (k, oldValue) => value);

        public bool TryGetValue(string key, out object value) => _current.TryGetValue(key, out value);

        public bool Remove(string key) => _current.TryRemove(key, out _);

        public object GetOrAdd(string key, Func<string, object> valueFactory) => _current.GetOrAdd(key, valueFactory);
    }
}
