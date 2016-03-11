#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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
        [Test]
        public void GeneratePoco_WorksOk()
        {
            var pocoType = EmitHelpers.CreatePocoType("Jalla",
                                                      "Dummy",
                                                      new KeyValuePair<string, Type>[]
                                                      { new KeyValuePair<string, Type>("Lala", typeof(string)), });
            var pocoInstance = Activator.CreateInstance(pocoType);
            Assert.That(pocoInstance, Is.Not.Null);
        }


        [Test]
        public void GenerateProxy_ForInterface_CopiesVersionFromProxiedTypeToAssembly()
        {
            var proxy = RuntimeProxyFactory<RedirectProxy, ITakesNothingReturnsInt>.Create();
            var version = proxy.GetType().Assembly.GetName().Version;
            Assert.That(version, Is.EqualTo(typeof(ITakesNothingReturnsInt).Assembly.GetName().Version));
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


        public object TestProxyMethod<TProxyTarget>(Func<TProxyTarget, object> invokeMethodFunc,
                                                    out Tuple<MethodInfo, object[]> logEntry,
                                                    object returnValue = null,
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


        public interface IHasMethodWithSameSignatureAsProxy
        {
            long TheCrazyMethod(bool hei);
        }

        public interface ITakesArgumentsReturnsObject
        {
            object TheMethod(int a, long b, string c, double? d);
        }

        public interface ITakesNothingReturnsInt
        {
            int TheMethod();
        }

        public interface ITakesNothingReturnsString
        {
            string TheMethod();
        }

        public interface ITakesNothingReturnsValue
        {
            void TheMethod();
        }

        public class RedirectProxy
        {
            public bool CrazyMethodWasCalled { get; set; }

            public List<Tuple<MethodInfo, object[]>> InvokeLog { get; } = new List<Tuple<MethodInfo, object[]>>();

            public object ReturnValue { get; set; }


            public object OnInvokeMethod(MethodInfo methodInfo, object[] args)
            {
                InvokeLog.Add(new Tuple<MethodInfo, object[]>(methodInfo, args));
                return ReturnValue;
            }


            public virtual long TheCrazyMethod(bool hei)
            {
                CrazyMethodWasCalled = true;
                return 12345678;
            }
        }
    }
}