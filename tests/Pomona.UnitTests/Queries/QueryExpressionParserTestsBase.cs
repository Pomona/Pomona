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
using System.Linq.Expressions;
using System.Reflection;

using NUnit.Framework;

using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.Queries;
using Pomona.TestHelpers;

namespace Pomona.UnitTests.Queries
{
    public abstract class QueryExpressionParserTestsBase : ExpressionTestsBase
    {
        #region TestEnum enum

        public enum TestEnum
        {
            Fi,
            Fa,
            Foo,
            Moo
        }

        #endregion

        protected QueryExpressionParser parser;


        [SetUp]
        public void SetUp()
        {
            var typeMapper = new TypeMapper(new PomonaTestConfiguration());
            this.parser = new QueryExpressionParser(new QueryTypeResolver(typeMapper));
        }


        protected void ParseAndAssert<TRet>(string expr, Expression<Func<Dummy, TRet>> expected)
        {
            var actual = this.parser.Parse(typeof(Dummy), expr);
            Console.WriteLine("{0} => {1}", expr, actual);
            var evaluateClosureMemberVisitor = new EvaluateClosureMemberVisitor();
            expected = (Expression<Func<Dummy, TRet>>)evaluateClosureMemberVisitor.Visit(expected);
            actual.AssertEquals(expected);
        }

        #region Nested type: Dummy

        public class Dummy
        {
            public TestEnum AnEnumValue { get; set; }
            public IDictionary<string, string> Attributes { get; set; }
            public IList<Dummy> Children { get; set; }
            public Dummy Friend { get; set; }
            public Guid Guid { get; set; }

            public string IsNotAllowedInQueries
            {
                get { return "bah"; }
            }

            public IList<decimal> ListOfDecimals { get; set; }
            public IList<double> ListOfDoubles { get; set; }
            public IList<float> ListOfFloats { get; set; }
            public IList<int> ListOfInts { get; set; }
            public TestEnum? NullableEnum { get; set; }
            public int Number { get; set; }
            public IDictionary<string, object> ObjectAttributes { get; set; }
            public bool OnOrOff { get; set; }
            public Dummy Parent { get; set; }
            public double Precise { get; set; }
            public decimal SomeDecimal { get; set; }
            public IList<string> SomeStrings { get; set; }
            public string Text { get; set; }
            public DateTime Time { get; set; }
            public object UnknownProperty { get; set; }
            public bool and { get; set; }
            public bool? NullableBool { get; set; }
            public int? NullableNumber { get; set; }
            public long? NullableInt64 { get; set; }
        }

        #endregion

        #region Nested type: PomonaTestConfiguration

        private class PomonaTestConfiguration : PomonaConfigurationBase
        {
            public override IEnumerable<Type> SourceTypes
            {
                get
                {
                    yield return typeof(TestEnum);
                    yield return typeof(Dummy);
                }
            }

            public override ITypeMappingFilter TypeMappingFilter
            {
                get { return new PomonaTestTypeMappingFilter(SourceTypes); }
            }
        }

        #endregion

        #region Nested type: PomonaTestTypeMappingFilter

        private class PomonaTestTypeMappingFilter : TypeMappingFilterBase
        {
            public PomonaTestTypeMappingFilter(IEnumerable<Type> sourceTypes)
                : base(sourceTypes)
            {
            }


            public override PropertyFlags? GetPropertyFlags(PropertyInfo propertyInfo)
            {
                if (propertyInfo.Name == "IsNotAllowedInQueries")
                    return base.GetPropertyFlags(propertyInfo) & ~PropertyFlags.AllowsFiltering;
                return base.GetPropertyFlags(propertyInfo);
            }
        }

        #endregion
    }
}