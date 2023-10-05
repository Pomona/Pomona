#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Pomona.Common.Internals;

namespace Pomona.UnitTests
{
    [TestFixture]
    public class EnumerableExtensionsTests
    {
        [Test]
        public void Flatten_DoesNotThrowStackOverflowException()
        {
            var node = new Node();
            node.Children.Add(new Node());
            node.Children.Add(new Node());
            Assert.That(node.WrapAsEnumerable().Flatten(x => x.Children).Count(), Is.EqualTo(3));
        }


        public class Node
        {
            public List<Node> Children { get; } = new List<Node>();
        }
    }
}

