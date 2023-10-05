#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;

using Critters.Client;

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
        protected List<ClientRequestLogEventArgs> RequestLog { get; } = new List<ClientRequestLogEventArgs>();

        protected IWebClient WebClient => Client.WebClient;


        public override CritterClient CreateHttpTestingClient(string baseUri)
        {
            return new CritterClient(baseUri,
                                     new HttpWebClient(new HttpClient() { DefaultRequestHeaders = { { "MongoHeader", "lalaal" } } }));
        }


        public override CritterClient CreateInMemoryTestingClient(string baseUri,
                                                                  CritterBootstrapper critterBootstrapper)
        {
            var nancyTestingWebClient = new NancyTestingHttpMessageHandler(critterBootstrapper.GetEngine());
            return new CritterClient(baseUri, new HttpWebClient(nancyTestingWebClient));
        }


        public override void SetUp()
        {
            base.SetUp();
            RequestLog.Clear();
        }


        public override void SetupRequestCompletedHandler()
        {
            Client.RequestCompleted += ClientOnRequestCompleted;
        }


        public override void TeardownRequestCompletedHandler()
        {
            Client.RequestCompleted -= ClientOnRequestCompleted;
        }


        public IList<TResource> TestQuery<TResource, TEntity>(Expression<Func<TResource, bool>> resourcePredicate,
                                                              Func<TEntity, bool> entityPredicate,
                                                              string message = null,
                                                              int? expectedResultCount = null)
            where TResource : IEntityBase
            where TEntity : EntityBase
        {
            var callingStackFrame = new StackFrame(1);
            var callingMethod = callingStackFrame.GetMethod();
            Assert.That(callingMethod.Name, Does.StartWith("Query" + typeof(TEntity).Name));

            var allEntities = Repository.List<TEntity>();
            var entities = allEntities.Where(entityPredicate).OrderBy(x => x.Id).ToList();
            var fetchedResources = Client.Query<TResource>().Where(resourcePredicate).ToList();
            var fetchedResourceIds = fetchedResources.Select(x => x.Id);
            var entityIds = entities.Select(x => x.Id);

            Assert.That(fetchedResourceIds, Is.EquivalentTo(entityIds), message);

            if (expectedResultCount.HasValue)
            {
                Assert.That(fetchedResources.Count,
                            Is.EqualTo(expectedResultCount.Value),
                            "Expected result count wrong.");
            }

            return fetchedResources;
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


        private void ClientOnRequestCompleted(object sender, ClientRequestLogEventArgs e)
        {
            RequestLog.Add(e);
            if (RequestTraceEnabled)
            {
                Console.WriteLine("Sent:\r\n{0}\r\nReceived:\r\n{1}\r\n",
                                  e.Request.ToStringWithContent(),
                                  e.Response?.ToStringWithContent() ?? "(nothing received)");
            }
        }

        #region Nested type: IHasCustomAttributes

        public interface IHasCustomAttributes : IDictionaryContainer
        {
            string WrappedAttribute { get; set; }
        }

        #endregion
    }
}

