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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Pomona.Common.Internals;
using Pomona.Common.Linq;
using Pomona.Common.Loading;
using Pomona.Common.Proxies;
using Pomona.Common.Serialization.Json;
using Pomona.Common.Serialization.Patch;
using Pomona.Common.Web;

namespace Pomona.Common
{
    public class PomonaClient : IPomonaClient
    {
        private readonly IRequestDispatcher dispatcher;
        private readonly ClientTypeMapper typeMapper;


        public PomonaClient(ClientTypeMapper typeMapper)
            : this(typeMapper, (IRequestDispatcher)null)
        {
        }


        public PomonaClient(ClientTypeMapper typeMapper, IWebClient webClient)
            : this(typeMapper, CreateDefaultRequestDispatcher(typeMapper, webClient))
        {
        }


        public PomonaClient(ClientTypeMapper typeMapper, IRequestDispatcher dispatcher)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            dispatcher = dispatcher ?? CreateDefaultRequestDispatcher(typeMapper);

            this.typeMapper = typeMapper;
            this.dispatcher = dispatcher;

            Settings = new ClientSettings();

            dispatcher.RequestCompleted += (s, e) => RaiseRequestCompleted(e.Request, e.Response, e.ThrownException);
        }


        public IEnumerable<Type> ResourceTypes
        {
            get { return this.typeMapper.ResourceTypes; }
        }


        public string GetRelativeUriForType(Type type)
        {
            var resourceInfo = this.GetResourceInfoForType(type);
            return resourceInfo.UrlRelativePath;
        }


        protected void RaiseRequestCompleted(HttpRequestMessage request,
                                             HttpResponseMessage response,
                                             Exception thrownException = null)
        {
            var eh = RequestCompleted;
            if (eh != null)
                eh(this, new ClientRequestLogEventArgs(request, response, thrownException));
        }


        private void AddIfMatchToPatch(object postForm, RequestOptions options)
        {
            string etagValue = null;
            var resourceInfo = TypeMapper.GetMostInheritedResourceInterfaceInfo(postForm.GetType());
            if (resourceInfo.HasEtagProperty)
                etagValue = (string)resourceInfo.EtagProperty.GetValue(postForm, null);

            if (etagValue != null)
            {
                options.ModifyRequest(
                    request => request.Headers.Add("If-Match", string.Format("\"{0}\"", etagValue)));
            }
        }


        private static IRequestDispatcher CreateDefaultRequestDispatcher(ClientTypeMapper typeMapper, IWebClient webClient = null)
        {
            return new RequestDispatcher(
                typeMapper,
                webClient ?? new HttpWebRequestClient(),
                new PomonaJsonSerializerFactory());
        }


        private ClientSerializationContextProvider GetSerializationContextProvider(RequestOptions requestOptions)
        {
            var resourceLoader = requestOptions == null || requestOptions.ResourceLoader == null
                ? Settings.LazyMode == LazyMode.Disabled
                    ? (IResourceLoader)new DisabledResourceLoader()
                    : new DefaultResourceLoader(this)
                : requestOptions.ResourceLoader;

            return new ClientSerializationContextProvider(this.typeMapper, this, resourceLoader);
        }


        private static string GetUriOfForm(object form)
        {
            if (form == null)
                throw new ArgumentNullException("form");
            var delta = form as IDelta;
            if (delta != null)
                return ((IHasResourceUri)delta.Original).Uri;
            var extendedProxy = form as IExtendedResourceProxy;
            if (extendedProxy != null)
                return GetUriOfForm(extendedProxy.WrappedResource);
            throw new InvalidOperationException("Unable to retrieve uri from resource of type "
                                                + form.GetType().FullName);
        }


        public virtual void Delete(object resource, RequestOptions options)
        {
            if (resource == null)
                throw new ArgumentNullException("resource");
            var uri = ((IHasResourceUri)resource).Uri;
            this.dispatcher.SendRequest(uri, "DELETE", null, GetSerializationContextProvider(options), options);
        }


        public virtual object Get(string uri, Type type, RequestOptions requestOptions)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            if (requestOptions == null)
                requestOptions = new RequestOptions(type);
            else if (type != null && requestOptions.ExpectedResponseType == null)
                requestOptions.ExpectedResponseType = type;

            return this.dispatcher.SendRequest(uri, "GET", null, GetSerializationContextProvider(requestOptions), requestOptions);
        }


        public virtual object Patch(object form, RequestOptions options)
        {
            if (form == null)
                throw new ArgumentNullException("form");

            var uri = GetUriOfForm(form);

            AddIfMatchToPatch(form, options);
            return this.dispatcher.SendRequest(uri, "PATCH", form, GetSerializationContextProvider(options), options);
        }


        public virtual object Post(string uri, object form, RequestOptions options)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            if (form == null)
                throw new ArgumentNullException("form");

            return this.dispatcher.SendRequest(uri, "POST", form, GetSerializationContextProvider(options), options);
        }


        public virtual IQueryable<T> Query<T>(string uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            return
                this.typeMapper.WrapExtendedQuery<T>(
                    st => new RestQueryProvider(this).CreateQuery(uri, st));
        }


        public virtual T Reload<T>(T resource)
        {
            var resourceWithUri = resource as IHasResourceUri;
            if (resourceWithUri == null)
            {
                throw new ArgumentException("Could not find resource URI, resouce not of type IHasResourceUri.",
                                            "resource");
            }

            if (resourceWithUri.Uri == null)
                throw new ArgumentException("Uri on resource was null.", "resource");

            if (!typeof(T).IsInterface)
                throw new ArgumentException("Type should be an interface inherited from a known resource type.");

            var resourceType = this.GetMostInheritedResourceInterface(typeof(T));
            return (T)Get(resourceWithUri.Uri, resourceType, new RequestOptions(typeof(T)));
        }


        public event EventHandler<ClientRequestLogEventArgs> RequestCompleted;
        public ClientSettings Settings { get; private set; }


        public virtual bool TryGetResourceInfoForType(Type type, out ResourceInfoAttribute resourceInfo)
        {
            return this.typeMapper.TryGetResourceInfoForType(type, out resourceInfo);
        }


        public virtual ClientTypeMapper TypeMapper
        {
            get { return this.typeMapper; }
        }

        public virtual IWebClient WebClient
        {
            get { return this.dispatcher.WebClient; }
        }
    }
}