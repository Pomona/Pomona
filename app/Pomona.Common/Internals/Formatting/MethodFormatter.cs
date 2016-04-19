#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Reflection;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Internals.Formatting
{
    internal class MethodFormatter : FormatterBase
    {
        private readonly MethodInfo method;


        public MethodFormatter(MethodInfo method)
            : base(method.Maybe().Select(m => m.Name).OrDefault())
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            this.method = method;
        }


        public override string ToString()
        {
            var parameters = this.method.GetParameters().Select(x => new TypeFormatter(x.ParameterType));
            var parameterString = String.Join(", ", parameters);

            return
                $"{new TypeFormatter(this.method.ReturnType)} {new TypeFormatter(this.method.DeclaringType)}.{base.ToString()}({parameterString})";
        }


        protected override Type[] GetGenericArguments()
        {
            if (this.method.DeclaringType == null || !this.method.IsGenericMethod)
                return null;

            var genericMethodDefinitionArguments = this.method
                                                       .GetGenericMethodDefinition()
                                                       .GetGenericArguments();

            return this.method
                       .GetGenericArguments()
                       .Zip(genericMethodDefinitionArguments, (a, p) => a ?? p)
                       .ToArray();
        }
    }
}