// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.UnitTests.TypeSystem
{
    [TestFixture]
    public class ConstructorSpecTests
    {
        public abstract class Super
        {
            protected Super(int theUntouchable, int theOverridable)
            {
                TheUntouchable = theUntouchable;
                TheOverridable = theOverridable;
            }


            public virtual int TheOverridable { get; set; }
            public abstract int TheAbstract { get; set; }
            public int TheUntouchable { get; set; }
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
                get { return theAbstract; }
                set { theAbstract = value; }
            }

            public string TheOptional { get; set; }
            public string TheRequired { get; set; }
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

        public class MockedPropertySource : IConstructorPropertySource<Inherited>
        {
            private Inherited source;

            public MockedPropertySource(Inherited source)
            {
                this.source = source;
            }

            public Inherited Requires()
            {
                throw new NotImplementedException();
            }

            public Inherited Optional()
            {
                throw new NotImplementedException();
            }

            public TParentType Parent<TParentType>()
            {
                throw new NotImplementedException();
            }

            public TContext Context<TContext>()
            {
                throw new NotImplementedException();
            }

            public TProperty GetValue<TProperty>(PropertyInfo propertyInfo, Func<TProperty> defaultFactory)
            {
                return (TProperty) propertyInfo.GetValue(source, null);
            }
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

            Assert.That(pspecs.Select((x, i) => new {x.Position, i}).All(x => x.Position == x.i), Is.True);
        }

        [Test]
        public void FromConstructorInfo_ReturnsCorrectConstructorSpec()
        {
            var cspec = ConstructorSpec.FromConstructorInfo(typeof (Inherited).GetConstructors().First());
            Console.WriteLine(cspec.ConstructorExpression.ToCsharpString());
            AssertConstructorSpecWithFiveArguments(cspec);
        }


        [Test]
        public void GetParameterSpec_FromConstructorControl_UsingDeclaredProperty_IsSuccessful()
        {
            var cspec = GetConstructorSpecWithFiveArguments();
            var propInfo = typeof (Inherited).GetProperty("TheRequired");
            var pspec = cspec.GetParameterSpec(propInfo);
            Assert.That(pspec, Is.Not.Null);
        }


        [Test]
        public void GetParameterSpec_FromConstructorControl_UsingInheritedProperty_IsSuccessful()
        {
            var cspec = GetConstructorSpecWithFiveArguments();
            var propInfo = typeof (Inherited).GetProperty("TheUntouchable");
            var pspec = cspec.GetParameterSpec(propInfo);
            Assert.That(pspec, Is.Not.Null);
        }

        [Test]
        public void GetParameterSpec_FromConstructorControl_UsingOverridedAbstractProperty_IsSuccessful()
        {
            var cspec = GetConstructorSpecWithFiveArguments();
            var propInfo = typeof (Inherited).GetProperty("TheAbstract");
            var pspec = cspec.GetParameterSpec(propInfo);
            Assert.That(pspec, Is.Not.Null);
        }


        [Test]
        public void GetParameterSpec_FromConstructorControl_UsingOverridedProperty_IsSuccessful()
        {
            var cspec = GetConstructorSpecWithFiveArguments();
            var propInfo = typeof (Inherited).GetProperty("TheOverridable");
            var pspec = cspec.GetParameterSpec(propInfo);
            Assert.That(pspec, Is.Not.Null);
        }

        [Test]
        public void InjectingConstructorExpression_IsCorrectlyTransformed()
        {
            var cspec = GetConstructorSpecWithFiveArguments();
            var source = new Inherited(1, 2, 3, "4", "5");
            var copied =
                ((Expression<Func<IConstructorPropertySource<Inherited>, Inherited>>)
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
    }
}