﻿#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Text;

using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class UriResolver : IUriResolver
    {
        private readonly IBaseUriProvider baseUriProvider;
        private readonly ITypeResolver typeResolver;


        public UriResolver(ITypeResolver typeResolver, IBaseUriProvider baseUriProvider)
        {
            if (typeResolver == null)
                throw new ArgumentNullException(nameof(typeResolver));

            if (baseUriProvider == null)
                throw new ArgumentNullException(nameof(baseUriProvider));

            this.typeResolver = typeResolver;
            this.baseUriProvider = baseUriProvider;
        }


        private string BuildRelativeUri(object entity, PropertySpec property)
        {
            var sb = new StringBuilder();
            BuildRelativeUri(entity, property, sb);
            return sb.ToString();
        }


        private void BuildRelativeUri(object entity, PropertySpec property, StringBuilder sb)
        {
            var entityType = entity.GetType();
            var type = this.typeResolver.FromType(entityType) as ResourceType;
            if (type == null)
                throw new InvalidOperationException($"Can't get URI for {entityType}; can only get Uri for a ResourceType.");

            type.AppendUri(entity, sb);

            if (property != null)
            {
                if (sb.Length > 0)
                    sb.Append('/');
                sb.Append(((ResourceProperty)property).UriName);
            }
        }


        public string GetUriFor(PropertySpec property, object entity)
        {
            return RelativeToAbsoluteUri(BuildRelativeUri(entity, property));
        }


        public string GetUriFor(object entity)
        {
            return RelativeToAbsoluteUri(BuildRelativeUri(entity, null));
        }


        public virtual string RelativeToAbsoluteUri(string path)
        {
            var baseUrl = GetBaseUrl();
            return $"{baseUrl}{((baseUrl.EndsWith("/") || path == string.Empty) ? string.Empty : "/")}{path}";
        }


        public string ToRelativePath(string url)
        {
            var baseUrl = GetBaseUrl().TrimEnd('/');
            if (!(url.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase)
                  && (baseUrl.Length == url.Length || url[baseUrl.Length] == '/')))
                throw new ArgumentException("Url does not have the correct base url.", nameof(url));
            return url.Substring(baseUrl.Length);
        }


        private string GetBaseUrl()
        {
            var baseUri = this.baseUriProvider.BaseUri;

            if (baseUri == null)
                throw new InvalidOperationException($"{this.baseUriProvider.GetType()}.BaseUri is null.");

            return baseUri.ToString();
        }
    }
}