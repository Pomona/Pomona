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

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Pomona.Example;
using Pomona.Example.Models;

namespace Pomona.UnitTests.Queries
{
    [TestFixture]
    public class PomonaQueryFilterParserTests
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            typeMapper = new TypeMapper(new CritterTypeMappingFilter());
            filterParser = new PomonaQueryFilterParser(typeMapper);
        }

        #endregion

        private TypeMapper typeMapper;
        private PomonaQueryFilterParser filterParser;

        private List<PomonaQuery.Condition> DoParse<T>(string filterString)
        {
            bool parsingError;
            return
                filterParser.Parse((TransformedType) typeMapper.GetClassMapping<T>(), filterString, out parsingError).
                    ToList();
        }

        [Test]
        public void Parse_WithMultiLevelPropertyExpression_ReturnsCorrectConditions()
        {
            var conditions = DoParse<Critter>("hat.hattype$eq$whatever");
            Assert.That(conditions, Has.Count.EqualTo(1));
            var condition = conditions.First();
            Assert.That(condition.Operator, Is.EqualTo(PomonaQuery.Operator.Eq));
            Assert.That(condition.PropertyName, Is.EqualTo("Hat.HatType"));
            Assert.That(condition.Value, Is.EqualTo("whatever"));
        }

        [Test]
        public void Parse_WithMultipleExpressions_ReturnsCorrectConditions()
        {
            var conditions = DoParse<Critter>("name$eq$curry,okdayIsFun$like$bananas");
            Assert.That(conditions, Has.Count.EqualTo(2));

            var nameCondition = conditions[0];
            Assert.That(nameCondition.PropertyName, Is.EqualTo("Name"));
            Assert.That(nameCondition.Value, Is.EqualTo("curry"));
            Assert.That(nameCondition.Operator, Is.EqualTo(PomonaQuery.Operator.Eq));

            var okdayCondition = conditions[1];
            Assert.That(okdayCondition.PropertyName, Is.EqualTo("OkdayIsFun"));
            Assert.That(okdayCondition.Value, Is.EqualTo("bananas"));
            Assert.That(okdayCondition.Operator, Is.EqualTo(PomonaQuery.Operator.Like));
        }

        [Test]
        public void Parse_WithSingleExpression_ReturnsCorrectConditions()
        {
            var conditions = DoParse<Critter>("name$eq$curry");
            Assert.That(conditions, Has.Count.EqualTo(1));
            var condition = conditions.First();
            Assert.That(condition.Operator, Is.EqualTo(PomonaQuery.Operator.Eq));
            Assert.That(condition.PropertyName, Is.EqualTo("Name"));
            Assert.That(condition.Value, Is.EqualTo("curry"));
        }
    }
}