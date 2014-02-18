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
using System.Linq;
using System.Linq.Expressions;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class QueryableNode<TItem, TId> : QueryableNode<TItem>
        where TItem : class
    {
        public QueryableNode(ITypeMapper typeMapper,
            PathNode parent,
            string name,
            Func<object> valueFetcher,
            TypeSpec collectionType)
            : base(typeMapper, parent, name, valueFetcher, collectionType)
        {
        }


        public override bool Exists
        {
            get { return true; }
        }


        public override PathNode GetChildNode(string name)
        {
            if (ItemResourceType.PrimaryId == null)
                throw new ArgumentException("Resource in collection needs to have a primary id.");

            return CreateNode(TypeMapper, this, name, () => GetChildNodeFromQueryableById(name), ItemResourceType);
        }


        private TItem GetChildNodeFromQueryableById(string name)
        {
            var id = (TId)ParseId(name);
            var predicateParam = Expression.Parameter(typeof(TItem));
            var predicate = Expression.Lambda<Func<TItem, bool>>(
                Expression.Equal(ItemResourceType.PrimaryId.CreateGetterExpression(predicateParam),
                    Expression.Constant(id)),
                predicateParam);
            var queryable = ((IQueryable<TItem>)GetAsQueryable()).EmptyIfNull().Where(predicate);
            
            var result = queryable.FirstOrDefault();
            return result;
        }
    }

    public abstract class QueryableNode<T> : QueryableNode
    {
        protected QueryableNode(ITypeMapper typeMapper,
            PathNode parent,
            string name,
            Func<object> valueFetcher,
            TypeSpec collectionType)
            : base(typeMapper, parent, name, valueFetcher, collectionType)
        {
        }
    }

    /// <summary>
    /// Node representing an URI segment with a collection of resources.
    /// TODO: Rename this to CollectionNode?
    /// </summary>
    public abstract class QueryableNode : PathNode
    {
        private readonly TypeSpec collectionType;
        private readonly System.Lazy<object> valueLazy;


        protected QueryableNode(ITypeMapper typeMapper,
            PathNode parent,
            string name,
            Func<object> valueFetcher,
            TypeSpec collectionType)
            : base(typeMapper, parent, name)
        {
            if (valueFetcher == null)
                throw new ArgumentNullException("valueFetcher");
            if (collectionType == null)
                throw new ArgumentNullException("collectionType");

            if (!collectionType.IsCollection || !(collectionType.ElementType is ResourceType))
                throw new ArgumentException("Need to be collection of resources.", "collectionType");

            this.valueLazy = new System.Lazy<object>(valueFetcher);
            this.collectionType = collectionType;
        }


        public override HttpMethod AllowedMethods
        {
            get { return ItemResourceType.AllowedMethods; }
        }

        public override bool IsLoaded
        {
            get { return this.valueLazy.IsValueCreated; }
        }

        public ResourceType ItemResourceType
        {
            get { return (ResourceType)this.collectionType.ElementType; }
        }

        public override object Value
        {
            get { return this.valueLazy.Value; }
        }

        protected TypeSpec ItemIdType
        {
            get { return ItemResourceType.PrimaryId.PropertyType; }
        }

        protected internal override TypeSpec ExpectedPostType
        {
            get { return ItemResourceType; }
        }


        public IQueryable GetAsQueryable(TypeSpec ofType = null)
        {
            return GetQueryableResolver().Resolve(this, ofType);
        }


        protected override TypeSpec OnGetType()
        {
            return this.collectionType;
        }


        protected object ParseId(string id)
        {
            return Convert.ChangeType(id, ItemIdType.Type);
        }
    }
}