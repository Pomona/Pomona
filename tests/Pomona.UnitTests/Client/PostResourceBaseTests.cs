#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2016 Karsten Nikolai Strand
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

using Pomona.Common.Proxies;
using Pomona.UnitTests.TestResources;

namespace Pomona.UnitTests.Client
{
    [TestFixture]
    public class PostResourceBaseTests
    {
        [Test]
        public void Get_property_of_type_IList_returns_new_list_proxy()
        {
            var form = new TestResourcePostForm();
            var list = form.Children;
            Assert.That(list, Is.TypeOf<PostResourceList<ITestResource>>());
        }


        [Test]
        public void Get_property_of_type_ISet_returns_new_set_proxy()
        {
            var form = new TestResourcePostForm();
            var set = form.Set;
            Assert.That(set, Is.TypeOf<PostResourceSet<ITestResource>>());
        }


        [Test]
        public void Get_property_of_type_ISet_second_time_returns_same_set_as_first_time()
        {
            // Make sure caching of wrappers work
            var form = new TestResourcePostForm();
            var setFirstTime = form.Set;
            var setSecondTime = form.Set;
            Assert.That(setFirstTime, Is.EqualTo(setSecondTime));
        }
    }
}