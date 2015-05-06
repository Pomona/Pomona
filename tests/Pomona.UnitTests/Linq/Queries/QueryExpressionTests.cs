#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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
using System.Runtime.CompilerServices;
using System.Text;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Linq.Queries;

namespace Pomona.UnitTests.Linq.Queries
{
    [TestFixture]
    public class QueryExpressionTests
    {
        private readonly string queryExpressionCodeTemplate = @"    public class {0}Expression : QueryChainedExpression
    {{
        private static {0}Factory factory;

        public static readonly MethodInfo Method =
            ReflectionHelper.GetMethodDefinition<IQueryable<object>>(x => x.{4}(???));


        private {0}Expression(MethodCallExpression node, QueryExpression source)
            : base(node, source)
        {{
        }}


        public static QueryExpressionFactory Factory
        {{
            get {{ return factory ?? (factory = new {0}Factory()); }}
        }}

        public {3} {1}
        {{
            get {{ return ???; }}
        }}


        public static {0}Expression Create(QueryExpression source, {3} {2})
        {{
            return factory.Create(source, {2});
        }}


        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {{
            var origSource = Source;
            var orig{1} = {1};
            var visitedSource = visitor.VisitAndConvert(origSource, ""VisitChildren"");
            var visited{1} = visitor.VisitAndConvert(orig{1}, ""VisitChildren"");
            if (visitedSource != origSource || visited{1} != orig{1})
                return Create(visitedSource, visited{1});
            return this;
        }}

        #region Nested type: {0}Factory

        private class {0}Factory : QueryChainedExpressionFactory<{0}Expression>
        {{
            public {0}Expression Create(QueryExpression source, {3} {2})
            {{
                if (source == null)
                    throw new ArgumentNullException(""source"");
                if ({2} == null)
                    throw new ArgumentNullException(""{2}"");
                return new {0}Expression(Call(Method.MakeGenericMethod(source.ElementType),
                                                source.Node,
                                                ConvertAndQuote({2}, source.ElementType)),
                                           source);
            }}
        }}

        #endregion
    }}
";
        private readonly string queryExpressionCodeTemplate3 = @"    public class {0}Expression : QueryChainedExpression
    {{
        private static {0}Factory factory;

        public static readonly MethodInfo Method =
            ReflectionHelper.GetMethodDefinition<IQueryable<object>>(x => x.{4}(???, ???));


        private {0}Expression(MethodCallExpression node, QueryExpression source)
            : base(node, source)
        {{
        }}


        public static QueryExpressionFactory Factory
        {{
            get {{ return factory ?? (factory = new {0}Factory()); }}
        }}

        public {3} {1}
        {{
            get {{ return Arguments[1]; }}
        }}


        public {7} {5}
        {{
            get {{ return Arguments[2]; }}
        }}


        public static {0}Expression Create(QueryExpression source, {3} {2}, {7} {6})
        {{
            return factory.Create(source, {2}, {6});
        }}


        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {{
            var origSource = Source;
            var orig{1} = {1};
            var visitedSource = visitor.VisitAndConvert(origSource, ""VisitChildren"");
            var visited{1} = visitor.VisitAndConvert(orig{1}, ""VisitChildren"");
            if (visitedSource != origSource || visited{1} != orig{1})
                return Create(visitedSource, visited{1});
            return this;
        }}

        #region Nested type: {0}Factory

        private class {0}Factory : QueryChainedExpressionFactory<{0}Expression>
        {{
            public {0}Expression Create(QueryExpression source, {3} {2}, {7} {6})
            {{
                if (source == null)
                    throw new ArgumentNullException(""source"");
                if ({2} == null)
                    throw new ArgumentNullException(""{2}"");
                if ({6} == null)
                    throw new ArgumentNullException(""{6}"");
                return new {0}Expression(Call(Method.MakeGenericMethod(source.ElementType),
                                                source.Node,
                                                ConvertAndQuote({2}, source.ElementType)),
                                                ConvertAndQuote({6}, source.ElementType)),
                                           source);
            }}
        }}

        #endregion
    }}
";

        protected QuerySourceExpression source =
            (QuerySourceExpression)QueryExpression.Wrap(Enumerable.Empty<Dummy>().AsQueryable().Expression);

        private static List<Type> QueryExpressionTypes
        {
            get
            {
                return typeof(QueryExpression).Assembly
                                              .GetTypes()
                                              .Where(x => typeof(QueryExpression).IsAssignableFrom(x) && x.IsPublic && !x.IsAbstract)
                                              .ToList();
            }
        }


