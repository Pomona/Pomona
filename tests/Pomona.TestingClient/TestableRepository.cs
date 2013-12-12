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
using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Proxies;

namespace Pomona.TestingClient
{
    public class MockedRepository<TResource, TPostReturnType> : IClientRepository<TResource, TPostReturnType>
        where TResource : class, IClientResource
        where TPostReturnType : IClientResource
    {
        private TestableClientProxyBase client;

        public TestableClientProxyBase Client
        {
            get { return client; }
            set { client = value; }
        }

        public Type ElementType
        {
            get { return typeof (TResource); }
        }

        public Expression Expression
        {
            get { return Expression.Constant(client.Query<TResource>()); }
        }

        public IQueryProvider Provider
        {
            get { return client.Query<TResource>().Provider; }
        }

        string IClientRepository.Uri
        {
            get { throw new NotImplementedException(); }
        }


        public virtual TPostReturnType Post(IPostForm form)
        {
            return (TPostReturnType) client.OnPost(form);
        }


        public virtual TPostReturnType Post<TSubResource>(Action<TSubResource> postAction)
            where TSubResource : class, TResource
        {
            var resInfo = client.GetResourceInfoForType(typeof (TSubResource));
            var form = (TSubResource) Activator.CreateInstance(resInfo.PostFormType);
            postAction(form);
            return (TPostReturnType) client.OnPost((PostResourceBase) ((object) form));
        }


        public TResource Get(object id)
        {
            ResourceInfoAttribute resourceInfo;
            if (!client.TryGetResourceInfoForType(typeof (TResource), out resourceInfo))
                throw new InvalidOperationException("Expected TResource to have a ResourceInfoAttribute here.");

            var idProp = resourceInfo.IdProperty;

            var param = Expression.Parameter(typeof (TResource), "x");
            var compareIdExpr =
                Expression.Lambda<Func<TResource, bool>>(
                    Expression.Equal(Expression.Convert(Expression.Constant(id), idProp.PropertyType),
                        Expression.MakeMemberAccess(param, idProp)), param);

            return Query<TResource>().Where(compareIdExpr).First();
        }


        public IEnumerator<TResource> GetEnumerator()
        {
            return Query().GetEnumerator();
        }


        public TResource GetLazy(object id)
        {
            return Get(id);
        }


        public object OnInvokeMethod(MethodInfo methodInfo, object[] args)
        {
            var parameters = methodInfo.GetParameters();
            if (methodInfo.Name == "Post" && parameters.Length == 1
                && typeof (PostResourceBase).IsAssignableFrom(parameters[0].ParameterType))
                return Post((PostResourceBase) args[0]);
            if (methodInfo.Name == "Get" && parameters.Length == 1)
                return Get(args[0]);
            throw new NotImplementedException();
        }


        public TSubResource Patch<TSubResource>(TSubResource resource, Action<TSubResource> patchAction,
            Action<IRequestOptions<TSubResource>> options) where TSubResource : class, TResource
        {
            return client.OnPatch(resource, patchAction);
        }


        public TSubResource Patch<TSubResource>(TSubResource resource, Action<TSubResource> patchAction)
            where TSubResource : class, TResource
        {
            return client.OnPatch(resource, patchAction);
        }


        public TPostReturnType Post(Action<TResource> postAction)
        {
            throw new NotImplementedException();
        }


        public object Post<TPostForm>(TResource resource, TPostForm form)
            where TPostForm : class, IPostForm, IClientResource
        {
            throw new NotImplementedException();
        }


        public IQueryable<TResource> Query()
        {
            return client.Query<TResource>();
        }


        public IQueryable<TSubResource> Query<TSubResource>() where TSubResource : TResource
        {
            return client.Query<TResource>().OfType<TSubResource>();
        }


        protected virtual TPropType OnGet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
        {
            throw new NotImplementedException();
        }


        protected virtual void OnSet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property, TPropType value)
        {
            throw new NotImplementedException();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}