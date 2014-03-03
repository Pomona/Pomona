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
using System.Linq;
using System.Linq.Expressions;

using Pomona.Common.TypeSystem;

namespace Pomona.Queries
{
    public class DataSourceQueryableResolver : QueryableResolverBase
    {
        private readonly IPomonaDataSource dataSource;


        public DataSourceQueryableResolver(IPomonaDataSource dataSource)
        {
            if (dataSource == null)
                throw new ArgumentNullException("dataSource");
            this.dataSource = dataSource;
        }


        protected override IQueryable<TResource> Resolve<TResource, TBaseResource>(ResourceCollectionNode<TBaseResource> node)
        {
            if (node.Value is IQueryable<TResource>)
                return (IQueryable<TResource>)node.Value;

            var itemResourceType = node.ItemResourceType;

            var queryable = this.dataSource.Query<TResource>();

            if (itemResourceType.IsRootResource && node.Parent is DataSourceRootNode)
                return queryable;

            return GetByParentId(node, itemResourceType, queryable);
        }


        private static IQueryable<TResource> GetByParentId<TResource>(ResourceCollectionNode node,
            ResourceType itemResourceType,
            IQueryable<TResource> queryable)
        {
            if (((ResourceType)node.Parent.Type).UriBaseType != itemResourceType.ParentResourceType)
            {
                // Fallback to AsQueryable. TODO: Do this in a more elegant way!
                return (IQueryable<TResource>)((IEnumerable)node.Value).AsQueryable();
            }

            // Do query like this: dataSource.Query<TResource>().Where(x => x.Parent.Id == parentId);
            var parentIdProperty = itemResourceType.ParentResourceType.PrimaryId;
            var parentId = parentIdProperty.GetValue(node.Parent.Value);
            var predicateParam = Expression.Parameter(typeof(TResource));
            var predicate =
                Expression.Lambda<Func<TResource, bool>>(
                    Expression.Equal(
                        parentIdProperty.CreateGetterExpression(
                            itemResourceType.ChildToParentProperty.CreateGetterExpression(predicateParam)),
                        Expression.Constant(parentId, parentIdProperty.PropertyType.Type)),
                    predicateParam);
            return queryable.Where(predicate);
        }
    }
}