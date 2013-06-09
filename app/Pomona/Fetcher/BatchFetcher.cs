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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Pomona.Common.Internals;
using Pomona.Internals;

namespace Pomona.Fetcher
{
    public class BatchFetcher
    {
        private static readonly MethodInfo selectMethod =
            ReflectionHelper.GetMethodDefinition<IEnumerable<object>>(x => x.Select(y => y));

        private static readonly MethodInfo containsMethod =
            ReflectionHelper.GetMethodDefinition<IEnumerable<object>>(x => x.Contains(null));

        private static readonly MethodInfo expandManyToOneMethod =
            ReflectionHelper.GetMethodDefinition<BatchFetcher>(
                x => x.ExpandManyToOne<object, object>(null, null, null));

        private static readonly MethodInfo expandCollectionMethod =
            ReflectionHelper.GetMethodDefinition<BatchFetcher>(
                x => x.ExpandCollection<object, object>(null, null, null));

        private static readonly MethodInfo expandMethod =
            ReflectionHelper.GetMethodDefinition<BatchFetcher>(x => x.Expand<object>(null, null));

        private readonly IBatchFetchDriver driver;
        private readonly HashSet<string> expandedPaths;

        public BatchFetcher(IBatchFetchDriver driver, string expandedPaths)
        {
            this.expandedPaths = ExpandPathsUtils.GetExpandedPaths(expandedPaths);
            this.driver = driver;
        }

        private IEnumerable<TEntity> FetchEntitiesById<TEntity>(IEnumerable<object> ids, PropertyInfo idProp)
        {
            if (idProp == null) throw new ArgumentNullException("idProp");
            var fetchPredicateParam = Expression.Parameter(typeof (TEntity), "x");
            var fetchPredicate = Expression.Lambda<Func<TEntity, bool>>(
                Expression.Call(
                    containsMethod.MakeGenericMethod(typeof (object)),
                    Expression.Constant(ids),
                    Expression.Convert(Expression.Property(fetchPredicateParam, idProp), typeof (object))),
                fetchPredicateParam);
            var results = driver.Query<TEntity>()
                                .Where(fetchPredicate)
                                .ToList();
            return results;
        }

        private void ExpandManyToOne<TParentEntity, TReferencedEntity>(IEnumerable<TParentEntity> entities, string path,
                                                                       PropertyInfo prop)
            where TReferencedEntity : class
        {
            PropertyInfo idProp;
            var idGetter = CreateIdGetExpression<TReferencedEntity>(out idProp).Compile();

            var objectsToWalk = entities
                .Select(x => new {Parent = x, Reference = (TReferencedEntity) prop.GetValue(x, null)})
                .Where(x => x.Reference != null)
                .Select(x => new {x.Parent, x.Reference, RefId = idGetter(x.Reference)})
                .ToList();

            var objectsToExpand = objectsToWalk
                .Where(x => !driver.IsLoaded(x.Reference))
                .ToList();

            var ids = objectsToExpand.Select(x => idGetter(x.Reference)).Distinct().ToArray();

            foreach (
                var item in
                    FetchEntitiesById<TReferencedEntity>(ids, idProp)
                        .Join(objectsToExpand, idGetter, x => x.RefId, (a, b) => new {Child = a, b.Parent}))
            {
                prop.SetValue(item.Parent, item.Child, null);
            }

            Expand<TReferencedEntity>(objectsToWalk.Select(x => x.Reference), path);
        }

        public void Expand(object entity, Type entityType)
        {
            expandMethod.MakeGenericMethod(entityType).Invoke(this, new[] {entity, ""});
        }

        public void Expand<TEntity>(object entitiesUncast, string path = "")
            where TEntity : class
        {
            var entities = entitiesUncast as IEnumerable<TEntity>;
            if (entities == null)
            {
                var singleEntity = entitiesUncast as TEntity;
                if (singleEntity == null)
                    throw new InvalidOperationException("Unexpected entity type sent to Expand method.");
                entities = singleEntity.WrapAsEnumerable();
            }

            foreach (var prop in typeof (TEntity).GetProperties())
            {
                var subPath = string.IsNullOrEmpty(path) ? prop.Name : path + "." + prop.Name;
                if (expandedPaths.Contains(subPath.ToLower()) || driver.PathIsExpanded(subPath, prop))
                {
                    if (IsManyToOne(prop))
                    {
                        expandManyToOneMethod.MakeGenericMethod(typeof (TEntity), prop.PropertyType)
                                             .Invoke(this, new object[] {entities, subPath, prop});
                    }
                    else
                    {
                        Type elementType;
                        if (IsCollection(prop, out elementType))
                        {
                            expandCollectionMethod.MakeGenericMethod(typeof (TEntity), elementType)
                                                  .Invoke(this, new object[] {entities, subPath, prop});
                        }
                    }
                }
            }
        }

        private Expression<Func<TEntity, object>> CreateIdGetExpression<TEntity>()
        {
            PropertyInfo tmp;
            return CreateIdGetExpression<TEntity>(out tmp);
        }

