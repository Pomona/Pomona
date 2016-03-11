#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.IO;

using NUnit.Framework;

using Pomona.CodeGen;

namespace Pomona.UnitTests
{
    [TestFixture]
    public class AnonymousTypeBuilderTests
    {
        private AnonymousTypeBuilder builder;
        private string[] propNames;


        [Test]
        public void CanConstructObject()
        {
            Assert.DoesNotThrow(() => CreateAnonObject(1337, "hello"));
        }


        [Test]
        public void CanSetPropertiesAndReadBack()
        {
            var obj = CreateAnonObject(1337, "hello");

            AssertObjectHasPropWithValue(obj, "Foo", 1337);
            AssertObjectHasPropWithValue(obj, "Bar", "hello");
        }


        [Test]
        public void EqualsReturnsFalseForDifferentObjectType()
        {
            var obj = CreateAnonObject(1337, "hello");
            Assert.That(obj.Equals("whatever"), Is.False);
        }


        [Test]
        public void EqualsReturnsFalseForSameTypeWithDifferentValues()
        {
            var obj = CreateAnonObject(1337, "hello");
            var objOther = Activator.CreateInstance(obj.GetType(), 42, "hello");
            Assert.That(obj.Equals(objOther), Is.False);
        }


        [Test]
        public void EqualsReturnsTrueForSameTypeWithSameValues()
        {
            var obj = CreateAnonObject(1337, "hello");
            var objOther = Activator.CreateInstance(obj.GetType(), 1337, "hello");
            Assert.That(obj.Equals(objOther), Is.True);
        }


        [Test]
        public void GetHashCodeReturnsExpectedValue()
        {
            var runtimeGenerated = CreateAnonObject(new FixedHash(1337), new FixedHash(0xdead));
            Assert.That(runtimeGenerated.GetHashCode(), Is.EqualTo(-2040804512));
        }


        [SetUp]
        public void SetUp()
        {
            this.propNames = new[] { "Foo", "Bar" };
            this.builder = new AnonymousTypeBuilder(this.propNames);
        }


        [Test]
        public void ToStringReturnsHasCorrectFormatting()
        {
            var obj = CreateAnonObject(1337, "hello");
            var str = obj.ToString();
            Assert.That(str, Is.EqualTo("{ Foo = 1337, Bar = hello }"));
        }


        [Test]
        [Ignore]
        public void WriteAssemblyToFileForDebugging()
        {
            var tb = new AnonymousTypeBuilder(new[] { "Foo", "Bar" });
            var def = tb.BuildAnonymousType();
            def.Module.Assembly.Write("tempasm.dll");

            Assert.Fail("TODO: Remove me");
        }


        private void AssertObjectHasPropWithValue<T>(object obj, string name, T expected)
        {
            var type = obj.GetType();
            var prop = type.GetProperty(name);
            Assert.That(prop, Is.Not.Null);
            Assert.That(prop.GetValue(obj, null), Is.EqualTo(expected));
        }


        private Type BuildTypeAndLoad()
        {
            var typeDef = this.builder.BuildAnonymousType();
            var memStream = new MemoryStream();
            typeDef.Module.Assembly.Write(memStream);
            var loadedAsm = AppDomain.CurrentDomain.Load(memStream.ToArray());
            return loadedAsm.GetType(typeDef.Name);
        }


        private object CreateAnonObject<TFoo, TBar>(TFoo foo, TBar bar)
        {
            var type = BuildTypeAndLoad();
            var typeInstance = type.MakeGenericType(typeof(TFoo), typeof(TBar));
            var obj = Activator.CreateInstance(typeInstance, foo, bar);
            return obj;
        }


        private struct FixedHash
        {
            public FixedHash(int value)
            {
                Value = value;
            }


            public int Value { get; }


            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                return obj is FixedHash && Equals((FixedHash)obj);
            }


            public override int GetHashCode()
            {
                return Value;
            }


            private bool Equals(FixedHash other)
            {
                return Value == other.Value;
            }
        }
    }
}