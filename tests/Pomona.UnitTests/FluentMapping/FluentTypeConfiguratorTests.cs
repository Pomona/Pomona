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
using System.Linq;

using NUnit.Framework;

using Pomona.Common.TypeSystem;
using Pomona.Example;
using Pomona.FluentMapping;

namespace Pomona.UnitTests.FluentMapping
{
    [TestFixture]
    public class FluentTypeConfiguratorTests : FluentMappingTestsBase
    {
        [Test]
        public void HasChildren_TypeMappingOptionsAreApplied()
        {
            CheckHowChangeInTypeRuleAffectsFilter<TestEntityBase, string>(
                x => x.HasChildren(y => y.Children, y => y.Parent, y =>
                {
                    Console.WriteLine(y.GetType());
                    return y.Named("SuperChild");
                }),
                (y, t) => y.GetTypeMappedName(typeof(ChildEntity)),
                "ChildEntity",
                "SuperChild");
        }


        [Test]
        public void AsAbstract_GivenTypeThatWouldBeConcreteByConvention_MakesTypeAbstract()
        {
            CheckHowChangeInTypeRuleAffectsFilter<Top, bool>(
                x => x.AsAbstract(),
                (f, t) => f.GetTypeIsAbstract(t),
                (origValue, changedValue) =>
                {
                    Assert.That(changedValue, Is.Not.EqualTo(origValue), "Test no use if change in filter has no effect");
                    Assert.That(changedValue, Is.EqualTo(true));
                });
        }


        [Test]
        public void ExposedAt_WillModifyUrlRelativePath()
        {
            CheckHowChangeInTypeRuleAffectsFilter<Top, string>(
                x => x.ExposedAt("newpath"),
                (f, t) => f.GetUrlRelativePath(t),
                (origValue, changedValue) =>
                {
                    Assert.That(changedValue, Is.Not.EqualTo(origValue), "Test no use if change in filter has no effect");
                    Assert.That(changedValue, Is.EqualTo("newpath"));
                });
        }


        [Test]
        public void ExposedAt_WithLeadingPathSeparator_LeadingPathSeparatorIsStrippedFromMappedType()
        {
            CheckHowChangeInTypeRuleAffectsFilter<Top, ResourceType>(
                x => x.ExposedAt("/newpath"),
                (f, t) => (ResourceType)new TypeMapper(f, new []  {typeof(Top)}, null, null).GetClassMapping<Top>(),
                (origValue, changedValue) =>
                {
                    Assert.That(changedValue.UrlRelativePath, Is.Not.EqualTo(origValue), "Test no use if change in filter has no effect");
                    Assert.That(changedValue.UrlRelativePath, Is.EqualTo("newpath"));
                });
        }


        [Test]
        public void AsConcrete_GivenTypeThatWouldBeAbstractByConvention_MakesTypeNonAbstract()
        {
            CheckHowChangeInTypeRuleAffectsFilter<TestEntityBase, bool>(
                x => x.AsConcrete(),
                (f, t) => f.GetTypeIsAbstract(t),
                (origValue, changedValue) =>
                {
                    Assert.That(changedValue, Is.Not.EqualTo(origValue), "Test no use if change in filter has no effect");
                    Assert.That(changedValue, Is.EqualTo(false));
                });
        }


        [Test]
        public void DefaultPropertyInclusionMode_SetToExcludedByDefault_IncludesOverriddenPropertyInInheritedClass()
        {
            var filter = GetMappingFilter(DefaultPropertyInclusionMode.AllPropertiesAreExcludedByDefault);
            Assert.That(
                filter.PropertyIsIncluded(typeof(TestEntityBase), GetPropInfo<TestEntityBase>(x => x.ToBeOverridden)),
                Is.True);

            var propInfo = typeof(Top).GetProperty("ToBeOverridden");
            Assert.That(filter.PropertyIsIncluded(typeof(Top), propInfo), Is.True);
        }


        [Test]
        public void DefaultPropertyInclusionMode_SetToExcludedByDefault_IncludesPropertyInInheritedClass()
        {
            var filter = GetMappingFilter(DefaultPropertyInclusionMode.AllPropertiesAreExcludedByDefault);
            Assert.That(filter.PropertyIsIncluded(typeof(TestEntityBase), GetPropInfo<TestEntityBase>(x => x.Id)),
                        Is.True);
            Assert.That(filter.PropertyIsIncluded(typeof(Specialized), GetPropInfo<Specialized>(x => x.Id)), Is.True);
        }


        [Test]
        public void DefaultPropertyInclusionMode_SetToExcludedByDefault_MakesPropertyExcludedByDefault()
        {
            var filter = GetMappingFilter(DefaultPropertyInclusionMode.AllPropertiesAreExcludedByDefault);
            Assert.That(
                filter.PropertyIsIncluded(typeof(Specialized), GetPropInfo<Specialized>(x => x.WillMapToDefault)),
                Is.False);
        }


        [Test]
        public void DefaultPropertyInclusionMode_SetToIncludedByDefault_MakesPropertyIncludedByDefault()
        {
            var filter = GetMappingFilter(DefaultPropertyInclusionMode.AllPropertiesAreIncludedByDefault);
            Assert.That(
                filter.PropertyIsIncluded(typeof(Specialized), GetPropInfo<Specialized>(x => x.WillMapToDefault)),
                Is.True);
        }


        [Test]
        public void Named_OverridesDefaultNameOfType()
        {
            CheckHowChangeInTypeRuleAffectsFilter<Top, string>(x => x.Named("HolaHola"),
                                                               (x, t) => x.GetTypeMappedName(t),
                                                               "Top",
                                                               "HolaHola");
        }


        [Test]
        public void OnDeserializedRule_IsAppliedToMappingFilter()
        {
            var fluentMappingFilter = GetMappingFilter();

            var onDeserializedHook = fluentMappingFilter.GetOnDeserializedHook(typeof(Top));
            Assert.That(onDeserializedHook, Is.Not.Null);
            var top = new Top();
            onDeserializedHook(top);
            Assert.That(top.DeserializeHookWasRun, Is.True);
        }


        [Test]
        public void RuleForBaseClass_IsAlsoAppliedToInheritedClass()
        {
            var fluentMappingFilter = GetMappingFilter();
            var propertyInfo = GetPropInfo<Specialized>(x => x.ToBeRenamed);
            Assert.That(
                fluentMappingFilter.GetPropertyMappedName(propertyInfo.ReflectedType, propertyInfo),
                Is.EqualTo("NewName"));
        }


        [Category("TODO")]
        [Test]
        public void Stuff_Not_Yet_Covered_By_Tests()
        {
            Assert.Fail("TODO: Test that default inclusion mode works.");
            Assert.Fail(
                "TODO: Test that explicit inclusion mode throws exception if not all properties are accounted for.");
        }


        [Test]
        public void TestGenerateTemplateFluentRules()
        {
            var code =
                FluentTypeMappingFilter.BuildPropertyMappingTemplate(
                    CritterRepository.GetEntityTypes().Where(x => !x.IsEnum));
            Console.Write(code);
        }


        [Test]
        public void WithPluralName_OverridesDefaultNameOfType()
        {
            CheckHowChangeInTypeRuleAffectsFilter<Top, string>(x => x.WithPluralName("HolaHolas"),
                                                               (x, t) => x.GetPluralNameForType(t),
                                                               "Tops",
                                                               "HolaHolas");
        }
    }
}