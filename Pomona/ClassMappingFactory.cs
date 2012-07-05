using System;
using System.Collections.Generic;

namespace Pomona
{
    public class ClassMappingFactory
    {
        private Dictionary<Type, IMappedType> mappings = new Dictionary<Type, IMappedType>();

        public IMappedType GetClassMapping<T>()
        {
            var type = typeof(T);

            return GetClassMapping(type);
        }


        public IMappedType GetClassMapping(Type type)
        {
            IMappedType mappedType;
            if (!mappings.TryGetValue(type, out mappedType))
            {
                mappedType = CreateClassMapping(type);
            }

            return mappedType;
        }

        private IMappedType CreateClassMapping(Type type)
        {
            if (type.Assembly == typeof(System.String).Assembly)
            {
                if (type.IsGenericType)
                {
                    var newSharedType = new SharedType(type.GetGenericTypeDefinition(), this);
                    foreach (var genericTypeArg in type.GetGenericArguments())
                    {
                        if (genericTypeArg == type)
                        {
                            // Special case, self referencing generics
                            newSharedType.GenericArguments.Add(newSharedType);
                        }
                        else
                        {
                            newSharedType.GenericArguments.Add(GetClassMapping(genericTypeArg));
                        }
                    }
                    return newSharedType;
                }
                return new SharedType(type, this);
            }

            if (type.Namespace == "Pomona.TestModel")
            {
                var classDefinition = new TransformedType(type, type.Name, this);

                // Add to cache before filling out, in case of self-references
                mappings[type] = classDefinition;

                classDefinition.FillWithType(type);

                return classDefinition;
            }

            throw new InvalidOperationException("Don't know how to map " + type.FullName);
        }



    }
}