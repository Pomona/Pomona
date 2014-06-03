#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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

namespace Pomona.Common.Proxies
{
    public abstract class LazyCollectionProxy : ILazyProxy, IHasResourceUri
    {
        protected readonly IPomonaClient clientBase;
        protected readonly string uri;


        protected LazyCollectionProxy(string uri, IPomonaClient clientBase)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            this.uri = uri;
            this.clientBase = clientBase;
        }


        public abstract bool IsLoaded { get; }

        public string Uri
        {
            get { return this.uri; }
            set { }
        }


        internal static object CreateForType(Type collectionType, string uri, IPomonaClient clientBase)
        {
            Type[] genArgs;
            if (collectionType.TryExtractTypeArguments(typeof(ISet<>), out genArgs))
            {
                return Activator.CreateInstance(typeof(LazySetProxy<>).MakeGenericType(genArgs), uri, clientBase);
            }
            if (collectionType.TryExtractTypeArguments(typeof(IEnumerable<>), out genArgs))
            {
                return Activator.CreateInstance(typeof(LazyListProxy<>).MakeGenericType(genArgs), uri, clientBase);
            }
            throw new NotSupportedException("Unable to create lazy list proxy for collection type " + collectionType);
        }
    }
}