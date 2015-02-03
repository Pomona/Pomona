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
using Pomona.FluentMapping;

namespace Pomona.UnitTests.FluentMapping
{
    public abstract class FluentMappingTestsBase
    {
        protected static FluentTypeMappingFilter GetMappingFilter(
            DefaultPropertyInclusionMode? defaultPropertyInclusionMode = null,
            Action<ITypeMappingConfigurator<TestEntityBase>> mappingOverride = null)
        {
            return GetMappingFilter<TestEntityBase>(defaultPropertyInclusionMode, mappingOverride);
        }


        protected static FluentTypeMappingFilter GetMappingFilter<T>(
            DefaultPropertyInclusionMode? defaultPropertyInclusionMode = null,
            Action<ITypeMappingConfigurator<T>> mappingOverride = null)
        {
            var sourceTypes = typeof(FluentMappingTestsBase).GetNestedTypes().ToList();
            var typeMappingFilter = new TestTypeMappingFilter(sourceTypes, defaultPropertyInclusionMode);
            var fluentRuleDelegates = mappingOverride != null ? new Delegate[] { mappingOverride } : new Delegate[] { };
            var fluentMappingFilter = new FluentTypeMappingFilter(
                typeMappingFilter,
                new object[] { new FluentRules(defaultPropertyInclusionMode) },
                fluentRuleDelegates,
                sourceTypes);
            return fluentMappingFilter;
        }


        protected static PropertyInfo GetPropInfo<TInstance>(Expression<Func<TInstance, object>> expr)
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


        protected Tuple<TFilterResult, TFilterResult> CheckHowChangeInPropertyRuleAffectsFilter
            <TProperty, TFilterResult>(
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


        protected Tuple<TFilterResult, TFilterResult> CheckHowChangeInTypeRuleAffectsFilter<T, TFilterResult>(
            Action<ITypeMappingConfigurator<T>>
                typeConfigurator,
            Func<ITypeMappingFilter, Type, TFilterResult> filterExecutor,
            TFilterResult expectedBefore,
            TFilterResult expectedAfter)
        {
            return CheckHowChangeInTypeRuleAffectsFilter(typeConfigurator,
                filterExecutor,
                (before, after) =>
                {
                    Assert.That(before, Is.EqualTo(expectedBefore));
                    Assert.That(after, Is.EqualTo(expectedAfter));
                });
        }


        protected Tuple<TFilterResult, TFilterResult> CheckHowChangeInTypeRuleAffectsFilter<T, TFilterResult>(
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



        #region Nested type: ChildEntity

        public class ChildEntity
        {
            public virtual int Id { get; set; }
            public virtual TestEntityBase Parent { get; set; }
        }

        #endregion

        #region Nested type: FluentRules

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

        #endregion

        #region Nested type: Specialized

        public class Specialized : Top
        {
            public virtual string WillMapToDefault { get; set; }
        }

        #endregion

        #region Nested type: TestEntityBase

        public abstract class TestEntityBase
        {
            public virtual IEnumerable<ChildEntity> Children { get; set; }
            public virtual int Id { get; set; }
            public abstract string ToBeOverridden { get; set; }
        }

        #endregion

        #region Nested type: TestTypeMappingFilter

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

        #endregion

        #region Nested type: Top

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

        #endregion
    }
}