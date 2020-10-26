using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ClassInTheMiddle.Library.Services
{
    class InstanceCollection : IFactoryAdder
    {
        ConcurrentDictionary<Type, Func<object>> factories = new ConcurrentDictionary<Type, Func<object>>();

        public IFactoryAdder AddFactory<T>(Func<object> func)
        {
            factories.TryAdd(typeof(T), func);
            return this;
        }

        public bool TryGetInstance(Type type, out object result)
        {
            result = null;
            if (factories.ContainsKey(type))
            {
                result = factories[type].Invoke();
                return true;
            }
            return false;
        }

        public bool TryGetInstance<T>(out object result)
        {
            result = null;
            if (factories.ContainsKey(typeof(T)))
            {
                result = factories[typeof(T)].Invoke();
                return true;
            }
            return false;
        }

        public T GetInstance<T>()
        {
            return (T)factories[typeof(T)].Invoke();
        }
    }
}
