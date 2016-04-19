#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Pomona.CodeGen
{
    public static class CecilExtensions
    {
        internal static MethodReference MakeHostInstanceGeneric(this MethodReference self,
                                                                params TypeReference[] arguments)
        {
            var genericInstanceType = self.DeclaringType.MakeGenericInstanceType(arguments);
            var reference = new MethodReference(self.Name, self.ReturnType, genericInstanceType)
            {
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention
            };

            foreach (var parameter in self.Parameters)
            {
                // TODO: This won't work with methods that has generic arguments, since the ParameterType needs to be replaced. @asbjornu
                var parameterDefinition = new ParameterDefinition(parameter.Name,
                                                                  parameter.Attributes,
                                                                  parameter.ParameterType);
                reference.Parameters.Add(parameterDefinition);
            }

            foreach (var genericParameter in self.GenericParameters)
                reference.GenericParameters.Add(new GenericParameter(genericParameter.Name, reference));

            return reference;
        }
    }
}