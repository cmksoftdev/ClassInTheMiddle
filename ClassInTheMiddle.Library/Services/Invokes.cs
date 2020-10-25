using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ClassInTheMiddle.Library.Services
{
    public class Invokes
    {
        Dictionary<string, Func<object[], object>> Functions = new Dictionary<string, Func<object[], object>>();

        public static string GetMethodName(MethodInfo methodInfo) => $"{methodInfo.DeclaringType.FullName}.{methodInfo.Name}";

        public void SetFunction<T>(Expression<Action<T>> expression, Func<object[], object> function)
        {
            Functions.Add(GetMethodName(((MethodCallExpression)expression.Body).Method), function);
        }

        public void SetFunction(MethodInfo methodInfo, Func<object[], object> function)
        {
            Functions.Add(GetMethodName(methodInfo), function);
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
