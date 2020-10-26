using System;

namespace ClassInTheMiddle.Library.Services
{
    public interface IFactoryAdder
    {
        IFactoryAdder AddFactory<T>(Func<object> func);
    }
}