#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using NUnit.Framework;

using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.Queries;

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
        protected TypeMapper typeMapper;


        [SetUp]
        public void SetUp()
        {
            this.typeMapper = new TypeMapper(new PomonaTestConfiguration());
            this.parser = new QueryExpressionParser(new QueryTypeResolver(this.typeMapper));
        }


        protected void ParseAndAssert<TRet>(string expr, Expression<Func<Dummy, TRet>> expected)
        {
            var actual = this.parser.Parse(typeof(Dummy), expr);
            Console.WriteLine("{0} => {1}", expr, actual);
            var evaluateClosureMemberVisitor = new EvaluateClosureMemberVisitor();
            expected = (Expression<Func<Dummy, TRet>>)evaluateClosureMemberVisitor.Visit(expected);
            AssertExpressionEquals(actual, expected);
        }

        #region Nested type: Dummy

        public class Dummy
        {
            public bool and { get; set; }
            public TestEnum AnEnumValue { get; set; }
            public IDictionary<string, string> Attributes { get; set; }
            public IList<Dummy> Children { get; set; }
            public Dummy Friend { get; set; }
            public Guid Guid { get; set; }

            public string IsNotAllowedInQueries => "bah";

            public IList<int> ListOfDateTimes { get; set; }

            public IList<decimal> ListOfDecimals { get; set; }
            public IList<double> ListOfDoubles { get; set; }
            public IList<float> ListOfFloats { get; set; }
            public IList<int> ListOfInts { get; set; }
            public bool? NullableBool { get; set; }
            public TestEnum? NullableEnum { get; set; }
            public long? NullableInt64 { get; set; }
            public int? NullableNumber { get; set; }
            public int Number { get; set; }
            public IDictionary<string, object> ObjectAttributes { get; set; }
            public bool OnOrOff { get; set; }
            public Dummy Parent { get; set; }
            public double Precise { get; set; }
            public decimal SomeDecimal { get; set; }
            public IList<string> SomeStrings { get; set; }
            public string Text { get; set; }
            public DateTime Time { get; set; }
            public DateTimeOffset TimeOffset { get; set; }
            public object UnknownProperty { get; set; }
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

            public override ITypeMappingFilter TypeMappingFilter => new PomonaTestTypeMappingFilter(SourceTypes);
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
