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
using System.Reflection;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Proxies;
using Pomona.Common.Serialization;

namespace Pomona.TestingClient
{
    public class TestableClientProxyBase : IPomonaClient, ITestableClient
    {
        private static readonly MethodInfo getResourceCollection =
            ReflectionHelper.GetMethodDefinition<TestableClientProxyBase>(x => x.GetResourceCollection<object>());

        private static readonly MethodInfo mapFormDictionaryToResourceDictionaryMethod =
            ReflectionHelper.GetMethodDefinition<TestableClientProxyBase>(
                x => x.MapFormDictionaryToResourceDictionary<object, object>(null));

        private static readonly MethodInfo mapFormListToResourceListMethod =
            ReflectionHelper.GetMethodDefinition<TestableClientProxyBase>(
                x => x.MapFormListToResourceList<object>(null));

        private static readonly MethodInfo onGetRepositoryMethod =
            ReflectionHelper.GetMethodDefinition<TestableClientProxyBase>(
                x =>
                    x
                        .OnGetRepository
                        <object, IClientRepository<IClientResource, IClientResource, object>, IClientResource, IClientResource, object>(
                            null));

        private static readonly MethodInfo querySubResourceMethod =
            ReflectionHelper.GetMethodDefinition<TestableClientProxyBase>(x => x.QuerySubResource<object, object>());

        private static readonly MethodInfo saveInternalMethod =
            ReflectionHelper.GetMethodDefinition<TestableClientProxyBase>(x => x.SaveInternal<IClientResource>(null));

        private readonly Dictionary<Type, Delegate> postHandlers = new Dictionary<Type, Delegate>();

        private readonly Dictionary<string, object> repositoryCache = new Dictionary<string, object>();

        private readonly Dictionary<Type, object> resourceCollections = new Dictionary<Type, object>();
        private readonly ClientTypeMapper typeMapper;
        private long idCounter = 1;


        public TestableClientProxyBase()
        {
            var proxiedClientInterface =
                GetType().GetInterfaces().Except((typeof(TestableClientProxyBase).GetInterfaces())).Single();
            this.typeMapper = new ClientTypeMapper(proxiedClientInterface.Assembly);
        }


        public ClientTypeMapper TypeMapper
        {
            get { return this.typeMapper; }
        }


        public virtual object OnInvokeMethod(MethodInfo methodInfo, object[] args)
        {
            throw new NotImplementedException();
        }


        public virtual T OnPatch<T>(T resource, Action<T> patchAction)
            where T : IClientResource
        {
            patchAction(resource);
            return resource;
        }


        public virtual object OnPost(IPostForm form)
        {
            var resourceInterface = this.GetMostInheritedResourceInterface(form.GetType());

            Delegate del;
            if (this.postHandlers.TryGetValue(resourceInterface, out del))
                return del.DynamicInvoke(form);

            return SaveResourceFromForm(form);
        }


        public T Reload<T>(T resource)
        {
            return resource;
        }


        public virtual IQueryable<T> Query<T>()
        {
            return this.typeMapper.WrapExtendedQuery<T>(Query);
        }


        public virtual void Save(object resource)
        {
            var resourceInterface = this.GetMostInheritedResourceInterface(resource.GetType());
            ResourceInfoAttribute resourceInfo;
            if (!TryGetResourceInfoForType(resourceInterface, out resourceInfo))
                throw new InvalidOperationException("Expected to get a resource info here.");

            saveInternalMethod.MakeGenericMethod(resourceInfo.UriBaseType).Invoke(this, new[] { resource });
        }


        public virtual object SaveResourceFromForm(IPostForm form)
        {
            var formType = form.GetType();
            var resourceInterface = this.GetMostInheritedResourceInterface(formType);
            ResourceInfoAttribute resInfo;
            if (!TryGetResourceInfoForType(resourceInterface, out resInfo))
                throw new InvalidOperationException("Unable to get resource info for " + formType.FullName);

            var resource = Activator.CreateInstance(resInfo.PocoType);

            foreach (var formProp in formType.GetProperties())
            {
                if (((IPomonaSerializable)form).PropertyIsSerialized(formProp.Name))
                {
                    var resProp = resInfo.PocoType.GetProperty(formProp.Name);
                    var value = formProp.GetValue(form, null);
                    Type valueType = null;

                    if (value != null)
                    {
                        valueType = value.GetType();
                        if (value is PostResourceBase)
                            value = SaveResourceFromForm((PostResourceBase)value);
                        else if (valueType != typeof(string))
                        {
                            Type elementType;

                            Type[] dictTypeArgs;
                            if (valueType.TryExtractTypeArguments(typeof(IDictionary<,>), out dictTypeArgs))
                            {
                                value = mapFormDictionaryToResourceDictionaryMethod
                                    .MakeGenericMethod(dictTypeArgs)
                                    .Invoke(this, new[] { value });
                            }
                            else if (valueType.TryGetEnumerableElementType(out elementType))
                            {
                                value = mapFormListToResourceListMethod.MakeGenericMethod(elementType)
                                    .Invoke(this, new[] { value });
                            }
                        }
                    }

                    resProp.SetValue(resource, value, null);
                }
            }

            if (!resInfo.IsValueObject)
                Save(resource);
            return resource;
        }


        public object Get(string uri, Type type, RequestOptions requestOptions)
        {
            throw new NotImplementedException();
        }


