﻿#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.TypeSystem;
using Pomona.Example;
using Pomona.Example.ModelProxies;
using Pomona.Example.Models;
using Pomona.Example.SimpleExtraSite;

namespace Pomona.UnitTests
{
    [TestFixture]
    public class TypeMapperTests
    {
        private TypeMapper typeMapper;

        private TypeMapper TypeMapper => this.typeMapper ?? (this.typeMapper = new TypeMapper(new CritterPomonaConfiguration()));


        [Test]
        public void AnonymousCompilerGeneratedType_IsMappedAsValueObject()
        {
            var anonObject = new { Foo = "hoohoo" };
            var type = TypeMapper.FromType(anonObject.GetType());
            Assert.That(type, Is.TypeOf<ComplexType>());
            Assert.That(((StructuredType)type).MappedAsValueObject, Is.True);
        }


        [Test]
        public void ChangePluralNameWorksCorrectly()
        {
            Assert.That(((ResourceType)TypeMapper.FromType<RenamedThing>()).PluralName,
                        Is.EqualTo("ThingsWithNewName"));
        }


        [Test]
        public void ChangeTypeNameWorksCorrectly()
        {
            Assert.That(TypeMapper.FromType<RenamedThing>().Name, Is.EqualTo("GotNewName"));
        }


        [Test]
        public void DoesNotDuplicatePropertiesWhenDerivedFromHiddenBaseClassInMiddle()
        {
            var tt = TypeMapper.FromType<InheritsFromHiddenBase>();
            Assert.That(tt.Properties.Count(x => x.Name == "Id"), Is.EqualTo(1));
            var idProp = tt.Properties.First(x => x.Name == "Id");
            Assert.That(idProp.DeclaringType, Is.EqualTo(TypeMapper.FromType<EntityBase>()));
        }


        [Test]
        public void FromType_QueryResult_ReturnsQueryResultType()
        {
            this.typeMapper = new TypeMapper(new SimplePomonaConfiguration());
            var type = TypeMapper.FromType(typeof(QueryResult<Critter>));
            Assert.That(type, Is.InstanceOf<QueryResultType>());
            Assert.That(type, Is.InstanceOf<QueryResultType<Critter>>());
        }


        [Test]
        public void GetClassMapping_ByInvalidName_ThrowsUnknownTypeException()
        {
            Assert.Throws<UnknownTypeException>(() => TypeMapper.FromType("WTF"));
        }


        [Test]
        public void GetClassMapping_ByValidName_ReturnsCorrectType()
        {
            var critterType = TypeMapper.FromType("Critter");
            Assert.IsNotNull(critterType);
            Assert.That(critterType.Type, Is.EqualTo(typeof(Critter)));
        }


        [Test]
        public void GetPropertyFormula_ForTypeNotHavingFormulaSpecified_ReturnsNull()
        {
            var idProp = TypeMapper.FromType<Critter>().GetPropertyByName("Id", false);
            Assert.That(idProp.GetPropertyFormula(), Is.Null);
        }


        [Test]
        public void GetTypeForProxyTypeInheritedFromMappedType_ReturnsMappedBaseType()
        {
            Assert.That(TypeMapper.FromType(typeof(BearProxy)).Type, Is.EqualTo(typeof(Bear)));
        }


        [Test]
        public void InterfaceIGrouping_IsMappedAsValueObject()
        {
            var type = TypeMapper.FromType(typeof(IGrouping<string, string>));
            Assert.That(type, Is.TypeOf<ComplexType>());
            Assert.That(((StructuredType)type).MappedAsValueObject, Is.True);
        }


        [Test]
        public void Property_removed_from_filter_in_GetAllPropertiesOfType_is_not_mapped()
        {
            var type = this.typeMapper.FromType<Critter>();
            Assert.That(type.Properties.Where(x => x.Name == "PropertyExcludedByGetAllPropertiesOfType"), Is.Empty);
        }


        [Test]
        public void Property_ThatIsPublicWritableOnServer_AndReadOnlyThroughApi_IsNotPublic()
        {
            var tt =
                (StructuredProperty)
                    TypeMapper.FromType<Critter>().Properties.First(
                        x => x.Name == "PublicAndReadOnlyThroughApi");
            Assert.That(!tt.AccessMode.HasFlag(HttpMethod.Post));
        }


        [Test]
        public void Property_WithFluentlyAddedAttribute_GotAttributeAddedToPropertySpec()
        {
            var tt = TypeMapper.FromType<Critter>();
            var prop = tt.Properties.SingleOrDefault(x => x.Name == "PropertyWithAttributeAddedFluently");
            Assert.That(prop, Is.Not.Null, "Unable to find property PropertyWithAttributeAddedFluently");
            Assert.That(prop.DeclaredAttributes.OfType<ObsoleteAttribute>().Any(), Is.True);
        }


        [Test]
        public void PropertyOfExposedInterfaceFromNonExposedBaseInterfaceGotCorrectDeclaringType()
        {
            var tt = TypeMapper.FromType<IExposedInterface>();
            var prop = tt.Properties.SingleOrDefault(x => x.Name == "PropertyFromInheritedInterface");
            Assert.That(prop, Is.Not.Null, "Unable to find property PropertyFromInheritedInterface");
            Assert.That(prop.DeclaringType, Is.EqualTo(tt));
        }


        [Test]
        public void Resolve_types_from_a_lot_of_threads_does_not_cause_exceptions_due_to_race_conditions()
        {
            var kickoffEvent = new ManualResetEvent(false);
            var sourceTypes = new CritterPomonaConfiguration().SourceTypes;
            ConcurrentBag<Exception> exceptions = new ConcurrentBag<Exception>();
            var threads = Enumerable.Range(0, 40).Select(i =>
            {
                var thread = new Thread(y =>
                {
                    try
                    {
                        kickoffEvent.WaitOne();
                        var t = this.typeMapper.FromType(typeof(Critter));
                        var name = t.Name;
                        var rt = t as ResourceType;
                        if (rt != null)
                        {
                            var uriBaseType = rt.UriBaseType;
                        }
                        var props = t.Properties.ToList();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        exceptions.Add(ex);
                        throw new InvalidOperationException("Thread faild", ex);
                    }
                });
                thread.Start();
                return thread;
            }).ToList();

            kickoffEvent.Set();

            foreach (var t in threads)
                t.Join();
            Assert.That(exceptions, Is.Empty);
        }

        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            this.typeMapper = new TypeMapper(new CritterPomonaConfiguration());
        }

        #endregion

        [Test]
        public void StaticProperty_IsExcludedByDefault()
        {
            Assert.That(
                TypeMapper.FromType(typeof(Critter)).Properties.Where(x => x.Name == "TheIgnoredStaticProperty"),
                Is.Empty);
        }

        #region Setup/Teardown

        [TearDown]
        public void TearDown()
        {
            this.typeMapper = null;
        }

        #endregion
    }
}

