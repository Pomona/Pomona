#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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
    public class FetchContext
    {
        private readonly bool debugMode;
        private readonly HashSet<string> expandedPaths;
        private readonly PomonaSession session;
        private readonly TypeMapper typeMapper;
        private readonly Func<object, string> uriResolver;


        public FetchContext(
            Func<object, string> uriResolver,
            string expandedPaths,
            bool debugMode,
            PomonaSession session)
        {
            this.uriResolver = uriResolver;
            this.debugMode = debugMode;
            this.session = session;
            this.typeMapper = session.TypeMapper;
            this.expandedPaths = ExpandPathsUtils.GetExpandedPaths(expandedPaths);
        }


        public bool DebugMode
        {
            get { return this.debugMode; }
        }

        public TypeMapper TypeMapper
        {
            get { return this.typeMapper; }
        }

        internal HashSet<string> ExpandedPaths
        {
            get { return this.expandedPaths; }
        }


        public ObjectWrapper CreateWrapperFor(object target, string path, IMappedType expectedBaseType)
        {
            return new ObjectWrapper(target, path, this, expectedBaseType);
        }


        public string GetUri(object value)
        {
            return this.uriResolver(value);
        }


        internal bool PathToBeExpanded(string path)
        {
            return this.expandedPaths.Contains(path.ToLower());
        }
    }
}