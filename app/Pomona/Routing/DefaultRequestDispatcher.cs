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
using System.Linq;

using Nancy;

using Pomona.Common;
using Pomona.Common.Internals;

namespace Pomona.Routing
{
    public class DefaultRequestDispatcher : IRequestDispatcher
    {
        public PomonaResponse Dispatch(IPomonaContext context)
        {
            PathNode node = context.ResolvePath(context.NancyContext.Request.Url.Path);

            var pomonaRequest = context.CreateOuterRequest(node);

            if (!node.AllowedMethods.HasFlag(pomonaRequest.Method))
                ThrowMethodNotAllowedForType(pomonaRequest.Method, node.AllowedMethods);

            var response = context.Module.Pipeline.Process(pomonaRequest);
            if (response == null)
                throw new PomonaException("Unable to find RequestProcessor able to handle request.");

            return response;
        }


        private static void ThrowMethodNotAllowedForType(HttpMethod requestMethod, HttpMethod allowedMethods)
        {
            var httpMethods = Enum.GetValues(typeof(HttpMethod))
                .Cast<HttpMethod>()
                .Where(x => allowedMethods.HasFlag(x))
                .Select(x => x.ToString().ToUpper());

            var allowedMethodsString = String.Join(", ", httpMethods);

            var allowHeader = new KeyValuePair<string, string>("Allow", allowedMethodsString);

            throw new PomonaServerException(string.Format("Method {0} not allowed!", requestMethod.ToString().ToUpper()),
                                      null,
                                      HttpStatusCode.MethodNotAllowed,
                                      allowHeader.WrapAsEnumerable());
        }
    }
}