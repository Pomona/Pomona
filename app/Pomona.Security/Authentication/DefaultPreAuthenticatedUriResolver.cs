#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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

namespace Pomona.Security.Authentication
{
    public class DefaultPreAuthenticatedUriResolver : IPreAuthenticatedUriResolver
    {
        private readonly IPreAuthenticatedUriProvider authenticatedUrlHelper;
        private readonly IUriResolver uriResolver;


        public DefaultPreAuthenticatedUriResolver(IUriResolver uriResolver,
                                                  IPreAuthenticatedUriProvider authenticatedUrlHelper)
        {
            if (uriResolver == null)
                throw new ArgumentNullException("uriResolver");
            if (authenticatedUrlHelper == null)
                throw new ArgumentNullException("authenticatedUrlHelper");
            this.uriResolver = uriResolver;
            this.authenticatedUrlHelper = authenticatedUrlHelper;
        }


        public string GetPreAuthenticatedUriFor(object entity, DateTime? expiration = null)
        {
            return this.authenticatedUrlHelper.CreatePreAuthenticatedUrl(this.uriResolver.GetUriFor(entity), expiration);
        }
    }
}