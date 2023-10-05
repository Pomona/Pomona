﻿#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using NUnit.Framework;

using Pomona.Common.TypeSystem;

namespace Pomona.UnitTests.TypeSystem
{
    [TestFixture]
    public class ConstructorSpecTests
    {
        [Test]
        public void FromConstructorInfo_ReturnsCorrectConstructorSpec()
        {
            var cspec = ConstructorSpec.FromConstructorInfo(typeof(Inherited).GetConstructors().First());
            AssertConstructorSpecWithFiveArguments(cspec);
        }


        [Test]
        public void GetParameterSpec_FromConstructorControl_UsingDeclaredProperty_IsSuccessful()
        {
            var cspec = GetConstructorSpecWithFiveArguments();
            var propInfo = typeof(Inherited).GetProperty("TheRequired");
            var pspec = cspec.GetParameterSpec(propInfo);
            Assert.That(pspec, Is.Not.Null);
        }


        [Test]
        public void GetParameterSpec_FromConstructorControl_UsingInheritedProperty_IsSuccessful()
        {
            var cspec = GetConstructorSpecWithFiveArguments();
            var propInfo = typeof(Inherited).GetProperty("TheUntouchable");
            var pspec = cspec.GetParameterSpec(propInfo);
            Assert.That(pspec, Is.Not.Null);
        }


        [Test]
        public void GetParameterSpec_FromConstructorControl_UsingOverridedAbstractProperty_IsSuccessful()
        {
            var cspec = GetConstructorSpecWithFiveArguments();
            var propInfo = typeof(Inherited).GetProperty("TheAbstract");
            var pspec = cspec.GetParameterSpec(propInfo);
            Assert.That(pspec, Is.Not.Null);
        }


        [Test]
        public void GetParameterSpec_FromConstructorControl_UsingOverridedProperty_IsSuccessful()
        {
            var cspec = GetConstructorSpecWithFiveArguments();
            var propInfo = typeof(Inherited).GetProperty("TheOverridable");
            var pspec = cspec.GetParameterSpec(propInfo);
            Assert.That(pspec, Is.Not.Null);
        }


        [Test]
        public void
            InjectingConstructorExpression_FromEntityWithNonNullablePropertyPairedWithNullableConstructorParam_BehavesCorrectly
            ()
        {
            Expression<Func<IConstructorControl<NullableTestClass>, NullableTestClass>> cspecExpr =
                c => new NullableTestClass(c.Optional().Foo);
            var cspec = new ConstructorSpec(cspecExpr);
            var func = ((
                Expression<Func<IConstructorPropertySource, NullableTestClass>>)
                cspec.InjectingConstructorExpression).Compile();
            var result = func(new MockedPropertySourceHavingZeroProperties());
            Assert.That(result.Foo, Is.EqualTo(1337));
        }


        [Test]
        public void InjectingConstructorExpression_IsCorrectlyTransformed()
        {
            var cspec = GetConstructorSpecWithFiveArguments();
            var source = new Inherited(1, 2, 3, "4", "5");
            var copied =
                ((Expression<Func<IConstructorPropertySource, Inherited>>)
                    cspec.InjectingConstructorExpression).Compile()(new MockedPropertySource(source));
            Assert.That(copied.TheAbstract, Is.EqualTo(source.TheAbstract));
            Assert.That(copied.TheOptional, Is.EqualTo(source.TheOptional));
            Assert.That(copied.TheOverridable, Is.EqualTo(source.TheOverridable));
            Assert.That(copied.TheRequired, Is.EqualTo(source.TheRequired));
            Assert.That(copied.TheUntouchable, Is.EqualTo(source.TheUntouchable));
        }


        [Test]
        public void ParametersSpecs_FromConstructorControl_AreCorrectlyDefined()
        {
            var constructorSpec = GetConstructorSpecWithFiveArguments();
            AssertConstructorSpecWithFiveArguments(constructorSpec);
        }


