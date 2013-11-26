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
    public class ClientType : RuntimeTypeSpec
    {
        public class ClientTypeFactory : ITypeFactory
        {
            public int Priority
            {
                get { return -20; }
            }

            public TypeSpec CreateFromType(ITypeResolver typeResolver, Type type)
            {
                if (typeof(IClientResource).IsAssignableFrom(type) && type != typeof(IClientResource))
                {
                    var interfaceType = GetResourceNonProxyInterfaceType(type);
                    if (interfaceType == type)
                         return new ClientType(interfaceType, typeResolver);
                    return typeResolver.FromType(interfaceType);
                }

                return null;
            }


            public Type GetResourceNonProxyInterfaceType(Type type)
            {
                if (!type.IsInterface)
                {
                    var interfaces =
                        type.GetInterfaces().Where(x => typeof(IClientResource).IsAssignableFrom(x)).ToArray();
                    IEnumerable<Type> exceptTheseInterfaces =
                        interfaces.SelectMany(
                            x => x.GetInterfaces().Where(y => typeof(IClientResource).IsAssignableFrom(y))).
                                   Distinct().ToArray();
                    var mostSubtypedInterface =
                        interfaces
                            .Except(
                                exceptTheseInterfaces)
                            .Single();

                    type = mostSubtypedInterface;
                }

                return type;
            }

        }

        private readonly ResourceInfoAttribute resourceInfo;

        public ClientType(Type mappedTypeInstance, ITypeResolver typeMapper) : base(typeMapper, mappedTypeInstance)
        {
            //SerializationMode = TypeSerializationMode.Complex;

            resourceInfo = mappedTypeInstance
                .GetCustomAttributes(typeof (ResourceInfoAttribute), false)
                .OfType<ResourceInfoAttribute>()
                .First();
        }

        protected internal override string OnLoadName()
        {
            return resourceInfo.JsonTypeName;
        }

        public ResourceInfoAttribute ResourceInfo
        {
            get { return resourceInfo; }
        }


        public override object Create(IDictionary<PropertySpec, object> args)
        {
            var instance = Activator.CreateInstance(resourceInfo.PocoType);
            foreach (var kvp in args)
                kvp.Key.Setter(instance, kvp.Value);
            return instance;
        }


        protected internal override IEnumerable<PropertySpec> OnLoadProperties()
        {
            return base.OnLoadProperties();
        }

    }
}