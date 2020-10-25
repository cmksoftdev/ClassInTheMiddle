using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace ClassInTheMiddle.Library.Services
{
    class OpcodeGenerator
    {
        ILGenerator il;

        public OpcodeGenerator(ILGenerator il)
        {
            this.il = il;
        }

        IEnumerable<Tuple<OpCode, int?>> getOpcodesForParameters(int parameterCount)
        {
            var list = new List<Tuple<OpCode, int?>>();
            switch (parameterCount)
            {
                case 0:
                    break;
                case 1:
                    list.Add(Tuple.Create<OpCode, int?>(OpCodes.Ldarg_0, null));
                    break;
                case 2:
                    list.Add(Tuple.Create<OpCode, int?>(OpCodes.Ldarg_0, null));
                    list.Add(Tuple.Create<OpCode, int?>(OpCodes.Ldarg_1, null));
                    break;
                case 3:
                    list.Add(Tuple.Create<OpCode, int?>(OpCodes.Ldarg_0, null));
                    list.Add(Tuple.Create<OpCode, int?>(OpCodes.Ldarg_1, null));
                    list.Add(Tuple.Create<OpCode, int?>(OpCodes.Ldarg_2, null));
                    break;
                default:
                    list.Add(Tuple.Create<OpCode, int?>(OpCodes.Ldarg_0, null));
                    list.Add(Tuple.Create<OpCode, int?>(OpCodes.Ldarg_1, null));
                    list.Add(Tuple.Create<OpCode, int?>(OpCodes.Ldarg_2, null));
                    list.Add(Tuple.Create<OpCode, int?>(OpCodes.Ldarg_3, null));
                    for (int i = 4; i < parameterCount; i++)
                    {
                        list.Add(Tuple.Create<OpCode, int?>(OpCodes.Ldarg, i));
                    }
                    break;
            }
            return list;
        }

        void createArrayWithParameters(ParameterInfo[] parameterInfos)
        {
            LocalBuilder paramValues = il.DeclareLocal(typeof(object[]));
            il.Emit(OpCodes.Ldc_I4_S, parameterInfos.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            var list = getOpcodesForParameters(parameterInfos.Length + 1).ToArray();
            int i = 0;
            foreach (var item in parameterInfos)
            {
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4, i);
                emitArg(list[i + 1]);
                il.Emit(OpCodes.Box, item.ParameterType);
                il.Emit(OpCodes.Stelem_Ref);
                i++;
            }
            il.Emit(OpCodes.Stloc, paramValues);
            il.Emit(OpCodes.Ldloc, paramValues);
        }

        void emitArg(Tuple<OpCode, int?> arg)
        {
            if (arg.Item2 == null)
                il.Emit(arg.Item1);
            else
                il.Emit(arg.Item1, arg.Item2.Value);
        }

        void createOpcodeForParameters(ParameterInfo[] parameterInfos, bool box = true, bool This = true)
        {
            var opCodes = getOpcodesForParameters(parameterInfos.Length + 1);

            if (box)
            {
                if (parameterInfos.Length <= 0)
                    return;
                if (This)
                {
                    emitArg(opCodes.First());
                }
                opCodes = opCodes.Skip(1);
                int i = 0;
                foreach (var opCode in opCodes)
                {
                    if (parameterInfos[i].ParameterType.IsValueType)
                        il.Emit(OpCodes.Box, parameterInfos[i].ParameterType);
                    emitArg(opCode);
                    ++i;
                }
            }
            else
            {
                if (!This)
                    opCodes = opCodes.Skip(1);
                foreach (var opCode in opCodes)
                    emitArg(opCode);
            }
        }

        void createOpcodeForParameters(int parameterCount, bool box = true, bool This = true)
        {
            var opCodes = getOpcodesForParameters(parameterCount + 1);
            if (!This)
                opCodes = opCodes.Skip(1);
            foreach (var opCode in opCodes)
                emitArg(opCode);
        }

        public void createGetMethodOpcode(MethodInfo methodInfo)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldind_I4, 1);
            il.Emit(OpCodes.Ret);
        }

        public void createSetMethodOpcode(MethodInfo methodInfo)
        {
            il.Emit(OpCodes.Ret);
        }

        public void CreateOpcode(FieldInfo invokesFieldInfo, MethodInfo methodInfo, bool isInterface, MethodInfo realMethodInfo = null, FieldInfo fieldInfo = null)
        {
            var parameterCount = methodInfo.GetParameters().Length;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, invokesFieldInfo);
            il.Emit(OpCodes.Ldstr, Invokes.GetMethodName(methodInfo));
            if (parameterCount > 0)
            {
                createArrayWithParameters(methodInfo.GetParameters());
                il.Emit(OpCodes.Call, typeof(Invokes).GetMethod("InvokeVoidWithParameters"));
            }
            else
            {
                il.Emit(OpCodes.Call, typeof(Invokes).GetMethod("InvokeVoid"));
            }
            il.Emit(OpCodes.Pop);
            if (realMethodInfo != null && fieldInfo != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fieldInfo);
                createOpcodeForParameters(methodInfo.GetParameters(), false, false);
                il.Emit(OpCodes.Callvirt, realMethodInfo);
            }
            il.Emit(OpCodes.Ret);
        }
    }
}
