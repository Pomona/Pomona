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
using System.IO;
using System.Linq;

using Nancy;
using Nancy.ErrorHandling;

namespace Pomona
{
    public class ErrorHandler : IErrorHandler
    {
        private readonly HttpStatusCode[] _supportedStatusCodes = new[]
        {
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError
        };

        #region IErrorHandler Members

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            object exceptionObject;
            context.Items.TryGetValue("ERROR_EXCEPTION", out exceptionObject);

            var exception = exceptionObject as Exception;

            // We're not that interested in Nancys exception really
            if (exception is RequestExecutionException)
                exception = exception.InnerException;

            if (exception is ResourceNotFoundException)
            {
                context.Response = new NotFoundResponse();
                return;
            }

            var resp = new Response();
            object errorTrace;
            context.Items.TryGetValue("ERROR_TRACE", out errorTrace);

            resp.Contents = stream =>
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    if (exception != null)
                    {
                        streamWriter.WriteLine("Exception:");
                        streamWriter.WriteLine(exception);
                    }
                    if (errorTrace != null)
                    {
                        streamWriter.WriteLine("Trace:");
                        streamWriter.WriteLine(errorTrace);
                    }
                    streamWriter.WriteLine("Ey.. Got an exception there matey!!");
                }
            };
            resp.ContentType = "text/plain";
            resp.StatusCode = HttpStatusCode.InternalServerError;
            context.Response = resp;
        }


        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return this._supportedStatusCodes.Any(s => s == statusCode);
        }

        #endregion
    }
}