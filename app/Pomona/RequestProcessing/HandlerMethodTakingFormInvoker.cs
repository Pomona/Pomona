#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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

