using ClassInTheMiddle.Library;
using ClassInTheMiddle.Library.Services;
using System;
using System.Collections.Generic;

namespace ClassInTheMiddle
{
    public class test1 //: Itest1
    {
        int i;

        public void Set(int i)
        {
            this.i = i;
        }

        public string Get()
        {
            return "Hallo";
        }

        public int Get2()
        {
            return i;
        }
    }

    public class test3 : test1
    {
        public new void Set(int i)
        {
        }

        public new int Get()
        {
            return 123;
        }
    }

    public class test2
    {
        test1 test;

        public test2(test1 test)
        {
            this.test = test;
        }
        public void Set(int i)
        {
            test.Set(i);
        }

        public string Get()
        {
            return test.Get();
        }
        public int Get2()
        {
            return test.Get2();
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var classAnalyser = new ClassAnalyser<test2>(new Dictionary<Type, Func<object>>
            {
                {typeof(test1), () => new test1() }
            });
            var sut = classAnalyser.SUT;
            classAnalyser.Invokes.SetFunction<test1>(x => x.Set(0), x =>
            {
                Console.WriteLine("set");
                return null;
            });
            classAnalyser.Invokes.SetFunction<test1>(x => x.Get(), x =>
            {
                Console.WriteLine("get");
                return null;
            });
            sut.Set(15);
            var id = sut.Get();

            Console.WriteLine("Hello World! " + id);
        }
    }
}
