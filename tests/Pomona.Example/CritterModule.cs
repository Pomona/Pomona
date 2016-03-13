#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Net;

using Nancy.Validation;

using Pomona.Common;
using Pomona.Example.Models;
using Pomona.Nancy;

namespace Pomona.Example
{
    [PomonaConfiguration(typeof(CritterPomonaConfiguration))]
    public class CritterModule : PomonaModule
    {
        protected override PomonaError OnException(Exception exception)
        {
            if (exception is ModelValidationException)
                return new PomonaError(HttpStatusCode.BadRequest, new ErrorStatus(exception.Message, 1337));

            if (exception is ResourceValidationException)
            {
                var validationException = (ResourceValidationException)exception;
                return new PomonaError(HttpStatusCode.BadRequest,
                                       new ErrorStatus(validationException.Message,
                                                       0xdead,
                                                       validationException.MemberName));
            }

            if (exception is PomonaException)
                return base.OnException(exception);
            return new PomonaError(HttpStatusCode.InternalServerError,
                                   new ErrorStatus(exception.Message, -1, exception : exception.ToString()));
        }
    }
}