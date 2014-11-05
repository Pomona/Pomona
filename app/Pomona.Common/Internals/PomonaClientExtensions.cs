#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
using System.Reflection;

namespace Pomona.Common.Internals
{
    public static class PomonaClientExtensions
    {
        internal static Type GetResourceBaseInterface(this Type type)
        {
            return type.GetResourceInfoAttribute().BaseType;
        }


        internal static ResourceInfoAttribute GetResourceInfoAttribute(this Type type)
        {
            ResourceInfoAttribute ria;

            if (!type.TryGetResourceInfoAttribute(out ria))
                throw new InvalidOperationException("Unable to get resource info attribute");

            return ria;
        }


        internal static PropertyInfo GetResourceProperty(this Type type, string propertyName)
        {
            return
                type.WalkTree(x => x.GetResourceBaseInterface()).Select(x => x.GetProperty(propertyName)).FirstOrDefault
                    (x => x != null);
        }


        internal static bool TryGetResourceInfoAttribute(this Type type, out ResourceInfoAttribute resourceInfoAttribute)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            resourceInfoAttribute = type
                .GetCustomAttributes(typeof(ResourceInfoAttribute), false)
                .OfType<ResourceInfoAttribute>()
                .FirstOrDefault();

            return resourceInfoAttribute != null;
        }
    }
}