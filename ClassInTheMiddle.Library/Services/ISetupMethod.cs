using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ClassInTheMiddle.Library.Services
{
    public interface ISetupAction<T1> { public void SetUp(Expression<Action<T1>> action); }
    public interface ISetupAction<T1, T2> { public void SetUp(Expression<Action<T1, T2>> action); }
    public interface ISetupAction<T1, T2, T3> { public void SetUp(Expression<Action<T1, T2, T3>> action); }
    public interface ISetupAction<T1, T2, T3, T4> { public void SetUp(Expression<Action<T1, T2, T3, T4>> action); }
    public interface ISetupAction<T1, T2, T3, T4, T5> { public void SetUp(Expression<Action<T1, T2, T3, T4, T5>> action); }
    public interface ISetupAction<T1, T2, T3, T4, T5, T6> { public void SetUp(Expression<Action<T1, T2, T3, T4, T5, T6>> action); }
    public interface ISetupAction<T1, T2, T3, T4, T5, T6, T7> { public void SetUp(Expression<Action<T1, T2, T3, T4, T5, T6, T7>> action); }
    public interface ISetupAction<T1, T2, T3, T4, T5, T6, T7, T8> { public void SetUp(Expression<Action<T1, T2, T3, T4, T5, T6, T7, T8>> action); }

    public interface ISetupFunc<T1> { public void SetUp(Expression<Func<T1>> func); }
    public interface ISetupFunc<T1, T2> { public void SetUp(Expression<Func<T1, T2>> func); }
    public interface ISetupFunc<T1, T2, T3> { public void SetUp(Expression<Func<T1, T2, T3>> func); }
    public interface ISetupFunc<T1, T2, T3, T4> { public void SetUp(Expression<Func<T1, T2, T3, T4>> func); }
    public interface ISetupFunc<T1, T2, T3, T4, T5> { public void SetUp(Expression<Func<T1, T2, T3, T4, T5>> func); }
    public interface ISetupFunc<T1, T2, T3, T4, T5, T6> { public void SetUp(Expression<Func<T1, T2, T3, T4, T5, T6>> func); }
    public interface ISetupFunc<T1, T2, T3, T4, T5, T6, T7> { public void SetUp(Expression<Func<T1, T2, T3, T4, T5, T6, T7>> func); }
    public interface ISetupFunc<T1, T2, T3, T4, T5, T6, T7, T8> { public void SetUp(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8>> func); }

}
