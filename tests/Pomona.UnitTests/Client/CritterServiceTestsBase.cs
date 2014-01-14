#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
        private string baseUri;

        private TClient client;

        private CritterHost critterHost;

        public virtual bool UseSelfHostedHttpServer
        {
            get { return UseSelfHostedHttpServerDefault; }
        }

        public CritterRepository Repository { get; private set; }

        protected string BaseUri
        {
            get { return this.baseUri; }
        }

        protected TClient Client
        {
            get { return this.client; }
        }

        protected ICollection<Critter> CritterEntities
        {
            get { return Repository.List<Critter>(); }
        }

        public abstract TClient CreateHttpTestingClient(string baseUri);
        public abstract TClient CreateInMemoryTestingClient(string baseUri, CritterBootstrapper critterBootstrapper);
        public abstract void SetupRequestCompletedHandler();


        public void AssertIsOrderedBy<T, TOrderKey>(
            IEnumerable<T> enumerable,
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


        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            if (UseSelfHostedHttpServer)
            {
                var rng = new Random();
                this.baseUri = "http://localhost:" + rng.Next(10000, 23000) + "/";
                Console.WriteLine("Starting CritterHost on " + this.baseUri);
                this.critterHost = new CritterHost(new Uri(this.baseUri));
                this.critterHost.Start();
                this.client = CreateHttpTestingClient(this.baseUri);
                Repository = this.critterHost.Repository;
            }
            else
            {
                this.baseUri = "http://test/";

                if (cachedNancyTestingClient == null)
                {
                    var critterBootstrapper = new CritterBootstrapper();
                    cachedNancyTestingClientRepository = critterBootstrapper.Repository;
                    cachedNancyTestingClient = CreateInMemoryTestingClient(this.baseUri, critterBootstrapper);
                }
                this.client = cachedNancyTestingClient;
                Repository = cachedNancyTestingClientRepository;
            }

            SetupRequestCompletedHandler();
        }


        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            if (UseSelfHostedHttpServer)
                this.critterHost.Stop();
        }


        [SetUp]
        public void SetUp()
        {
            Repository.ResetTestData();
        }


        protected T Save<T>(T entity)
        {
            return Repository.Save(entity);
        }
    }
}