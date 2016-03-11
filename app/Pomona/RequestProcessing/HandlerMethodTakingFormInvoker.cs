#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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

namespace Pomona.RequestProcessing
{
    internal class HandlerMethodTakingFormInvoker : HandlerMethodInvoker<HandlerMethodTakingFormInvoker.InvokeState>
    {
        private readonly HandlerParameter formParameter;
        private readonly HandlerParameter targetResourceParameter;


        public HandlerMethodTakingFormInvoker(HandlerMethod method,
                                              HandlerParameter formParameter,
                                              HandlerParameter targetResourceParameter = null)
            : base(method)
        {
            if (formParameter == null)
                throw new ArgumentNullException(nameof(formParameter));
            if (formParameter.Method != method)
                throw new ArgumentException("Parameter provided does not belong to method.", nameof(formParameter));
            if (targetResourceParameter != null && targetResourceParameter.Method != method)
                throw new ArgumentException("Parameter provided does not belong to method.", nameof(targetResourceParameter));
            this.formParameter = formParameter;
            this.targetResourceParameter = targetResourceParameter;
        }


        public override bool CanProcess(PomonaContext context)
        {
            object form;
            return context.TryBindAsType(this.formParameter.TypeSpec, out form);
        }


        protected override object OnGetArgument(HandlerParameter parameter, PomonaContext context, InvokeState state)
        {
            if (parameter == this.targetResourceParameter)
                return context.Node.Value;
            if (parameter == this.formParameter)
                return state.Form;
            return base.OnGetArgument(parameter, context, state);
        }


        protected override object OnInvoke(object target, PomonaContext context, InvokeState state)
        {
            state.Form = context.Bind(this.formParameter.TypeSpec);
            return base.OnInvoke(target, context, state);
        }

        #region Nested type: InvokeState

        public class InvokeState
        {
            public object Form { get; set; }
        }

        #endregion
    }
}