        public object Get(string uri, Type type)
        {
            throw new NotImplementedException();
        }


        public T Get<T>(string uri)
        {
            throw new NotImplementedException();
        }


        public IList<TResource> GetResourceCollection<TResource>()
        {
            var type = typeof(TResource);

            object collection;
            if (!this.resourceCollections.TryGetValue(type, out collection))
            {
                collection = new List<TResource>();
                this.resourceCollections[type] = collection;
            }

            return (IList<TResource>)collection;
        }


        public string GetUriOfType(Type type)
        {
            throw new NotImplementedException();
        }


        public void SetupPostHandler<TResource>(
            Func<TResource, object> func)
        {
            this.postHandlers[typeof(TResource)] = func;
        }


        public bool TryGetResourceInfoForType(Type type, out ResourceInfoAttribute resourceInfo)
        {
            resourceInfo =
                type.GetCustomAttributes(typeof(ResourceInfoAttribute), false)
                    .OfType<ResourceInfoAttribute>()
                    .FirstOrDefault();
            return resourceInfo != null;
        }


        protected virtual TPropType OnGet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
        {
            var propType = typeof(TPropType);
            if (typeof(IClientRepository).IsAssignableFrom(propType))
            {
                Type[] typeArgs;
                Type postResponseType;
                Type resourceType;
                if (!propType.TryExtractTypeArguments(typeof(IPostableRepository<,>), out typeArgs))
                {
                    if (!propType.TryExtractTypeArguments(typeof(IQueryableRepository<>), out typeArgs))
                        throw new InvalidOperationException("Unable to generate proxy for " + propType.FullName);
                }
                resourceType = typeArgs[0];
                postResponseType = typeArgs.Last(); // [1] when IPostableRepository implemented, [0] when not.

                Type primaryIdType = typeof(object);
                Type[] gettableRepoType;
                if (propType.TryExtractTypeArguments(typeof(IGettableRepository<,>), out gettableRepoType))
                {
                    primaryIdType = gettableRepoType[1];
                }

                object repository;
                if (!this.repositoryCache.TryGetValue(property.Name, out repository))
                {
                    repository = onGetRepositoryMethod.MakeGenericMethod(typeof(TOwner),
                        propType,
                        resourceType,
                        postResponseType,
                        primaryIdType)
                        .Invoke(this, new object[] { property });
                    this.repositoryCache[property.Name] = repository;
                }

                return (TPropType)repository;
            }

            throw new NotImplementedException();
        }


        protected virtual TPropType OnGetRepository<TOwner, TPropType, TResource, TPostResponseType, TId>(
            PropertyWrapper<TOwner, TPropType> property)
            where TPropType : IClientRepository
            where TResource : class, IClientResource
            where TPostResponseType : IClientResource
        {
            var mockedRepo = RuntimeProxyFactory<MockedRepository<TResource, TPostResponseType, TId>, TPropType>.Create();
            ((MockedRepository<TResource, TPostResponseType, TId>)((object)mockedRepo)).Client = this;
            return mockedRepo;
        }


        protected virtual void OnSet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property, TPropType value)
        {
            throw new NotImplementedException();
        }


        protected virtual void SetupPostHandler<TResource>(Delegate func)
        {
            this.postHandlers[typeof(TResource)] = func;
        }


        protected object MapFormDictionaryToResourceDictionary<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            return
                dict.ToDictionary(
                    x =>
                        x.Key is PostResourceBase
                            ? (TKey)SaveResourceFromForm((PostResourceBase)((object)x.Key))
                            : x.Key,
                    x =>
                        x.Value is PostResourceBase
                            ? (TValue)SaveResourceFromForm((PostResourceBase)((object)x.Value))
                            : x.Value);
        }


        protected object MapFormListToResourceList<TElement>(IEnumerable<TElement> items)
        {
            return
                items.Select(
                    x => x is PostResourceBase ? (TElement)SaveResourceFromForm((PostResourceBase)((object)x)) : x)
                    .ToList();
        }


        private IList GetResourceCollection(Type resourceType)
        {
            return (IList)getResourceCollection.MakeGenericMethod(resourceType).Invoke(this, null);
        }


        private IQueryable Query(Type resourceType)
        {
            var resInfo = this.GetResourceInfoForType(resourceType);
            var baseType = resInfo.UriBaseType;
            if (baseType != resourceType)
                return (IQueryable)querySubResourceMethod.MakeGenericMethod(baseType, resourceType).Invoke(this, null);

            return GetResourceCollection(resourceType).AsQueryable();
        }


        private IQueryable<TSubResource> QuerySubResource<TResource, TSubResource>()
        {
            return GetResourceCollection<TResource>().AsQueryable().OfType<TSubResource>();
        }


        private void SaveInternal<TResource>(TResource resource)
            where TResource : IClientResource
        {
            var resInfo = this.GetResourceInfoForType(typeof(TResource));
            if (resInfo.HasIdProperty &&
                (resInfo.IdProperty.PropertyType == typeof(int) || resInfo.IdProperty.PropertyType == typeof(long)))
            {
                resInfo.IdProperty.SetValue(resource,
                    Convert.ChangeType(this.idCounter++, resInfo.IdProperty.PropertyType),
                    null);
            }

            GetResourceCollection<TResource>().Add(resource);
        }
    }
}