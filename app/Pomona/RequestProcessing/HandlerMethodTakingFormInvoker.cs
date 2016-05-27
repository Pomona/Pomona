#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Threading.Tasks;

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


        protected override async Task<object> OnGetArgument(HandlerParameter parameter, PomonaContext context, InvokeState state)
        {
            if (parameter == this.targetResourceParameter)
                return await context.Node.GetValueAsync();
            if (parameter == this.formParameter)
                return state.Form;
            return await base.OnGetArgument(parameter, context, state);
        }


        protected override async Task<object> OnInvoke(object target, PomonaContext context, InvokeState state)
        {
            state.Form = await context.Bind(this.formParameter.TypeSpec);
            return await base.OnInvoke(target, context, state);
        }

        #region Nested type: InvokeState

        public class InvokeState
        {
            public object Form { get; set; }
        }

        #endregion
    }
}