#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;

namespace Pomona.Common.TypeSystem
{
    public class DictionaryTypeSpec : RuntimeTypeSpec
    {
        private readonly Lazy<TypeSpec> keyType;
        private readonly Lazy<TypeSpec> valueType;


        public DictionaryTypeSpec(ITypeResolver typeResolver,
                                  Type type,
                                  Func<TypeSpec> keyType,
                                  Func<TypeSpec> valueType)
            : base(typeResolver, type)
        {
            if (keyType == null)
                throw new ArgumentNullException(nameof(keyType));
            if (valueType == null)
                throw new ArgumentNullException(nameof(valueType));
            this.keyType = CreateLazy(keyType);
            this.valueType = CreateLazy(valueType);
        }


        public override bool IsDictionary => true;

        public virtual TypeSpec KeyType => this.keyType.Value;

        public override TypeSerializationMode SerializationMode => TypeSerializationMode.Dictionary;

        public virtual TypeSpec ValueType => this.valueType.Value;


        public new static ITypeFactory GetFactory()
        {
            return new DictionaryTypeSpecFactory();
        }

        #region Nested type: DictionaryTypeSpecFactory

        internal class DictionaryTypeSpecFactory : ITypeFactory
        {
            public TypeSpec CreateFromType(ITypeResolver typeResolver, Type type)
            {
                Type[] typeArgs;
                if (!type.TryExtractTypeArguments(typeof(IDictionary<,>), out typeArgs))
                    return null;
                var keyType = typeArgs[0];
                var valueType = typeArgs[1];
                return new DictionaryTypeSpec(typeResolver,
                                              type,
                                              () => typeResolver.FromType(keyType),
                                              () => typeResolver.FromType(valueType));
            }


            public int Priority => 0;
        }

        #endregion
    }
}