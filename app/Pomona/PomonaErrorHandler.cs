#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Net;
using System.Reflection;

using Pomona.Common;

namespace Pomona
{
    public class PomonaErrorHandler : IPomonaErrorHandler
    {
        protected virtual PomonaError OnException(Exception exception)
        {
            if (exception is PomonaSerializationException)
                return new PomonaError(HttpStatusCode.BadRequest, exception.Message);
            var pomonaException = exception as PomonaServerException;

            return pomonaException != null
                ? new PomonaError(pomonaException.StatusCode, pomonaException.Entity ?? pomonaException.Message)
                : new PomonaError(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString());
        }


        protected virtual Exception UnwrapException(Exception exception)
        {
            if (exception is TargetInvocationException || exception is AggregateException)
                return exception.InnerException != null ? UnwrapException(exception.InnerException) : exception;

            return exception;
        }


        public PomonaError HandleException(Exception exception)
        {
            return OnException(UnwrapException(exception));
        }
    }
}