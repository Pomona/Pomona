#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright � 2012 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;

namespace Pomona
{
    public class PomonaContext
    {
        private readonly Type baseType;
        private readonly ClassMappingFactory classMappingFactory;
        private readonly bool debugMode;
        private readonly HashSet<string> expandedPaths;
        private readonly Func<object, string> uriResolver;


        public PomonaContext(
            Type baseType,
            Func<object, string> uriResolver,
            string expandedPaths,
            bool debugMode,
            ClassMappingFactory classMappingFactory)
        {
            this.baseType = baseType;
            this.uriResolver = uriResolver;
            this.debugMode = debugMode;
            this.classMappingFactory = classMappingFactory;
            this.expandedPaths = ExpandPathsUtils.GetExpandedPaths(expandedPaths);
        }


        public ClassMappingFactory ClassMappingFactory
        {
            get { return this.classMappingFactory; }
        }

        public bool DebugMode
        {
            get { return this.debugMode; }
        }

        internal HashSet<string> ExpandedPaths
        {
            get { return this.expandedPaths; }
        }


        public ObjectWrapper CreateWrapperFor(object target, string path, IMappedType expectedBaseType)
        {
            return new ObjectWrapper(target, path, this, expectedBaseType);
        }


        public IMappedType GetClassMapping<T>()
        {
            return this.classMappingFactory.GetClassMapping<T>();
        }


        public string GetUri(object value)
        {
            return this.uriResolver(value);
        }


        public bool IsWrittenAsObject(Type type)
        {
            return this.baseType.IsAssignableFrom(type);
        }


        internal bool PathToBeExpanded(string path)
        {
            return this.expandedPaths.Contains(path.ToLower());
        }
    }
}