#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
using System.Reflection;

using Pomona.Common.Proxies;

namespace Pomona.Common.Serialization.Patch
{
    public class RepositoryDeltaProxyBase<TElement, TRepository> : CollectionDelta<TElement>, IDelta<TRepository>
    {

        protected RepositoryDeltaProxyBase()
            : base()
        {
        }


        public new TRepository Original
        {
            get { return (TRepository)base.Original; }
        }


        protected virtual TPropType OnGet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
        {
            return property.Get((TOwner)base.Original);
        }


        protected virtual object OnInvokeMethod(MethodInfo methodInfo, object[] args)
        {
            if (methodInfo.Name == "Post" && args.Length == 1)
            {
                Add((TElement)args[0]);
                return null;
            }
            if (methodInfo.Name == "Delete" && args.Length == 1)
            {
                Remove((TElement)args[0]);
                return null;
            }
            throw new NotImplementedException();
        }


        protected virtual void OnSet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property, TPropType value)
        {
            throw new NotSupportedException("Setting property " + property.Name
                                            + " is not supported through delta proxy.");
        }
    }
}