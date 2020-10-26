using ClassInTheMiddle.Library.Services;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace ClassInTheMiddle.Library
{

    public class ClassInTheMiddle : IInvokesSetFunktion, IFactoryAdder
    {
        InstanceCollection instanceCollection = new InstanceCollection();
        ConcurrentDictionary<Type, object> classAnalysers = new ConcurrentDictionary<Type, object>();
        Invokes invokes = new Invokes();

        public IFactoryAdder AddFactory<T>(Func<object> func)
        {
            return instanceCollection.AddFactory<T>(func);
        }

        public T GetInstance<T>() => instanceCollection.GetInstance<T>();

        public T GetSut<T>()
        {
            var classAnalyser = new ClassAnalyser<T>(instanceCollection, invokes);
            classAnalysers.TryAdd(typeof(T), classAnalyser);
            return classAnalyser.SUT;
        }

        public IInvokesSetFunktion SetFunction(MethodInfo methodInfo, Func<object[], object> function)
        {
            return invokes.SetFunction(methodInfo, function);
        }

        public IInvokesSetFunktion SetFunction<T>(Expression<Action<T>> expression, Func<object[], object> function)
        {
            return invokes.SetFunction(expression, function);
        }
    }
}
