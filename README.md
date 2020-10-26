# ClassInTheMiddle
C# Class Proxy to makes it possible to mock dependencies, even if they are no interfaces. See unit tests or example project.

### Normal:
#### SUT 
1. =calls=> Dependency.Method

### Using ClassInTheMiddle:

#### SUT 
1. =calls=> ProxyClass.Method 
2. =calls=> configure Func with parameters
3. =calls=> Dependency.Method (optional)
