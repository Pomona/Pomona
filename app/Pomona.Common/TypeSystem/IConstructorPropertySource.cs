#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Reflection;

namespace Pomona.Common.TypeSystem
{
    public interface IConstructorPropertySource : IConstructorControl
    {
        TProperty GetValue<TProperty>(PropertyInfo propertyInfo, Func<TProperty> defaultFactory);
    }
}
