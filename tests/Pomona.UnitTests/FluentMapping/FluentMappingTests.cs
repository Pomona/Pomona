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
using System.Linq.Expressions;
using System.Reflection;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Example;
using Pomona.FluentMapping;

namespace Pomona.UnitTests.FluentMapping
{
    [TestFixture]
    public class FluentMappingTests
    {
        public class ChildEntity
        {
            public virtual int Id { get; set; }
        }

        public abstract class TestEntityBase
        {
            public virtual IEnumerable<ChildEntity> Children { get; set; }
            public virtual int Id { get; set; }
            public abstract string ToBeOverridden { get; set; }
        }

        public class Top : TestEntityBase
        {
            private string toBeOverridden;

            // ReSharper disable ConvertToAutoProperty

            public virtual bool DeserializeHookWasRun { get; set; }

            public virtual string ToBeRenamed { get; set; }

            public override string ToBeOverridden
                // ReSharper restore ConvertToAutoProperty
            {
                get { return this.toBeOverridden; }
                set { this.toBeOverridden = value; }
            }
        }

        public class Specialized : Top
        {
            public virtual string WillMapToDefault { get; set; }
        }

        public class FluentRules
        {
            private readonly DefaultPropertyInclusionMode? defaultPropertyInclusionMode;


            public FluentRules(DefaultPropertyInclusionMode? defaultPropertyInclusionMode = null)
            {
                this.defaultPropertyInclusionMode = defaultPropertyInclusionMode;
            }


            public void Map(ITypeMappingConfigurator<Specialized> map)
            {
            }


            public void Map(ITypeMappingConfigurator<TestEntityBase> map)
            {
                map.Include(x => x.Id);
                map.Include(x => x.ToBeOverridden);

                switch (this.defaultPropertyInclusionMode)
                {
                    case null:
                        break;
                    case DefaultPropertyInclusionMode.AllPropertiesRequiresExplicitMapping:
                        map.AllPropertiesRequiresExplicitMapping();
                        break;

                    case DefaultPropertyInclusionMode.AllPropertiesAreIncludedByDefault:
                        map.AllPropertiesAreIncludedByDefault();
                        break;

                    case DefaultPropertyInclusionMode.AllPropertiesAreExcludedByDefault:
                        map.AllPropertiesAreExcludedByDefault();
                        break;
                }
            }


            public void Map(ITypeMappingConfigurator<Top> map)
            {
                map.OnDeserialized(x => x.DeserializeHookWasRun = true);
                map.Include(x => x.ToBeRenamed, o => o.Named("NewName"));
            }
        }

        public class TestTypeMappingFilter : TypeMappingFilterBase
        {
            private readonly DefaultPropertyInclusionMode? defaultPropertyInclusion;


            public TestTypeMappingFilter(IEnumerable<Type> sourceTypes,
                DefaultPropertyInclusionMode? defaultPropertyInclusion = null)
                : base(sourceTypes)
            {
                this.defaultPropertyInclusion = defaultPropertyInclusion;
            }


            public override DefaultPropertyInclusionMode GetDefaultPropertyInclusionMode()
            {
                return this.defaultPropertyInclusion.HasValue
                    ? this.defaultPropertyInclusion.Value
                    : base.GetDefaultPropertyInclusionMode();
            }
        }


        private static PropertyInfo GetPropInfo<TInstance>(Expression<Func<TInstance, object>> expr)
        {
            var body = expr.Body;

            while (body.NodeType == ExpressionType.Convert)
                body = ((UnaryExpression)body).Operand;

            var memberExpr = body as MemberExpression;

            if (memberExpr == null)
                throw new ArgumentException("Expected expression with MemberExpression as body", "expr");

            var propInfo = memberExpr.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException("Expected MemberExpression with property acccess");

            return typeof(TInstance).GetProperty(propInfo.Name);

            //return propInfo;
        }


        private static FluentTypeMappingFilter GetMappingFilter(
            DefaultPropertyInclusionMode? defaultPropertyInclusionMode = null,
            Action<ITypeMappingConfigurator<TestEntityBase>> mappingOverride = null)
        {
            return GetMappingFilter<TestEntityBase>(defaultPropertyInclusionMode, mappingOverride);
        }


        private static FluentTypeMappingFilter GetMappingFilter<T>(
            DefaultPropertyInclusionMode? defaultPropertyInclusionMode = null,
            Action<ITypeMappingConfigurator<T>> mappingOverride = null)
        {
            var sourceTypes = typeof(FluentMappingTests).GetNestedTypes().Where(
                x => typeof(T).IsAssignableFrom(x)).ToList();
            var typeMappingFilter = new TestTypeMappingFilter(sourceTypes, defaultPropertyInclusionMode);
            var fluentRuleDelegates = mappingOverride != null ? new Delegate[] { mappingOverride } : new Delegate[] { };
            var fluentMappingFilter = new FluentTypeMappingFilter(
                typeMappingFilter,
                new object[] { new FluentRules(defaultPropertyInclusionMode) },
                fluentRuleDelegates,
                sourceTypes);
            return fluentMappingFilter;
        }


