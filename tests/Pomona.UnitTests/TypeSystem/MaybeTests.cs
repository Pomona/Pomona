#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

using NUnit.Framework;

using Pomona.Common.TypeSystem;

namespace Pomona.UnitTests.TypeSystem
{
    [TestFixture]
    public class MaybeTests
    {
        [Test]
        public void Maybe_FromEmptyNullable_HasNoValue()
        {
            Assert.That(default(DateTime?).Maybe().HasValue, Is.False);
        }


        [Test]
        public void Maybe_FromNull_HasNoValue()
        {
            Assert.That(((SuperClass)null).Maybe().HasValue, Is.False);
        }


        [Test]
        public void OrDefault_FromMaybeWithoutValue_ReturnsDefaultValue()
        {
            Assert.That(Maybe<string>.Empty.OrDefault(() => "ImDefault"), Is.EqualTo("ImDefault"));
            Assert.That(Maybe<string>.Empty.OrDefault(), Is.EqualTo(null));
        }


        [Test]
        public void Select_NonNullValue_HasValue()
        {
            Assert.That("blah".Maybe().Select(x => x).HasValue, Is.True);
        }


        [Test]
        public void Select_NullValue_HasNoValue()
        {
            Assert.That("blah".Maybe().Select(x => (string)null).HasValue, Is.False);
        }


        [Test]
        public void Switch_TakingEmptyMaybe_MatchingNoCase_ReturnsEmptyMaybe()
        {
            Assert.That(
                Maybe<Animal>
                    .Empty
                    .Switch<string>()
                    .Case<GoldenRetriever>().Then(x => "golden woff")
                    .Case<Dog>().Then(x => "woff")
                    .EndSwitch()
                    .HasValue,
                Is.False);
        }


        [Test]
        public void Switch_TakingNonEmptyMaybe_MatchingFirstCastCase_ExecutesFirstFunc()
        {
            var animal = new Cat();
            Assert.That(animal
                            .Maybe()
                            .Switch<string>()
                            .Case<Cat>().Then(x => "miauu")
                            .Case<Dog>().Then(x => "woff")
                            .EndSwitch()
                            .Value,
                        Is.EqualTo("miauu"));
        }


        [Test]
        public void Switch_TakingNonEmptyMaybe_MatchingNoCase_ReturnsEmptyMaybe()
        {
            var cat = new Cat();
            Assert.That(cat
                            .Maybe()
                            .Switch<string>()
                            .Case<GoldenRetriever>().Then(x => "golden woff")
                            .Case<Dog>().Then(x => "woff")
                            .EndSwitch()
                            .HasValue,
                        Is.False);
        }


        [Test]
        public void Switch_TakingNonEmptyMaybe_MatchingPredicate_ExecutesFunc()
        {
            var cat = new Cat() { Name = "oswald" };
            Assert.That(cat
                            .Maybe()
                            .Switch<string>()
                            .Case<GoldenRetriever>().Then(x => "golden woff")
                            .Case<Cat>(x => x.Name == "oswald").Then(x => "see mouse!")
                            .EndSwitch()
                            .Value,
                        Is.EqualTo("see mouse!"));
        }


        [Test]
        public void Switch_TakingNonEmptyMaybe_MatchingSecondCastCase_ExecutesSecondFunc()
        {
            var animal = new Dog();
            Assert.That(animal
                            .Maybe()
                            .Switch<string>()
                            .Case<Cat>().Then(x => "miauu")
                            .Case<Dog>().Then(x => "woff")
                            .EndSwitch()
                            .Value,
                        Is.EqualTo("woff"));
        }


        [Test]
        public void SwitchNonGeneric_TakingEmptyMaybe_MatchingNoCase_ReturnsEmptyMaybe()
        {
            Assert.That(
                Maybe<Animal>
                    .Empty
                    .Switch()
                    .Case<GoldenRetriever>().Then(x => "golden woff")
                    .Case<Dog>().Then(x => "woff")
                    .EndSwitch()
                    .HasValue,
                Is.False);
        }


        [Test]
        public void SwitchNonGeneric_TakingNonEmptyMaybe_MatchingNoCase_ReturnsEmptyMaybe()
        {
            var cat = new Cat();
            Assert.That(cat
                            .Maybe()
                            .Switch()
                            .Case<GoldenRetriever>().Then(x => "golden woff")
                            .Case<Dog>().Then(x => "woff")
                            .EndSwitch()
                            .HasValue,
                        Is.False);
        }


        [Test]
        public void SwitchNonGeneric_TakingNonEmptyMaybe_MatchingPredicate_ExecutesFunc()
        {
            var cat = new Cat() { Name = "oswald" };
            Assert.That(cat
                            .Maybe()
                            .Switch()
                            .Case<GoldenRetriever>().Then(x => "golden woff")
                            .Case<Cat>(x => x.Name == "oswald").Then(x => "see mouse!")
                            .EndSwitch()
                            .Value,
                        Is.EqualTo("see mouse!"));
        }


        [Test]
        public void SwitchNonGeneric_TakingNonEmptyMaybe_MatchingSecondCastCase_ExecutesSecondFunc()
        {
            var animal = new Dog();
            Assert.That(animal
                            .Maybe()
                            .Switch()
                            .Case<Cat>().Then(x => "miauu")
                            .Case<Dog>().Then(x => "woff")
                            .EndSwitch()
                            .Value,
                        Is.EqualTo("woff"));
        }


        [Test]
        public void SwitchWithCasesAsLambdaArgument_HavingNullDelegate_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => "what".Maybe().Switch<string>(null));
        }


        [Test]
        public void SwitchWithCasesAsLambdaArgument_InvokesCasesDelegate()
        {
            Assert.That("what".Maybe()
                              .Switch<string>(c => c
                                                  .Case(x => x == "what").Then(x => "yeah")
                                                  .Case(x => x == "moo").Then(x => "cow")
                            ).Value,
                        Is.EqualTo("yeah"));
        }


        public abstract class Animal
        {
            public string Name { get; set; }
        }

        public class Cat : Animal
        {
        }

        public class Dog
        {
        }

        public class GoldenRetriever : Dog
        {
        }

        public class InheritedClass : SuperClass
        {
        }

        public class SuperClass
        {
        }
    }
}
