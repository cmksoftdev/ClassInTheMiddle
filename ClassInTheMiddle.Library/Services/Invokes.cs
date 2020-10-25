using System;
using System.Collections.Generic;

namespace ClassInTheMiddle.Library.Services
{
    public static class Invokes
    {
        public static Dictionary<string, Func<object[], object>> Functions = new Dictionary<string, Func<object[], object>>();

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
