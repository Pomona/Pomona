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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Critters.Client;
using NUnit.Framework;
using Nancy.Testing;
using Pomona.Common;
using Pomona.Example;
using Pomona.Example.Models;
using Pomona.TestHelpers;

namespace Pomona.SystemTests
{
    public class ClientTestsBase
    {
        public const bool UseSelfHostedHttpServerDefault = false;
        private static Client cachedNancyTestingClient;
        private static Pomona.Example.CritterRepository cachedNancyTestingClientRepository;
        private string baseUri;

        protected Client client;
        private CritterHost critterHost;

        public virtual bool UseSelfHostedHttpServer
        {
            get { return UseSelfHostedHttpServerDefault; }
        }

        protected string BaseUri
        {
            get { return baseUri; }
        }

        public Pomona.Example.CritterRepository Repository { get; private set; }

        protected ICollection<Critter> CritterEntities
        {
            get { return this.Repository.List<Critter>(); }
        }

        protected T Save<T>(T entity)
        {
            return this.Repository.Save(entity);
        }


        public void AssertIsOrderedBy<T, TOrderKey>(
            IEnumerable<T> enumerable, Func<T, TOrderKey> orderby, SortOrder sortOrder)
            where T : IEntityBase
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
                baseUri = "http://localhost:" + rng.Next(10000, 23000) + "/";
                Console.WriteLine("Starting CritterHost on " + baseUri);
                critterHost = new CritterHost(new Uri(baseUri));
                critterHost.Start();
                client = new Client(baseUri);
                this.Repository = critterHost.Repository;
            }
            else
            {
                baseUri = "http://test/";

                if (cachedNancyTestingClient == null)
                {
                    var critterBootstrapper = new CritterBootstrapper();
                    cachedNancyTestingClientRepository = critterBootstrapper.Repository;
                    var nancyTestingWebClient = new NancyTestingWebClient(new Browser(critterBootstrapper));
                    cachedNancyTestingClient = new Client(baseUri, nancyTestingWebClient);
                }
                client = cachedNancyTestingClient;
                this.Repository = cachedNancyTestingClientRepository;
            }

            client.RequestCompleted += ClientOnRequestCompleted;
        }

        private void ClientOnRequestCompleted(object sender, ClientRequestLogEventArgs e)
        {
            Console.WriteLine("Sent:\r\n{0}\r\nReceived:\r\n{1}\r\n", e.Request,
                              (object) e.Response ?? "(nothing received)");
        }


        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            if (UseSelfHostedHttpServer)
                critterHost.Stop();
        }


        [SetUp]
        public void SetUp()
        {
            this.Repository.ResetTestData();
        }


        public IList<TResource> TestQuery<TResource, TEntity>(
            Expression<Func<TResource, bool>> resourcePredicate,
            Func<TEntity, bool> entityPredicate,
            string message = null,
            int? expectedResultCount = null)
            where TResource : IEntityBase
            where TEntity : EntityBase
        {
            var callingStackFrame = new StackFrame(1);
            var callingMethod = callingStackFrame.GetMethod();
            Assert.That(callingMethod.Name, Is.StringStarting("Query" + typeof (TEntity).Name));

            var allEntities = this.Repository.List<TEntity>();
            var entities =
                allEntities.Where(entityPredicate).OrderBy(x => x.Id).ToList();
            var fetchedResources = client.Query<TResource>().Where(resourcePredicate).Take(1024*1024).ToList();
            Assert.That(fetchedResources.Select(x => x.Id), Is.EquivalentTo(entities.Select(x => x.Id)), message);

            if (expectedResultCount.HasValue)
            {
                Assert.That(fetchedResources.Count, Is.EqualTo(expectedResultCount.Value),
                            "Expected result count wrong.");
            }

            return fetchedResources;
        }


        protected bool IsAllowedType(Type t)
        {
            return FlattenGenericTypeHierarchy(t).All(x => IsAllowedClientReferencedAssembly(x.Assembly));
        }


        protected IHat PostAHat(string hatType)
        {
            var hat = client.Post<IHat>(
                x => { x.HatType = hatType; });
            return (IHat) hat;
        }


        private IEnumerable<Type> FlattenGenericTypeHierarchy(Type t)
        {
            if (t.IsGenericType)
            {
                yield return t.GetGenericTypeDefinition();
                foreach (var genarg in t.GetGenericArguments())
                {
                    foreach (var gent in FlattenGenericTypeHierarchy(genarg))
                        yield return gent;
                }
            }
            else
                yield return t;
        }


        private bool IsAllowedClientReferencedAssembly(Assembly assembly)
        {
            return assembly == typeof (object).Assembly ||
                   assembly == typeof (ICritter).Assembly ||
                   assembly == typeof (ClientBase).Assembly ||
                   assembly == typeof (Uri).Assembly;
        }

        #region Nested type: IHasCustomAttributes

        public interface IHasCustomAttributes : IDictionaryContainer
        {
            string WrappedAttribute { get; set; }
        }

        #endregion
    }
}