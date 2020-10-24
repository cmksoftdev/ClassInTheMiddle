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

        void createOpcodeForParameters(int parameterCount, bool This = true)
        {
            var opCodes = getOpcodesForParameters(parameterCount + 1);
            if (!This)
                opCodes = opCodes.Skip(1);
            foreach (var opCode in opCodes)
                il.Emit(opCode);
        }

        public void CreateOpcode(MethodInfo methodInfo, MethodInfo realMethodInfo = null, FieldInfo fieldInfo = null)
        {
            var name = methodInfo.Name;
            var parameterCount = methodInfo.GetParameters().Length;

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, name);
            il.Emit(OpCodes.Call, typeof(Invokes).GetMethod("Invoke"));
            il.Emit(OpCodes.Pop);

            if (realMethodInfo != null && fieldInfo != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fieldInfo);
                createOpcodeForParameters(parameterCount, false);
                il.Emit(OpCodes.Call, realMethodInfo);
            }

            il.Emit(OpCodes.Ret);
        }
    }
}
