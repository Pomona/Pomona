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
using Pomona.Common.TypeSystem;

namespace Pomona.RequestProcessing
{
    public class DefaultHandlerMethodInvoker : HandlerMethodInvoker
    {
        public DefaultHandlerMethodInvoker(HandlerMethod method)
            : base(method)
        {
        }


        public IList<HandlerParameter> Parameters
        {
            get { return Method.Parameters; }
        }


        public override object Invoke(object target, PomonaRequest request)
        {
            var args = new object[Parameters.Count];
            object resourceArg = null;
            object resourceIdArg = null;
            var httpMethod = request.Method;

            ResourceType parentResourceType = null;

            if (request.Node.NodeType == PathNodeType.Resource)
            {
                switch (httpMethod)
                {
                    case HttpMethod.Get:
                    {
                        var resourceNode = (ResourceNode)request.Node;
                        object parsedId;
                        if (!resourceNode.Name.TryParse(resourceNode.Type.PrimaryId.PropertyType, out parsedId) &&
                            !typeof(IQueryable<object>).IsAssignableFrom(Method.ReturnType))
                            throw new NotImplementedException("What to do when ID won't parse here??");

                        resourceIdArg = parsedId;
                    }
                        break;
                    case HttpMethod.Patch:
                    case HttpMethod.Post:
                        resourceArg = request.Bind();
                        break;
                    default:
                        resourceArg = request.Node.Value;
                        break;
                }
            }
            else if (request.Node.NodeType == PathNodeType.Collection)
            {
                switch (httpMethod)
                {
                    case HttpMethod.Post:
                        resourceArg = request.Bind();
                        break;
                }
            }

            // If the method returns an IQueryable<Object> and takes a parent resource parameter,
            // check that the parameter is actually the parent resource type of the resouce type.
            if (typeof(IQueryable<Object>).IsAssignableFrom(Method.ReturnType))
            {
                var resourceType = request.Node.Type as ResourceType;
                if (resourceType != null)
                    parentResourceType = resourceType.ParentResourceType;
                var resourceCount = Parameters.Count(x => x.IsResource);
                var resourceParameter = Parameters.FirstOrDefault(x => x.IsResource);

                if (resourceCount == 0 && parentResourceType != null)
                {
                    throw new PomonaException("Type " + request.Node.Type.Name +
                                              " has the parent resource type " +
                                              parentResourceType.Name +
                                              ", but no parent element was specified.");
                }

                if (resourceCount == 1)
                {
                    if (parentResourceType == null)
                    {
                        throw new PomonaException("Type " + request.Node.Type.Name +
                                                  " has no parent resource type, but a parent element of type " +
                                                  resourceParameter.Type.Name +
                                                  " was specified.");
                    }

                    if (parentResourceType != resourceParameter.Type)
                    {
                        throw new PomonaException("Type " + request.Node.Type.Name +
                                                  " has the parent resource type " +
                                                  parentResourceType.Name +
                                                  ", but a parent element of type " + resourceParameter.Type.Name +
                                                  " was specified.");
                    }

                    resourceArg = request.Node.Parent.Value;
                }
            }

            for (var i = 0; i < Parameters.Count; i++)
            {
                var p = Parameters[i];

                if (p.IsResource && p.Type.IsInstanceOfType(resourceArg))
                    args[i] = resourceArg;
                else if (p.Type == typeof(PomonaRequest))
                    args[i] = request;
                else if (p.Type == typeof(NancyContext))
                    args[i] = request.NancyContext;
                else if (p.Type == typeof(TypeMapper))
                    args[i] = request.TypeMapper;
                else if (resourceIdArg != null && p.Type == resourceIdArg.GetType())
                    args[i] = resourceIdArg;
                else
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Unable to invoke handler {0}.{1}, don't know how to provide value for parameter {2}",
                            Method.MethodInfo.ReflectedType,
                            Method.Name,
                            p.Name));
                }
            }

            return Method.MethodInfo.Invoke(target, args);
        }
    }
}