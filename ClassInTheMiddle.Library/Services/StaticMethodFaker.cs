using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ClassInTheMiddle.Library.Services
{
    public class StaticMethodFaker
    {
        List<StaticInvoke> staticInvokes;

        public void FakeStaticMethod (Type type, string name)
        {
            var method = type.GetMethod(name);
        }
        
        public void CreateFakesForSetup()
        {

        }

        public static MethodInfo GetMethodInfoFromExpression(MethodCallExpression method)
        {
            var methodInfo = method.Method;
            if (methodInfo == null)
            {
                throw new ArgumentException("The lambda expression 'method' should point to a valid Method");
            }
            return methodInfo;
        }

        public void FakeStaticProperty<T>(Expression<Func<T>> property)
        {
            var propertyInfo = ((MemberExpression)property.Body).Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException("The lambda expression 'property' should point to a valid Property");
            }
        }

        public ISetupAction<T1> FakeStaticMethod<T1>(Expression<Action<T1>> method)
        {
            var methodInfo = GetMethodInfoFromExpression((MethodCallExpression)method.Body);
            var staticInvoke = new StaticInvokeAction<T1>(methodInfo);
            staticInvokes.Add(staticInvoke);
            return staticInvoke;
        }

        public ISetupAction<T1, T2> FakeStaticMethod<T1, T2>(Expression<Action<T1, T2>> method)
        {
            var methodInfo = GetMethodInfoFromExpression((MethodCallExpression)method.Body);
            var staticInvoke = new StaticInvokeAction<T1, T2>(methodInfo);
            staticInvokes.Add(staticInvoke);
            return staticInvoke;
        }

        public ISetupAction<T1, T2, T3> FakeStaticMethod<T1, T2, T3>(Expression<Action<T1, T2, T3>> method)
        {
            var methodInfo = GetMethodInfoFromExpression((MethodCallExpression)method.Body);
            var staticInvoke = new StaticInvokeAction<T1, T2, T3>(methodInfo);
            staticInvokes.Add(staticInvoke);
            return staticInvoke;
        }



        public ISetupFunc<T1> FakeStaticMethod<T1>(Expression<Func<T1>> method)
        {
            var methodInfo = GetMethodInfoFromExpression((MethodCallExpression)method.Body);
            var staticInvoke = new StaticInvokeFunc<T1>(methodInfo);
            staticInvokes.Add(staticInvoke);
            return staticInvoke;
        }

        public ISetupFunc<T1, T2> FakeStaticMethod<T1, T2>(Expression<Func<T1, T2>> method)
        {
            var methodInfo = GetMethodInfoFromExpression((MethodCallExpression)method.Body);
            var staticInvoke = new StaticInvokeFunc<T1, T2>(methodInfo);
            staticInvokes.Add(staticInvoke);
            return staticInvoke;
        }

        public ISetupFunc<T1, T2, T3> FakeStaticMethod<T1, T2, T3>(Expression<Func<T1, T2, T3>> method)
        {
            var methodInfo = GetMethodInfoFromExpression((MethodCallExpression)method.Body);
            var staticInvoke = new StaticInvokeFunc<T1, T2, T3>(methodInfo);
            staticInvokes.Add(staticInvoke);
            return staticInvoke;
        }
    }
}
