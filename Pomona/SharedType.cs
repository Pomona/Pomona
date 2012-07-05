using System;
using System.Collections.Generic;

namespace Pomona
{
    /// <summary>
    /// Represents a type that is shared between server and client.
    /// strings, integers etc.. mapped like this
    /// </summary>
    public class SharedType : IMappedType
    {
        public SharedType(Type targetType, ClassMappingFactory classMappingFactory)
        {
            if (targetType == null) throw new ArgumentNullException("targetType");
            if (classMappingFactory == null) throw new ArgumentNullException("classMappingFactory");
            this.targetType = targetType;
            this.classMappingFactory = classMappingFactory;
            GenericArguments = new List<IMappedType>();
        }

        // If this is a generic type its the GenericTypeDefinition! Not an instance, that is defined by IMappedType list GenericArguments
        private readonly Type targetType;
        private readonly ClassMappingFactory classMappingFactory;

        public Type TargetType { get { return targetType; } }

        public string Name
        {
            get { return targetType.Name; }
        }

        public bool IsGenericType
        {
            get { return targetType.IsGenericType; }
        }

        public bool IsGenericTypeDefinition
        {
            get { return false; }
        }

        public IList<IMappedType> GenericArguments { get; private set; }

        public IMappedType BaseType
        {
            get { return (SharedType)classMappingFactory.GetClassMapping(targetType.BaseType); }
        }
    }
}