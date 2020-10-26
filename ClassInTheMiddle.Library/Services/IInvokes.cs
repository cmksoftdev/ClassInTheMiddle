using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ClassInTheMiddle.Library.Services
{
    public interface IInvokesSetFunktion
    {
        IInvokesSetFunktion SetFunction(MethodInfo methodInfo, Func<object[], object> function);
        IInvokesSetFunktion SetFunction<T>(Expression<Action<T>> expression, Func<object[], object> function);
    }
}