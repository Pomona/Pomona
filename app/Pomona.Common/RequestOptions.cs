// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.Common.Web;

namespace Pomona.Common
{
    internal class RequestOptions : IRequestOptions
    {
        private readonly Type expectedResponseType;

        public Type ExpectedResponseType
        {
            get { return this.expectedResponseType; }
        }

        private readonly StringBuilder expandedPaths = new StringBuilder();

        private readonly List<Action<WebClientRequestMessage>> requestModifyActions =
            new List<Action<WebClientRequestMessage>>();


        internal RequestOptions(Type expectedResponseType = null)
        {
            this.expectedResponseType = expectedResponseType;
        }


        public string ExpandedPaths
        {
            get { return expandedPaths.ToString(); }
        }

        public void ApplyRequestModifications(WebClientRequestMessage request)
        {
            foreach (var action in requestModifyActions)
            {
                action(request);
            }
            if (!string.IsNullOrEmpty(ExpandedPaths))
            {
                request.Headers.Add("X-Pomona-Expand", ExpandedPaths);
            }
        }

        public IRequestOptions ModifyRequest(Action<WebClientRequestMessage> action)
        {
            requestModifyActions.Add(action);
            return this;
        }


        protected void Expand(LambdaExpression expression)
        {
            if (expandedPaths.Length > 0)
                expandedPaths.Append(',');
            expandedPaths.Append(expression.GetPropertyPath(true));
        }

        public static RequestOptions Create<T>(Action<IRequestOptions<T>> optionActions, Type expectedResponseType = null)
        {
            var requestOptions = new RequestOptions<T>(expectedResponseType);
            if (optionActions != null)
                optionActions(requestOptions);
            return requestOptions;
        }
    }

    internal class RequestOptions<T> : RequestOptions, IRequestOptions<T>
    {
        internal RequestOptions(Type expectedResponseType = null)
            : base(expectedResponseType)
        {
        }


        IRequestOptions<T> IRequestOptions<T>.ModifyRequest(Action<WebClientRequestMessage> action)
        {
            ModifyRequest(action);
            return this;
        }

        IRequestOptions<T> IRequestOptions<T>.Expand<TRetValue>(Expression<Func<T, TRetValue>> expression)
        {
            Expand(expression);
            return this;
        }

    }
}