#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Text;

using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class UriResolver : IUriResolver
    {
        private readonly IBaseUriProvider baseUriProvider;
        private readonly ITypeResolver typeMapper;


        public UriResolver(ITypeResolver typeMapper, IBaseUriProvider baseUriProvider)
        {
            if (typeMapper == null)
                throw new ArgumentNullException(nameof(typeMapper));
            if (baseUriProvider == null)
                throw new ArgumentNullException(nameof(baseUriProvider));
            this.typeMapper = typeMapper;
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
            var type = this.typeMapper.FromType(entityType) as ResourceType;
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
            var baseUri = this.baseUriProvider.BaseUri.ToString();
            return $"{baseUri}{((baseUri.EndsWith("/") || path == string.Empty) ? string.Empty : "/")}{path}";
        }


        public string ToRelativePath(string url)
        {
            var baseUrl = this.baseUriProvider.BaseUri.ToString().TrimEnd('/');
            if (
                !(url.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase)
                  && (baseUrl.Length == url.Length || url[baseUrl.Length] == '/')))
                throw new ArgumentException("Url does not have the correct base url.", nameof(url));
            return url.Substring(baseUrl.Length);
        }
    }
}

