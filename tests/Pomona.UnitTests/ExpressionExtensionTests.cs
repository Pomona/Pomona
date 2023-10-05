#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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

