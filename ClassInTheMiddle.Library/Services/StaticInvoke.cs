using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ClassInTheMiddle.Library.Services
{
    abstract class StaticInvoke
    {
        protected StaticInvoke(MethodInfo originMethodInfo)
        {
            OriginMethodInfo = originMethodInfo;
        }
        public bool IsFieldNeeded { get; protected set; } = false;
        public MethodInfo OriginMethodInfo { get; protected set; }
        public MethodInfo FakeMethodInfo { get; protected set; }
    }

    class StaticInvokeAction<T1> : StaticInvoke, ISetupAction<T1>
    {
        public StaticInvokeAction(MethodInfo originMethodInfo) : base(originMethodInfo) {}

        public void SetUp(Expression<Action<T1>> action)
        {
            FakeMethodInfo = StaticMethodFaker.GetMethodInfoFromExpression((MethodCallExpression)action.Body);
        }
    }

    class StaticInvokeAction<T1, T2> : StaticInvoke, ISetupAction<T1, T2>
    {
        public StaticInvokeAction(MethodInfo originMethodInfo) : base(originMethodInfo) { }

        public void SetUp(Expression<Action<T1, T2>> action)
        {
            FakeMethodInfo = StaticMethodFaker.GetMethodInfoFromExpression((MethodCallExpression)action.Body);
        }
    }

    class StaticInvokeAction<T1, T2, T3> : StaticInvoke, ISetupAction<T1, T2, T3>
    {
        public StaticInvokeAction(MethodInfo originMethodInfo) : base(originMethodInfo) { }

        public void SetUp(Expression<Action<T1, T2, T3>> action)
        {
            FakeMethodInfo = StaticMethodFaker.GetMethodInfoFromExpression((MethodCallExpression)action.Body);
        }
    }

    class StaticInvokeFunc<T1> : StaticInvoke, ISetupFunc<T1>
    {
        public StaticInvokeFunc(MethodInfo originMethodInfo) : base(originMethodInfo) { }

        public void SetUp(Expression<Func<T1>> action)
        {
            FakeMethodInfo = StaticMethodFaker.GetMethodInfoFromExpression((MethodCallExpression)action.Body);
        }
    }

    class StaticInvokeFunc<T1, T2> : StaticInvoke, ISetupFunc<T1, T2>
    {
        public StaticInvokeFunc(MethodInfo originMethodInfo) : base(originMethodInfo) { }

        public void SetUp(Expression<Func<T1, T2>> action)
        {
            FakeMethodInfo = StaticMethodFaker.GetMethodInfoFromExpression((MethodCallExpression)action.Body);
        }
    }

    class StaticInvokeFunc<T1, T2, T3> : StaticInvoke, ISetupFunc<T1, T2, T3>
    {
        public StaticInvokeFunc(MethodInfo originMethodInfo) : base(originMethodInfo) { }

        public void SetUp(Expression<Func<T1, T2, T3>> action)
        {
            FakeMethodInfo = StaticMethodFaker.GetMethodInfoFromExpression((MethodCallExpression)action.Body);
        }
    }


}