        private static void AssertConstructorSpecWithFiveArguments(ConstructorSpec constructorSpec)
        {
            var pspecs = constructorSpec.ParameterSpecs.ToList();

            Assert.That(pspecs.Count, Is.EqualTo(5));
            Assert.That(pspecs[0].PropertyInfo.Name, Is.EqualTo("TheUntouchable"));
            Assert.That(pspecs[0].IsRequired, Is.True);

            Assert.That(pspecs[1].PropertyInfo.Name, Is.EqualTo("TheOverridable"));
            Assert.That(pspecs[1].IsRequired, Is.True);

            Assert.That(pspecs[2].PropertyInfo.Name, Is.EqualTo("TheAbstract"));
            Assert.That(pspecs[2].IsRequired, Is.True);

            Assert.That(pspecs[3].PropertyInfo.Name, Is.EqualTo("TheRequired"));
            Assert.That(pspecs[3].IsRequired, Is.True);

            Assert.That(pspecs[4].PropertyInfo.Name, Is.EqualTo("TheOptional"));
            Assert.That(pspecs[4].IsRequired, Is.False);

            Assert.That(pspecs.Select((x, i) => new { x.Position, i }).All(x => x.Position == x.i), Is.True);
        }


        private static ConstructorSpec GetConstructorSpecWithFiveArguments()
        {
            Expression<Func<IConstructorControl<Inherited>, Inherited>> expr =
                c =>
                    new Inherited(c.Requires().TheUntouchable,
                                  c.Requires().TheOverridable,
                                  c.Requires().TheAbstract,
                                  c.Requires().TheRequired,
                                  c.Optional().TheOptional);
            var constructorSpec = new ConstructorSpec(expr);
            return constructorSpec;
        }


        public class Inherited : Super
        {
            private int theAbstract;


            public Inherited(int theUntouchable,
                             int theOverridable,
                             int theAbstract,
                             string theRequired,
                             string theOptional = null)
                : base(theUntouchable, theOverridable)
            {
                this.theAbstract = theAbstract;
                TheRequired = theRequired;
                TheOptional = theOptional;
            }


            public override int TheAbstract
            {
                get { return this.theAbstract; }
                set { this.theAbstract = value; }
            }

            public string TheOptional { get; set; }
            public string TheRequired { get; set; }
        }

        public class MockedPropertySource : IConstructorPropertySource
        {
            private readonly Inherited source;


            public MockedPropertySource(Inherited source)
            {
                this.source = source;
            }


            public Inherited Optional()
            {
                throw new NotImplementedException();
            }


            public Inherited Requires()
            {
                throw new NotImplementedException();
            }


            public TContext Context<TContext>()
            {
                throw new NotImplementedException();
            }


            public TProperty GetValue<TProperty>(PropertyInfo propertyInfo, Func<TProperty> defaultFactory)
            {
                return (TProperty)propertyInfo.GetValue(this.source, null);
            }


            public TParentType Parent<TParentType>()
            {
                throw new NotImplementedException();
            }
        }

        public class MockedPropertySourceHavingZeroProperties : IConstructorPropertySource
        {
            public TContext Context<TContext>()
            {
                throw new NotImplementedException();
            }


            public TProperty GetValue<TProperty>(PropertyInfo propertyInfo, Func<TProperty> defaultFactory)
            {
                if (defaultFactory == null)
                    throw new NotImplementedException();
                return defaultFactory();
            }


            public TParentType Parent<TParentType>()
            {
                throw new NotImplementedException();
            }
        }

        public class NullableTestClass
        {
            public NullableTestClass(int? foo)
            {
                Foo = foo ?? 1337;
            }


            public int Foo { get; private set; }
        }

        public abstract class Super
        {
            protected Super(int theUntouchable, int theOverridable)
            {
                TheUntouchable = theUntouchable;
                TheOverridable = theOverridable;
            }


            public abstract int TheAbstract { get; set; }
            public virtual int TheOverridable { get; set; }
            public int TheUntouchable { get; set; }
        }
    }
}

