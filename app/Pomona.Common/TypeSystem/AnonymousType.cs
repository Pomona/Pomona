#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;

namespace Pomona.Common.TypeSystem
{
    public class AnonymousType : StructuredType
    {
        public AnonymousType(IStructuredTypeResolver typeResolver,
                             Type type,
                             Func<IEnumerable<TypeSpec>> genericArguments = null)
            : base(typeResolver, type, genericArguments)
        {
        }


        public new static ITypeFactory GetFactory()
        {
            return new TypeFactory();
        }


        private class TypeFactory : ITypeFactory
        {
            public TypeSpec CreateFromType(ITypeResolver typeResolver, Type type)
            {
                if (typeResolver == null)
                    throw new ArgumentNullException(nameof(typeResolver));
                var structuredTypeResolver = typeResolver as IStructuredTypeResolver;
                if (structuredTypeResolver == null)
                {
                    throw new ArgumentException("typerResolver must be of type " + typeof(IStructuredTypeResolver).FullName,
                                                nameof(typeResolver));
                }
                if (type == null)
                    throw new ArgumentNullException(nameof(type));
                if (!type.IsAnonymous())
                    return null;

                return new AnonymousType(structuredTypeResolver, type);
            }


            public int Priority
            {
                get { return 0; }
            }
        }
    }
}