#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;

using Pomona.Common.Proxies;

namespace Pomona.UnitTests.TestResources
{
    public class TestResourcePostForm : PostResourceBase, ITestResource
    {
        private static readonly PropertyWrapper<ITestResource, IList<ITestResource>> childrenPropWrapper =
            new PropertyWrapper<ITestResource, IList<ITestResource>>("Children");

        private static readonly PropertyWrapper<ITestResource, IDictionary<string, string>> dictionaryPropWrapper =
            new PropertyWrapper<ITestResource, IDictionary<string, string>>("Dictionary");

        private static readonly PropertyWrapper<ITestResource, IDictionary<string, object>> attributesPropWrapper =
            new PropertyWrapper<ITestResource, IDictionary<string, object>>("Attributes");

        private static readonly PropertyWrapper<ITestResource, ITestResource> friendPropWrapper =
            new PropertyWrapper<ITestResource, ITestResource>("Friend");

        private static readonly PropertyWrapper<ITestResource, int> idPropWrapper =
            new PropertyWrapper<ITestResource, int>("Id");

        private static readonly PropertyWrapper<ITestResource, string> infoPropWrapper =
            new PropertyWrapper<ITestResource, string>("Info");

        private static readonly PropertyWrapper<ITestResource, ITestResource> spousePropWrapper =
            new PropertyWrapper<ITestResource, ITestResource>("Spouse");

        private static readonly PropertyWrapper<ITestResource, ISet<ITestResource>> setPropWrapper =
            new PropertyWrapper<ITestResource, ISet<ITestResource>>("Set");

        public IDictionary<string, object> Attributes
        {
            get { return OnGet(attributesPropWrapper); }
            set { OnSet(attributesPropWrapper, value); }
        }

        public IList<ITestResource> Children
        {
            get { return OnGet(childrenPropWrapper); }
            set { OnSet(childrenPropWrapper, value); }
        }

        public IDictionary<string, string> Dictionary
        {
            get { return OnGet(dictionaryPropWrapper); }
            set { OnSet(dictionaryPropWrapper, value); }
        }

        public ITestResource Friend
        {
            get { return OnGet(friendPropWrapper); }
            set { OnSet(friendPropWrapper, value); }
        }

        public int Id
        {
            get { return OnGet(idPropWrapper); }
            set { OnSet(idPropWrapper, value); }
        }

        public string Info
        {
            get { return OnGet(infoPropWrapper); }
            set { OnSet(infoPropWrapper, value); }
        }

        public ISet<ITestResource> Set
        {
            get { return OnGet(setPropWrapper); }
            set { OnSet(setPropWrapper, value); }
        }

        public ITestResource Spouse
        {
            get { return OnGet(spousePropWrapper); }
            set { OnSet(spousePropWrapper, value); }
        }
    }
}

