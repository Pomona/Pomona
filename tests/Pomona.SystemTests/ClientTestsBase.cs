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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Critters.Client;

using Nancy.Testing;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Web;
using Pomona.Example;
using Pomona.Example.Models;
using Pomona.TestHelpers;
using Pomona.UnitTests.Client;

namespace Pomona.SystemTests
{
    public class ClientTestsBase : CritterServiceTestsBase<CritterClient>
    {
        private readonly List<ClientRequestLogEventArgs> requestLog = new List<ClientRequestLogEventArgs>();

        protected List<ClientRequestLogEventArgs> RequestLog
        {
            get { return requestLog; }
        }

        protected IWebClient WebClient
        {
            get { return Client.WebClient; }
        }


        private void ClientOnRequestCompleted(object sender, ClientRequestLogEventArgs e)
        {
            requestLog.Add(e);
            if (RequestTraceEnabled)
            {
                Console.WriteLine("Sent:\r\n{0}\r\nReceived:\r\n{1}\r\n",
                                  e.Request,
                                  (object)e.Response ?? "(nothing received)");
            }
        }


        public override CritterClient CreateHttpTestingClient(string baseUri)
        {
            return new CritterClient(baseUri,
                                     new HttpWebRequestClient(new HttpHeaders() { { "MongoHeader", "lalaal" } }));
        }


        public override void SetUp()
        {
            base.SetUp();
            requestLog.Clear();
        }


        public override CritterClient CreateInMemoryTestingClient(string baseUri,
                                                                  CritterBootstrapper critterBootstrapper)
        {
            var nancyTestingWebClient = new NancyTestingWebClient(new Browser(critterBootstrapper));
            return new CritterClient(baseUri, nancyTestingWebClient);
        }


        public override void SetupRequestCompletedHandler()
        {
            Client.RequestCompleted += ClientOnRequestCompleted;
        }


        public override void TeardownRequestCompletedHandler()
        {
            Client.RequestCompleted -= ClientOnRequestCompleted;
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
            Assert.That(callingMethod.Name, Is.StringStarting("Query" + typeof(TEntity).Name));

            var allEntities = Repository.List<TEntity>();
            var entities =
                allEntities.Where(entityPredicate).OrderBy(x => x.Id).ToList();
            var fetchedResources = Client.Query<TResource>().Where(resourcePredicate).ToList();
            Assert.That(fetchedResources.Select(x => x.Id), Is.EquivalentTo(entities.Select(x => x.Id)), message);

            if (expectedResultCount.HasValue)
            {
                Assert.That(fetchedResources.Count,
                            Is.EqualTo(expectedResultCount.Value),
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
            var hat = Client.Post<IHat>(
                x =>
                {
                    x.HatType = hatType;
                });
            return (IHat)hat;
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
            return assembly == typeof(object).Assembly ||
                   assembly == typeof(ICritter).Assembly ||
                   assembly == typeof(ClientBase).Assembly ||
                   assembly == typeof(IQueryProvider).Assembly ||
                   assembly == typeof(Uri).Assembly;
        }

        #region Nested type: IHasCustomAttributes

        public interface IHasCustomAttributes : IDictionaryContainer
        {
            string WrappedAttribute { get; set; }
        }

        #endregion
    }
}