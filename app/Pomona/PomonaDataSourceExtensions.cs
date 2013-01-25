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
    public static class PomonaDataSourceExtensions
    {
        private static readonly MethodInfo getByIdMethod;
        private static readonly MethodInfo postMethod;


        static PomonaDataSourceExtensions()
        {
            getByIdMethod = GetStaticMethodUsingReflection("GetByIdGeneric", typeof (IPomonaDataSource), typeof (object));
            postMethod = GetStaticMethodUsingReflection("PostGeneric", typeof (IPomonaDataSource), typeof (object));
        }


        public static object GetById(this IPomonaDataSource dataSource, Type type, object id)
        {
            return getByIdMethod.MakeGenericMethod(type).Invoke(null, new[] {dataSource, id});
        }


        public static object Post(this IPomonaDataSource dataSource, object submittedObject)
        {
            if (submittedObject == null)
                throw new ArgumentNullException("submittedObject");
            return postMethod.MakeGenericMethod(submittedObject.GetType()).Invoke(
                null, new[] {dataSource, submittedObject});
        }


        private static object GetByIdGeneric<T>(IPomonaDataSource dataSource, object id)
        {
            return dataSource.GetById<T>(id);
        }


        private static MethodInfo GetStaticMethodUsingReflection(string methodName, params Type[] paramTypes)
        {
            var method = typeof (PomonaDataSourceExtensions).GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Static,
                null,
                paramTypes,
                null);
            if (method == null)
                throw new InvalidOperationException("Expected to find method " + methodName);
            return method;
        }


        private static object PostGeneric<T>(IPomonaDataSource dataSource, object submittedObject)
        {
            return dataSource.Post((T) submittedObject);
        }
    }
}