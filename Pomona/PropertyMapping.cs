#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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
using System.Reflection;

namespace Pomona
{
    public class PropertyMapping
    {
        #region PropertyAccessMode enum

        public enum PropertyAccessMode
        {
            ReadWrite,
            ReadOnly,
            WriteOnly
        }

        #endregion

        #region PropertyCreateMode enum

        public enum PropertyCreateMode
        {
            Excluded, // Default for all generated properties.
            Optional, // Default for all publicly writable properties, 
            Required, // Default for properties that got a matching argument in shortest constructor
        }

        #endregion

        private readonly IMappedType declaringType;
        private readonly string name;
        private readonly PropertyInfo propertyInfo;
        private readonly IMappedType propertyType;


        public PropertyMapping(
            string name, IMappedType declaringType, IMappedType propertyType, PropertyInfo propertyInfo)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (declaringType == null)
                throw new ArgumentNullException("declaringType");
            if (propertyType == null)
                throw new ArgumentNullException("propertyType");
            this.name = name;
            this.declaringType = declaringType;
            this.propertyType = propertyType;
            this.propertyInfo = propertyInfo;
            ConstructorArgIndex = -1;
        }


        public PropertyAccessMode AccessMode { get; set; }

        public int ConstructorArgIndex { get; set; }

        public PropertyCreateMode CreateMode { get; set; }

        public IMappedType DeclaringType
        {
            get { return this.declaringType; }
        }

        public Func<object, object> Getter { get; set; }

        public bool IsWriteable
        {
            get { return AccessMode == PropertyAccessMode.WriteOnly || AccessMode == PropertyAccessMode.ReadWrite; }
        }

        public string JsonName
        {
            get { return this.name.Substring(0, 1).ToLower() + this.name.Substring(1); }
        }

        public string Name
        {
            get { return this.name; }
        }

        public IMappedType PropertyType
        {
            get { return this.propertyType; }
        }

        public Action<object, object> Setter { get; set; }

        protected PropertyInfo PropertyInfo
        {
            get { return this.propertyInfo; }
        }
    }
}