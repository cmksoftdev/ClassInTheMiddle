using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ClassInTheMiddle.Library.Services
{
    public class MethodChanger
    {
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
                }
            }
        }
    }
}
