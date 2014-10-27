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