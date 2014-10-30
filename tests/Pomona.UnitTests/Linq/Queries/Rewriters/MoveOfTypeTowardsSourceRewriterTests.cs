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

using System.Linq;

using NUnit.Framework;

using Pomona.Common.Linq.Queries.Rewriters;

namespace Pomona.UnitTests.Linq.Queries.Rewriters
{
    [TestFixture]
    public class MoveOfTypeTowardsSourceRewriterTests : RewriterTestBase<MoveOfTypeTowardsSourceRewriter>
    {
        [Test]
        public void Visit_Redundant_OfType_Casting_To_Same_Type_Removes_OfType_And_Returns_Source()
        {
            AssertRewrite(() => this.Animals.OfType<Animal>(),
                          () => this.Animals);
        }


        [Test]
        public void Visit_Where_OfType_When_Casting_To_Inherited_Type_Returns_OfType_Where()
        {
            AssertRewrite(() => this.Animals.Where(x => x.Id == 33).OfType<Dog>(),
                          () => this.Animals.OfType<Dog>().Where(x => x.Id == 33));
        }


        [Test]
        public void Visit_Where_OfType_When_Casting_To_Non_Inherited_Type_Returns_Umodified_Expression()
        {
            AssertDoesNotRewrite(() => this.Animals.Where(x => x.Id == 33).OfType<object>());
        }
    }
}