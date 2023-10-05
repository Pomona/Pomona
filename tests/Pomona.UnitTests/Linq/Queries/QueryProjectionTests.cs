#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Pomona.Common.Linq.NonGeneric;

namespace Pomona.UnitTests.Linq.Queries
{
    [TestFixture]
    public class QueryProjectionTests
    {
        [Test]
        public void Execute_AsEnumerable_Projection_On_Collection_Returns_Unmodified_Source()
        {
            var source = Q(1, 2, 4, 8, 16);
            var result = QueryProjection.AsEnumerable.Execute<IEnumerable<int>>(source);
            Assert.That(result, Is.EqualTo(source));
        }


        [Test]
        public void Execute_First_Projection_On_Empty_Collection_Throws_InvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => QueryProjection.First.Execute<string>(Empty<string>()));
        }


        [Test]
        public void Execute_First_Projection_On_Non_Empty_Collection_Returns_First_Element()
        {
            Assert.That(QueryProjection.FirstOrDefault.Execute<string>(Q("gangnam", "style")), Is.EqualTo("gangnam"));
        }


        [Test]
        public void Execute_FirstOrDefault_Projection_On_Empty_Collection_Returns_Default()
        {
            Assert.That(QueryProjection.FirstOrDefault.Execute<string>(Empty<string>()), Is.Null);
        }


        [Test]
        public void Execute_Last_Projection_On_Empty_Collection_Throws_InvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => QueryProjection.Last.Execute<string>(Empty<string>()));
        }


        [Test]
        public void Execute_Last_Projection_On_Non_Empty_Collection_Returns_Last_Element()
        {
            Assert.That(QueryProjection.LastOrDefault.Execute<string>(Q("gangnam", "style")), Is.EqualTo("style"));
        }


        [Test]
        public void Execute_LastOrDefault_Projection_On_Empty_Collection_Returns_Default()
        {
            Assert.That(QueryProjection.LastOrDefault.Execute<string>(Empty<string>()), Is.Null);
        }


        [Test]
        public void Execute_Sum_Projection_On_Empty_Collection_Returns_Zero()
        {
            Assert.That(QueryProjection.Sum.Execute<int>(Empty<int>()), Is.EqualTo(0));
        }


        [Test]
        public void Execute_Sum_Projection_On_Empty_Integer_Collection_Returns_Zero()
        {
            Assert.That(QueryProjection.Sum.Execute<int>(Empty<int>()), Is.EqualTo(0));
        }


        [Test]
        public void Execute_Sum_Projection_On_Non_Empty_Integer_Collection_Returns_Sum_Of_Elements()
        {
            Assert.That(QueryProjection.Sum.Execute<int>(Q(1, 2, 4, 8, 16)), Is.EqualTo(31));
        }


        [Test]
        public void Execute_Sum_Projection_On_Unsupported_Collection_Throws_Exception()
        {
            Assert.Throws<NotSupportedException>(() => QueryProjection.Sum.Execute(Q("can't", "touch", "this")));
        }


        private IQueryable<T> Empty<T>()
        {
            return Enumerable.Empty<T>().AsQueryable();
        }


        private IQueryable<T> Q<T>(params T[] args)
        {
            return args.AsQueryable();
        }
    }
}

