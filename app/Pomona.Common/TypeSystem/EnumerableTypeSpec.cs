#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Common.TypeSystem
{
    public class EnumerableTypeSpec : RuntimeTypeSpec
    {
        private readonly Lazy<TypeSpec> itemType;


        private EnumerableTypeSpec(ITypeResolver typeResolver, Type type, Lazy<TypeSpec> itemType)
            : base(typeResolver, type)
        {
            if (itemType == null)
                throw new ArgumentNullException(nameof(itemType));
            this.itemType = itemType;
        }


        public override TypeSpec ElementType
        {
            get { return ItemType; }
        }

        public override bool IsAlwaysExpanded
        {
            get { return false; }
        }

        public override bool IsCollection
        {
            get { return true; }
        }

        public virtual TypeSpec ItemType
        {
            get { return this.itemType.Value; }
        }


        public new static ITypeFactory GetFactory()
        {
            return new EnumerableTypeSpecFactory();
        }


        protected override TypeSerializationMode OnLoadSerializationMode()
        {
            if (Type == typeof(byte[]))
                return TypeSerializationMode.Value;
            return TypeSerializationMode.Array;
        }


        public class EnumerableTypeSpecFactory : ITypeFactory
        {
            public TypeSpec CreateFromType(ITypeResolver typeResolver, Type type)
            {
                if (typeResolver == null)
                    throw new ArgumentNullException(nameof(typeResolver));
                if (type == null)
                    throw new ArgumentNullException(nameof(type));

                Type itemTypeLocal;
                if (type == typeof(string) || !type.TryGetEnumerableElementType(out itemTypeLocal))
                    return null;

                return new EnumerableTypeSpec(typeResolver, type,
                                              CreateLazy(() => typeResolver.FromType(itemTypeLocal)));
            }


            public int Priority
            {
                get { return 50; }
            }
        }
    }
}