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
using NUnit.Framework;
using Pomona.Example;
using Pomona.Example.Models;
using System.Linq;

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

        [Test]
        public void Parse_WithSingleExpression_ReturnsCorrectConditions()
        {
            var conditions = DoParse<Critter>("hat$eq$curry");
            Assert.That(conditions, Has.Count.EqualTo(1));
            var condition = conditions.First();
            Assert.That(condition.Operator, Is.EqualTo(PomonaQuery.Operator.Eq));
            Assert.That(condition.PropertyName, Is.EqualTo("Hat"));
            Assert.That(condition.Value, Is.EqualTo("curry"));
        }

        [Test]
        public void Parse_WithMultiLevelExpression_ReturnsCorrectConditions()
        {
            var conditions = DoParse<Critter>("hat.hattype$eq$whatever");
            Assert.That(conditions, Has.Count.EqualTo(1));
            var condition = conditions.First();
            Assert.That(condition.Operator, Is.EqualTo(PomonaQuery.Operator.Eq));
            Assert.That(condition.PropertyName, Is.EqualTo("Hat.HatType"));
            Assert.That(condition.Value, Is.EqualTo("whatever"));
        }

        private List<PomonaQuery.Condition> DoParse<T>(string filterString)
        {
            bool parsingError;
            return filterParser.Parse((TransformedType) typeMapper.GetClassMapping<T>(), filterString, out parsingError).ToList();
        }
    }
}