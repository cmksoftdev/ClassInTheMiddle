using ClassInTheMiddle.Library;
using System;
using System.Collections.Generic;

namespace ClassInTheMiddle
{
    public class test1 : Itest1
    {
        int i;

        public void Set(int i)
        {
            this.i = i;
        }

        public int Get()
        {
            return i;
        }
    }

    public class test2
    {
        Itest1 test;

        public test2(Itest1 test)
        {
            this.test = test;
        }
        public void Set(int i)
        {
            test.Set(i);
        }

        public int Get()
        {
            return test.Get();
        }
    }

    public class Program
    {
        static void testtt(object o)
        {
            var i = Convert.ToInt32(o);
        }

        public static void Main(string[] args)
        {
            testtt(15);
            var classAnalyser = new ClassAnalyser<test2>(new Dictionary<Type, Func<object>>
            {
                {typeof(Itest1), () => new test1() }
            });
            var sut = classAnalyser.SUT;
            Invokes.Actions.Add("Set", () =>
            {
                Console.WriteLine("set");
            });
            Invokes.Actions.Add("Get", () =>
            {
                Console.WriteLine("get");
            });
            Invokes.SetRealObject(new test1());
            sut.Set(15);
            var id = sut.Get();

            Console.WriteLine("Hello World!");
        }
    }
}
