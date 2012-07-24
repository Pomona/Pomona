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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pomona
{
    public abstract class TypeMappingFilterBase : ITypeMappingFilter
    {
        private static readonly HashSet<Type> defaultAllowedNativeTypes;
        private HashSet<Type> sourceTypesCached;


        static TypeMappingFilterBase()
        {
            defaultAllowedNativeTypes = new HashSet<Type>()
            {
                typeof(string),
                typeof(int),
                typeof(long),
                typeof(double),
                typeof(float),
                typeof(decimal),
                typeof(DateTime),
                typeof(object)
            };
        }


        private HashSet<Type> SourceTypes
        {
            get
            {
                if (this.sourceTypesCached == null)
                    this.sourceTypesCached = new HashSet<Type>(GetSourceTypes());
                return this.sourceTypesCached;
            }
        }

        public abstract object GetIdFor(object entity);

        public abstract IEnumerable<Type> GetSourceTypes();


        public virtual Type GetUriBaseType(Type type)
        {
            return type;
        }


        public virtual bool PropertyIsIncluded(PropertyInfo propertyInfo)
        {
            return true;
        }


        public virtual bool TypeIsMapped(Type type)
        {
            return TypeIsMappedAsTransformedType(type) || defaultAllowedNativeTypes.Contains(type)
                   || TypeIsMappedAsCollection(type);
        }


        public virtual bool TypeIsMappedAsCollection(Type type)
        {
            return
                type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>));
        }


        public virtual bool TypeIsMappedAsSharedType(Type type)
        {
            return defaultAllowedNativeTypes.Contains(type) || TypeIsMappedAsCollection(type);
        }


        public virtual bool TypeIsMappedAsTransformedType(Type type)
        {
            return SourceTypes.Contains(type);
        }


        public virtual bool TypeIsMappedAsValueObject(Type type)
        {
            return false;
        }


        // TODO: Replace this with a way to find out what property has the Id.
    }
}