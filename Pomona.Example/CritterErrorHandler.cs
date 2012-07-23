using System;
using System.IO;
using System.Linq;

using Nancy;
using Nancy.ErrorHandling;

namespace Pomona.Example
{
    public class CritterErrorHandler : IErrorHandler
    {
        private readonly HttpStatusCode[] _supportedStatusCodes = new[]
        {
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError
        };

        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return this._supportedStatusCodes.Any(s => s == statusCode);
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            var resp = new Response();
            resp.StatusCode = HttpStatusCode.InternalServerError;

            object exceptionObject;
            context.Items.TryGetValue("ERROR_EXCEPTION", out exceptionObject);

            Exception exception = exceptionObject as Exception;

            // We're not that interested in Nancys exception really
            if (exception is RequestExecutionException)
                exception = exception.InnerException;

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
            resp.StatusCode = HttpStatusCode.OK;
            context.Response = resp;
        }
    }
}