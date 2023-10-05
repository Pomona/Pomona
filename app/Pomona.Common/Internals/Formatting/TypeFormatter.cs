#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq;

namespace Pomona.Common.Internals.Formatting
{
    internal class TypeFormatter : FormatterBase
    {
        private readonly Type type;


        public TypeFormatter(Type type)
            : base(GetName(type))
        {
            this.type = type;
        }


        protected override Type[] GetGenericArguments()
        {
            if (!this.type.IsGenericType)
                return null;

            var genericTypeDefinitionArguments = this.type
                                                     .GetGenericTypeDefinition()
                                                     .GetGenericArguments();

            return this.type
                       .GetGenericArguments()
                       .Zip(genericTypeDefinitionArguments, (a, p) => a ?? p)
                       .ToArray();
        }


        private static string GetName(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                var name = genericTypeDefinition.FullName ?? genericTypeDefinition.Name;
                var tickIndex = name.IndexOf('`');
                return tickIndex > -1 ? name.Substring(0, tickIndex) : name;
            }

            return type.FullName ?? type.Name;
        }
    }
}
