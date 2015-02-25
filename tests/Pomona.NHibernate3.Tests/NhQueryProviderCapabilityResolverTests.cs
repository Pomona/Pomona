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
        #region Setup/Teardown

        [TestFixtureSetUp]
        public void FixtureSetUp()
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

        private NhQueryProviderCapabilityResolver resolver;
        private ISessionFactory sessionFactory;


        [Test]
        public void PropertyIsMapped_With_Property_Mapped_By_Nhibernate_Returns_False()
        {
            Assert.That(this.resolver.PropertyIsMapped(typeof(Order).GetProperty("Lines")), Is.True);
        }


        [Test]
        public void IdPropertyIsMapped_With_Property_Mapped_By_Nhibernate_Returns_False()
        {
            Assert.That(this.resolver.PropertyIsMapped(typeof(Order).GetProperty("Id")), Is.True);
        }


        [Test]
        public void PropertyIsMapped_With_Property_Not_Mapped_By_Nhibernate_Returns_False()
        {
            Assert.That(this.resolver.PropertyIsMapped(typeof(Order).GetProperty("LinesWithOddIds")), Is.False);
        }
    }
}