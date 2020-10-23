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
    public class CodeCreator
    {
        public static TypeBuilder GetTypeBuilder()
        {
            var typeSignature = "MyDynamicType";
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
                    null);
            return tb;
        }
    }

    static class Method
    {
        public static MethodInfo Of<TResult>(Expression<Func<TResult>> f) => ((MethodCallExpression)f.Body).Method;
        public static MethodInfo Of<T>(Expression<Action<T>> f) => ((MethodCallExpression)f.Body).Method;
        public static MethodInfo Of(Expression<Action> f) => ((MethodCallExpression)f.Body).Method;
    }

    public class Proxy
    {
        public Action action { get; set; }

        public void Call()
        {
            action();
        }

        public void Dummy()
        {

        }
    }

    public static class Invokes
    {
        public static Dictionary<string, Action> Actions = new Dictionary<string, Action>();
        static object realObject;
        static Type Type;

        public static void SetRealObject<T>(T obj)
        {
            realObject = obj;
            Type = typeof(T);
        }

        public static void SetRealObject(Type t, object obj)
        {
            realObject = obj;
            Type = t;
        }

        public static void Invoke(string name)
        {
            if(Actions.ContainsKey(name))
                Actions[name].Invoke();
        }
    }

    public interface IInstanceKeeper
    {
    }

    public class InstanceKeeper<T> : IInstanceKeeper
    {
        public Type GenericType;
        public Func<T> Func { get; set; }

        public InstanceKeeper(Func<T> func)
        {
            GenericType = typeof(T);
            Func = func;
        }
    }

    public class ClassAnalyser<T>
    {
        public Proxy proxy;

        Dictionary<Type, object> allowedTypes = new Dictionary<Type, object>
        {
            {typeof(string), ""},
            {typeof(int), 1},
            {typeof(double), 1d},
            {typeof(decimal), 1m}
        };

        T t;
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

        IEnumerable<OpCode> getOpcodesForParameters(int parameterCount)
        {
            var list = new List<OpCode>();
            switch (parameterCount)
            {
                case 0:
                    break;
                case 1:
                    list.Add(OpCodes.Ldarg_0);
                    break;
                case 2:
                    list.Add(OpCodes.Ldarg_0);
                    list.Add(OpCodes.Ldarg_1);
                    break;
                case 3:
                    list.Add(OpCodes.Ldarg_0);
                    list.Add(OpCodes.Ldarg_1);
                    list.Add(OpCodes.Ldarg_2);
                    break;
                default:
                    list.Add(OpCodes.Ldarg_0);
                    list.Add(OpCodes.Ldarg_1);
                    list.Add(OpCodes.Ldarg_2);
                    list.Add(OpCodes.Ldarg_3);
                    //for (int i = 4; i < parameterCount; i++)
                    //{
                    //    list.Add(OpCodes.Ldarg, methodInfo.GetParameters()[i].ParameterType);
                    //}
                    break;
            }
            return list;
        }

        void createOpcodeForParameters(ILGenerator il, int parameterCount, bool This = true)
        {
            var opCodes = getOpcodesForParameters(parameterCount + 1);
            if (!This)
                opCodes = opCodes.Skip(1);
            foreach (var opCode in opCodes)
                il.Emit(opCode);
        }

        void createOpcode(ILGenerator il, MethodInfo methodInfo, MethodInfo realMethodInfo = null, FieldInfo fieldInfo = null)
        {
            var name = methodInfo.Name;
            var parameterCount = methodInfo.GetParameters().Length;

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, name);
            il.Emit(OpCodes.Call, typeof(Invokes).GetMethod("Invoke"));
            il.Emit(OpCodes.Pop);

            if(realMethodInfo != null && fieldInfo != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fieldInfo);
                createOpcodeForParameters(il, parameterCount, false);
                il.Emit(OpCodes.Call, realMethodInfo);
            }

            il.Emit(OpCodes.Ret);
        }

        public T createSut(ConstructorInfo constructorInfo, Dictionary<Type, Func<object>> instanceKeepers)
        {
            var parameterlist = parametersPerConstructor[constructorInfo];
            List<object> parameters = new List<object>();

            foreach (var parameter in parameterlist)
            {
                if (parameter.ParameterType.IsInterface)
                {
                    var typeBuilder = CodeCreator.GetTypeBuilder();
                    typeBuilder.AddInterfaceImplementation(parameter.ParameterType);

                    object instance = null;
                    FieldBuilder fieldBuilder = null;
                    if(instanceKeepers.ContainsKey(parameter.ParameterType))
                    {
                        instance = instanceKeepers[parameter.ParameterType]();
                        fieldBuilder = typeBuilder.DefineField("realInstance", instance.GetType(), FieldAttributes.Private);
                    }

                    var methods = parameter.ParameterType.GetMethods();
                    foreach (var method in methods)
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

                        ILGenerator il = methodBuilder.GetILGenerator();
                        if(instance == null)
                            this.createOpcode(il, method);
                        else
                            this.createOpcode(il, method, instance.GetType().GetMethod(method.Name), fieldBuilder);
                        typeBuilder.DefineMethodOverride(methodBuilder, parameter.ParameterType.GetMethod(method.Name));
                    }
                    var mock = Activator.CreateInstance(typeBuilder.CreateType());
                    if (instance != null)
                    {
                        var mockType = mock.GetType();
                        var field = mockType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                        field[0].SetValue(mock, instance);
                    }
                    parameters.Add(mock);
                }
                else if (allowedTypes.Keys.Any(x => x == parameter.ParameterType))
                {

                }
            }

            return (T)Activator.CreateInstance(type, args: parameters.ToArray());
        }
    }
}
