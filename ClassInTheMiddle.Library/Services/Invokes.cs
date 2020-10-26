using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ClassInTheMiddle.Library.Services
{
    public class Invokes : IInvokesSetFunktion
    {
        ConcurrentDictionary<string, Func<object[], object>> Functions = new ConcurrentDictionary<string, Func<object[], object>>();

        public static string GetMethodName(MethodInfo methodInfo) => $"{methodInfo.DeclaringType.FullName}.{methodInfo.Name}";

        public IInvokesSetFunktion SetFunction<T>(Expression<Action<T>> expression, Func<object[], object> function)
        {
            Functions.TryAdd(GetMethodName(((MethodCallExpression)expression.Body).Method), function);
            return this;
        }

        public IInvokesSetFunktion SetFunction(MethodInfo methodInfo, Func<object[], object> function)
        {
            Functions.TryAdd(GetMethodName(methodInfo), function);
            return this;
        }

        public void InvokeVoid(string name)
        {
            if (Functions.ContainsKey(name))
                Functions[name].Invoke(null);
        }

        public void InvokeVoidWithParameters(string name, params object[] parameters)
        {
            if (Functions.ContainsKey(name))
                Functions[name].Invoke(parameters);
        }

        public object Invoke(string name)
        {
            if (Functions.ContainsKey(name))
                return Functions[name].Invoke(null);
            return null;
        }

        public object InvokeWithParameters(string name, params object[] parameters)
        {
            if (Functions.ContainsKey(name))
                return Functions[name].Invoke(parameters);
            return null;
        }
    }
}
