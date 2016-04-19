#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using NSubstitute;

using NUnit.Framework;

using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.FluentMapping;
using Pomona.Routing;

namespace Pomona.UnitTests.Routing
{
    [TestFixture]
    public class RouteTests
    {
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

            RouteMatchTree routeMatchTree = new RouteMatchTree(root, "peons/fillifjonka/animals/1234/tail/end",
                                                               Substitute.For<IPomonaSession>());
            routeMatchTree.Leafs.ForEach(Console.WriteLine);
            var match1 = routeMatchTree.Root;
            Assert.That(match1.SelectedFinalMatch, Is.Null);
            var fork = match1.NextFork();
            fork.SelectedChild = fork.Children.First();
            Console.WriteLine(match1.SelectedFinalMatch);

            Console.WriteLine(match1);
        }


        public class Animal
        {
            public int Id { get; set; }
            public Peon Owner { get; set; }
        }

        public class Bacon
        {
            private decimal FatPercentage { get; set; }
        }

        public class Config : PomonaConfigurationBase
        {
            public override IEnumerable<object> FluentRuleObjects
            {
                get { yield return new Rules(); }
            }

            public override IEnumerable<Type> SourceTypes => new[]
            {
                typeof(Root), typeof(Peon), typeof(Animal), typeof(Pig), typeof(Cow), typeof(Bacon), typeof(PigTail),
                typeof(CowTail),
                typeof(CowTailEnd), typeof(PigTailEnd)
            };

            public override ITypeMappingFilter TypeMappingFilter => new DefaultTypeMappingFilter(SourceTypes);
        }

        public class Cow : Animal
        {
            public CowTail Tail { get; set; }
        }

        public class CowTail
        {
            public CowTailEnd End { get; set; }
        }

        public class CowTailEnd
        {
        }

        public class King
        {
            public string Name { get; set; }
        }

        public class Peon
        {
            public ICollection<Animal> Animals { get; set; }
            public string Name { get; set; }
            public Root Root { get; set; } // TODO: Should not be necesarry
        }

        public class Pig : Animal
        {
            public Bacon Bacon { get; set; }
            public PigTail Tail { get; set; }
        }

        public class PigTail
        {
            public PigTailEnd End { get; set; }
        }

        public class PigTailEnd
        {
        }

        public class Root
        {
            public IEnumerable<Peon> Peons { get; set; }
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
    }
}