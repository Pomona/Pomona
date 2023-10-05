#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db.CsharpSqlite;

using NHibernate;
using NHibernate.Tool.hbm2ddl;

using NUnit.Framework;

using Pomona.NHibernate3.Tests.Mapping;
using Pomona.NHibernate3.Tests.Models;

namespace Pomona.NHibernate3.Tests
{
    [TestFixture]
    public class NhQueryProviderCapabilityResolverTests
    {
        private NhQueryProviderCapabilityResolver resolver;
        private ISessionFactory sessionFactory;


        [Test]
        public void IdPropertyIsMapped_With_Property_Mapped_By_Nhibernate_Returns_False()
        {
            Assert.That(this.resolver.PropertyIsMapped(typeof(Order).GetProperty("Id")), Is.True);
        }


        [Test]
        public void PropertyIsMapped_With_Property_Mapped_By_Nhibernate_Returns_False()
        {
            Assert.That(this.resolver.PropertyIsMapped(typeof(Order).GetProperty("Lines")), Is.True);
        }


        [Test]
        public void PropertyIsMapped_With_Property_Not_Mapped_By_Nhibernate_Returns_False()
        {
            Assert.That(this.resolver.PropertyIsMapped(typeof(Order).GetProperty("LinesWithOddIds")), Is.False);
        }

        #region Setup/Teardown

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            this.sessionFactory = Fluently.Configure()
                                          .Database(CsharpSqliteConfiguration.Standard.InMemory)
                                          .Mappings(m => m.AutoMappings.Add(AutoMap.AssemblyOf<Order>(new AutomappingConfiguration())))
                                          .ExposeConfiguration(cfg => new SchemaExport(cfg).Create(true, true))
                                          .BuildSessionFactory();
        }


        [SetUp]
        public void SetUp()
        {
            this.resolver = new NhQueryProviderCapabilityResolver(this.sessionFactory);
        }

        #endregion
    }
}