        private Tuple<TFilterResult, TFilterResult> CheckHowChangeInPropertyRuleAffectsFilter<TProperty, TFilterResult>(
            Expression<Func<TestEntityBase, TProperty>> propertyExpr,
            Func<IPropertyOptionsBuilder<TestEntityBase, TProperty>, IPropertyOptionsBuilder<TestEntityBase, TProperty>>
                propertyOptions,
            Func<ITypeMappingFilter, PropertyInfo, TFilterResult> filterExecutor,
            Action<TFilterResult, TFilterResult> origChangedAssertAction)
        {
            var property = propertyExpr.ExtractPropertyInfo();
            Action<ITypeMappingConfigurator<TestEntityBase>> map = x => x.Include(propertyExpr, propertyOptions);
            var origFilter = GetMappingFilter<TestEntityBase>();
            var changedFilter = GetMappingFilter(mappingOverride : map);
            var origValue = filterExecutor(origFilter, property);
            var changedValue = filterExecutor(changedFilter, property);
            origChangedAssertAction(origValue, changedValue);
            return new Tuple<TFilterResult, TFilterResult>(origValue, changedValue);
        }


        private Tuple<TFilterResult, TFilterResult> CheckHowChangeInTypeRuleAffectsFilter<T, TFilterResult>(
            Action<ITypeMappingConfigurator<T>>
                typeConfigurator,
            Func<ITypeMappingFilter, Type, TFilterResult> filterExecutor,
            Action<TFilterResult, TFilterResult> origChangedAssertAction)
        {
            var origFilter = GetMappingFilter<T>();
            var changedFilter = GetMappingFilter(mappingOverride : typeConfigurator);
            var origValue = filterExecutor(origFilter, typeof(T));
            var changedValue = filterExecutor(changedFilter, typeof(T));
            origChangedAssertAction(origValue, changedValue);
            return new Tuple<TFilterResult, TFilterResult>(origValue, changedValue);
        }


        [Test]
        public void AllowMethodForProperty_CombinesAllowedMethodWithConventionBasedPermissions()
        {
            CheckHowChangeInPropertyRuleAffectsFilter(x => x.Children,
                x => x.Allow(HttpMethod.Delete),
                (f, p) => f.GetPropertyAccessMode(p, null),
                (origValue, changedValue) =>
                {
                    Assert.That(changedValue, Is.Not.EqualTo(origValue), "Test no use if change in filter has no effect");
                    Assert.That(changedValue, Is.EqualTo(origValue | HttpMethod.Delete));
                });
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
        public void DenyMethodForProperty_RemovesMethodFromConventionBasedPermissions()
        {
            CheckHowChangeInPropertyRuleAffectsFilter(x => x.Children,
                x => x.Deny(HttpMethod.Get),
                (f, p) => f.GetPropertyAccessMode(p, null),
                (origValue, changedValue) =>
                {
                    Assert.That(changedValue, Is.Not.EqualTo(origValue), "Test no use if change in filter has no effect");
                    Assert.That(changedValue, Is.EqualTo(origValue & ~HttpMethod.Get));
                });
        }


        [Test]
        public void ItemsAllowMethodForProperty_CombinesAllowedMethodWithConventionBasedPermissions()
        {
            CheckHowChangeInPropertyRuleAffectsFilter(x => x.Children,
                x => x.ItemsAllow(HttpMethod.Delete),
                (f, p) => f.GetPropertyItemAccessMode(p),
                (origValue, changedValue) =>
                {
                    Assert.That(changedValue, Is.Not.EqualTo(origValue), "Test no use if change in filter has no effect");
                    Assert.That(changedValue, Is.EqualTo(origValue | HttpMethod.Delete));
                });
        }


        [Test]
        public void ItemsDenyMethodForProperty_RemovesMethodFromConventionBasedPermissions()
        {
            CheckHowChangeInPropertyRuleAffectsFilter(x => x.Children,
                x => x.ItemsDeny(HttpMethod.Get),
                (f, p) => f.GetPropertyItemAccessMode(p),
                (origValue, changedValue) =>
                {
                    Assert.That(changedValue, Is.Not.EqualTo(origValue), "Test no use if change in filter has no effect");
                    Assert.That(changedValue, Is.EqualTo(origValue & ~HttpMethod.Get));
                });
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
        public void RenameRule_GivesPropertyANewName()
        {
            var fluentMappingFilter = GetMappingFilter();
            Assert.That(
                fluentMappingFilter.GetPropertyMappedName(GetPropInfo<Top>(x => x.ToBeRenamed)),
                Is.EqualTo("NewName"));
        }


        [Test]
        public void RuleForBaseClass_IsAlsoAppliedToInheritedClass()
        {
            var fluentMappingFilter = GetMappingFilter();
            Assert.That(
                fluentMappingFilter.GetPropertyMappedName(GetPropInfo<Specialized>(x => x.ToBeRenamed)),
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
    }
}