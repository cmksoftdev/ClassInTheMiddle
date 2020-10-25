using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ClassInTheMiddle.Library.Services
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
}
