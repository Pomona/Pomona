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
using System.Collections.Generic;
using System.Linq;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.Queries;

namespace Pomona
{
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


        public virtual HttpMethod AllowedMethods
        {
            get { return 0; }
        }

        public abstract bool Exists { get; }
        public abstract bool IsLoaded { get; }

        public string Name
        {
            get { return this.name; }
        }

        public PathNode Parent
        {
            get { return this.parent; }
        }

        public IMappedType Type
        {
            get { return OnGetType(); }
        }

        public ITypeMapper TypeMapper
        {
            get { return this.typeMapper; }
        }

        public abstract object Value { get; }

        protected internal virtual IMappedType ExpectedPostType
        {
            get { return null; }
        }

        public abstract PathNode GetChildNode(string name);


        public virtual IQueryExecutor GetQueryExecutor()
        {
            if (Parent == null)
                return new DefaultQueryExecutor();
            return Parent.GetQueryExecutor();
        }


        public IEnumerable<IPomonaRequestProcessor> GetRequestProcessors(PomonaRequest request)
        {
            var nodeProcessor = OnGetRequestProcessor(request);
            var head = nodeProcessor != null
                ? nodeProcessor.WrapAsEnumerable()
                : Enumerable.Empty<IPomonaRequestProcessor>();

            return Parent != null ? head.Concat(Parent.GetRequestProcessors(request)) : head;
        }


        protected abstract IMappedType OnGetType();


        protected static PathNode CreateNode(ITypeMapper typeMapper,
            PathNode parent,
            string name,
            Func<object> valueFetcher,
            IMappedType expectedType)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            if (expectedType is ResourceType)
                return new ResourceNode(typeMapper, parent, name, valueFetcher, (ResourceType)expectedType);
            if (expectedType.IsCollection && expectedType.ElementType is ResourceType)
            {
                var elementType = (ResourceType)expectedType.ElementType;

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
                    valueFetcher,
                    expectedType);
            }

            throw new NotImplementedException();
        }


        protected virtual IQueryableResolver GetQueryableResolver()
        {
            if (Parent != null)
                return Parent.GetQueryableResolver();
            return new DefaultQueryableResolver();
        }


        protected virtual IPomonaRequestProcessor OnGetRequestProcessor(PomonaRequest request)
        {
            return null;
        }
    }
}