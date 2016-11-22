#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Pomona.CodeGen;
using Pomona.Common;
using Pomona.UnitTests.Queries;

namespace Pomona.UnitTests.Client
{
    public abstract class QueryPredicateBuilderTestsBase : ExpressionTestsBase
    {
        #region TestEnum enum

        public enum TestEnum
        {
            Tick,
            Tack,
            Tock
        }

        #endregion

        #region Nested type: FooBar

        public class FooBar : IClientResource
        {
            public decimal SomeDecimal { get; set; }
            public double SomeDouble { get; set; }
            public int SomeInt { get; set; }
            public string SomeString { get; set; }
            public IList<TestResource> TestResources { get; set; }
        }

        #endregion

        #region Nested type: IQueryableFooBarRepo

        public interface IQueryableFooBarRepo : IQueryable<FooBar>
        {
        }

        #endregion

        #region Nested type: TestResource

        public class TestResource : IClientResource
        {
            public DateTimeOffset BirthDayOffset { get; set; }
            public IDictionary<string, string> Attributes { get; set; }
            public DateTime Birthday { get; set; }
            public string Bonga { get; set; }
            public dynamic Boo { get; set; }
            public decimal CashAmount { get; set; }
            public Guid Guid { get; set; }
            public int Id { get; set; }
            public string Jalla { get; set; }
            public float LessPrecise { get; set; }
            public IList<DateTime> ListOfDateTimes { get; set; }
            public IList<decimal> ListOfDecimals { get; set; }
            public IList<double> ListOfDoubles { get; set; }
            public IList<int> ListOfInts { get; set; }
            public IList<DateTime?> ListOfNullableDateTimes { get; set; }
            public bool? NullableBool { get; set; }
            public int? NullableInt32 { get; set; }
            public long? NullableInt64 { get; set; }
            public bool OnOrOff { get; set; }
            public double Precise { get; set; }
            public TestEnum SomeEnum { get; set; }
            public IList<FooBar> SomeList { get; set; }
            public TestEnum? SomeNullableEnum { get; set; }
            public IQueryableFooBarRepo SomeQueryable { get; set; }
            public StringEnumTemplate SomeStringEnum { get; set; }
            public IDictionary<string, object> StringObjectAttributes { get; set; }
            public TimeSpan TimeSpan { get; set; }
            public object UnknownProperty { get; set; }
        }

        #endregion
    }
}