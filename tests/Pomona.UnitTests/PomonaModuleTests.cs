#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Nancy;
using Nancy.Testing;

using NUnit.Framework;

using Pomona.FluentMapping;
using Pomona.Nancy;

namespace Pomona.UnitTests
{
    [TestFixture]
    public class PomonaModuleTests
    {
        [Test]
        public void Get_resource_using_handler_from_module_without_custom_data_source_is_successful()
        {
            var browser = new Browser(with => with.Module<NoCustomDataSourceModule>(), bc => bc.Header("Accept", "application/json"));
            Assert.That(browser.Get("/dummies").StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }


        public class Dummy
        {
            public int Id { get; set; }
        }

        public class DummyHandler
        {
            public IQueryable<Dummy> QueryDummies()
            {
                return new Dummy[] { }.AsQueryable();
            }
        }

        public class NoCustomDataSourceConfiguration : PomonaConfigurationBase
        {
            public override IEnumerable<object> FluentRuleObjects => new object[] { new NoCustomDataSourceFluentRules() };

            public override IEnumerable<Type> SourceTypes => new[] { typeof(Dummy) };
        }

        public class NoCustomDataSourceFluentRules
        {
            public void Map(ITypeMappingConfigurator<Dummy> map)
            {
                map.HandledBy<DummyHandler>();
            }
        }

        [PomonaConfiguration(typeof(NoCustomDataSourceConfiguration))]
        private class NoCustomDataSourceModule : PomonaModule
        {
        }
    }
}