#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq.Expressions;

using Pomona.Common.TypeSystem;

namespace Pomona.FluentMapping
{
    public static class PropertyOptionsBuilderExtensions
    {
        public static IPropertyOptionsBuilder<TDeclaringType, TPropertyType> Expand
            <TDeclaringType, TPropertyType>(this IPropertyOptionsBuilder<TDeclaringType, TPropertyType> builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            return builder.Expand(ExpandMode.Full);
        }


        public static IPropertyOptionsBuilder<TDeclaringType, TPropertyType> ExpandShallow
            <TDeclaringType, TPropertyType>(this IPropertyOptionsBuilder<TDeclaringType, TPropertyType> builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            return builder.Expand(ExpandMode.Shallow);
        }


        public static IPropertyOptionsBuilder<TDeclaringType, TPropertyType> OnGetAndQuery
            <TDeclaringType, TPropertyType>(this IPropertyOptionsBuilder<TDeclaringType, TPropertyType> builder,
                                            Expression<Func<TDeclaringType, TPropertyType>> getter)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            return builder.OnQuery(getter).OnGet(getter.Compile());
        }
    }
}

