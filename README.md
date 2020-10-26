# ClassInTheMiddle
C# Class Proxy to makes it possible to mock dependencies, even if they are no interfaces. See unit tests or example project.

Normal:
SUT =calls=> Dependency.Method

Using ClassInTheMiddle:
SUT =calls=> ProxyClass.Method =calls=> ConfigedFunc with parameters
                   (optional)  =calls=> Dependency.Method
