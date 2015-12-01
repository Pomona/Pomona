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
                throw new ArgumentNullException("typeMapper");
            if (baseUriProvider == null)
                throw new ArgumentNullException("baseUriProvider");
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
            return String.Format("{0}{1}{2}", baseUri, (baseUri.EndsWith("/") || path == string.Empty) ? string.Empty : "/", path);
        }


        public string ToRelativePath(string url)
        {
            var baseUrl = this.baseUriProvider.BaseUri.ToString().TrimEnd('/');
            if (
                !(url.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase)
                  && (baseUrl.Length == url.Length || url[baseUrl.Length] == '/')))
                throw new ArgumentException("Url does not have the correct base url.", "url");
            return url.Substring(baseUrl.Length);
        }
    }
}