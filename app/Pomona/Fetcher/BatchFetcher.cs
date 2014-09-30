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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Internals;

namespace Pomona.Fetcher
{
    public class BatchFetcher
    {
        private static readonly MethodInfo containsMethod =
            ReflectionHelper.GetMethodDefinition<IEnumerable<object>>(x => x.Contains(null));

        private static readonly MethodInfo expandCollectionBatchedMethod =
            ReflectionHelper.GetMethodDefinition<BatchFetcher>(
                x => x.ExpandCollectionBatched<object, object>(null, null, null));

        private static readonly MethodInfo expandManyToOneMethod =
            ReflectionHelper.GetMethodDefinition<BatchFetcher>(
                x => x.ExpandManyToOne<object, object>(null, null, null));

        private static readonly Action<Type, BatchFetcher, object, string> expandMethod =
            GenericInvoker.Instance<BatchFetcher>().CreateAction1<object, string>(x => x.Expand<object>(null, null));

        private readonly int batchFetchCount;
        private readonly IBatchFetchDriver driver;

        private readonly HashSet<string> expandedPaths;

        private readonly Func<Type, Type, BatchFetcher, object[], PropertyInfo, IEnumerable<object>>
            fetchEntitiesByIdInBatches =
                GenericInvoker.Instance<BatchFetcher>().CreateFunc2<object[], PropertyInfo, IEnumerable<object>>(
                    x => x.FetchEntitiesByIdInBatches<object, object>(null, null));


        public BatchFetcher(IBatchFetchDriver driver, string expandedPaths)
            : this(driver, expandedPaths, 100)
        {
        }


        public BatchFetcher(IBatchFetchDriver driver, string expandedPaths, int batchFetchCount)
        {
            this.expandedPaths = ExpandPathsUtils.GetExpandedPaths(expandedPaths);
            this.driver = driver;
            this.batchFetchCount = batchFetchCount;
        }


        public int BatchFetchCount
        {
            get { return this.batchFetchCount; }
        }


        public void Expand(object entitiesUncast, Type entityType)
        {
            var entities = entitiesUncast as IEnumerable;
            if (entities == null)
            {
                if (entitiesUncast == null)
                    throw new InvalidOperationException("Unexpected entity type sent to Expand method.");
                entities = new[] { entitiesUncast };
            }

            Expand(entities);
        }


        private void Expand(IEnumerable entitiesUncast, string path = "")
        {
            var entitiesGroupedByType =
                entitiesUncast
                    .Cast<object>()
                    .Where(x => x != null)
                    .GroupBy(x => x.GetType())
                    .ToList();

            entitiesGroupedByType.ForEach(x => expandMethod(x.Key, this, x.Cast(x.Key), path));
        }

        public void Expand<TEntity>(IEnumerable<TEntity> entities, string path = "")
            where TEntity : class
        {
            foreach (var prop in this.driver.GetProperties(typeof(TEntity)))
            {
                var subPath = string.IsNullOrEmpty(path) ? prop.Name : path + "." + prop.Name;
                if (this.expandedPaths.Contains(subPath.ToLower()) || this.driver.PathIsExpanded(subPath, prop))
                {
                    if (IsManyToOne(prop))
                    {
                        expandManyToOneMethod.MakeGenericMethod(typeof(TEntity), prop.PropertyType)
                            .Invoke(this, new object[] { entities, subPath, prop });
                    }
                    else
                    {
                        Type elementType;
                        if (IsCollection(prop, out elementType))
                        {
                            expandCollectionBatchedMethod.MakeGenericMethod(typeof(TEntity), elementType)
                                .Invoke(this, new object[] { entities, subPath, prop });
                        }
                    }
                }
            }
        }


