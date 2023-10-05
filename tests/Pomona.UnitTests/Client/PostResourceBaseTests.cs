#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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

