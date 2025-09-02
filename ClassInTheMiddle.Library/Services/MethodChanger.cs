using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace ClassInTheMiddle.Library.Services
{
    public class MethodChanger
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        const uint PAGE_EXECUTE_READWRITE = 0x40;


        public static void Change(ConstructorInfo methodToReplace, ConstructorInfo methodToInject)
        {

            //Thread.Sleep(1000);

            unsafe
            {


                if (IntPtr.Size == 4)
                {
                    int* init_inj = (int*)methodToInject.MethodHandle.Value.ToPointer() + 2;
                    int* init_tar = (int*)methodToReplace.MethodHandle.Value.ToPointer() + 2;

                    RuntimeHelpers.PrepareMethod(methodToReplace.MethodHandle);
                    RuntimeHelpers.PrepareMethod(methodToInject.MethodHandle);

                    int* inj = (int*)methodToInject.MethodHandle.Value.ToPointer() + 2;
                    int* tar = (int*)methodToReplace.MethodHandle.Value.ToPointer() + 2;

                    do
                    {
                        inj = (int*)methodToInject.MethodHandle.Value.ToPointer() + 2;
                        tar = (int*)methodToReplace.MethodHandle.Value.ToPointer() + 2;
                    }
                    while (init_inj == inj || init_tar == tar);

                    *tar = *inj;
                }
                else
                {
                    //if ((long*)methodToInject.MethodHandle.Value.ToPointer() == (long*)0 || (long*)methodToReplace.MethodHandle.Value.ToPointer() == (long*)0)
                    //{
                    //}
                    //long* inj = (long*)methodToInject.MethodHandle.Value.ToPointer() + 1;
                    //long* tar = (long*)methodToReplace.MethodHandle.Value.ToPointer() + 1;



                    long init_inj = (long)methodToInject.MethodHandle.GetFunctionPointer().ToPointer();
                    long init_tar = (long)methodToReplace.MethodHandle.GetFunctionPointer().ToPointer();

                    try
                    {
                        var test = Activator.CreateInstance(methodToReplace.DeclaringType, new object[] { (object)null, true, IntPtr.Zero, true });
                    }
                    catch (Exception e)
                    {
                    }

                    RuntimeHelpers.PrepareMethod(methodToReplace.MethodHandle);
                    RuntimeHelpers.PrepareMethod(methodToInject.MethodHandle);

                    long _inj = (long)methodToInject.MethodHandle.Value.ToPointer() + 1;
                    long _tar = (long)methodToReplace.MethodHandle.Value.ToPointer() + 1;

                    int i = 0;
                    do
                    {
                        Thread.Sleep(10);
                        init_inj = (long)methodToInject.MethodHandle.GetFunctionPointer().ToPointer();
                        init_tar = (long)methodToReplace.MethodHandle.GetFunctionPointer().ToPointer();
                        _inj = (long)methodToInject.MethodHandle.Value.ToPointer() + 1;
                        _tar = (long)methodToReplace.MethodHandle.Value.ToPointer() + 1;
                    }
                    while (i++ < 20 /*&& (init_inj == _inj || init_tar == _tar)*/);

                    long* inj = (long*)methodToInject.MethodHandle.Value.ToPointer() + 1;
                    long* tar = (long*)methodToReplace.MethodHandle.Value.ToPointer() + 1;

                    *tar = *inj;

                    try
                    {
                        var test = Activator.CreateInstance(methodToReplace.DeclaringType, new object[] { (object)null, true, IntPtr.Zero, true });
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
        }

        public static void Change(MethodInfo methodToReplace, MethodInfo methodToInject)
        {
            RuntimeHelpers.PrepareMethod(methodToReplace.MethodHandle);
            RuntimeHelpers.PrepareMethod(methodToInject.MethodHandle);

            unsafe
            {
                // 32-bit vs 64-bit Pointergröße
                int ptrSize = IntPtr.Size;
                byte* tarPtr;
                byte* injPtr;

                if (ptrSize == 8) // x64
                {
                    // Offsets innerhalb MethodDesc x64 (nur m_pCodeOrIL)
                    const int offset = 8 + 2 + 4;// + 2 + 4 + 8; // m_pMethodTable+ m_wFlags+padding+ m_slot+ m_pILHeader

                    tarPtr = (byte*)methodToReplace.MethodHandle.Value.ToPointer() + offset;
                    injPtr = (byte*)methodToInject.MethodHandle.Value.ToPointer() + offset;

                    *(ulong*)tarPtr = *(ulong*)injPtr; // nur 8 Byte schreiben
                }
                else // x86
                {
                    // Offsets innerhalb MethodDesc x86 (nur m_pCodeOrIL)
                    const int offset = 4 + 2 + 2 + 4 + 4; // m_pMethodTable + m_wFlags + padding + m_slot + m_pILHeader

                    tarPtr = (byte*)methodToReplace.MethodHandle.Value.ToPointer() + offset;
                    injPtr = (byte*)methodToInject.MethodHandle.Value.ToPointer() + offset;

                    *(uint*)tarPtr = *(uint*)injPtr; // nur 4 Byte schreiben
                }
            }
        }

        //public static void Change(MethodInfo methodToReplace, MethodInfo methodToInject)
        //{
        //    unsafe
        //    {
        //        RuntimeHelpers.PrepareMethod(methodToReplace.MethodHandle);
        //        RuntimeHelpers.PrepareMethod(methodToInject.MethodHandle);

        //        IntPtr injPtr = methodToInject.MethodHandle.GetFunctionPointer();
        //        IntPtr tarPtr = methodToReplace.MethodHandle.GetFunctionPointer();

        //        uint oldProtect;
        //        VirtualProtect(tarPtr, (UIntPtr)IntPtr.Size, PAGE_EXECUTE_READWRITE, out oldProtect);

        //        if (IntPtr.Size == 4)
        //        {
        //            *(int*)tarPtr = *(int*)injPtr;
        //        }
        //        else
        //        {
        //            *(long*)tarPtr = *(long*)injPtr;
        //        }

        //        Thread.Sleep(300);
        //        VirtualProtect(tarPtr, (UIntPtr)IntPtr.Size, oldProtect, out oldProtect);
        //    }
        //}
    }
}
