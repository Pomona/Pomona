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

using Pomona.Example.Models;

namespace Pomona.Example
{
    public class CritterTypeMappingFilter : TypeMappingFilterBase
    {
        public override object GetIdFor(object entity)
        {
            return ((EntityBase)entity).Id;
        }


        public override IEnumerable<Type> GetSourceTypes()
        {
            return CritterDataSource.GetEntityTypes();
        }

        public override string GetPropertyMappedName(System.Reflection.PropertyInfo propertyInfo)
        {
            if (propertyInfo.DeclaringType == typeof(JunkWithRenamedProperty)
                && propertyInfo.Name == "ReallyUglyPropertyName")
            {
                return "BeautifulAndExposed";
            }

            return base.GetPropertyMappedName(propertyInfo);
        }


        public override Type GetUriBaseType(Type type)
        {
            if (type == typeof(MusicalCritter))
                return typeof(Critter);

            return base.GetUriBaseType(type);
        }


        public override bool TypeIsMapped(Type type)
        {
            if (type == typeof(ExcludedThing))
                return false;

            return base.TypeIsMapped(type);
        }


        public override bool TypeIsMappedAsValueObject(Type type)
        {
            if (type == typeof(CrazyValueObject))
                return true;
            return base.TypeIsMappedAsValueObject(type);
        }
    }
}