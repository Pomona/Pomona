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

using Nancy;

using Pomona.Common;

namespace Pomona.Routing
{
    internal class PomonaEngine
    {
        private readonly IPomonaSession session;


        public PomonaEngine(IPomonaSession session)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            this.session = session;
        }


        public PomonaResponse Handle(NancyContext context, string modulePath)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (modulePath == null)
                throw new ArgumentNullException("modulePath");

            HttpMethod httpMethod =
                (HttpMethod)Enum.Parse(typeof(HttpMethod), context.Request.Method, true);

            var moduleRelativePath = context.Request.Path.Substring(modulePath.Length);
            var request = new PomonaRequest(context.Request.Url.ToString(), moduleRelativePath, httpMethod,
                                            context.Request.Headers, context.Request.Body,
                                            context.Request.Query);

            return this.session.Dispatch(request);
        }
    }
}