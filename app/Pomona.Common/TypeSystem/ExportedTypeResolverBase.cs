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
using System.Linq;

namespace Pomona.Common.TypeSystem
{
    public abstract class ExportedTypeResolverBase : TypeResolver, IExportedTypeResolver
    {
        public abstract IEnumerable<ComplexType> GetAllComplexTypes();
        public abstract ComplexPropertyDetails LoadComplexPropertyDetails(ComplexProperty complexProperty);
        public abstract ComplexTypeDetails LoadComplexTypeDetails(ComplexType exportedType);
        public abstract ResourceTypeDetails LoadResourceTypeDetails(ResourceType resourceType);

        public override IEnumerable<PropertySpec> LoadRequiredProperties(TypeSpec typeSpec)
        {
            if (typeSpec == null)
                throw new ArgumentNullException("typeSpec");
            var transformedType = typeSpec as ComplexType;
            if (transformedType != null)
            {
                if (!transformedType.PostAllowed)
                    return Enumerable.Empty<PropertySpec>();
                if (transformedType.Constructor != null)
                {
                    IEnumerable<PropertySpec> requiredProperties =
                        transformedType.Constructor.ParameterSpecs.Where(x => x.IsRequired).Select(
                            x => typeSpec.GetPropertyByName(x.PropertyInfo.Name, true)).ToList();
                    return
                        requiredProperties;
                }
            }

            return base.LoadRequiredProperties(typeSpec);
        }
    }
}