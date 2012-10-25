#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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

using NUnit.Framework;

using Pomona.Client;

namespace Pomona.UnitTests.Client
{
    [TestFixture]
    public class QueryBuilderTests
    {
        public class TestResource : IClientResource
        {
            public string Jalla { get; set; }
        }

        public class Container
        {
            public string Junk { get; set; }
        }


        [Test]
        public void BuildConstantExpression_UsingNestedClosureAccess_ReturnsConstant()
        {
            var container = new Container() { Junk = "Kirk" };
            var builder = new QueryPredicateBuilder<TestResource>(x => x.Jalla == container.Junk);
            var queryString = builder.ToString();
            Assert.That(queryString, Is.EqualTo("(jalla eq 'Kirk')"));
        }


        [Test]
        public void BuildEqualExpression_ReturnsCorrectExpression()
        {
            var builder = new QueryPredicateBuilder<TestResource>(x => x.Jalla == "What");
            var queryString = builder.ToString();
            Assert.That(queryString, Is.EqualTo("(jalla eq 'What')"));
        }
    }
}