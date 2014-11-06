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

using Pomona.Common.Linq.NonGeneric;

namespace Pomona.UnitTests.Linq.Queries
{
    [TestFixture]
    public class QueryProjectionTests
    {
        private IQueryable<T> Empty<T>()
        {
            return Enumerable.Empty<T>().AsQueryable();
        }


        private IQueryable<T> Q<T>(params T[] args)
        {
            return args.AsQueryable();
        }


        [Test]
        public void Execute_AsEnumerable_Projection_On_Collection_Returns_Unmodified_Source()
        {
            var source = Q(1, 2, 4, 8, 16);
            var result = QueryProjection.AsEnumerable.Execute<IEnumerable<int>>(source);
            Assert.That(result, Is.EqualTo(source));
        }


        [Test]
        public void Execute_FirstOrDefault_Projection_On_Empty_Collection_Returns_Default()
        {
            Assert.That(QueryProjection.FirstOrDefault.Execute<string>(Empty<string>()), Is.Null);
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
    }
}