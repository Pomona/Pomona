#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq;
using System.Text;

namespace Pomona.Common.Internals.Formatting
{
    internal abstract class FormatterBase
    {
        private readonly string name;


        protected FormatterBase(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            this.name = name;
        }


        public override string ToString()
        {
            var genericParameterString = GetGenericParameterString();
            return String.Concat(this.name, genericParameterString);
        }


        protected abstract Type[] GetGenericArguments();


        private string GetGenericParameterString()
        {
            var genericArguments = GetGenericArguments();

            if (genericArguments == null || !genericArguments.Any())
                return null;

            var genericParameterBuilder = new StringBuilder("<");

            for (int i = 0; i < genericArguments.Length; i++)
            {
                var genericArgument = new TypeFormatter(genericArguments[i]);
                genericParameterBuilder.Append(genericArgument);

                if (i < genericArguments.Length - 1)
                    genericParameterBuilder.Append(", ");
            }

            genericParameterBuilder.Append(">");

            return genericParameterBuilder.ToString();
        }
    }
}
