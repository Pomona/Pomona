#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Globalization;
using System.Reflection;

namespace Pomona.Common.TypeSystem
{
    internal class VirtualMethodInfo : MethodInfo
    {
        private readonly MethodInfo baseDefinition;
        private readonly Delegate del;
        private readonly ParameterInfo[] parameters;


        internal VirtualMethodInfo(string name,
                                   Type declaringType,
                                   Type reflectedType,
                                   MethodAttributes attributes,
                                   ParameterInfo[] parameters = null,
                                   MethodInfo baseDefinition = null,
                                   Delegate del = null)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (declaringType == null)
                throw new ArgumentNullException(nameof(declaringType));
            if (reflectedType == null)
                throw new ArgumentNullException(nameof(reflectedType));
            this.parameters = parameters ?? new ParameterInfo[] { };
            Name = name;
            DeclaringType = declaringType;
            ReflectedType = reflectedType;
            Attributes = attributes;
            this.baseDefinition = baseDefinition;
            this.del = del;
            MetadataToken = VirtualMemberMetadataTokenAllocator.AllocateToken();
            MethodHandle = new RuntimeMethodHandle();
        }


        public override MethodAttributes Attributes { get; }

        public override Type DeclaringType { get; }

        public override int MetadataToken { get; }

        public override RuntimeMethodHandle MethodHandle { get; }

        public override Module Module
        {
            get { return GetType().Module; }
        }

        public override string Name { get; }

        public override Type ReflectedType { get; }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get { return null; }
        }


        public override MethodInfo GetBaseDefinition()
        {
            return this.baseDefinition ?? this;
        }


        public override object[] GetCustomAttributes(bool inherit)
        {
            return new object[] { };
        }


        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return new object[] { };
        }


        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return default(MethodImplAttributes);
        }


        public override ParameterInfo[] GetParameters()
        {
            return (ParameterInfo[])this.parameters.Clone();
        }


        public override object Invoke(object obj,
                                      BindingFlags invokeAttr,
                                      Binder binder,
                                      object[] parameters,
                                      CultureInfo culture)
        {
            if (this.del == null)
                throw new InvalidOperationException("No delegate to invoke set on VirtualMethodInfo.");
            if (obj != null)
            {
                if (parameters == null)
                    parameters = new[] { obj };
                else
                {
                    var modParams = new object[1 + parameters.Length];
                    modParams[0] = obj;
                    Array.Copy(parameters, 0, modParams, 1, parameters.Length);
                    parameters = modParams;
                }
            }
            return this.del.DynamicInvoke(parameters);
        }


        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return false;
        }
    }
}