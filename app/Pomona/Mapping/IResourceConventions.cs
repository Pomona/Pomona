#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pomona.Mapping
{
    public interface IResourceConventions
    {
        PropertyInfo GetChildToParentProperty(Type type);
        PropertyInfo GetParentToChildProperty(Type type);
        string GetPluralNameForType(Type type);
        Type GetPostReturnType(Type type);
        IEnumerable<Type> GetResourceHandlers(Type type);


        /// <summary>
        /// This returns what URI this type will be mapped to.
        /// For example if this method returns the type Animal when passed Dog
        /// it means that dogs will be available on same url as Animal.
        /// (ie. http://somehost/animal/{id}, not http://somehost/dog/{id})
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Type GetUriBaseType(Type type);


        string GetUrlRelativePath(Type type);
        bool TypeIsExposedAsRepository(Type type);
        bool TypeIsSingletonResource(Type type);
    }
}
