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
            public IDictionary<string, string> Attributes { get; set; }
            public DateTime Birthday { get; set; }
            public string Bonga { get; set; }
            public dynamic Boo { get; set; }
            public decimal CashAmount { get; set; }
            public Guid Guid { get; set; }
            public int Id { get; set; }
            public string Jalla { get; set; }
            public float LessPrecise { get; set; }
            public IList<decimal> ListOfDecimals { get; set; }
            public IList<double> ListOfDoubles { get; set; }
            public IList<int> ListOfInts { get; set; }
            public bool OnOrOff { get; set; }
            public double Precise { get; set; }
            public TestEnum SomeEnum { get; set; }
            public IList<FooBar> SomeList { get; set; }
            public TestEnum? SomeNullableEnum { get; set; }
            public IQueryableFooBarRepo SomeQueryable { get; set; }
            public IDictionary<string, object> StringObjectAttributes { get; set; }
            public TimeSpan TimeSpan { get; set; }
            public object UnknownProperty { get; set; }
        }

        #endregion
    }
}