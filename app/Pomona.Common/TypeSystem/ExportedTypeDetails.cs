#region License

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

#endregion

using System;
using System.Linq;

namespace Pomona.Common.TypeSystem
{
    public class ExportedTypeDetails
    {
        private readonly HttpMethod allowedMethods;
        private readonly bool alwaysExpand;

        private readonly bool mappedAsValueObject;
        private readonly Action<object> onDeserialized;
        private readonly string pluralName;
        private readonly TransformedType type;


        public ExportedTypeDetails(TransformedType type,
            HttpMethod allowedMethods,
            string pluralName,
            Action<object> onDeserialized,
            bool mappedAsValueObject,
            bool alwaysExpand, bool isAbstract)
        {
            this.type = type;
            this.allowedMethods = allowedMethods;
            this.pluralName = pluralName;
            this.onDeserialized = onDeserialized;
            this.mappedAsValueObject = mappedAsValueObject;
            this.type = type;
            this.alwaysExpand = alwaysExpand;
            this.isAbstract = isAbstract;
        }


        public HttpMethod AllowedMethods
        {
            get { return this.allowedMethods; }
        }

        public bool AlwaysExpand
        {
            get { return this.alwaysExpand; }
        }

        public PropertyMapping ETagProperty
        {
            get { return this.type.Properties.FirstOrDefault(x => x.IsEtagProperty); }
        }

        public bool MappedAsValueObject
        {
            get { return this.mappedAsValueObject; }
        }

        public Action<object> OnDeserialized
        {
            get { return this.onDeserialized; }
        }

        public string PluralName
        {
            get { return this.pluralName; }
        }

        private readonly bool isAbstract;

        public PropertyMapping PrimaryId
        {
            get { return this.type.AllProperties.OfType<PropertyMapping>().FirstOrDefault(x => x.IsPrimaryKey); }
        }

        public bool IsAbstract
        {
            get { return isAbstract; }
        }
    }
}