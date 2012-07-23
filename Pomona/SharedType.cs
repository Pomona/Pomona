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

namespace Pomona
{
    /// <summary>
    /// Represents a type that is shared between server and client.
    /// strings, integers etc.. mapped like this
    /// </summary>
    public class SharedType : IMappedType
    {
        private static Type[] basicWireTypes = {
            typeof(int), typeof(double), typeof(float), typeof(string), typeof(bool), typeof(decimal), typeof(DateTime)
        };

        private readonly Type targetType;
        private readonly TypeMapper typeMapper;
        private bool isCollection;


        public SharedType(Type targetType, TypeMapper typeMapper)
        {
            if (targetType == null)
                throw new ArgumentNullException("targetType");
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            this.targetType = targetType;
            this.typeMapper = typeMapper;
            this.isCollection =
                targetType.GetInterfaces().Where(x => x.IsGenericType)
                    .Select(x => x.IsGenericTypeDefinition ? x : x.GetGenericTypeDefinition()).Any
                    (x => x == typeof(ICollection<>));
            GenericArguments = new List<IMappedType>();
        }


        public Type TargetType
        {
            get { return this.targetType; }
        }

        #region IMappedType Members

        public IMappedType BaseType
        {
            get { return (SharedType)this.typeMapper.GetClassMapping(this.targetType.BaseType); }
        }

        public IList<IMappedType> GenericArguments { get; private set; }

        public bool IsBasicWireType
        {
            get { return basicWireTypes.Contains(this.targetType); }
        }

        public bool IsCollection
        {
            get { return this.isCollection; }
        }

        public bool IsGenericType
        {
            get { return this.targetType.IsGenericType; }
        }

        public bool IsGenericTypeDefinition
        {
            get { return false; }
        }

        public bool IsValueType
        {
            get { return this.targetType.IsValueType; }
        }

        public string Name
        {
            get { return this.targetType.Name; }
        }

        #endregion
    }
}