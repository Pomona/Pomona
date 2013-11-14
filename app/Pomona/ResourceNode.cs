#region License

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

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

using Nancy;

using Pomona.Common;
using Pomona.Common.TypeSystem;
using Pomona.Internals;
using Pomona.Queries;

namespace Pomona
{
    public class DefaultGetRequestProcessor : IPomonaRequestProcessor
    {
        public PomonaResponse Process(PomonaRequest request)
        {
            if (request.Method != HttpMethod.Get)
                return null;

            var queryableNode = request.Node as QueryableNode;
            if (queryableNode != null)
            {
                return request.ParseQuery().ApplyAndExecute(queryableNode.GetAsQueryable());
            }
            var resourceNode = request.Node as ResourceNode;
            if (resourceNode != null)
            {
                return new PomonaResponse(resourceNode.Value, expandedPaths: request.ExpandedPaths);
            }
            return null;
        }
    }

    public interface IPomonaRequestProcessor
    {
        PomonaResponse Process(PomonaRequest request);
    }

    public class PomonaRequest
    {
        private readonly PathNode node;
        private readonly NancyContext context;

        public Request NancyRequest {
            get { return context.Request; }
        }

        public HttpMethod Method
        {
            get { return (HttpMethod)Enum.Parse(typeof(HttpMethod), NancyRequest.Method, true); }
        }

        public string ExpandedPaths
        {
            get { return NancyRequest.Query["$expand"].HasValue ? NancyRequest.Query["$expand"] : string.Empty; }
        }

        public PomonaQuery ParseQuery()
        {
            var queryableNode = node as QueryableNode;
            if (queryableNode == null)
                throw new InvalidOperationException("Queries are only valid for Queryable nodes.");

            return
                new PomonaHttpQueryTransformer(node.TypeMapper,
                    new QueryExpressionParser(new QueryTypeResolver(node.TypeMapper))).TransformRequest(context,
                        queryableNode.ItemResourceType);
        }

        public PomonaRequest(PathNode node, NancyContext context)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            if (context == null)
                throw new ArgumentNullException("context");
            this.node = node;
            this.context = context;
        }


