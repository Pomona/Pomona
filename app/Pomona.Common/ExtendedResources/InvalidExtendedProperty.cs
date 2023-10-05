#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;

using Pomona.Common.Proxies;

namespace Pomona.Common.ExtendedResources
{
    internal class InvalidExtendedProperty : ExtendedProperty
    {
        public InvalidExtendedProperty(PropertyInfo property, string errorMessage)
            : base(property)
        {
            ErrorMessage = errorMessage;
        }


        public string ErrorMessage { get; }


        public override object GetValue(object obj, IDictionary<string, IExtendedResourceProxy> cache)
        {
            throw new NotSupportedException(ErrorMessage);
        }


        public override void SetValue(object obj, object value, IDictionary<string, IExtendedResourceProxy> cache)
        {
            throw new NotSupportedException(ErrorMessage);
        }
    }
}

