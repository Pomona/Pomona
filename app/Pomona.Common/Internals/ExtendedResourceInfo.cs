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
using System.Linq;
using System.Reflection;

namespace Pomona.Common.Internals
{
    public class ExtendedResourceInfo
    {
        private readonly Type extendedType;
        private readonly PropertyInfo dictProperty;
        private readonly Type serverType;

        private ExtendedResourceInfo(Type extendedType, Type serverType, PropertyInfo dictProperty)
        {
            this.extendedType = extendedType;
            this.serverType = serverType;
            this.dictProperty = dictProperty;
        }

        public Type ExtendedType
        {
            get { return this.extendedType; }
        }

        public Type ServerType
        {
            get { return serverType; }
        }

        public PropertyInfo DictProperty
        {
            get { return dictProperty; }
        }

        internal static bool TryGetExtendedResourceInfo(Type clientType, IClientTypeResolver client, out ExtendedResourceInfo info)
        {
            info = null;
            var serverTypeInfo = client.GetMostInheritedResourceInterfaceInfo(clientType);
            if (!clientType.IsInterface || serverTypeInfo == null)
                return false;

            var serverType = serverTypeInfo.InterfaceType;

            if (serverType == clientType)
                return false;

            var dictProperty = GetAttributesDictionaryPropertyFromResource(serverType);
            info = new ExtendedResourceInfo(clientType, serverType, dictProperty);
            return true;
        }

        private static PropertyInfo GetAttributesDictionaryPropertyFromResource(Type serverKnownType)
        {
            var attrProp =
                serverKnownType.GetAllInheritedPropertiesFromInterface().FirstOrDefault(
                    x => x.GetCustomAttributes(typeof (ResourceAttributesPropertyAttribute), true).Any());

            return attrProp;
        }
    }
}