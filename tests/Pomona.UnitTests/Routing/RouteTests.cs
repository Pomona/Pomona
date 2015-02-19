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

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

using NSubstitute;

using NUnit.Framework;

using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.Example;
using Pomona.Example.Models;
using Pomona.Example.Models.Existence;
using Pomona.FluentMapping;
using Pomona.Routing;

namespace Pomona.UnitTests.Routing
{
    [TestFixture]
    public class RouteTests
    {

        public class Animal
        {
            public Peon Owner { get; set; }
            public int Id { get; set; }
        }

        public class Bacon
        {
            decimal FatPercentage { get; set; }
        }

        public class PigTailEnd
        {
        }

        public class CowTailEnd
        {
        }

        public class PigTail
        {
            public PigTailEnd End { get; set; }
        }

        public class CowTail
        {
            public CowTailEnd End { get; set; }
        }

        public class Pig : Animal
        {
            public Bacon Bacon { get; set; }
            public PigTail Tail { get; set; }

        }

        public class Cow : Animal
        {
            public CowTail Tail { get; set; }
        }

        public class Peon
        {
            public Root Root { get; set; } // TODO: Should not be necesarry
            public string Name { get;set; }

            public ICollection<Animal> Animals { get; set; }
        }

        public class King
        {
            public string Name {get; set; }
        }

        public class Root
        {
            public IEnumerable<Peon> Peons { get; set; }
            //public King King { get; set; }
        }

        public class Rules
        {
            public void Map(ITypeMappingConfigurator<Root> map)
            {
                map.AsUriBaseType();
                map.HasChildren(x => x.Peons, x => x.Root, o => o.Include(x => x.Name, y => y.AsPrimaryKey()).Exclude(x => x.Root));
            }


            public void Map(ITypeMappingConfigurator<Animal> map)
            {
                map.AsUriBaseType();
                map.AsChildResourceOf(x => x.Owner, x => x.Animals);
                map.Exclude(x => x.Owner);
            }
        }

        public class Config : PomonaConfigurationBase
        {
            public override IEnumerable<Type> SourceTypes
            {
                get { return new[] { typeof(Root), typeof(Peon), typeof(Animal), typeof(Pig), typeof(Cow), typeof(Bacon), typeof(PigTail), typeof(CowTail), typeof(CowTailEnd), typeof(PigTailEnd) }; }
            }

            public override ITypeMappingFilter TypeMappingFilter
            {
                get { return new DefaultTypeMappingFilter(SourceTypes); }
            }

            public override IEnumerable<object> FluentRuleObjects
            {
                get { yield return new Rules(); }
            }
        }


        [Test]
        public void 
            MatchChildren_FromRootRoute_IsSuccessful()
        {

            // Route might be dependant on parent node, for example: PigTail GetTail(Pig pig)
            // ..or it might not be: PigTail GetTail(int peonId, int animalId) <-- We need to make sure animal is of type Pig before invoking Route
            // WHEN ROUTE IS AMBIGUOUS BY TYPE, WE ALWAYS NEED TO FETCH THE LAST SINGLE-RESOURCE NODE, OR AT LEAST DETERMINE TYPE

            var tm = new TypeMapper(new Config());
            var root = new RootRoute((ResourceType)tm.FromType<Root>());

            var allNodes = root.WrapAsEnumerable<Route>().Flatten(x => x.Children).ToList();

            RouteMatchTree routeMatchTree = new RouteMatchTree(root, "peons/fillifjonka/animals/1234/tail/end", Substitute.For<IPomonaSession>());
            routeMatchTree.Leafs.ForEach(Console.WriteLine);
            var match1 = routeMatchTree.Root;
            Assert.That(match1.SelectedFinalMatch, Is.Null);
            var fork = match1.NextFork();
            fork.SelectedChild = fork.Children.First();
            Console.WriteLine(match1.SelectedFinalMatch);

            Console.WriteLine(match1);

        }

    }
}