        [Test]
        public void All_Query_Expressions_Types_Has_Factory_Property_Exposed()
        {
            StringBuilder errorLog = new StringBuilder();
            foreach (var type in QueryExpressionTypes)
            {
                var factoryProperty = type.GetProperty("Factory",
                                                       BindingFlags.Public | BindingFlags.Static
                                                       | BindingFlags.DeclaredOnly);
                if (factoryProperty == null)
                {
                    errorLog.AppendFormat("{0} is missing required static Factory property.\r\n", type);
                    continue;
                }
                if (factoryProperty.PropertyType != typeof(QueryExpressionFactory))
                {
                    errorLog.AppendFormat("Factory property of {0} is of type {1}, should be of type {2}\r\n",
                                          factoryProperty.PropertyType,
                                          typeof(QueryExpressionFactory));
                }
            }

            Assert.That(errorLog.Length == 0, errorLog.ToString());
        }


        [Test]
        public void All_Query_Expressions_Types_Has_Matching_Extension_Method()
        {
            StringBuilder errorLog = new StringBuilder();

            var extensionMethods =
                typeof(QueryExpressionExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(
                    x => x.HasAttribute<ExtensionAttribute>(false)).ToList();

            foreach (var type in QueryExpressionTypes.Where(x => typeof(QueryChainedExpression).IsAssignableFrom(x)))
            {
                if (!type.Name.EndsWith("Expression"))
                {
                    errorLog.AppendFormat("Wrong naming convention of {0}, name should end with Expression\r\n", type);
                    continue;
                }
                var extMethodName = type.Name.Substring(0,
                                                        type.Name.LastIndexOf("Expression",
                                                                              StringComparison.InvariantCulture));
                var matchingMethod =
                    extensionMethods.FirstOrDefault(x => x.Name == extMethodName &&
                                                         x.GetParameters().Length >= 1
                                                         && x.GetParameters()[0].ParameterType
                                                         == typeof(QueryExpression));
                if (matchingMethod == null)
                {
                    errorLog.AppendFormat("No extension method named {0} to create {1} on a given source.\r\n",
                                          extMethodName,
                                          type);
                    continue;
                }

                if (matchingMethod.ReturnType != type)
                    errorLog.AppendFormat("Return type of extension method {0} is not {1}\r\n", extMethodName, type);
            }

            Assert.That(errorLog.Length == 0, errorLog.ToString());
        }


        [Test]
        public void All_Query_Expressions_Types_Has_Only_Private_Constructors()
        {
            StringBuilder errorLog = new StringBuilder();
            foreach (var type in QueryExpressionTypes)
            {
                var ctors =
                    type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();
                if (ctors.Any(x => !x.IsPrivate))
                {
                    errorLog.AppendFormat("{0} has {1} constructors, is required to have exactly one private.\r\n",
                                          type,
                                          ctors.Count);
                }
            }

            Assert.That(errorLog.Length == 0, errorLog.ToString());
        }


        [Test]
        public void Create_DefaultIfEmptyExpression_Successful()
        {
            var defaultIfEmpty = this.source.DefaultIfEmpty();
            Assert.That(defaultIfEmpty.Source, Is.EqualTo(this.source));
        }


        [Test]
        public void Create_DistinctExpression_Successful()
        {
            var distinct = this.source.Distinct();
            Assert.That(distinct.Source, Is.EqualTo(this.source));
        }


        [Test]
        public void Create_GroupByExpression_With_Valid_KeySelector_Is_Successful()
        {
            Expression<Func<Dummy, int>> keySelector = x => x.Id;
            var where = this.source.GroupBy(keySelector);
            Assert.That(where.KeySelector, Is.EqualTo(keySelector));
            Assert.That(where.Source, Is.EqualTo(this.source));
        }


        [Test]
        public void Create_OfTypeExpression_Successful()
        {
            var ofType = this.source.OfType(typeof(IDisposable));
            Assert.That(ofType.ElementType, Is.EqualTo(typeof(IDisposable)));
            Assert.That(ofType.Source, Is.EqualTo(this.source));
        }


        [Test]
        public void Create_SelectExpression_With_Valid_Selector_Is_Successful()
        {
            Expression<Func<Dummy, int>> selector = x => x.Id;
            var select = this.source.Select(selector);
            Assert.That(select.Selector, Is.EqualTo(selector));
            Assert.That(select.Source, Is.EqualTo(this.source));
        }


        [Test]
        public void Create_SelectManyExpression_With_Valid_Selector_Is_Successful()
        {
            Expression<Func<Dummy, IEnumerable<Dummy>>> selector = x => new[] { x };
            var select = this.source.SelectMany(selector);
            Assert.That(select.Selector, Is.EqualTo(selector));
            Assert.That(select.Source, Is.EqualTo(this.source));
            Assert.That(select.ElementType, Is.EqualTo(typeof(Dummy)));
        }


        [Test]
        public void Create_SkipExpression_Successful()
        {
            var skip = this.source.Skip(1234);
            Assert.That(skip.Source, Is.EqualTo(this.source));
            Assert.That(skip.Count, Is.EqualTo(1234));
        }


        [Test]
        public void Create_TakeExpression_Successful()
        {
            var take = this.source.Take(1234);
            Assert.That(take.Source, Is.EqualTo(this.source));
            Assert.That(take.Count, Is.EqualTo(1234));
        }


        [Test]
        public void Create_WhereExpression_With_Valid_Predicate_Is_Successful()
        {
            Expression<Func<Dummy, bool>> predicate = x => x.Id == 0x1337;
            var where = this.source.Where(predicate);
            Assert.That(where.Predicate, Is.EqualTo(predicate));
            Assert.That(where.Source, Is.EqualTo(this.source));
        }


        [Test]
        public void Create_ZipExpression_With_Valid_Source_And_Selector_Is_Successful()
        {
            Expression<Func<Dummy, object, int>> resultSelector = (a, b) => a.Id + b.GetHashCode();
            var source2 = QueryExpression.Wrap(Enumerable.Empty<object>().AsQueryable().Expression);
            var select = this.source.Zip(source2, resultSelector);
            Assert.That(select.ResultSelector, Is.EqualTo(resultSelector));
            Assert.That(select.Source, Is.EqualTo(this.source));
            Assert.That(select.Source2, Is.EqualTo(source2));
        }


        [Test]
        public void GenerateQueryExpressionCode()
        {
            var queryExtMethods = from m in typeof(Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                                  let p = m.GetParameters()
                                  let name = GetNameForMethod(m, p)
                                  let exprType = QueryExpressionTypes.FirstOrDefault(x => x.Name == name + "Expression")
                                  where
                                      m.ReturnType.IsGenericInstanceOf(typeof(IQueryable<>)) &&
                                      m.HasAttribute<ExtensionAttribute>(false) &&
                                      p.Length == 2 &&
                                      typeof(IQueryable).IsAssignableFrom(p[0].ParameterType)
                                  select new { name, method = m, parms = p, exprType };

            foreach (var m in queryExtMethods.Where(x => x.exprType == null))
            {
                var p1type = m.parms[1].ParameterType;
                if (typeof(LambdaExpression).IsAssignableFrom(p1type))
                    p1type = typeof(LambdaExpression);
                Console.WriteLine(this.queryExpressionCodeTemplate,
                                  m.name,
                                  m.parms[1].Name.CapitalizeFirstLetter(),
                                  m.parms[1].Name,
                                  p1type,
                                  m.method.Name);
            }
        }


        [Test]
        public void GenerateQueryExpressionCodeP1()
        {
            var queryExtMethods = from m in typeof(Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                                  let p = m.GetParameters()
                                  let name = GetNameForMethod(m, p)
                                  let exprType = QueryExpressionTypes.FirstOrDefault(x => x.Name == name + "Expression")
                                  where
                                      m.ReturnType.IsGenericInstanceOf(typeof(IQueryable<>)) &&
                                      m.HasAttribute<ExtensionAttribute>(false) &&
                                      p.Length == 1 &&
                                      typeof(IQueryable).IsAssignableFrom(p[0].ParameterType)
                                  select new { name, method = m, parms = p, exprType };

            foreach (var m in queryExtMethods.Where(x => x.exprType == null))
                Console.WriteLine(m.name);
        }


        [Test]
        public void GenerateQueryExpressionCodeP3()
        {
            var queryExtMethods = from m in typeof(Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                                  let p = m.GetParameters()
                                  let name = GetNameForMethod(m, p)
                                  let exprType = QueryExpressionTypes.FirstOrDefault(x => x.Name == name + "Expression")
                                  where
                                      m.ReturnType.IsGenericInstanceOf(typeof(IQueryable<>)) &&
                                      m.HasAttribute<ExtensionAttribute>(false) &&
                                      p.Length == 3 &&
                                      typeof(IQueryable).IsAssignableFrom(p[0].ParameterType)
                                  select new { name, method = m, parms = p, exprType };

            foreach (var m in queryExtMethods.Where(x => x.exprType == null))
            {
                var mp = (from p in m.parms.Skip(1)
                          let exposedType = GetExposedTypeTEMP(p)
                          let propName = p.Name.CapitalizeFirstLetter()
                          select new { exposedType, propName, par = p }).ToList();

                Console.WriteLine(this.queryExpressionCodeTemplate3,
                                  m.name,
                                  mp[0].propName,
                                  mp[0].par.Name,
                                  mp[0].exposedType,
                                  m.method.Name,
                                  mp[1].propName,
                                  mp[1].par.Name,
                                  mp[1].exposedType
                    );
            }
        }


        [Test]
        public void Wrap_DefaultIfEmpty_MethodCallExpression_Returns_DefaultIfEmptyExpression()
        {
            WrapAndAssert<DefaultIfEmptyExpression>(q => q.DefaultIfEmpty());
        }


        [Test]
        public void Wrap_Distinct_MethodCallExpression_Returns_DistinctExpression()
        {
            WrapAndAssert<DistinctExpression>(q => q.Distinct());
        }


        [Test]
        public void Wrap_GroupBy_MethodCallExpression_Returns_GroupByExpression()
        {
            WrapAndAssert<GroupByExpression>(q => q.GroupBy(x => x.Id));
        }


        [Test]
        public void Wrap_OfType_MethodCallExpression_Returns_OfTypeExpression()
        {
            WrapAndAssert<OfTypeExpression>(q => q.OfType<IDisposable>());
        }


        [Test]
        public void Wrap_Select_MethodCallExpression_Returns_SelectExpression()
        {
            WrapAndAssert<SelectExpression>(q => q.Select(x => x.Id));
        }


        [Test]
        public void Wrap_SelectMany_MethodCallExpression_Returns_SelectManyExpression()
        {
            WrapAndAssert<SelectManyExpression>(q => q.SelectMany(x => Enumerable.Range(0, x.Id)));
        }


        [Test]
        public void Wrap_Skip_MethodCallExpression_Returns_SkipExpression()
        {
            WrapAndAssert<SkipExpression>(q => q.Skip(1234));
        }


        [Test]
        public void Wrap_Take_MethodCallExpression_Returns_TakeExpression()
        {
            WrapAndAssert<TakeExpression>(q => q.Take(1234));
        }


        [Test]
        public void Wrap_Where_MethodCallExpression_Returns_WhereExpression()
        {
            WrapAndAssert<WhereExpression>(q => q.Where(x => x.Id == 1234));
        }


        [Category("TODO")]
        [Test]
        public void Wrap_Zip_MethodCallExpression_HavingNonQueryableSecondSource_Returns_ZipExpression()
        {
            var zipExpr = WrapAndAssert<ZipExpression>(q => q.Zip(Enumerable.Empty<object>(), (a, b) => a));
            var source2 = zipExpr.Source2;
            Assert.Fail(
                "Known to not work since IEnumerable constant can not be converted to QueryExpression. Don't know if we need to support this?");
        }


        [Test]
        public void Wrap_Zip_MethodCallExpression_Returns_ZipExpression()
        {
            WrapAndAssert<ZipExpression>(q => q.Zip(Enumerable.Empty<object>().AsQueryable(), (a, b) => a));
        }


        private Type GetExposedTypeTEMP(ParameterInfo pi)
        {
            if (typeof(LambdaExpression).IsAssignableFrom(pi.ParameterType))
                return typeof(LambdaExpression);
            if (typeof(IEnumerable<>).IsGenericInstanceOf(pi.ParameterType))
                return typeof(QueryExpression);
            return pi.ParameterType;
        }


        private string GetNameForMethod(MethodInfo m, ParameterInfo[] parms)
        {
            var lamParams =
                from p in parms.Skip(1).Take(1)
                where p.ParameterType.IsGenericInstanceOf(typeof(Expression<>))
                let invokeMethod = p.ParameterType.GetGenericArguments()[0].GetDelegateInvokeMethod()
                where invokeMethod != null
                let indexArgParam = invokeMethod.GetParameters().Select(x => x.ParameterType).ElementAtOrDefault(1)
                where indexArgParam != null
                select indexArgParam;

            if (lamParams.Any())
                return m.Name + "Indexed";
            return m.Name;
        }


        private TQueryExpression WrapAndAssert<TQueryExpression>(Func<IQueryable<Dummy>, IQueryable> queryBuildAction)
            where TQueryExpression : QueryExpression
        {
            if (queryBuildAction == null)
                throw new ArgumentNullException("queryBuildAction");
            var queryExpr = queryBuildAction(Enumerable.Empty<Dummy>().AsQueryable()).Expression;
            var wrapper = QueryExpression.Wrap(queryExpr);
            Assert.That(wrapper, Is.InstanceOf<TQueryExpression>());
            return (TQueryExpression)wrapper;
        }


        public class Dummy
        {
            public int Id { get; set; }
        }
    }
}