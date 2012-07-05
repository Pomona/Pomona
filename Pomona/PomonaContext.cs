using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Pomona
{
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