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

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using NUnit.Framework;

using Pomona.Common.Internals;
using Pomona.Common.Linq;

namespace Pomona.UnitTests
{
    [TestFixture]
    public class ExpressionExtensionTests
    {
        public string GetPath<TRet>(Expression<Func<Dummy, TRet>> propertySelector, bool jsonNameStyle = false)
        {
            return propertySelector.GetPropertyPath(jsonNameStyle);
        }


        [Test]
        public void GetPropertyPath_WithDefaultNameStyle_ReturnsCorrectString()
        {
            Assert.That(GetPath(x => x.Foo.Bar.HelloThere), Is.EqualTo("Foo.Bar.HelloThere"));
        }


        [Test]
        public void GetPropertyPath_WithJsonNameStyle_ReturnsCorrectString()
        {
            Assert.That(GetPath(x => x.Foo.Bar.HelloThere, true), Is.EqualTo("foo.bar.helloThere"));
        }


        [Category("TODO")]
        [Test(Description = "Not yet working, need to rework GetPropertyPath a little.")]
        public void GetPropertyPathThroughEnumerable_WithChainedInnerExpand_ReturnsCorrectString()
        {
            Assert.That(GetPath(x => x.Foo.Children.Expand(y => y.Bar).Expand(y => y.Dummy)),
                        Is.EqualTo("Foo.Children.Bar,Foo.Children.Dummy"));
        }


        [Test]
        public void GetPropertyPathThroughEnumerable_WithDefaultNameStyle_ReturnsCorrectString()
        {
            Assert.That(GetPath(x => x.Foo.Children.Expand(y => y.Bar)), Is.EqualTo("Foo.Children.Bar"));
        }


        [Test]
        public void GetPropertyPathThroughEnumerable_WithJsonNameStyle_ReturnsCorrectString()
        {
            Assert.That(GetPath(x => x.Foo.Children.Expand(y => y.Bar), true), Is.EqualTo("foo.children.bar"));
        }


        public class Bar
        {
            public string HelloThere { get; set; }
        }

        public class Child
        {
            public Bar Bar { get; set; }
            public Dummy Dummy { get; set; }
        }

        public class Dummy
        {
            public Foo Foo { get; set; }
        }

        public class Foo
        {
            public Bar Bar { get; set; }
            public IEnumerable<Child> Children { get; set; }
        }
    }
}