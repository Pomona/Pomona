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

using Pomona.Common;
using Pomona.Common.TypeSystem;

namespace Pomona
{
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


        public override HttpMethod AllowedMethods
        {
            get { return ItemResourceType.AllowedMethods; }
        }

        public ResourceType ItemResourceType
        {
            get { return (ResourceType)this.collectionType.ElementType; }
        }

        public override object Value
        {
            get { return this.value; }
        }

        protected IMappedType ItemIdType
        {
            get { return ItemResourceType.PrimaryId.PropertyType; }
        }

        protected internal override IMappedType ExpectedPostType
        {
            get { return ItemResourceType; }
        }


        public IQueryable GetAsQueryable(IMappedType ofType = null)
        {
            return GetQueryableResolver().Resolve(this, ofType);
        }


        protected override IMappedType OnGetType()
        {
            return this.collectionType;
        }


        protected object ParseId(string id)
        {
            return Convert.ChangeType(id, ItemIdType.MappedTypeInstance);
        }
    }
}