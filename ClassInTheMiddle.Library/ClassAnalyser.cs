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
    public class TypeBuilderFactory
    {
        public static TypeBuilder GetTypeBuilder(string typeName = "UnnamedType", Type baseType = null)
        {
            var typeSignature = typeName;
            var an = new AssemblyName(typeSignature);
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            TypeBuilder tb = moduleBuilder.DefineType(typeSignature,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    baseType);
            return tb;
        }
    }

    static class Method
    {
        public static MethodInfo Of<TResult>(Expression<Func<TResult>> f) => ((MethodCallExpression)f.Body).Method;
        public static MethodInfo Of<T>(Expression<Action<T>> f) => ((MethodCallExpression)f.Body).Method;
        public static MethodInfo Of(Expression<Action> f) => ((MethodCallExpression)f.Body).Method;
    }

    public static class Invokes
    {
        public static Dictionary<string, Action> Actions = new Dictionary<string, Action>();

        public static void Invoke(string name)
        {
            if (Actions.ContainsKey(name))
                Actions[name].Invoke();
        }
    }

    public class ClassAnalyser<T>
    {
        Type type;

        Dictionary<ConstructorInfo, List<ParameterInfo>> parametersPerConstructor = new Dictionary<ConstructorInfo, List<ParameterInfo>>();
        Dictionary<MethodInfo, List<ParameterInfo>> methodsAndParameters = new Dictionary<MethodInfo, List<ParameterInfo>>();

        public ClassAnalyser(Dictionary<Type, Func<object>> instanceKeepers)
        {
            type = typeof(T);
            analyseDependencies();
            analyseMethods();
            SUT = createSut(parametersPerConstructor.First().Key, instanceKeepers);
        }

        public T SUT;

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

        void createProxyMethod(TypeBuilder typeBuilder, MethodInfo method, object instance, FieldBuilder fieldBuilder)
        {
            var methodparameters = method.GetParameters();
            var p = methodparameters?.Select(x => x.ParameterType).ToList();
            p.Insert(0, this.GetType());
            var methodBuilder = typeBuilder.DefineMethod(
                method.Name,
                (MethodAttributes)(method.Attributes - MethodAttributes.Abstract),
                method.ReturnType,
                methodparameters?.Select(x => x.ParameterType).ToArray());

            foreach (var methodparameter in methodparameters)
            {
                var parameterBuilder = methodBuilder.DefineParameter(methodparameter.Position + 1, methodparameter.Attributes, methodparameter.Name);
            }

            OpcodeGenerator opcodeGenerator = new OpcodeGenerator(methodBuilder.GetILGenerator());
            if (instance == null)
                opcodeGenerator.CreateOpcode(method);
            else
                opcodeGenerator.CreateOpcode(method, instance.GetType().GetMethod(method.Name), fieldBuilder);
            //typeBuilder.DefineM
            typeBuilder.DefineMethodOverride(methodBuilder, method);// parameter.ParameterType.GetMethod(method.Name));
        }

        object createFieldForRealInstance(TypeBuilder typeBuilder, Dictionary<Type, Func<object>> instanceKeepers, Type parameterType, out FieldBuilder fieldBuilder)
        {
            object instance = null;
            fieldBuilder = null;
            if (instanceKeepers.ContainsKey(parameterType))
            {
                instance = instanceKeepers[parameterType]();
                fieldBuilder = typeBuilder.DefineField("realInstance", instance.GetType(), FieldAttributes.Private);
            }
            return instance;
        }

        public object createMock(TypeBuilder typeBuilder, object instance, FieldBuilder fieldBuilder, Type parameterType)
        {
            var methods = parameterType.GetMethods();
            foreach (var method in methods)
            {
                createProxyMethod(typeBuilder, method, instance, fieldBuilder);
            }
            return Activator.CreateInstance(typeBuilder.CreateType());
        }

        public void setInstanceToFiled(object instance, object mock)
        {
            if (instance == null)
                return;
            var mockType = mock.GetType();
            var field = mockType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            field[0].SetValue(mock, instance);
        }

        public T createSut(ConstructorInfo constructorInfo, Dictionary<Type, Func<object>> instanceKeepers)
        {
            var parameterlist = parametersPerConstructor[constructorInfo];
            List<object> parameters = new List<object>();

            foreach (var parameter in parameterlist)
            {
                bool isInterface = false;
                TypeBuilder typeBuilder;
                if (parameter.ParameterType.IsInterface)
                {
                    isInterface = true;
                    typeBuilder = TypeBuilderFactory.GetTypeBuilder(type.Name + "_Proxy");
                    typeBuilder.AddInterfaceImplementation(parameter.ParameterType);
                }
                else if (parameter.ParameterType.IsClass)
                {
                    typeBuilder = TypeBuilderFactory.GetTypeBuilder(type.Name + "_Proxy", parameter.ParameterType);
                }
                else //if (allowedTypes.Keys.Any(x => x == parameter.ParameterType))
                {
                    continue;
                }


                object instance = createFieldForRealInstance(typeBuilder, instanceKeepers, parameter.ParameterType, out FieldBuilder fieldBuilder);

                var mock = createMock(typeBuilder, instance, fieldBuilder, parameter.ParameterType);
                setInstanceToFiled(instance, mock);
                parameters.Add(mock);
            }

            return (T)Activator.CreateInstance(type, args: parameters.ToArray());
        }
    }
}
