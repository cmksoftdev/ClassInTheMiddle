using ClassInTheMiddle.Library.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace ClassInTheMiddle.Library
{

    static class Method
    {
        public static MethodInfo Of<TResult>(Expression<Func<TResult>> f) => ((MethodCallExpression)f.Body).Method;
        public static MethodInfo Of<T>(Expression<Action<T>> f) => ((MethodCallExpression)f.Body).Method;
        public static MethodInfo Of(Expression<Action> f) => ((MethodCallExpression)f.Body).Method;
    }


    public class ClassAnalyser<T>
    {
        Type type;

        const string REAL_INSTANCE_FIELD_NAME = "___realInstance";
        const string INVOKES_INSTANCE_FIELD = "___invokes";
        const string REAL_METHOD_ENDING = "___real";
        const string PROXY_CLASS_NAME_ENDING = "___proxy";

        Dictionary<ConstructorInfo, List<ParameterInfo>> parametersPerConstructor = new Dictionary<ConstructorInfo, List<ParameterInfo>>();
        Dictionary<MethodInfo, List<ParameterInfo>> methodsAndParameters = new Dictionary<MethodInfo, List<ParameterInfo>>();

        public List<object> List = new List<object>();

        public ClassAnalyser(Dictionary<Type, Func<object>> instanceKeepers)
        {
            type = typeof(T);
            analyseDependencies();
            analyseMethods();
            SUT = createSut(parametersPerConstructor.First().Key, instanceKeepers);
        }

        public T SUT;
        public Invokes Invokes { get; } = new Invokes();

        void analyseDependencies()
        {
            var ctors = type.GetConstructors();
            foreach (var ctor in ctors)
            {
                var list = new List<ParameterInfo>();
                foreach (var parameter in ctor.GetParameters())
                    list.Add(parameter);
                parametersPerConstructor.Add(ctor, list);
            }
        }

        void analyseMethods()
        {
            var methods = type.GetMethods();
            foreach (var method in methods)
            {
                var list = new List<ParameterInfo>();
                foreach (var parameter in method.GetParameters())
                    list.Add(parameter);
                methodsAndParameters.Add(method, list);
            }
        }

        void createProxyMethod(TypeBuilder typeBuilder, MethodInfo method, object instance, FieldBuilder fieldBuilder, FieldBuilder invokesFieldBuilder, bool isInterface)
        {
            var methodparameters = method.GetParameters();
            var p = methodparameters?.Select(x => x.ParameterType).ToList();
            p.Insert(0, this.GetType());
            var methodBuilder = typeBuilder.DefineMethod(
                method.Name,
                method.Attributes,
                method.ReturnType,
                methodparameters?.Select(x => x.ParameterType).ToArray());
            OpcodeGenerator opcodeGenerator;
            MethodBuilder realMethod = null;
            if (!isInterface && instance != null)
            {
                realMethod = typeBuilder.DefineMethod(
                    method.Name + REAL_METHOD_ENDING,
                    method.Attributes | MethodAttributes.Private,
                    method.ReturnType,
                    methodparameters?.Select(x => x.ParameterType).ToArray());

                foreach (var methodparameter in methodparameters)
                {
                    var parameterBuilder = realMethod.DefineParameter(methodparameter.Position + 1, methodparameter.Attributes, methodparameter.Name);
                }

                opcodeGenerator = new OpcodeGenerator(realMethod.GetILGenerator());
                if(method.ReturnType == typeof(void))
                    opcodeGenerator.createSetMethodOpcode(realMethod);
                else
                    opcodeGenerator.createGetMethodOpcode(realMethod);
            }

            foreach (var methodparameter in methodparameters)
            {
                var parameterBuilder = methodBuilder.DefineParameter(methodparameter.Position + 1, methodparameter.Attributes, methodparameter.Name);
            }
            opcodeGenerator = new OpcodeGenerator(methodBuilder.GetILGenerator());

            if (instance == null)
                opcodeGenerator.CreateOpcode(invokesFieldBuilder, method, isInterface);
            else
            {
                opcodeGenerator.CreateOpcode(
                    invokesFieldBuilder,
                    method, 
                    isInterface,
                    !isInterface && instance != null ? realMethod : instance.GetType().GetMethod(method.Name), 
                    fieldBuilder);
            }
            if(isInterface)
                typeBuilder.DefineMethodOverride(methodBuilder, method);
        }

        object createFieldForRealInstance(TypeBuilder typeBuilder, Dictionary<Type, Func<object>> instanceKeepers, Type parameterType, out FieldBuilder fieldBuilder)
        {
            object instance = null;
            fieldBuilder = null;
            if (instanceKeepers.ContainsKey(parameterType))
            {
                instance = instanceKeepers[parameterType]();
                fieldBuilder = typeBuilder.DefineField(REAL_INSTANCE_FIELD_NAME, instance.GetType(), FieldAttributes.Private);
            }
            return instance;
        }

        FieldBuilder createFieldForProxyInvoker(TypeBuilder typeBuilder)
        {
            return typeBuilder.DefineField(INVOKES_INSTANCE_FIELD, Invokes.GetType(), FieldAttributes.Private);
        }

        public object createMock(TypeBuilder typeBuilder, object instance, FieldBuilder fieldBuilder, FieldBuilder invokesFieldBuilder, Type parameterType, bool isInterface = false)
        {
            var methods = parameterType.GetMethods();
            var methodsToIgnore = typeof(object).GetMethods();
            foreach (var method in methods)
            {
                if (methodsToIgnore.Any(x => x.Name == method.Name))
                    continue;
                createProxyMethod(typeBuilder, method, instance, fieldBuilder, invokesFieldBuilder, isInterface);
            }
            var mock = Activator.CreateInstance(typeBuilder.CreateType());
            methods = parameterType.GetMethods();
            if(!isInterface && instance != null)
            {
                foreach (var method in methods)
                {
                    if (methodsToIgnore.Any(x => x.Name == method.Name))
                        continue;
                    var methodDummy = mock.GetType().GetMethod(method.Name + REAL_METHOD_ENDING, BindingFlags.Instance | BindingFlags.NonPublic);
                    MethodChanger.Change(methodDummy, method);
                    MethodChanger.Change(method, mock.GetType().GetMethod(method.Name));
                }
            }
            return mock;
        }

        public void setInstanceToFiled(string name, object instance, object mock)
        {
            if (instance == null)
                return;
            var mockType = mock.GetType();
            var field = mockType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(mock, instance);
        }

        public bool TryCreateProxyClass(Type resultType, out object result, Dictionary<Type, Func<object>> instanceKeepers)
        {
            bool isInterface = false;
            TypeBuilder typeBuilder = null;
            result = null;
            if (resultType.IsInterface)
            {
                isInterface = true;
                typeBuilder = TypeBuilderFactory.GetTypeBuilder(type.Name + PROXY_CLASS_NAME_ENDING);
                typeBuilder.AddInterfaceImplementation(resultType);
            }
            else if (resultType.IsClass)
            {
                typeBuilder = TypeBuilderFactory.GetTypeBuilder(type.Name + PROXY_CLASS_NAME_ENDING, resultType);
            }
            else
            {
                return false;
            }

            object instance = createFieldForRealInstance(typeBuilder, instanceKeepers, resultType, out FieldBuilder fieldBuilder);
            var invokesFieldBuilder = createFieldForProxyInvoker(typeBuilder);

            var mock = createMock(typeBuilder, instance, fieldBuilder, invokesFieldBuilder, resultType, isInterface);
            List.Add(mock);
            setInstanceToFiled(REAL_INSTANCE_FIELD_NAME, instance, mock);
            setInstanceToFiled(INVOKES_INSTANCE_FIELD, Invokes, mock);
            result = mock;
            return true;
        }

        public T createSut(ConstructorInfo constructorInfo, Dictionary<Type, Func<object>> instanceKeepers)
        {
            var parameterlist = parametersPerConstructor[constructorInfo];
            List<object> parameters = new List<object>();

            foreach (var parameter in parameterlist)
            {
                if (TryCreateProxyClass(parameter.ParameterType, out object mock, instanceKeepers))
                {
                    parameters.Add(mock);
                }
            }

            return (T)Activator.CreateInstance(type, args: parameters.ToArray());
        }
    }
}
