#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.Routing;

namespace Pomona.RequestProcessing
{
    public abstract class HandlerMethodInvoker<TInvokeState> : RouteAction, IHandlerMethodInvoker
        where TInvokeState : class, new()
    {
        protected HandlerMethodInvoker(HandlerMethod method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            Method = method;
        }


        public HandlerMethod Method { get; }

        public IList<HandlerParameter> Parameters
        {
            get { return Method.Parameters; }
        }


        public override bool CanProcess(PomonaContext context)
        {
            return true;
        }


        public override PomonaResponse Process(PomonaContext context)
        {
            return InvokeAndWrap(context);
        }


        protected virtual object OnGetArgument(HandlerParameter parameter, PomonaContext context, TInvokeState state)
        {
            if (parameter.IsResource)
            {
                var parentNode = context
                    .Node
                    .Ascendants()
                    .FirstOrDefault(x => x.ResultType == parameter.TypeSpec);
                if (parentNode != null)
                    return parentNode.Value;
            }

            if (parameter.Type == typeof(PomonaContext))
                return context;

            Exception innerEx = null;
            try
            {
                // Never get value of transformed type parameter from IOC container
                if (!parameter.Type.IsValueType && !parameter.IsTransformedType)
                    return context.Session.GetInstance(parameter.Type);
            }
            catch (Exception ex)
            {
                innerEx = ex;
            }
            throw new HandlerMethodInvocationException(context,
                                                       this,
                                                       string.Format(
                                                           "Unable to invoke handler {0}.{1}, don't know how to provide value for parameter {2}",
                                                           Method.MethodInfo.ReflectedType,
                                                           Method.Name,
                                                           parameter.Name),
                                                       innerEx);
        }


        protected virtual object OnInvoke(object target, PomonaContext context, TInvokeState state)
        {
            var args = new object[Parameters.Count];

            for (var i = 0; i < Parameters.Count; i++)
            {
                //else if (resourceIdArg != null && p.Type == resourceIdArg.GetType())
                //    args[i] = resourceIdArg;
                //else
                args[i] = OnGetArgument(Parameters[i], context, state);
            }

            return Method.MethodInfo.Invoke(target, args);
        }


        private PomonaResponse InvokeAndWrap(PomonaContext context,
                                             HttpStatusCode? statusCode = null)
        {
            var handler = context.Session.GetInstance(Method.MethodInfo.ReflectedType);
            var result = Invoke(handler, context);
            var resultAsResponse = result as PomonaResponse;
            if (resultAsResponse != null)
                return resultAsResponse;

            var responseBody = result;
            if (ReturnType == typeof(void))
                responseBody = PomonaResponse.NoBodyEntity;

            if (responseBody == PomonaResponse.NoBodyEntity)
                statusCode = HttpStatusCode.NoContent;

            return new PomonaResponse(context, responseBody, statusCode ?? HttpStatusCode.OK, context.ExpandedPaths);
        }


        public virtual object Invoke(object target, PomonaContext context)
        {
            return OnInvoke(target, context, new TInvokeState());
        }


        public Type ReturnType
        {
            get { return Method.ReturnType; }
        }
    }
}