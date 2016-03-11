#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Nancy;
using Nancy.ErrorHandling;

namespace Pomona
{
    public class DefaultErrorHandler : IStatusCodeHandler
    {
        private readonly HttpStatusCode[] _supportedStatusCodes = new[]
        {
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.PreconditionFailed,
            HttpStatusCode.InternalServerError
        };

        #region IErrorHandler Members

        public virtual void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            object errorHandled;
            if (context.Items.TryGetValue("ERROR_HANDLED", out errorHandled) && (errorHandled as bool? ?? false))
                return;

            object exceptionObject;
            if (!context.Items.TryGetValue("ERROR_EXCEPTION", out exceptionObject))
                return;

            var exception = UnwrapException((Exception)exceptionObject);

            // We're not that interested in Nancys exception really
            if (exception is RequestExecutionException)
                exception = exception.InnerException;

            if (exception is ResourceNotFoundException)
            {
                context.Response = new NotFoundResponse();
                return;
            }

            if (exception is ResourcePreconditionFailedException)
            {
                context.Response = new Response
                {
                    StatusCode = HttpStatusCode.PreconditionFailed,
                    ContentType = "text/html"
                };
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


        public virtual bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return this._supportedStatusCodes.Any(s => s == statusCode);
        }


        protected virtual Exception UnwrapException(Exception exception)
        {
            if (exception is TargetInvocationException || exception is RequestExecutionException)
                return exception.InnerException != null ? UnwrapException(exception.InnerException) : exception;
            return exception;
        }

        #endregion
    }
}