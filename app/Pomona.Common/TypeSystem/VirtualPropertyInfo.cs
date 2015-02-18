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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Pomona.Common.TypeSystem
{
    public class VirtualPropertyInfo : PropertyInfo
    {
        private static readonly ConcurrentDictionary<string, VirtualPropertyInfo> internalizedCache =
            new ConcurrentDictionary<string, VirtualPropertyInfo>();

        private readonly PropertyAttributes attributes;
        private readonly PropertyInfo baseDefinition;
        private readonly Type declaringType;
        private readonly VirtualMethodInfo getMethod;
        private readonly int metadataToken;
        private readonly string name;
        private readonly Type propertyType;
        private readonly Type reflectedType;
        private readonly VirtualMethodInfo setMethod;


        internal VirtualPropertyInfo(string name,
                                     Type declaringType,
                                     Type reflectedType,
                                     Type propertyType,
                                     VirtualMethodInfo getMethod,
                                     VirtualMethodInfo setMethod,
                                     PropertyAttributes attributes,
                                     VirtualPropertyInfo baseDefinition)
        {
            if (declaringType == null)
                throw new ArgumentNullException("declaringType");
            if (reflectedType == null)
                throw new ArgumentNullException("reflectedType");
            if (propertyType == null)
                throw new ArgumentNullException("propertyType");
            if (name == null)
                throw new ArgumentNullException("name");
            this.declaringType = declaringType;
            this.reflectedType = reflectedType;
            this.propertyType = propertyType;
            this.getMethod = getMethod;
            this.setMethod = setMethod;
            this.name = name;
            this.attributes = attributes;
            this.baseDefinition = baseDefinition ?? this;
            this.metadataToken = VirtualMemberMetadataTokenAllocator.AllocateToken();
        }


        public override Module Module
        {
            get { return GetType().Module; }
        }

        public override int MetadataToken
        {
            get { return this.metadataToken; }
        }

        public override string Name
        {
            get { return this.name; }
        }

        public override Type DeclaringType
        {
            get { return this.declaringType; }
        }

        public override Type ReflectedType
        {
            get { return this.reflectedType; }
        }

        public override Type PropertyType
        {
            get { return this.propertyType; }
        }

        public override PropertyAttributes Attributes
        {
            get { return this.attributes; }
        }

        public override bool CanRead
        {
            get { return this.getMethod != null; }
        }

        public override bool CanWrite
        {
            get { return this.setMethod != null; }
        }

        internal PropertyInfo BaseDefinition
        {
            get { return this.baseDefinition; }
        }


        public static VirtualPropertyInfo Create(string name,
                                                 Type declaringType,
                                                 Type reflectedType,
                                                 Type propertyType,
                                                 PropertyAttributes attributes,
                                                 bool readable,
                                                 bool writable)
        {
            return
                internalizedCache.GetOrAdd(
                    GetUniqueKey(declaringType, reflectedType, propertyType, readable, writable, name, attributes),
                    key =>
                    {
                        VirtualMethodInfo getterBaseDefinition = null;
                        VirtualPropertyInfo baseProperty = null;
                        if (declaringType != reflectedType)
                        {
                            baseProperty = Create(name, declaringType, declaringType, propertyType, attributes, readable,
                                                  writable);
                            getterBaseDefinition = baseProperty.getMethod;
                        }
                        const MethodAttributes methodAttributes =
                            MethodAttributes.NewSlot | MethodAttributes.SpecialName |
                            MethodAttributes.HideBySig
                            | MethodAttributes.Virtual | MethodAttributes.Public;
                        var getter = new VirtualMethodInfo("get_" + name, declaringType, reflectedType, methodAttributes,
                                                           baseDefinition : getterBaseDefinition);
                        var prop = new VirtualPropertyInfo(name, declaringType, reflectedType, propertyType, getter,
                                                           null, attributes, baseProperty);
                        return prop;
                    });
        }


        private static string GetUniqueKey(Type declaringType,
                                           Type reflectedType,
                                           Type propertyType,
                                           bool readable,
                                           bool writable,
                                           string name,
                                           PropertyAttributes attributes)
        {
            return string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}", declaringType.AssemblyQualifiedName,
                                 reflectedType.AssemblyQualifiedName, propertyType.AssemblyQualifiedName, readable,
                                 writable, name, attributes);
        }


        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return new List<CustomAttributeData>();
        }


        public override object[] GetCustomAttributes(bool inherit)
        {
            return new object[] { };
        }


        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return false;
        }


        public override object GetValue(object obj,
                                        BindingFlags invokeAttr,
                                        Binder binder,
                                        object[] index,
                                        CultureInfo culture)
        {
            if (index != null)
                throw new NotSupportedException("VirtualPropertyInfo does not support properties with indexes.");
            if (this.getMethod == null)
                throw new InvalidOperationException("Property has no getter.");
            return this.getMethod.Invoke(obj, invokeAttr, binder, null, culture);
        }


        public override void SetValue(object obj,
                                      object value,
                                      BindingFlags invokeAttr,
                                      Binder binder,
                                      object[] index,
                                      CultureInfo culture)
        {
            if (index != null)
                throw new NotSupportedException("VirtualPropertyInfo does not support properties with indexes.");
            if (this.setMethod == null)
                throw new InvalidOperationException("Property has no setter.");
            this.setMethod.Invoke(obj, invokeAttr, binder, new object[] { value }, culture);
        }


        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            return new[] { this.getMethod, this.setMethod }.Where(x => x != null && (nonPublic || x.IsPublic)).ToArray();
        }


        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return this.getMethod;
        }


        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return this.setMethod;
        }


        public override ParameterInfo[] GetIndexParameters()
        {
            return new ParameterInfo[] { };
        }


        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return new object[] { };
        }
    }
}