        public PathNode Node { get { return node; } }
    }

    public interface IQueryableResolver
    {
        /// <summary>
        /// Get the QueryableNode as IQueryable
        /// </summary>
        /// <param name="node">The node to get corresponding IQueryable for.</param>
        /// <returns>The resulting IQueryable if success, null if not.</returns>
        IQueryable Resolve(QueryableNode node);
    }

    public abstract class QueryableResolverBase : IQueryableResolver
    {
        private static MethodInfo resolveMethod =
            ReflectionHelper.GetMethodDefinition<QueryableResolverBase>(x => x.Resolve<object>(null));


        public virtual IQueryable Resolve(QueryableNode node)
        {
            return
                (IQueryable)
                    resolveMethod.MakeGenericMethod(node.ItemResourceType.MappedTypeInstance).Invoke(this,
                        new object[] { node });
        }


        protected abstract IQueryable<TResource> Resolve<TResource>(QueryableNode<TResource> node)
            where TResource : class;
    }

    public class DataSourceQueryableResolver : QueryableResolverBase
    {
        private IPomonaDataSource dataSource;


        public DataSourceQueryableResolver(IPomonaDataSource dataSource)
        {
            if (dataSource == null)
                throw new ArgumentNullException("dataSource");
            this.dataSource = dataSource;
        }


        protected override IQueryable<TResource> Resolve<TResource>(QueryableNode<TResource> node)
        {
            if (node.Value is IQueryable<TResource>)
                return (IQueryable<TResource>)node.Value;

            var itemResourceType = node.ItemResourceType;

            var queryable = this.dataSource.Query<TResource>();

            if (itemResourceType.IsRootResource)
                return queryable;

            return GetByParentId(node, itemResourceType, queryable);
        }


        private static IQueryable<TResource> GetByParentId<TResource>(QueryableNode<TResource> node,
            ResourceType itemResourceType,
            IQueryable<TResource> queryable)
        {
            if (((ResourceType)node.Parent.Type).UriBaseType != itemResourceType.ParentResourceType)
                throw new NotImplementedException("Unable to get by parent id for non-owned child-resources.");

            // Do query like this: dataSource.Query<TResource>().Where(x => x.Parent.Id == parentId);
            var parentIdProperty = itemResourceType.ParentResourceType.PrimaryId;
            var parentId = parentIdProperty.Getter(node.Parent.Value);
            var predicateParam = Expression.Parameter(typeof(TResource));
            var predicate =
                Expression.Lambda<Func<TResource, bool>>(
                    Expression.Equal(
                        parentIdProperty.CreateGetterExpression(
                            itemResourceType.ChildToParentProperty.CreateGetterExpression(predicateParam)),
                        Expression.Constant(parentId, parentIdProperty.PropertyType.MappedTypeInstance)),
                    predicateParam);
            return queryable.Where(predicate);
        }
    }

    public class DefaultQueryableResolver : IQueryableResolver
    {
        public IQueryable Resolve(QueryableNode node)
        {
            return ((IEnumerable)node.Value).AsQueryable();
        }
    }

    public class ResourceLocator
    {
        public ResourceNode GetResourceAt(Request nancyRequest)
        {
            if (nancyRequest == null)
                throw new ArgumentNullException("nancyRequest");

            throw new NotImplementedException();
        }
    }

    public class DictionaryResourceNode
    {
    }

    public class DataSourceRootNode : PathNode
    {
        private static MethodInfo queryMethod =
            ReflectionHelper.GetMethodDefinition<IPomonaDataSource>(x => x.Query<object>());

        private readonly IPomonaDataSource dataSource;


        public DataSourceRootNode(ITypeMapper typeMapper, IPomonaDataSource dataSource)
            : base(typeMapper, null, "/")
        {
            if (dataSource == null)
                throw new ArgumentNullException("dataSource");
            this.dataSource = dataSource;
        }


        public override IMappedType Type
        {
            get { return null; }
        }

        public override object Value
        {
            get { return this.dataSource; }
        }


        public override PathNode GetChildNode(string name)
        {
            var type = ((TypeMapper)TypeMapper).TransformedTypes.OfType<ResourceType>().FirstOrDefault(
                x =>
                    x.IsUriBaseType && x.IsRootResource
                    && string.Equals(x.UriRelativePath, name, StringComparison.InvariantCultureIgnoreCase));
            var queryable = queryMethod.MakeGenericMethod(type.MappedTypeInstance).Invoke(this.dataSource, null);
            return CreateNode(TypeMapper, this, name, queryable, type);
        }


        protected override IQueryableResolver GetQueryableResolver()
        {
            return new DataSourceQueryableResolver(this.dataSource);
        }
    }

    public class QueryableNode<TItem, TId> : QueryableNode<TItem>
        where TItem : class
    {
        public QueryableNode(ITypeMapper typeMapper,
            PathNode parent,
            string name,
            IEnumerable<TItem> value,
            IMappedType collectionType)
            : base(typeMapper, parent, name, value, collectionType)
        {
        }


        public override PathNode GetChildNode(string name)
        {
            if (ItemResourceType.PrimaryId == null)
                throw new ArgumentException("Resource in collection needs to have a primary id.");

            var id = (TId)ParseId(name);
            var predicateParam = Expression.Parameter(typeof(TItem));
            var predicate = Expression.Lambda<Func<TItem, bool>>(
                Expression.Equal(ItemResourceType.PrimaryId.CreateGetterExpression(predicateParam),
                    Expression.Constant(id)),
                predicateParam);
            var queryable = ((IQueryable<TItem>)GetAsQueryable()).Where(predicate);
            var result = queryable.FirstOrDefault();
            if (result == null)
                throw new ResourceNotFoundException("Resource not found");

            return CreateNode(TypeMapper, this, name, result, ItemResourceType);
        }
    }

    public abstract class QueryableNode<T> : QueryableNode
    {
        protected QueryableNode(ITypeMapper typeMapper,
            PathNode parent,
            string name,
            IEnumerable value,
            IMappedType collectionType)
            : base(typeMapper, parent, name, value, collectionType)
        {
        }
    }

    public abstract class QueryableNode : PathNode
    {
        private readonly IMappedType collectionType;
        private readonly IEnumerable value;


        protected QueryableNode(ITypeMapper typeMapper,
            PathNode parent,
            string name,
            IEnumerable value,
            IMappedType collectionType)
            : base(typeMapper, parent, name)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (collectionType == null)
                throw new ArgumentNullException("collectionType");

            if (!collectionType.IsCollection || !(collectionType.ElementType is ResourceType))
                throw new ArgumentException("Need to be collection of resources.", "collectionType");

            this.value = value;
            this.collectionType = collectionType;
        }


        public ResourceType ItemResourceType
        {
            get { return (ResourceType)this.collectionType.ElementType; }
        }

        public override IMappedType Type
        {
            get { return this.collectionType; }
        }

        public override object Value
        {
            get { return this.value; }
        }

        protected IMappedType ItemIdType
        {
            get { return ItemResourceType.PrimaryId.PropertyType; }
        }


        public IQueryable GetAsQueryable()
        {
            return GetQueryableResolver().Resolve(this);
        }


        protected object ParseId(string id)
        {
            return Convert.ChangeType(id, ItemIdType.MappedTypeInstance);
        }
    }

    public class ResourceNode : PathNode
    {
        private readonly ResourceType type;
        private readonly object value;


        public ResourceNode(ITypeMapper typeMapper, PathNode parent, string name, object value, ResourceType type)
            : base(typeMapper, parent, name)
        {
            this.value = value;
            this.type = type;
        }


        public override IMappedType Type
        {
            get { return this.type; }
        }

        public override object Value
        {
            get { return this.value; }
        }


        public override PathNode GetChildNode(string name)
        {
            IPropertyInfo property;
            if (!Type.TryGetPropertyByUriName(name, out property))
                throw new ResourceNotFoundException("Resource not found");

            var value = property.Getter(Value);
            return CreateNode(TypeMapper, this, name, value, property.PropertyType);
        }
    }

    public abstract class PathNode
    {
        private readonly string name;
        private readonly PathNode parent;
        private readonly ITypeMapper typeMapper;


        protected PathNode(ITypeMapper typeMapper, PathNode parent, string name)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            if (name == null)
                throw new ArgumentNullException("name");
            this.typeMapper = typeMapper;
            this.parent = parent;
            this.name = name;
        }


        public string Name
        {
            get { return this.name; }
        }

        public PathNode Parent
        {
            get { return this.parent; }
        }

        public abstract IMappedType Type { get; }
        public abstract object Value { get; }

        public ITypeMapper TypeMapper
        {
            get { return this.typeMapper; }
        }

        public abstract PathNode GetChildNode(string name);


        protected static PathNode CreateNode(ITypeMapper typeMapper,
            PathNode parent,
            string name,
            object value,
            IMappedType expectedType)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            var actualType = value == null ? expectedType : typeMapper.GetClassMapping(value.GetType());
            if (actualType is ResourceType)
                return new ResourceNode(typeMapper, parent, name, value, (ResourceType)actualType);
            if (actualType.IsCollection && actualType.ElementType is ResourceType)
            {
                var elementType = (ResourceType)actualType.ElementType;

                if (elementType.PrimaryId == null)
                {
                    throw new InvalidOperationException(
                        "Unable to create queryable of resource type without PrimaryId set.");
                }

                var idType = elementType.PrimaryId.PropertyType.MappedTypeInstance;
                return (PathNode)Activator.CreateInstance(
                    typeof(QueryableNode<,>).MakeGenericType(new[] { elementType.MappedTypeInstance, idType }),
                    typeMapper,
                    parent,
                    name,
                    value,
                    actualType);
            }

            throw new NotImplementedException();
        }


        protected virtual IQueryableResolver GetQueryableResolver()
        {
            if (Parent != null)
                return Parent.GetQueryableResolver();
            return new DefaultQueryableResolver();
        }
    }
}