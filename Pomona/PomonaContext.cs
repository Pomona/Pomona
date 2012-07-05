using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Pomona
{
    public class ClassMappingFactory
    {
        public ClassMapping GetClassMapping<T>()
        {
            var type = typeof(T);

            return GetClassMapping(type);
        }


        public ClassMapping GetClassMapping(Type type)
        {
            var classDefinition = new ClassMapping(type);
            classDefinition.FillWithType(type);

            return classDefinition;
        }



    }

    public class PomonaContext
    {
        private readonly Type baseType;
        private readonly Func<object, string> uriResolver;
        private readonly bool debugMode;
        private readonly HashSet<string> expandedPaths;

        public PomonaContext(Type baseType, Func<object, string> uriResolver, string expandedPaths, bool debugMode)
        {
            this.baseType = baseType;
            this.uriResolver = uriResolver;
            this.debugMode = debugMode;
            this.expandedPaths = ExpandPathsUtils.GetExpandedPaths(expandedPaths);
            this.classMappingFactory = new ClassMappingFactory();
        }


        public bool DebugMode
        {
            get { return this.debugMode; }
        }

        internal HashSet<string> ExpandedPaths { get { return this.expandedPaths; } }


        internal bool PathToBeExpanded(string path)
        {
            return this.expandedPaths.Contains(path);
        }


        public bool IsWrittenAsObject(Type type)
        {
            return this.baseType.IsAssignableFrom(type);

        }


        public ObjectWrapper CreateWrapperFor(object target, string path, Type expectedBaseType)
        {
            Func<object, string, PomonaContext, Type, ObjectWrapper> creator = GetObjectWrapperConstructor(target.GetType());
            return creator(target, path, this, expectedBaseType);
        }

        private readonly Dictionary<Type, Func<object, string, PomonaContext, Type, ObjectWrapper>> objectWrapperConstructorCache = new Dictionary<Type, Func<object, string, PomonaContext, Type, ObjectWrapper>>();
        private ClassMappingFactory classMappingFactory;


        private Func<object, string, PomonaContext, Type, ObjectWrapper> GetObjectWrapperConstructor(Type targetType)
        {
            Func<object, string, PomonaContext, Type, ObjectWrapper> creator;

            if (!objectWrapperConstructorCache.TryGetValue(targetType, out creator))
            {
                var ctor = typeof(ObjectWrapper<>).MakeGenericType(targetType).GetConstructor(
                    new Type[] { targetType, typeof(string), typeof(PomonaContext), typeof(Type) });

                var targetParam = Expression.Parameter(typeof(object), "target");
                var pathParam = Expression.Parameter(typeof(string), "path");
                var contextParam = Expression.Parameter(typeof(PomonaContext), "context");
                var expectedBaseTypeParam = Expression.Parameter(typeof(Type), "expectedBaseType");

                var expression = Expression.Lambda<Func<object, string, PomonaContext, Type, ObjectWrapper>>(
                    Expression.New(ctor, Expression.Convert(targetParam, targetType), pathParam, contextParam, expectedBaseTypeParam), targetParam, pathParam, contextParam, expectedBaseTypeParam);

                creator = expression.Compile();

                objectWrapperConstructorCache[targetType] = creator;
            }

            return creator;
        }


        public string GetUri(object value)
        {
            return this.uriResolver(value);
        }


        public ClassMapping GetClassMapping<T>()
        {
            return classMappingFactory.GetClassMapping<T>();
        }
    }
}