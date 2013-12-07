using System;
using System.Globalization;
using System.Text;

using Nancy;
using Nancy.Helpers;

using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class UriResolver : IUriResolver
    {
        public UriResolver(ITypeMapper typeMapper, NancyContext context)
        {
            if (typeMapper == null) throw new ArgumentNullException("typeMapper");
            if (context == null) throw new ArgumentNullException("context");
            //if (routeResolver == null) throw new ArgumentNullException("routeResolver");
            this.typeMapper = typeMapper;
            this.context = context;
        }


        private readonly NancyContext context;
        private readonly ITypeMapper typeMapper;
        public NancyContext Context
        {
            get { return this.context; }
        }


        public virtual string RelativeToAbsoluteUri(string path)
        {
            if (String.IsNullOrEmpty(this.context.Request.Url.HostName))
            {
                return path;
            }

            return String.Format("{0}{1}", GetBaseUri(), path);
        }


        public string GetUriFor(PropertySpec property, object entity)
        {
            return RelativeToAbsoluteUri(BuildRelativeUri(entity, property));
        }


        private string BuildRelativeUri(object entity, PropertySpec property)
        {
            var sb = new StringBuilder();
            BuildRelativeUri(entity, property, sb);
            return sb.ToString();
        }

        private void BuildRelativeUri(object entity, PropertySpec property, StringBuilder sb)
        {
            var type = this.typeMapper.GetClassMapping(entity.GetType()) as ResourceType;
            if (type == null)
                throw new InvalidOperationException("Can only get Uri for a ResourceType.");

            if (type.ParentResourceType != null)
            {
                var parentEntity = type.ChildToParentProperty.Getter(entity);
                if (parentEntity != null)
                {
                    BuildRelativeUri(parentEntity, type.ParentToChildProperty, sb);
                }
            }
            else
            {
                sb.Append(type.UriRelativePath);
            }
            sb.Append('/');

            sb.AppendFormat("{0}", Common.HttpUtility.UrlPathEncode(Convert.ToString(type.GetId(entity), CultureInfo.InvariantCulture)));

            if (property != null)
            {
                sb.Append('/');
                sb.Append(((PropertyMapping)property).UriName);
            }
        }

        public string GetUriFor(object entity)
        {
            return RelativeToAbsoluteUri(BuildRelativeUri(entity, null));
        }

        public ITypeMapper TypeMapper
        {
            get { return this.typeMapper; }
        }

        protected virtual Uri GetBaseUri()
        {
            var request = this.context.Request;
            var appUrl = request.Url.BasePath ?? string.Empty;
            var uriString = String.Format("{0}://{1}:{2}{3}{4}", request.Url.Scheme, request.Url.HostName,
                request.Url.Port, appUrl, appUrl.EndsWith("/") ? String.Empty : "/");

            return new Uri(uriString);
        }
    }
}