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

using Pomona.Common.Internals;

namespace Pomona
{
    public static class PomonaDataSourceExtensions
    {
        private static readonly Func<Type, IPomonaDataSource, IQueryable> queryMethodInvoker =
            GenericInvoker.Instance<IPomonaDataSource>().CreateFunc1<IQueryable>(x => x.Query<object>());

        private static readonly Func<Type, IPomonaDataSource, object, object> postMethodInvoker =
            GenericInvoker.Instance<IPomonaDataSource>().CreateFunc1<object, object>(x => x.Post<object>(null));
        private static readonly Func<Type, IPomonaDataSource, object, object> patchMethodInvoker =
            GenericInvoker.Instance<IPomonaDataSource>().CreateFunc1<object, object>(x => x.Patch<object>(null));

        public static IQueryable Query(this IPomonaDataSource dataSource, Type type)
        {
            if (dataSource == null)
                throw new ArgumentNullException("dataSource");
            if (type == null)
                throw new ArgumentNullException("type");
            return queryMethodInvoker(type, dataSource);
        }


        public static object Patch(this IPomonaDataSource dataSource, Type type, object patchedObject)
        {
            if (dataSource == null)
                throw new ArgumentNullException("dataSource");
            if (type == null)
                throw new ArgumentNullException("type");
            if (patchedObject == null)
                throw new ArgumentNullException("patchedObject");

            return patchMethodInvoker(type, dataSource, patchedObject);
        }

        public static object Post(this IPomonaDataSource dataSource, Type type, object form)
        {
            if (dataSource == null)
                throw new ArgumentNullException("dataSource");
            if (type == null)
                throw new ArgumentNullException("type");
            if (form == null)
                throw new ArgumentNullException("form");

            return postMethodInvoker(type, dataSource, form);
        }
    }
}