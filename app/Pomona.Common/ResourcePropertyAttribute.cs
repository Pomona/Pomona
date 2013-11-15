using System;

using Pomona.Common.TypeSystem;

namespace Pomona.Common
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Interface)]
    public class ResourcePropertyAttribute : Attribute
    {
        public HttpMethod Method { get; set; }
    }
}