        private Expression<Func<TEntity, object>> CreateIdGetExpression<TEntity>(out PropertyInfo idProp)
        {
            idProp = driver.GetIdProperty(typeof (TEntity));
            var refParam = Expression.Parameter(typeof (TEntity), "ref");
            var idGetter =
                Expression.Lambda<Func<TEntity, object>>(
                    Expression.Convert(Expression.Property(refParam, idProp), typeof (object)), refParam);
            return idGetter;
        }

        private void ExpandCollection<TParentEntity, TCollectionElement>(IEnumerable<TParentEntity> entities,
                                                                         string path,
                                                                         PropertyInfo prop)
            where TCollectionElement : class
        {
            PropertyInfo parentIdProp;
            var getParentIdExpr = CreateIdGetExpression<TParentEntity>(out parentIdProp).Compile();
            PropertyInfo childIdProp;
            var getChildIdExpr = CreateIdGetExpression<TCollectionElement>(out childIdProp).Compile();

            var getChildLambdaParam = Expression.Parameter(typeof (TParentEntity), "z");
            var getChildLambda =
                Expression.Lambda<Func<TParentEntity, IEnumerable<TCollectionElement>>>(
                    Expression.Convert(Expression.Property(getChildLambdaParam, prop),
                                       typeof (IEnumerable<TCollectionElement>)),
                    getChildLambdaParam).Compile();

            var parentEntities = entities.Select(x => new {Parent = x, Collection = getChildLambda(x)}).ToList();

            var parentIdsToFetch =
                parentEntities
                    .Where(x => x.Collection != null && !driver.IsLoaded(x.Collection))
                    .Select(x => getParentIdExpr(x.Parent))
                    .Distinct()
                    .ToArray();

            var containsExprParam = Expression.Parameter(typeof (TParentEntity), "tp");
            var containsExpr =
                Expression.Lambda<Func<TParentEntity, bool>>(
                    Expression.Call(containsMethod.MakeGenericMethod(typeof (object)),
                                    Expression.Constant(parentIdsToFetch),
                                    Expression.Convert(Expression.Property(containsExprParam,
                                                                           parentIdProp), typeof (object))),
                    containsExprParam);
            //var lineOrderIdMap =
            //    db.Orders
            //    .Where(x => ordersIds.Contains(x.OrderId))
            //    .SelectMany(x => x.OrderLines.Select(y => new ParentChildRelation(x.OrderId, y.Id)))
            //    .ToList();

            var selectManyExprParam = Expression.Parameter(typeof (TParentEntity), "x");
            var selectManyExpr =
                Expression.Lambda<Func<TParentEntity, IEnumerable<TCollectionElement>>>(
                    Expression.Property(selectManyExprParam, prop), selectManyExprParam);

            var selectRelationLeftParam = Expression.Parameter(typeof (TParentEntity), "a");
            var selectRelationRightParam = Expression.Parameter(typeof (TCollectionElement), "b");
            var selectRelation =
                Expression.Lambda<Func<TParentEntity, TCollectionElement, ParentChildRelation>>(Expression.New(
                    typeof (ParentChildRelation).GetConstructors().First(),
                    Expression.Convert(Expression.Property(selectRelationLeftParam, parentIdProp), typeof (object)),
                    Expression.Convert(Expression.Property(selectRelationRightParam, childIdProp), typeof (object))
                                                                                                    ),
                                                                                                selectRelationLeftParam,
                                                                                                selectRelationRightParam);

            var relations = driver.Query<TParentEntity>()
                                  .Where(containsExpr)
                                  .SelectMany(selectManyExpr, selectRelation)
                                  .ToList();

            var childIdsToFetch = relations.Select(x => x.ChildId).Distinct().ToArray();

            var fetched =
                FetchEntitiesById<TCollectionElement>(childIdsToFetch, childIdProp)
                    .ToDictionary(getChildIdExpr, x => x);

            var bindings = parentEntities
                .Select(x => x.Parent)
                .GroupJoin(relations, getParentIdExpr, x => x.ParentId,
                           (a, b) =>
                           new KeyValuePair<TParentEntity, IEnumerable<TCollectionElement>>
                               (a, b.Select(y => fetched[y.ChildId])));
            driver.PopulateCollections(bindings, prop, typeof (TCollectionElement));

            Expand<TCollectionElement>(parentEntities.SelectMany(x => getChildLambda(x.Parent)), path);
        }


        private bool IsCollection(PropertyInfo prop, out Type elementType)
        {
            elementType = null;
            if (prop.PropertyType == typeof (string))
                return false;

            elementType =
                prop.PropertyType
                    .GetInterfaces()
                    .Where(x => x.GetGenericTypeDefinition() == typeof (IEnumerable<>))
                    .Select(x => x.GetGenericArguments()[0])
                    .FirstOrDefault();

            return elementType != null;
        }

        private bool IsManyToOne(PropertyInfo prop)
        {
            return driver.IsManyToOne(prop);
        }
    }
}