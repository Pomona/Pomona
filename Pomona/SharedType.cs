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

namespace Pomona
{
    /// <summary>
    /// Represents a type that is shared between server and client.
    /// strings, integers etc.. mapped like this
    /// </summary>
    public class SharedType : IMappedType
    {
        private readonly ClassMappingFactory classMappingFactory;
        private readonly Type targetType;


        public SharedType(Type targetType, ClassMappingFactory classMappingFactory)
        {
            if (targetType == null)
                throw new ArgumentNullException("targetType");
            if (classMappingFactory == null)
                throw new ArgumentNullException("classMappingFactory");
            this.targetType = targetType;
            this.classMappingFactory = classMappingFactory;
            GenericArguments = new List<IMappedType>();
        }


        public IMappedType BaseType
        {
            get { return (SharedType)this.classMappingFactory.GetClassMapping(this.targetType.BaseType); }
        }

        public IList<IMappedType> GenericArguments { get; private set; }

        public bool IsGenericType
        {
            get { return this.targetType.IsGenericType; }
        }

        public bool IsGenericTypeDefinition
        {
            get { return false; }
        }

        public string Name
        {
            get { return this.targetType.Name; }
        }

        public Type TargetType
        {
            get { return this.targetType; }
        }
    }
}