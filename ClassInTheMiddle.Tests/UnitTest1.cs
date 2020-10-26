using ClassInTheMiddle.Library;
using ClassInTheMiddle.Library.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ClassInTheMiddle.Tests
{
    public class TestDummy1
    {
        public int i1 = 0, i2 = 0, i3 = 0, i4 = 0, i5 = 0;

        public void SetOneParameter(int i1)
        {
            this.i1 = i1;
        }
        public void SetTwoParameters(int i1, int i2)
        {
            this.i1 = i1;
            this.i2 = i2;
        }
        public void SetThreeParameters(int i1, int i2, int i3)
        {
            this.i1 = i1;
            this.i2 = i2;
            this.i3 = i3;
        }
        public void SetFourParameters(int i1, int i2, int i3, int i4)
        {
            this.i1 = i1;
            this.i2 = i2;
            this.i3 = i3;
            this.i4 = i4;
        }
        public void SetFiveParameters(int i1, int i2, int i3, int i4, int i5)
        {
            this.i1 = i1;
            this.i2 = i2;
            this.i3 = i3;
            this.i4 = i4;
            this.i5 = i5;
        }

        public int Get1() => i1;
        public int Get2() => i2;
        public int Get3() => i3;
        public int Get4() => i4;
        public int Get5() => i5;
    }

    public class TestDummy2
    {
        TestDummy1 testDummy;

        public TestDummy2(TestDummy1 testDummy)
        {
            this.testDummy = testDummy;
        }

        public void SetOneParameter(int i1) => testDummy.SetOneParameter(i1);
        public void SetTwoParameters(int i1, int i2) => testDummy.SetTwoParameters(i1, i2);
        public void SetThreeParameters(int i1, int i2, int i3) => testDummy.SetThreeParameters(i1, i2, i3);
        public void SetFourParameters(int i1, int i2, int i3, int i4) => testDummy.SetFourParameters(i1, i2, i3, i4);
        public void SetFiveParameters(int i1, int i2, int i3, int i4, int i5) => testDummy.SetFiveParameters(i1, i2, i3, i4, i5);

        public int Get1() => testDummy.Get1();
        public int Get2() => testDummy.Get2();
        public int Get3() => testDummy.Get3();
        public int Get4() => testDummy.Get4();
        public int Get5() => testDummy.Get5();
    }

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void NoInterface_OneParameter()
        {
            // Arrange
            int expected = 123;
            bool setProxyMethodWasCalled = false;
            bool getProxyMethodWasCalled = false;

            var classInTheMiddle = new Library.ClassInTheMiddle();

            classInTheMiddle.AddFactory<TestDummy1>(() => new TestDummy1());
            classInTheMiddle.SetFunction<TestDummy1>(x => x.SetOneParameter(0), x =>
                {
                    Assert.AreEqual(expected, (int)x[0]);
                    setProxyMethodWasCalled = true;
                    return null;
                })
                .SetFunction<TestDummy1>(x => x.Get1(), x =>
                {
                    getProxyMethodWasCalled = true;
                    return null;
                });

            var sut = classInTheMiddle.GetSut<TestDummy2>();

            // Act
            sut.SetOneParameter(expected);
            var actual = sut.Get1();

            // Assert
            Assert.IsTrue(setProxyMethodWasCalled && getProxyMethodWasCalled);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void NoInterface_TwoParameters()
        {
            // Arrange
            int expected1 = 123;
            int expected2 = 321;
            bool setProxyMethodWasCalled = false;
            bool get1ProxyMethodWasCalled = false;
            bool get2ProxyMethodWasCalled = false;

            var classInTheMiddle = new Library.ClassInTheMiddle();

            classInTheMiddle.AddFactory<TestDummy1>(() => new TestDummy1());
            classInTheMiddle.SetFunction<TestDummy1>(x => x.SetTwoParameters(0, 1), x =>
                {
                    Assert.AreEqual(expected1, (int)x[0]);
                    Assert.AreEqual(expected2, (int)x[1]);
                    setProxyMethodWasCalled = true;
                    return null;
                })
                .SetFunction<TestDummy1>(x => x.Get1(), x =>
                {
                    get1ProxyMethodWasCalled = true;
                    return null;
                })
                .SetFunction<TestDummy1>(x => x.Get2(), x =>
                {
                    get2ProxyMethodWasCalled = true;
                    return null;
                });

            var sut = classInTheMiddle.GetSut<TestDummy2>();

            // Act
            sut.SetTwoParameters(expected1, expected2);
            var actual1 = sut.Get1();
            var actual2 = sut.Get2();

            // Assert
            Assert.IsTrue(
                setProxyMethodWasCalled && 
                get1ProxyMethodWasCalled && 
                get2ProxyMethodWasCalled);
            Assert.AreEqual(expected1, actual1);
            Assert.AreEqual(expected2, actual2);
        }
    }
}
