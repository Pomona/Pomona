#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

namespace Pomona.Common
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Interface)]
    public class ResourcePropertyAttribute : Attribute
    {
        public HttpMethod AccessMode { get; set; }
        public HttpMethod ItemAccessMode { get; set; }
        public bool Required { get; set; }
    }
}
