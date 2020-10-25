using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ClassInTheMiddle.Library.Services
{
    public static class Invokes
    {
        static Dictionary<string, Func<object[], object>> Functions = new Dictionary<string, Func<object[], object>>();

        public static string GetMethodName(MethodInfo methodInfo) => $"{methodInfo.DeclaringType.FullName}.{methodInfo.Name}";

        public static void SetFunction<T>(Expression<Action<T>> expression, Func<object[], object> function)
        {
            Functions.Add(GetMethodName(((MethodCallExpression)expression.Body).Method), function);
        }

        public static void SetFunction(MethodInfo methodInfo, Func<object[], object> function)
        {
            Functions.Add(GetMethodName(methodInfo), function);
        }

        public static void InvokeVoid(string name)
        {
            if (Functions.ContainsKey(name))
                Functions[name].Invoke(null);
        }

        public static void InvokeVoidWithParameters(string name, params object[] parameters)
        {
            if (Functions.ContainsKey(name))
                Functions[name].Invoke(parameters);
        }

        public static object Invoke(string name)
        {
            if (Functions.ContainsKey(name))
                return Functions[name].Invoke(null);
            return null;
        }

        public static object InvokeWithParameters(string name, params object[] parameters)
        {
            if (Functions.ContainsKey(name))
                return Functions[name].Invoke(parameters);
            return null;
        }
    }
}