        protected virtual IEnumerable<TEntity> FetchEntitiesById<TEntity, TId>(TId[] ids, PropertyInfo idProp)
        {
            if (idProp == null)
                throw new ArgumentNullException("idProp");
            var fetchPredicateParam = Expression.Parameter(typeof(TEntity), "x");
            var fetchPredicate = Expression.Lambda<Func<TEntity, bool>>(
                Expression.Call(
                    containsMethod.MakeGenericMethod(idProp.PropertyType),
                    Expression.Constant(ids),
                    Expression.Property(fetchPredicateParam, idProp)),
                fetchPredicateParam);
            var results = this.driver.Query<TEntity>()
                .Where(fetchPredicate)
                .ToList();
            return results;
        }


        private static IEnumerable<T[]> Partition<T>(IEnumerable<T> source, int partLength)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (partLength < 1)
                throw new ArgumentOutOfRangeException("partLength", "Can't divide sequence in parts less than length 1");
            T[] part = null;
            var offset = 0;
            foreach (var item in source)
            {
                if (offset >= partLength)
                {
                    yield return part;
                    offset = 0;
                    part = null;
                }

                if (part == null)
                    part = new T[partLength];

                part[offset++] = item;
            }

            if (part != null)
            {
                if (offset < partLength)
                    Array.Resize(ref part, offset);
                yield return part;
            }
        }


        private Expression<Func<TEntity, object>> CreateIdGetExpression<TEntity>()
        {
            PropertyInfo tmp;
            return CreateIdGetExpression<TEntity>(out tmp);
        }


        private Expression<Func<TEntity, object>> CreateIdGetExpression<TEntity>(out PropertyInfo idProp)
        {
            idProp = this.driver.GetIdProperty(typeof(TEntity));
            var refParam = Expression.Parameter(typeof(TEntity), "ref");
            var idGetter =
                Expression.Lambda<Func<TEntity, object>>(
                    Expression.Convert(Expression.Property(refParam, idProp), typeof(object)),
                    refParam);
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

            var getChildLambdaParam = Expression.Parameter(typeof(TParentEntity), "z");
            var getChildLambda =
                Expression.Lambda<Func<TParentEntity, IEnumerable<TCollectionElement>>>(
                    Expression.Convert(Expression.Property(getChildLambdaParam, prop),
                        typeof(IEnumerable<TCollectionElement>)),
                    getChildLambdaParam).Compile();

            var parentEntities = entities.Select(x => new { Parent = x, Collection = getChildLambda(x), ParentId = getParentIdExpr(x) }).ToList();

            var parentIdsToFetch =
                parentEntities
                    .Where(x => x.Collection != null && !this.driver.IsLoaded(x.Collection))
                    .Select(x => x.ParentId)
                    .Distinct()
                    .ToArray();

            if (parentIdsToFetch.Length == 0)
                return;

            var containsExprParam = Expression.Parameter(typeof(TParentEntity), "tp");
            var containsExpr =
                Expression.Lambda<Func<TParentEntity, bool>>(
                    Expression.Call(containsMethod.MakeGenericMethod(typeof(object)),
                        Expression.Constant(parentIdsToFetch),
                        Expression.Convert(Expression.Property(containsExprParam,
                            parentIdProp),
                            typeof(object))),
                    containsExprParam);
            //var lineOrderIdMap =
            //    db.Orders
            //    .Where(x => ordersIds.Contains(x.OrderId))
            //    .SelectMany(x => x.OrderLines.Select(y => new ParentChildRelation(x.OrderId, y.Id)))
            //    .ToList();

            var selectManyExprParam = Expression.Parameter(typeof(TParentEntity), "x");
            var selectManyExpr =
                Expression.Lambda<Func<TParentEntity, IEnumerable<TCollectionElement>>>(
                    Expression.Property(selectManyExprParam, prop),
                    selectManyExprParam);

            var selectRelationLeftParam = Expression.Parameter(typeof(TParentEntity), "a");
            var selectRelationRightParam = Expression.Parameter(typeof(TCollectionElement), "b");
            var selectRelation =
                Expression.Lambda<Func<TParentEntity, TCollectionElement, ParentChildRelation>>(Expression.New(
                    typeof(ParentChildRelation).GetConstructors().First(),
                    Expression.Convert(Expression.Property(selectRelationLeftParam, parentIdProp), typeof(object)),
                    Expression.Convert(Expression.Property(selectRelationRightParam, childIdProp), typeof(object))
                    ),
                    selectRelationLeftParam,
                    selectRelationRightParam);

            var relations = this.driver.Query<TParentEntity>()
                .Where(containsExpr)
                .SelectMany(selectManyExpr, selectRelation)
                .ToList();

            var childIdsToFetch = relations.Select(x => x.ChildId).Distinct().ToArray();

            var fetched =
                FetchEntitiesByIdInBatches<TCollectionElement>(childIdsToFetch, childIdProp)
                    .ToDictionary(getChildIdExpr, x => x);

            var bindings =
                parentEntities
                .Join(parentIdsToFetch, x => x.ParentId, x => x, (a,b) => a) // Only bind collections in parentIdsToFetch
                .GroupJoin(relations,
                    x => x.ParentId,
                    x => x.ParentId,
                    (a, b) =>
                        new KeyValuePair<TParentEntity, IEnumerable<TCollectionElement>>
                            (a.Parent, b.Select(y => fetched[y.ChildId])));
            this.driver.PopulateCollections(bindings, prop, typeof(TCollectionElement));

            Expand(parentEntities.SelectMany(x => getChildLambda(x.Parent)), path);
        }


