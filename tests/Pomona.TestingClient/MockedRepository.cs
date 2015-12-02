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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Proxies;

namespace Pomona.TestingClient
{
    public class MockedRepository<TResource, TPostReturnType, TId> : IClientRepository<TResource, TPostReturnType, TId>
        where TResource : class, IClientResource
        where TPostReturnType : IClientResource
    {
        public TestableClientProxyBase Client { get; set; }


        public virtual TResource Get(object id)
        {
            ResourceInfoAttribute resourceInfo;
            if (!Client.TryGetResourceInfoForType(typeof(TResource), out resourceInfo))
                throw new InvalidOperationException("Expected TResource to have a ResourceInfoAttribute here.");

            var idProp = resourceInfo.IdProperty;

            var param = Expression.Parameter(typeof(TResource), "x");
            var compareIdExpr =
                Expression.Lambda<Func<TResource, bool>>(
                    Expression.Equal(Expression.Convert(Expression.Constant(id), idProp.PropertyType),
                                     Expression.MakeMemberAccess(param, idProp)), param);

            return Query<TResource>().Where(compareIdExpr).First();
        }


        public virtual TResource GetLazy(object id)
        {
            return Get(id);
        }


        public virtual object OnInvokeMethod(MethodInfo methodInfo, object[] args)
        {
            var parameters = methodInfo.GetParameters();
            if (methodInfo.Name == "Post" && parameters.Length == 1
                && typeof(PostResourceBase).IsAssignableFrom(parameters[0].ParameterType))
                return Post((PostResourceBase)args[0]);
            if (methodInfo.Name == "Get" && parameters.Length == 1)
                return Get(args[0]);
            if (methodInfo.Name == "GetLazy" && parameters.Length == 1)
                return GetLazy(args[0]);
            throw new NotImplementedException();
        }


        protected virtual TPropType OnGet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
        {
            throw new NotImplementedException();
        }


        protected virtual void OnSet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property, TPropType value)
        {
            throw new NotImplementedException();
        }


        public virtual void Delete(TResource resource)
        {
            Client.Delete(resource);
        }


        public Task DeleteAsync(TResource resource)
        {
            throw new NotImplementedException();
        }


        public Type ElementType
        {
            get { return typeof(TResource); }
        }

        public Expression Expression
        {
            get { return Expression.Constant(Client.Query<TResource>()); }
        }


        public IEnumerator<TResource> GetEnumerator()
        {
            return Query().GetEnumerator();
        }


        public virtual TSubResource Patch<TSubResource>(TSubResource resource,
                                                        Action<TSubResource> patchAction,
                                                        Action<IRequestOptions<TSubResource>> options) where TSubResource : class, TResource
        {
            return Client.OnPatch(resource, patchAction);
        }


        public virtual TSubResource Patch<TSubResource>(TSubResource resource, Action<TSubResource> patchAction)
            where TSubResource : class, TResource
        {
            return Client.OnPatch(resource, patchAction);
        }


        public Task<TSubResource> PatchAsync<TSubResource>(TSubResource resource, Action<TSubResource> patchAction, Action<IRequestOptions<TSubResource>> options) where TSubResource : class, TResource
        {
            throw new NotImplementedException();
        }


        public virtual TPostReturnType Post(IPostForm form)
        {
            return (TPostReturnType)Client.OnPost(form);
        }


        public virtual TPostReturnType Post<TSubResource>(Action<TSubResource> postAction)
            where TSubResource : class, TResource
        {
            var postForm = Client.TypeMapper.CreatePostForm(typeof(TSubResource));

            var resInfo = Client.GetResourceInfoForType(typeof(TSubResource));
            var form = (TSubResource)Activator.CreateInstance(resInfo.PostFormType);
            postAction(form);
            return (TPostReturnType)Client.OnPost((PostResourceBase)((object)form));
        }


        public virtual TPostReturnType Post<TSubResource>(Action<TSubResource> postAction, Action<IRequestOptions<TPostReturnType>> options)
            where TSubResource : class, TResource
        {
            return Post(postAction);
        }


        public virtual TSubResponseResource Post<TSubResource, TSubResponseResource>(Action<TSubResource> postAction,
                                                                                     Action<IRequestOptions<TSubResponseResource>> options)
            where TSubResource : class, TResource
            where TSubResponseResource : TPostReturnType
        {
            var postForm = Client.TypeMapper.CreatePostForm(typeof(TSubResource));

            var resInfo = Client.GetResourceInfoForType(typeof(TSubResource));
            var form = (TSubResource)Activator.CreateInstance(resInfo.PostFormType);
            postAction(form);
            return (TSubResponseResource)Client.OnPost((PostResourceBase)((object)form));
        }


        public virtual TSubResponseResource Post<TSubResource, TSubResponseResource>(Action<TSubResource> postAction)
            where TSubResource : class, TResource where TSubResponseResource : TPostReturnType
        {
            return Post<TSubResource, TSubResponseResource>(postAction, x =>
            {
            });
        }


        public virtual TPostReturnType Post(Action<TResource> postAction)
        {
            return Post<TResource, TPostReturnType>(postAction);
        }


        public Task<TSubResponseResource> PostAsync<TSubResource, TSubResponseResource>(Action<TSubResource> postAction, Action<IRequestOptions<TSubResponseResource>> options) where TSubResource : class, TResource where TSubResponseResource : TPostReturnType
        {
            throw new NotImplementedException();
        }


        public virtual object Post<TPostForm>(TResource resource, TPostForm form)
            where TPostForm : class, IPostForm, IClientResource
        {
            throw new NotImplementedException();
        }


        public IQueryProvider Provider
        {
            get { return Client.Query<TResource>().Provider; }
        }


        public virtual IQueryable<TResource> Query()
        {
            return Client.Query<TResource>();
        }


        public virtual IQueryable<TSubResource> Query<TSubResource>() where TSubResource : TResource
        {
            return Client.Query<TSubResource>();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        string IClientRepository.Uri
        {
            get { throw new NotImplementedException(); }
        }
    }
}