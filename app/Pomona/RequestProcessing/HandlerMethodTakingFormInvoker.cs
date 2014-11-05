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
using System.Linq;

namespace Pomona.RequestProcessing
{
    internal class HandlerMethodTakingFormInvoker : HandlerMethodInvoker<HandlerMethodTakingFormInvoker.InvokeState>
    {
        private readonly HandlerParameter formParameter;
        private readonly HandlerParameter targetResourceParameter;


        public HandlerMethodTakingFormInvoker(HandlerMethod method, HandlerParameter formParameter, HandlerParameter targetResourceParameter = null)
            : base(method)
        {
            if (formParameter == null)
                throw new ArgumentNullException("formParameter");
            if (formParameter.Method != method)
                throw new ArgumentException("Parameter provided does not belong to method.", "formParameter");
            if (targetResourceParameter != null && targetResourceParameter.Method != method)
                throw new ArgumentException("Parameter provided does not belong to method.", "targetResourceParameter");
            this.formParameter = formParameter;
            this.targetResourceParameter = targetResourceParameter;
        }


        protected override object OnGetArgument(HandlerParameter parameter, PomonaRequest request, InvokeState state)
        {
            if (parameter == targetResourceParameter)
                return request.Node.Value;
            if (parameter == formParameter)
                return state.Form;
            return base.OnGetArgument(parameter, request, state);
        }


        public override bool CanProcess(PomonaRequest request)
        {
            object form;
            return request.TryBindAsType(formParameter.TypeSpec, out form);
        }


        protected override object OnInvoke(object target, PomonaRequest request, InvokeState state)
        {
            state.Form = request.Bind(formParameter.TypeSpec);
            return base.OnInvoke(target, request, state);
        }

        #region Nested type: InvokeState

        public class InvokeState
        {
            public object Form { get; set; }
        }

        #endregion
    }
}