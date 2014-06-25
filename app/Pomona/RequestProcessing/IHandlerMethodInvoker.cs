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

using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.RequestProcessing
{
    public class HandlerMethodTakingResourceId : HandlerMethodInvoker<object>
    {
        public HandlerMethodTakingResourceId(HandlerMethod method)
            : base(method)
        {
        }


        public override object OnGetArgument(HandlerParameter parameter, PomonaRequest request, object state)
        {
            var node = (ResourceNode)request.Node;
            var primaryIdType = node.ExpectedType.PrimaryId.PropertyType;
            if (parameter.Type == primaryIdType)
            {
                object parsedId;
                if (!node.Name.TryParse(primaryIdType, out parsedId))
                    throw new NotImplementedException("TODO: What to do when ID won't parse here?? (probably throw 404 yes)");
                return parsedId;
            }
            return base.OnGetArgument(parameter, request, state);
        }
    }


    public class HandlerMethodTakingExistingResource : HandlerMethodInvoker<object>
    {
        public HandlerMethodTakingExistingResource(HandlerMethod method)
            : base(method)
        {
        }


        public override object OnGetArgument(HandlerParameter parameter, PomonaRequest request, object state)
        {
            var resourceNode = request.Node as ResourceNode;
            if (parameter.IsResource && resourceNode != null && resourceNode.Exists
                && parameter.Type.IsInstanceOfType(resourceNode.Value))
            {
                return resourceNode.Value;
            }
            return base.OnGetArgument(parameter, request, state);
        }
    }

    public class HandlerMethodTakingFormInvoker : HandlerMethodInvoker<HandlerMethodTakingFormInvoker.InvokeState>
    {
        public HandlerMethodTakingFormInvoker(HandlerMethod method)
            : base(method)
        {
        }


        public class InvokeState
        {
            public object Form { get; set; }
        }

        protected override object OnInvoke(object target, PomonaRequest request, InvokeState state)
        {
            state.Form = request.Bind();
            return base.OnInvoke(target, request, state);
        }


        public override object OnGetArgument(HandlerParameter parameter, PomonaRequest request, InvokeState state)
        {
            if (parameter.IsResource && state.Form != null && parameter.Type.IsInstanceOfType(state.Form))
            {
                return state.Form;
            }
            return base.OnGetArgument(parameter, request, state);
        }
    }

    public interface IHandlerMethodInvoker
    {
        object Invoke(object target, PomonaRequest request);
        Type ReturnType { get; }
    }

    public abstract class HandlerMethodInvoker<TInvokeState> : IHandlerMethodInvoker
        where TInvokeState : class, new()
    {
        private readonly HandlerMethod method;


        protected HandlerMethodInvoker(HandlerMethod method)
        {
            if (method == null)
                throw new ArgumentNullException("method");
            this.method = method;
        }


        public HandlerMethod Method
        {
            get { return this.method; }
        }

        public Type ReturnType
        {
            get { return Method.ReturnType; }
        }

        protected virtual object OnInvoke(object target, PomonaRequest request, TInvokeState state)
        {
            var args = new object[Parameters.Count];


            for (var i = 0; i < Parameters.Count; i++)
            {
                //else if (resourceIdArg != null && p.Type == resourceIdArg.GetType())
                //    args[i] = resourceIdArg;
                //else
                args[i] = OnGetArgument(Parameters[i], request, state);
            }

            return Method.MethodInfo.Invoke(target, args);
        }

        public virtual object Invoke(object target, PomonaRequest request)
        {
            return OnInvoke(target, request, new TInvokeState());
        }

        public IList<HandlerParameter> Parameters
        {
            get { return Method.Parameters; }
        }


        public virtual object OnGetArgument(HandlerParameter parameter, PomonaRequest request, TInvokeState state)
        {
            if (parameter.IsResource)
            {
                var parentNode = request.Node.WalkTree(x => x.Parent)
                        .Skip(1).OfType<ResourceNode>()
                        .FirstOrDefault(x => x.Type == parameter.TypeSpec);
                if (parentNode != null)
                    return parentNode.Value;
            }

            if (parameter.Type == typeof(PomonaRequest))
                return request;
            else if (parameter.Type == typeof(NancyContext))
                return request.Context.NancyContext;
            else if (parameter.Type == typeof(TypeMapper))
                return request.TypeMapper;
            throw new InvalidOperationException(
                string.Format(
                    "Unable to invoke handler {0}.{1}, don't know how to provide value for parameter {2}",
                    Method.MethodInfo.ReflectedType,
                    Method.Name,
                    parameter.Name));
        }
    }
}