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
                il.Emit(list[i + 1]);
                il.Emit(OpCodes.Box, item.ParameterType);
                il.Emit(OpCodes.Stelem_Ref);
                i++;
            }
            il.Emit(OpCodes.Stloc, paramValues);
            il.Emit(OpCodes.Ldloc, paramValues);
        }

        void createOpcodeForParameters(ParameterInfo[] parameterInfos, bool box = true, bool This = true)
        {
            var opCodes = getOpcodesForParameters(parameterInfos.Length + 1);

            if (box)
            {
                if (parameterInfos.Length <= 0)
                    return;
                if (This)
                    il.Emit(opCodes.First());
                opCodes = opCodes.Skip(1);
                int i = 0;
                foreach (var opCode in opCodes)
                {
                    if (parameterInfos[i].ParameterType.IsValueType)
                        il.Emit(OpCodes.Box, parameterInfos[i].ParameterType);
                    il.Emit(opCode);
                    ++i;
                }
            }
            else
            {
                if (!This)
                    opCodes = opCodes.Skip(1);
                foreach (var opCode in opCodes)
                    il.Emit(opCode);
            }
        }

        void createOpcodeForParameters(int parameterCount, bool box = true, bool This = true)
        {
            var opCodes = getOpcodesForParameters(parameterCount + 1);
            if (!This)
                opCodes = opCodes.Skip(1);
            foreach (var opCode in opCodes)
                il.Emit(opCode);
        }

        public void createGetMethodOpcode(MethodInfo methodInfo)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldind_I4, 1);
            //il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ret);
        }

        public void createSetMethodOpcode(MethodInfo methodInfo)
        {
            //il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ret);
        }

        public void CreateOpcode(MethodInfo methodInfo, bool isInterface, MethodInfo realMethodInfo = null, FieldInfo fieldInfo = null)
        {
            var name = methodInfo.Name;
            var parameterCount = methodInfo.GetParameters().Length;
            LocalBuilder result = null;
            bool isReturning = methodInfo.ReturnType != typeof(void);
            if (isReturning)
                result = il.DeclareLocal(methodInfo.ReturnType);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, name);
            if (parameterCount > 0)
            {
                LocalBuilder paramValues = il.DeclareLocal(typeof(object[]));
                createArrayWithParameters(methodInfo.GetParameters());
                il.Emit(OpCodes.Call, typeof(Invokes).GetMethod("InvokeWithParameters"));
            }
            else
            {
                il.Emit(OpCodes.Call, typeof(Invokes).GetMethod("Invoke"));
            }
            il.Emit(OpCodes.Pop);

            if (realMethodInfo != null && fieldInfo != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                //if (isInterface)
                {
                    il.Emit(OpCodes.Ldfld, fieldInfo);
                    //il.Emit(OpCodes.Box, methodInfo.);
                }
                createOpcodeForParameters(methodInfo.GetParameters(), false, false);
                il.Emit(OpCodes.Callvirt, realMethodInfo);
                //il.Emit(OpCodes.Pop);
                //if (isReturning)
                //{
                //    if(methodInfo.ReturnType.IsValueType)
                //        il.Emit(OpCodes.Box, methodInfo.ReturnType);
                //    il.Emit(OpCodes.Stloc, result);
                //}
            }

            if (isReturning)
            {
                //il.Emit(OpCodes.Ldloca_S, 0);
                //il.Emit(OpCodes.Ldloc, result);
                //il.Emit(OpCodes.Unbox, methodInfo.ReturnType);
            }
            il.Emit(OpCodes.Ret);
        }
    }
}
