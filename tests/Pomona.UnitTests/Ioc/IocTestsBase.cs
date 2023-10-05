#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

using NUnit.Framework;

using Pomona.Ioc;

namespace Pomona.UnitTests.Ioc
{
    public abstract class IocTestsBase<T>
        where T : class, IDisposable
    {
        private T container;

        protected virtual T Container => this.container;


        [SetUp]
        public virtual void SetUp()
        {
            this.container = CreateContainer();
            Register<IDummyContract, DummyImplementation>();
        }


        [TearDown]
        public virtual void TearDown()
        {
            if (this.container != null)
            {
                this.container.Dispose();
                this.container = null;
            }
        }


        protected virtual T CreateContainer()
        {
            return Activator.CreateInstance<T>();
        }


        protected RuntimeContainerWrapper CreateWrapper()
        {
            return RuntimeContainerWrapper.Create(Container);
        }


        protected abstract void Register<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;
    }
}

