#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Example;
using Pomona.Example.Models;

namespace Pomona.UnitTests.Client
{
    public abstract class CritterServiceTestsBase<TClient>
    {
        public const bool UseSelfHostedHttpServerDefault = false;
        private static TClient cachedNancyTestingClient;
        private static CritterRepository cachedNancyTestingClientRepository;
        private CritterHost critterHost;
        public CritterRepository Repository { get; private set; }
        public TypeMapper TypeMapper { get; private set; }

        public virtual bool UseSelfHostedHttpServer => UseSelfHostedHttpServerDefault;

        protected string BaseUri { get; private set; }
        protected TClient Client { get; private set; }

        protected ICollection<Critter> CritterEntities => Repository.List<Critter>();

        protected bool RequestTraceEnabled { get; set; }


        public void AssertIsOrderedBy<T, TOrderKey>(IEnumerable<T> enumerable,
                                                    Func<T, TOrderKey> orderby,
                                                    SortOrder sortOrder)
        {
            var list = enumerable.ToList();
            IEnumerable<T> expected;

            if (sortOrder == SortOrder.Ascending)
                expected = list.OrderBy(@orderby);
            else
                expected = list.OrderByDescending(@orderby);

            Assert.That(list.SequenceEqual(expected), "Items in list was not ordered as expected.");
        }


        public abstract TClient CreateHttpTestingClient(string baseUri);
        public abstract TClient CreateInMemoryTestingClient(string baseUri, CritterBootstrapper critterBootstrapper);


        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (UseSelfHostedHttpServer)
            {
                BaseUri = "http://localhost:15841" + "/" + Guid.NewGuid().ToString("N") + "/";
                Console.WriteLine("Starting CritterHost on " + BaseUri);
                this.critterHost = new CritterHost(new Uri(BaseUri));
                this.critterHost.Start();
                TypeMapper = this.critterHost.TypeMapper;
                Client = CreateHttpTestingClient(BaseUri);
                Repository = this.critterHost.Repository;
            }
            else
            {
                BaseUri = "http://test/";

                if (cachedNancyTestingClient == null)
                {
                    var critterBootstrapper = new CritterBootstrapper();
                    critterBootstrapper.Initialise();
                    cachedNancyTestingClientRepository = critterBootstrapper.Repository;
                    cachedNancyTestingClient = CreateInMemoryTestingClient(BaseUri, critterBootstrapper);
                }
                TypeMapper = cachedNancyTestingClientRepository.TypeMapper;
                Client = cachedNancyTestingClient;
                Repository = cachedNancyTestingClientRepository;
            }

            SetupRequestCompletedHandler();
        }


        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            TeardownRequestCompletedHandler();

            if (UseSelfHostedHttpServer)
                this.critterHost.Stop();
        }


        [SetUp]
        public virtual void SetUp()
        {
            // RequestTraceEnabled = true;
            Repository.ResetTestData();
        }


        public abstract void SetupRequestCompletedHandler();
        public abstract void TeardownRequestCompletedHandler();


        protected T Save<T>(T entity)
        {
            return Repository.Save(entity);
        }
    }
}