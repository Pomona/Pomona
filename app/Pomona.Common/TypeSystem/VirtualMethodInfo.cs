#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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
using System.Globalization;
using System.Reflection;

namespace Pomona.Common.TypeSystem
{
    internal class VirtualMethodInfo : MethodInfo
    {
        private readonly MethodAttributes attributes;
        private readonly MethodInfo baseDefinition;
        private readonly Type declaringType;
        private readonly Delegate del;
        private readonly int metadataToken;
        private readonly string name;
        private readonly ParameterInfo[] parameters;
        private readonly Type reflectedType;
        private readonly RuntimeMethodHandle runtimeMethodHandle;


        internal VirtualMethodInfo(string name,
                                   Type declaringType,
                                   Type reflectedType,
                                   MethodAttributes attributes,
                                   ParameterInfo[] parameters = null,
                                   MethodInfo baseDefinition = null,
                                   Delegate del = null)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (declaringType == null)
                throw new ArgumentNullException("declaringType");
            if (reflectedType == null)
                throw new ArgumentNullException("reflectedType");
            this.parameters = parameters ?? new ParameterInfo[] { };
            this.name = name;
            this.declaringType = declaringType;
            this.reflectedType = reflectedType;
            this.attributes = attributes;
            this.baseDefinition = baseDefinition;
            this.del = del;
            this.metadataToken = VirtualMemberMetadataTokenAllocator.AllocateToken();
            this.runtimeMethodHandle = new RuntimeMethodHandle();
        }


        public override MethodAttributes Attributes
        {
            get { return this.attributes; }
        }

        public override Type DeclaringType
        {
            get { return this.declaringType; }
        }

        public override int MetadataToken
        {
            get { return this.metadataToken; }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get { return this.runtimeMethodHandle; }
        }

        public override Module Module
        {
            get { return GetType().Module; }
        }

        public override string Name
        {
            get { return this.name; }
        }

        public override Type ReflectedType
        {
            get { return this.reflectedType; }
        }

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