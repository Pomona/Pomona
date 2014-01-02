#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Pomona.Common.Proxies;

namespace Pomona.UnitTests.Proxies
{
    [TestFixture]
    public class IlEmitBasedProxyBuilderTests
    {
        public interface ITakesNothingReturnsValue
        {
            void TheMethod();
        }

        public interface ITakesArgumentsReturnsObject
        {
            object TheMethod(int a, long b, string c, double? d);
        }

        public interface ITakesNothingReturnsString
        {
            string TheMethod();
        }

        public interface ITakesNothingReturnsInt
        {
            int TheMethod();
        }


        public interface IHasMethodWithSameSignatureAsProxy
        {
            long TheCrazyMethod(bool hei);
        }

        public class RedirectProxy
        {
            private readonly List<Tuple<MethodInfo, object[]>> invokeLog = new List<Tuple<MethodInfo, object[]>>();

            public List<Tuple<MethodInfo, object[]>> InvokeLog
            {
                get { return invokeLog; }
            }

            public object ReturnValue { get; set; }

            public bool CrazyMethodWasCalled { get; set; }

            public object OnInvokeMethod(MethodInfo methodInfo, object[] args)
            {
                invokeLog.Add(new Tuple<MethodInfo, object[]>(methodInfo, args));
                return ReturnValue;
            }

            public virtual long TheCrazyMethod(bool hei)
            {
                CrazyMethodWasCalled = true;
                return 12345678;
            }
        }

        public object TestProxyMethod<TProxyTarget>(Func<TProxyTarget, object> invokeMethodFunc,
                                                    out Tuple<MethodInfo, object[]> logEntry, object returnValue = null,
                                                    int expectedInvokeLogCount = 1)
        {
            var proxy = RuntimeProxyFactory<RedirectProxy, TProxyTarget>.Create();
            var redirectProxy = ((RedirectProxy)((object)proxy));
            redirectProxy.ReturnValue = returnValue;
            var invokeLog = redirectProxy.InvokeLog;
            var retval = invokeMethodFunc(proxy);
            Assert.That(invokeLog.Count, Is.EqualTo(expectedInvokeLogCount));
            logEntry = invokeLog.FirstOrDefault();
            return retval;
        }


        [Test]
        public void GeneratePoco_WorksOk()
        {
            var pocoType = EmitHelpers.CreatePocoType("Jalla",
                "Dummy",
                new KeyValuePair<string, Type>[] { new KeyValuePair<string, Type>("Lala", typeof(string)), });
            var pocoInstance = Activator.CreateInstance(pocoType);
            Assert.That(pocoInstance, Is.Not.Null);
        }

        [Test]
        public void GenerateProxy_ForInterfaceImplementingMethodWithMatchingMethodInProxyBase_DoesNotGenerateMethod()
        {
            var proxy = RuntimeProxyFactory<RedirectProxy, IHasMethodWithSameSignatureAsProxy>.Create();
            var redirectProxy = ((RedirectProxy)(proxy));
            Assert.That(proxy.TheCrazyMethod(true), Is.EqualTo(12345678));
            Assert.That(redirectProxy.CrazyMethodWasCalled);
        }


        [Test]
        public void GenerateProxy_ForInterfaceWithMethodReturningInt_GeneratesWorkingProxy()
        {
            Tuple<MethodInfo, object[]> logEntry;
            var retval = TestProxyMethod<ITakesNothingReturnsInt>(x => x.TheMethod(), out logEntry, 1337);
            Assert.That(retval, Is.EqualTo(1337));
        }


        [Test]
        public void GenerateProxy_ForInterfaceWithMethodReturningObject_GeneratesWorkingProxy()
        {
            Tuple<MethodInfo, object[]> logEntry;
            var retval = TestProxyMethod<ITakesNothingReturnsString>(x => x.TheMethod(), out logEntry, "whatevs");
            Assert.That(retval, Is.EqualTo("whatevs"));
        }

        [Test]
        public void GenerateProxy_ForInterfaceWithMethodTakingMultipleArgumentsAndReturningObject_GeneratesWorkingProxy()
        {
            Tuple<MethodInfo, object[]> logEntry;
            var retval = TestProxyMethod<ITakesArgumentsReturnsObject>(x => x.TheMethod(0xa, 0xb, "c", 10.5),
                                                                       out logEntry, "bahaha");
            Assert.That(retval, Is.EqualTo("bahaha"));
            Assert.That(logEntry.Item1.Name, Is.EqualTo("TheMethod"));
            var args = logEntry.Item2;
            Assert.That(args.Length, Is.EqualTo(4));
            Assert.That(args[0], Is.EqualTo(0xa));
            Assert.That(args[1], Is.EqualTo((long)0xb));
            Assert.That(args[2], Is.EqualTo("c"));
            Assert.That(args[3], Is.EqualTo(10.5));
        }

        [Test]
        public void GenerateProxy_ForInterfaceWithVoidMethod_GeneratesWorkingProxy()
        {
            Tuple<MethodInfo, object[]> logEntry;
            TestProxyMethod<ITakesNothingReturnsValue>(x =>
                {
                    x.TheMethod();
                    return null;
                }, out logEntry);
        }
    }
}