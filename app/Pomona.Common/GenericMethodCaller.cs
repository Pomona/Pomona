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
using System.Linq.Expressions;
using System.Reflection;

namespace Pomona.Internals
{
    public class GenericMethodCaller<TInstance, T1, TResult>
    {
        private readonly Dictionary<Type, Func<TInstance, T1, TResult>> cachedFastCallers =
            new Dictionary<Type, Func<TInstance, T1, TResult>>();

        private readonly MethodInfo genericMethodDefinition;

        private object cacheLock = new object();


        public GenericMethodCaller(MethodInfo genericMethodDefinition)
        {
            if (genericMethodDefinition == null)
                throw new ArgumentNullException("genericMethodDefinition");

            if (!genericMethodDefinition.IsGenericMethodDefinition)
            {
                throw new ArgumentException(
                    "genericMethodDefinition is required to actually be a generic method definition.");
            }

            this.genericMethodDefinition = genericMethodDefinition;
        }


        public virtual MethodInfo GenericMethodDefinition
        {
            get { return genericMethodDefinition; }
        }


        public TResult Call(Type genTypeParam, TInstance target, T1 arg1)
        {
            Func<TInstance, T1, TResult> func;
            lock (cacheLock)
            {
                cachedFastCallers.TryGetValue(genTypeParam, out func);
            }

            if (func == null)
            {
                var instanceParameter = Expression.Parameter(typeof (TInstance), "instance");
                var arg1Param = Expression.Parameter(typeof (T1), "arg1");
                var methodInstance = genericMethodDefinition.MakeGenericMethod(genTypeParam);
                var expr = Expression.Lambda<Func<TInstance, T1, TResult>>(
                    Expression.Convert(Expression.Call(instanceParameter, methodInstance, arg1Param), typeof (TResult)),
                    instanceParameter,
                    arg1Param);
                func = expr.Compile();

                lock (cacheLock)
                {
                    cachedFastCallers[genTypeParam] = func;
                }
            }

            return func(target, arg1);
        }
    }
}