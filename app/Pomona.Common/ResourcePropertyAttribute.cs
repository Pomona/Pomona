using System;

using Pomona.Common.TypeSystem;

namespace Pomona.Common
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Interface)]
    public class ResourcePropertyAttribute : Attribute
    {
        public HttpAccessMode AccessMode { get; set; }
    }
}