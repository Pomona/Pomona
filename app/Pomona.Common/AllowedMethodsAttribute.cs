#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Common
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
    public class AllowedMethodsAttribute : Attribute
    {
        public AllowedMethodsAttribute(HttpMethod methods)
        {
            Methods = methods;
        }


        public HttpMethod Methods { get; }
    }
}