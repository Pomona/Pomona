#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.Queries
{
    public class QueryTypeResolver : IQueryTypeResolver
    {
        private static readonly Dictionary<string, Type> nativeTypes =
            TypeUtils.GetNativeTypes().ToDictionary(x => x.Name.ToLower(), x => x);

        private readonly ITypeResolver typeMapper;


        public QueryTypeResolver(ITypeResolver typeMapper)
        {
            if (typeMapper == null)
                throw new ArgumentNullException(nameof(typeMapper));
            this.typeMapper = typeMapper;
        }

        #region Implementation of IQueryTypeResolver

        public bool TryResolveProperty<TProperty>(Type type, string propertyPath, out TProperty property) where TProperty : PropertySpec
        {
            // TODO: Proper exception handling when type is not TransformedType [KNS]
            property = null;
            var transformedType = (StructuredType)this.typeMapper.FromType(type);
            PropertySpec uncastProperty;
            return transformedType.TryGetPropertyByName(propertyPath,
                                                        StringComparison.InvariantCultureIgnoreCase,
                                                        out uncastProperty) && (property = uncastProperty as TProperty) != null;
        }


        public Type ResolveType(string typeName)
        {
            Type type;

            if (typeName.EndsWith("?"))
                return typeof(Nullable<>).MakeGenericType(ResolveType(typeName.Substring(0, typeName.Length - 1)));

            if (nativeTypes.TryGetValue(typeName.ToLower(), out type))
                return type;

            return this.typeMapper.FromType(typeName).Type;
        }

        #endregion
    }
}