#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Proxies;
using Pomona.Common.Web;

namespace Pomona.TestingClient
{
    using PropertyType = IClientRepository<IClientResource, IClientResource, object>;

    public class TestableClientProxyBase : IPomonaRootResource, ITestableClient
    {
        private static readonly MethodInfo getResourceCollection;
        private static readonly MethodInfo mapFormDictionaryToResourceDictionaryMethod;
        private static readonly MethodInfo mapFormListToResourceListMethod;
        private static readonly MethodInfo onGetRepositoryMethod;
        private static readonly MethodInfo querySubResourceMethod;
        private static readonly MethodInfo saveInternalMethod;
        private readonly Dictionary<Type, Delegate> postHandlers;
        private readonly Dictionary<string, object> repositoryCache;
        private readonly Dictionary<Type, object> resourceCollections;
        private long idCounter;


        static TestableClientProxyBase()
        {
            getResourceCollection =
                ReflectionHelper.GetMethodDefinition<TestableClientProxyBase>(x => x.GetResourceCollection<object>());

            mapFormDictionaryToResourceDictionaryMethod =
                ReflectionHelper.GetMethodDefinition<TestableClientProxyBase>(
                    x => x.MapFormDictionaryToResourceDictionary<object, object>(null));

            mapFormListToResourceListMethod = ReflectionHelper
                .GetMethodDefinition<TestableClientProxyBase>(x => x.MapFormListToResourceList<object>(null));

            onGetRepositoryMethod = ReflectionHelper.GetMethodDefinition<TestableClientProxyBase>(
                x => x.OnGetRepository<object, PropertyType, IClientResource, IClientResource, object>(null));

            querySubResourceMethod =
                ReflectionHelper.GetMethodDefinition<TestableClientProxyBase>(x => x.QuerySubResource<object, object>());

            saveInternalMethod =
                ReflectionHelper.GetMethodDefinition<TestableClientProxyBase>(x => x.SaveInternal<IClientResource>(null));
        }


        public TestableClientProxyBase()
        {
            Settings = new ClientSettings();
            this.postHandlers = new Dictionary<Type, Delegate>();
            this.repositoryCache = new Dictionary<string, object>();
            this.resourceCollections = new Dictionary<Type, object>();
            this.idCounter = 1;
            var proxiedClientInterface =
                GetType().GetInterfaces().Except((typeof(TestableClientProxyBase).GetInterfaces())).Single();
            TypeMapper = new ClientTypeMapper(proxiedClientInterface.Assembly);
        }


        public virtual void Delete(IClientResource resource)
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
                if (form.PropertyIsSerialized(formProp.Name))
                {
                    var resProp = resInfo.PocoType.GetProperty(formProp.Name);
                    var value = formProp.GetValue(form, null);

                    if (value != null)
                    {
                        Type valueType = value.GetType();
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


        public object SendRequest(string uri,
                                  object body,
                                  string httpMethod,
                                  RequestOptions options = null,
                                  Type responseBaseType = null)
        {
            throw new NotImplementedException();
        }


        protected object MapFormDictionaryToResourceDictionary<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            Func<object, object> savePostResourceBase = x => x is PostResourceBase
                ? SaveResourceFromForm((PostResourceBase)(x))
                : x;

            return dict.ToDictionary(x => (TKey)savePostResourceBase.Invoke(x.Key),
                                     x => (TValue)savePostResourceBase.Invoke(x.Value));
        }


        protected object MapFormListToResourceList<TElement>(IEnumerable<TElement> items)
        {
            return items
                .Select(x => x is PostResourceBase
                    ? (TElement)SaveResourceFromForm((PostResourceBase)((object)x))
                    : x)
                .ToList();
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
                    primaryIdType = gettableRepoType[1];

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
            var mockedRepo =
                RuntimeProxyFactory<MockedRepository<TResource, TPostResponseType, TId>, TPropType>.Create();
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


        public void Delete(object resource, RequestOptions options)
        {
            throw new NotImplementedException();
        }


        public Task DeleteAsync(object resource, RequestOptions options)
        {
            throw new NotImplementedException();
        }


        public object Get(string uri, Type type, RequestOptions requestOptions)
        {
            throw new NotImplementedException();
        }


        public Task<object> GetAsync(string uri, Type type, RequestOptions requestOptions)
        {
            throw new NotImplementedException();
        }


        public object Patch(object form, RequestOptions options)
        {
            throw new NotImplementedException();
        }


        public Task<object> PatchAsync(object form, RequestOptions options)
        {
            throw new NotImplementedException();
        }


        public object Post(string uri, object form, RequestOptions options)
        {
            throw new NotImplementedException();
        }


        public Task<object> PostAsync(string uri, IPostForm form, RequestOptions options)
        {
            throw new NotImplementedException();
        }


        public virtual IQueryable<T> Query<T>()
        {
            return TypeMapper.WrapExtendedQuery<T>(Query);
        }


        public IQueryable<T> Query<T>(string uri)
        {
            throw new NotImplementedException();
        }


        public T Reload<T>(T resource)
        {
            return resource;
        }

#pragma warning disable 67
        public event EventHandler<ClientRequestLogEventArgs> RequestCompleted;
#pragma warning restore 67

        public ClientSettings Settings { get; }


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


        public ClientTypeMapper TypeMapper { get; }

        public IWebClient WebClient
        {
            get { throw new NotImplementedException(); }
        }
    }
}