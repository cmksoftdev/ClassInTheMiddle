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
        public static TypeBuilder GetTypeBuilder(string typeSignature = "UnnamedType", Type baseType = null)
        {
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

    class CodeInjetion
    {
        public static void InjectMethod(MethodInfo methodToReplace, MethodInfo methodToInject)
        {
            //            MethodInfo methodToReplace = typeof(Target).GetMethod("targetMethod" + funcNum, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            //           MethodInfo methodToInject = typeof(Injection).GetMethod("injectionMethod" + funcNum, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);


            RuntimeHelpers.PrepareMethod(methodToReplace.MethodHandle);
            RuntimeHelpers.PrepareMethod(methodToInject.MethodHandle);

            //try
            //{
            //    RuntimeHelpers.PrepareMethod(methodToReplace.MethodHandle);
            //}
            //catch {}
            //try
            //{
            //    RuntimeHelpers.PrepareMethod(methodToInject.MethodHandle);
            //}
            //catch {}

            unsafe
            {
                if (IntPtr.Size == 4)
                {
                    int* inj = (int*)methodToInject.MethodHandle.Value.ToPointer() + 2;
                    int* tar = (int*)methodToReplace.MethodHandle.Value.ToPointer() + 2;
#if DEBUG
                    Console.WriteLine("\nVersion x86 Debug\n");

                    byte* injInst = (byte*)*inj;
                    byte* tarInst = (byte*)*tar;

                    int* injSrc = (int*)(injInst + 1);
                    int* tarSrc = (int*)(tarInst + 1);

                    *tarSrc = (((int)injInst + 5) + *injSrc) - ((int)tarInst + 5);
#else
                    Console.WriteLine("\nVersion x86 Release\n");
                    *tar = *inj;
#endif
                }
                else
                {

                    long* inj = (long*)methodToInject.MethodHandle.Value.ToPointer() + 1;
                    long* tar = (long*)methodToReplace.MethodHandle.Value.ToPointer() + 1;
#if DEBUG
                    Console.WriteLine("\nVersion x64 Debug\n");
                    byte* injInst = (byte*)*inj;
                    byte* tarInst = (byte*)*tar;


                    int* injSrc = (int*)(injInst + 1);
                    int* tarSrc = (int*)(tarInst + 1);

                    *tarSrc = (((int)injInst + 5) + *injSrc) - ((int)tarInst + 5);
#else
                    Console.WriteLine("\nVersion x64 Release\n");
                    *tar = *inj;
#endif
                }
            }
        }
    }

    public class ClassAnalyser<T>
    {
        Type type;

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

        void createProxyMethod(TypeBuilder typeBuilder, MethodInfo method, object instance, FieldBuilder fieldBuilder, bool isInterface)
        {
            var methodparameters = method.GetParameters();
            var p = methodparameters?.Select(x => x.ParameterType).ToList();
            p.Insert(0, this.GetType());
            var methodBuilder = typeBuilder.DefineMethod(
                method.Name,
                method.Attributes,//(MethodAttributes)(method.Attributes - MethodAttributes.Abstract) | MethodAttributes.HideBySig,
                method.ReturnType,
                methodparameters?.Select(x => x.ParameterType).ToArray());
            OpcodeGenerator opcodeGenerator;
            MethodBuilder realMethod = null;
            if (!isInterface && instance != null)
            {
                realMethod = typeBuilder.DefineMethod(
                    method.Name + "_real",
                    method.Attributes | MethodAttributes.Private,//(MethodAttributes)(method.Attributes - MethodAttributes.Abstract) | MethodAttributes.HideBySig,
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
                opcodeGenerator.CreateOpcode(method, isInterface);
            else
            {
                opcodeGenerator.CreateOpcode(
                    method, 
                    isInterface,
                    !isInterface && instance != null ? realMethod : instance.GetType().GetMethod(method.Name), 
                    fieldBuilder);
            }

            //typeBuilder.DefineM
            if(isInterface)
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

        public object createMock(TypeBuilder typeBuilder, object instance, FieldBuilder fieldBuilder, Type parameterType, bool isInterface = false)
        {
            var methods = parameterType.GetMethods();
            var methodsToIgnore = typeof(object).GetMethods();
            foreach (var method in methods)
            {
                if (methodsToIgnore.Any(x => x.Name == method.Name))
                    continue;
                createProxyMethod(typeBuilder, method, instance, fieldBuilder, isInterface);
            }
            var mock = Activator.CreateInstance(typeBuilder.CreateType());
            methods = parameterType.GetMethods();// mock.GetType().GetMethods();
            if(!isInterface && instance != null)
            {
                foreach (var method in methods)
                {
                    if (methodsToIgnore.Any(x => x.Name == method.Name))
                        continue;
                    var methodDummy = mock.GetType().GetMethod(method.Name + "_real", BindingFlags.Instance | BindingFlags.NonPublic);
                    CodeInjetion.InjectMethod(methodDummy, method);
                    CodeInjetion.InjectMethod(method, mock.GetType().GetMethod(method.Name));
                    //CodeInjetion.InjectMethod(methodDummy, method);
                    //CodeInjetion.InjectMethod(method, methodDummy);
                }
            }
            //CodeInjetion.InjectMethod(methods[0], mock.GetType().GetMethod(methods[0].Name));
            //foreach (var method in methods)
            //{
            //    //CodeInjetion.InjectMethod(method, mock.GetType().GetMethod(method.Name));
            //}
            return mock;
        }

        public void setInstanceToFiled(object instance, object mock)
        {
            if (instance == null)
                return;
            var mockType = mock.GetType();
            var field = mockType.GetField("realInstance", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(mock, instance);
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

                var mock = createMock(typeBuilder, instance, fieldBuilder, parameter.ParameterType, isInterface);
                List.Add(mock);
                setInstanceToFiled(instance, mock);
                parameters.Add(mock);
            }

            return (T)Activator.CreateInstance(type, args: parameters.ToArray());
        }
    }
}
