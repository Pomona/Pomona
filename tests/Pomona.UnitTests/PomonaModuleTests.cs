#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2016 Karsten Nikolai Strand
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

using Nancy;
using Nancy.Testing;

using NUnit.Framework;

using Pomona.FluentMapping;

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