        private void ExpandCollectionBatched<TParentEntity, TCollectionElement>(IEnumerable<TParentEntity> entities,
            string path,
            PropertyInfo prop)
            where TCollectionElement : class
        {
            Partition(entities, this.batchFetchCount).ForEach(
                x => ExpandCollection<TParentEntity, TCollectionElement>(x, path, prop));
        }


        private void ExpandManyToOne<TParentEntity, TReferencedEntity>(IEnumerable<TParentEntity> entities,
            string path,
            PropertyInfo prop)
            where TReferencedEntity : class
        {
            PropertyInfo idProp;
            var idGetter = CreateIdGetExpression<TReferencedEntity>(out idProp).Compile();

            var objectsToWalk = entities
                .Select(x => new { Parent = x, Reference = (TReferencedEntity)prop.GetValue(x, null) })
                .Where(x => x.Reference != null)
                .Select(x => new { x.Parent, x.Reference, RefId = idGetter(x.Reference) })
                .ToList();

            var objectsToExpand = objectsToWalk
                .Where(x => !this.driver.IsLoaded(x.Reference))
                .ToList();

            var ids = objectsToExpand.Select(x => idGetter(x.Reference)).Distinct().ToArray();

            foreach (
                var item in
                    FetchEntitiesByIdInBatches<TReferencedEntity>(ids, idProp)
                        .Join(objectsToExpand, idGetter, x => x.RefId, (a, b) => new { Child = a, b.Parent }))
                prop.SetValue(item.Parent, item.Child, null);

            Expand(objectsToWalk.Select(x => x.Reference), path);
        }


        private IEnumerable FetchEntitiesByIdInBatches<TEntity, TId>(object[] ids, PropertyInfo idProp)
        {
            return
                Partition(ids.Cast<TId>().OrderBy(x => x), this.batchFetchCount).SelectMany(
                    x => FetchEntitiesById<TEntity, TId>(x, idProp));
        }


        private IEnumerable<TEntity> FetchEntitiesByIdInBatches<TEntity>(object[] ids, PropertyInfo idProp)
        {
            return
                ((IEnumerable<TEntity>)
                    this.fetchEntitiesByIdInBatches(typeof(TEntity), idProp.PropertyType, this, ids, idProp)).ToList();
        }


        private bool IsCollection(PropertyInfo prop, out Type elementType)
        {
            elementType = null;
            if (prop.PropertyType == typeof(string))
                return false;

            elementType =
                prop.PropertyType
                    .GetInterfaces()
                    .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .Select(x => x.GetGenericArguments()[0])
                    .FirstOrDefault();

            return elementType != null;
        }


        private bool IsManyToOne(PropertyInfo prop)
        {
            return this.driver.IsManyToOne(prop);
        }
    }
}