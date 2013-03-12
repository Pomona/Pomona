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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Pomona.Queries;
using Pomona.TestHelpers;

namespace Pomona.UnitTests.Queries
{
    public class QueryExpressionParserTestsBase
    {
        public enum TestEnum
        {
            Fi,
            Fa,
            Foo,
            Moo
        }

        protected QueryExpressionParser parser;

        [SetUp]
        public void SetUp()
        {
            parser = new QueryExpressionParser(new SimpleQueryPropertyResolver());
        }

        protected void ParseAndAssert<TRet>(string expr, Expression<Func<Dummy, TRet>> expected)
        {
            var actual = parser.Parse(typeof (Dummy), expr);
            Console.WriteLine("{0} => {1}", expr, actual);
            actual.AssertEquals(expected);
        }

        public class Dummy
        {
            public object UnknownProperty { get; set; }
            public bool OnOrOff { get; set; }
            public bool and { get; set; }
            public IDictionary<string, object> ObjectAttributes { get; set; }
            public IDictionary<string, string> Attributes { get; set; }
            public IEnumerable<Dummy> Children { get; set; }
            public Dummy Friend { get; set; }
            public Guid Guid { get; set; }
            public double Precise { get; set; }
            public int Number { get; set; }
            public Dummy Parent { get; set; }
            public IEnumerable<string> SomeStrings { get; set; }
            public string Text { get; set; }
            public DateTime Time { get; set; }
            public TestEnum AnEnumValue { get; set; }
        }

        public class SimpleQueryPropertyResolver : IQueryTypeResolver
        {
            #region IQueryPropertyResolver Members

            public Expression ResolveProperty(Expression rootInstance, string propertyPath)
            {
                return Expression.Property(
                    rootInstance,
                    rootInstance.Type.GetProperties().First(x => x.Name.ToLower() == propertyPath.ToLower()));
            }


            public Type ResolveType(string typeName)
            {
                if (typeName == "Int32")
                    return typeof (int);
                if (typeName == "String")
                    return typeof (string);
                throw new NotImplementedException();
            }

            #endregion
        }
    }
}