#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Pomona.Common.TypeSystem
{
    public class EnumTypeSpec : RuntimeTypeSpec
    {
        private readonly Lazy<IDictionary<string, long>> enumValues;


        public EnumTypeSpec(ITypeResolver typeResolver, Type type, Func<IEnumerable<TypeSpec>> genericArguments = null)
            : base(typeResolver, type, genericArguments)
        {
            this.enumValues =
                CreateLazy(
                    () =>
                        (IDictionary<string, long>)new ReadOnlyDictionary<string, long>(
                        Enum.GetValues(type).Cast<object>().ToDictionary(x => Enum.GetName(type, x), Convert.ToInt64)));
        }


        public IDictionary<string, long> EnumValues => this.enumValues.Value;

        public override TypeSerializationMode SerializationMode => TypeSerializationMode.Value;


        public new static ITypeFactory GetFactory()
        {
            return new EnumTypeSpecFactory();
        }


        public class EnumTypeSpecFactory : ITypeFactory
        {
            public TypeSpec CreateFromType(ITypeResolver typeResolver, Type type)
            {
                if (typeResolver == null)
                    throw new ArgumentNullException(nameof(typeResolver));
                if (type == null)
                    throw new ArgumentNullException(nameof(type));

                if (!type.IsEnum)
                    return null;

                return new EnumTypeSpec(typeResolver, type);
            }


            public int Priority => -400;
        }
    }
}
