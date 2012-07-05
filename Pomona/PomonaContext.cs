using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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

    public class PomonaContext
    {
        private readonly Type baseType;
        private readonly Func<object, string> uriResolver;
        private readonly bool debugMode;
        private readonly HashSet<string> expandedPaths;

        public PomonaContext(Type baseType, Func<object, string> uriResolver, string expandedPaths, bool debugMode, ClassMappingFactory classMappingFactory)
        {
            this.baseType = baseType;
            this.uriResolver = uriResolver;
            this.debugMode = debugMode;
            this.classMappingFactory = classMappingFactory;
            this.expandedPaths = ExpandPathsUtils.GetExpandedPaths(expandedPaths);
        }


        public bool DebugMode
        {
            get { return this.debugMode; }
        }

        internal HashSet<string> ExpandedPaths { get { return this.expandedPaths; } }


        internal bool PathToBeExpanded(string path)
        {
            return this.expandedPaths.Contains(path.ToLower());
        }


        public bool IsWrittenAsObject(Type type)
        {
            return this.baseType.IsAssignableFrom(type);

        }


        public ObjectWrapper CreateWrapperFor(object target, string path, IMappedType expectedBaseType)
        {
            return new ObjectWrapper(target, path, this, expectedBaseType);
        }

        private ClassMappingFactory classMappingFactory;

        public ClassMappingFactory ClassMappingFactory { get { return classMappingFactory; } }


        public string GetUri(object value)
        {
            return this.uriResolver(value);
        }


        public IMappedType GetClassMapping<T>()
        {
            return classMappingFactory.GetClassMapping<T>();
        }
    }
}