#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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

        public ITestResource Spouse
        {
            get { return OnGet(spousePropWrapper); }
            set { OnSet(spousePropWrapper, value); }
        }
    }
}