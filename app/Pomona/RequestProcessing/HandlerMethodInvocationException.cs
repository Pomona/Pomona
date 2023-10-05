#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Runtime.Serialization;

using Nancy;

namespace Pomona.RequestProcessing
{
    [Serializable]
    public class HandlerMethodInvocationException : PomonaServerException
    {
        protected HandlerMethodInvocationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        internal HandlerMethodInvocationException(PomonaContext context, IHandlerMethodInvoker invoker, string message)
            : this(context, invoker, message, null)
        {
        }


        internal HandlerMethodInvocationException(PomonaContext context,
                                                  IHandlerMethodInvoker invoker,
                                                  string message,
                                                  Exception innerException)
            : base(message, innerException, HttpStatusCode.InternalServerError)
        {
        }
    }
}

