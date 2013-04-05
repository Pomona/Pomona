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
    public class ClientType : SharedType
    {
        private readonly ResourceInfoAttribute resourceInfo;

        public ClientType(Type mappedTypeInstance, ITypeMapper typeMapper) : base(mappedTypeInstance, typeMapper)
        {
            SerializationMode = TypeSerializationMode.Complex;

            resourceInfo = mappedTypeInstance
                .GetCustomAttributes(typeof (ResourceInfoAttribute), false)
                .OfType<ResourceInfoAttribute>()
                .First();
        }

        public override string Name
        {
            get { return resourceInfo.JsonTypeName; }
        }


        public ResourceInfoAttribute ResourceInfo
        {
            get { return resourceInfo; }
        }


        public override object Create(IDictionary<IPropertyInfo, object> args)
        {
            var instance = Activator.CreateInstance(resourceInfo.PocoType);
            foreach (var kvp in args)
                kvp.Key.Setter(instance, kvp.Value);
            return instance;
        }


        protected override IEnumerable<IPropertyInfo> OnGetProperties()
        {
            // Include properties of all base types too
            var baseInterfaceProperties =
                MappedTypeInstance
                    .GetInterfaces()
                    .Where(x => x.IsInterface && typeof (IClientResource).IsAssignableFrom(x))
                    .SelectMany(x => x.GetProperties())
                    .Select(x => new SharedPropertyInfo(x, TypeMapper));

            return base.OnGetProperties().Concat(baseInterfaceProperties);
        }
    }
}