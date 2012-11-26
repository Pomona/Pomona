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

using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class PropertyMapping : IPropertyInfo
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

        #endregion

        private readonly TransformedType declaringType;
        private readonly string name;
        private readonly PropertyInfo propertyInfo;
        private readonly IMappedType propertyType;


        public PropertyMapping(
            string name, TransformedType declaringType, IMappedType propertyType, PropertyInfo propertyInfo)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (declaringType == null)
                throw new ArgumentNullException("declaringType");
            if (propertyType == null)
                throw new ArgumentNullException("propertyType");
            this.name = name;
            LowerCaseName = name.ToLower();
            JsonName = name.Substring(0, 1).ToLower() + name.Substring(1);
            this.declaringType = declaringType;
            this.propertyType = propertyType;
            this.propertyInfo = propertyInfo;
            ConstructorArgIndex = -1;
        }


        public PropertyAccessMode AccessMode { get; set; }
        public bool AlwaysExpand { get; set; }

        public int ConstructorArgIndex { get; set; }

        public PropertyCreateMode CreateMode { get; set; }

        public IMappedType DeclaringType
        {
            get { return this.declaringType; }
        }

        /// <summary>
        /// For one-to-many collection properties this defines which property on the
        /// many side refers to the one side of the relation.
        /// 
        /// This only applies to one-to-many collections..
        /// </summary>
        public PropertyMapping ElementForeignKey { get; set; }

        public Func<object, object> Getter { get; set; }

        public bool IsOneToManyCollection
        {
            get { return this.propertyType.IsCollection; }
        }

        public bool IsWriteable
        {
            get { return AccessMode == PropertyAccessMode.WriteOnly || AccessMode == PropertyAccessMode.ReadWrite; }
        }

        public string JsonName { get; set; }
        public string LowerCaseName { get; private set; }

        public string Name
        {
            get { return this.name; }
        }

        public PropertyInfo PropertyInfo
        {
            get { return this.propertyInfo; }
        }

        public IMappedType PropertyType
        {
            get { return this.propertyType; }
        }

        public Action<object, object> Setter { get; set; }

        public TypeMapper TypeMapper
        {
            get { return declaringType.TypeMapper; }
        }
    }
}