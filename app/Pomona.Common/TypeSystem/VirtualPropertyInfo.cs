#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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

        private readonly VirtualMethodInfo getMethod;
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
                throw new ArgumentNullException(nameof(declaringType));
            if (reflectedType == null)
                throw new ArgumentNullException(nameof(reflectedType));
            if (propertyType == null)
                throw new ArgumentNullException(nameof(propertyType));
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            DeclaringType = declaringType;
            ReflectedType = reflectedType;
            PropertyType = propertyType;
            this.getMethod = getMethod;
            this.setMethod = setMethod;
            Name = name;
            Attributes = attributes;
            BaseDefinition = baseDefinition ?? this;
            MetadataToken = VirtualMemberMetadataTokenAllocator.AllocateToken();
        }


        public override PropertyAttributes Attributes { get; }

        public override bool CanRead => this.getMethod != null;

        public override bool CanWrite => this.setMethod != null;

        public override Type DeclaringType { get; }

        public override int MetadataToken { get; }

        public override Module Module => GetType().Module;

        public override string Name { get; }

        public override Type PropertyType { get; }

        public override Type ReflectedType { get; }

        internal PropertyInfo BaseDefinition { get; }


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


        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            return new[] { this.getMethod, this.setMethod }.Where(x => x != null && (nonPublic || x.IsPublic)).ToArray();
        }


        public override object[] GetCustomAttributes(bool inherit)
        {
            return new object[] { };
        }


        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return new object[] { };
        }


        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return new List<CustomAttributeData>();
        }


        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return this.getMethod;
        }


        public override ParameterInfo[] GetIndexParameters()
        {
            return new ParameterInfo[] { };
        }


        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return this.setMethod;
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


        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return false;
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


        private static string GetUniqueKey(Type declaringType,
                                           Type reflectedType,
                                           Type propertyType,
                                           bool readable,
                                           bool writable,
                                           string name,
                                           PropertyAttributes attributes)
        {
            return
                $"{declaringType.AssemblyQualifiedName}:{reflectedType.AssemblyQualifiedName}:{propertyType.AssemblyQualifiedName}:{readable}:{writable}:{name}:{attributes}";
        }
    }
}