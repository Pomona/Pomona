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

using System;
using System.Collections.Generic;

namespace Pomona.Common
{
    public static class DictionaryExtensions
    {
        public static bool Contains<TKey, TValue>(
            this IDictionary<TKey, TValue> dict, TKey key, Func<TValue, bool> predicate)
        {
            return dict.ContainsKey(key) && predicate(dict[key]);
        }


        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : new()
        {
            TValue ret;
            if (!dictionary.TryGetValue(key, out ret))
            {
                ret = new TValue();
                dictionary[key] = ret;
            }
            return ret;
        }


        public static TValue GetOrCreate<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TValue> creator)
        {
            TValue ret;
            if (!dictionary.TryGetValue(key, out ret))
            {
                ret = creator();
                dictionary[key] = ret;
            }
            return ret;
        }


        public static TValue SafeGet<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> getDefaultFunc)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");
            if (getDefaultFunc == null)
                throw new ArgumentNullException("getDefaultFunc");
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
                return getDefaultFunc();
            return value;
        }

        public static TValue SafeGet<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
                return defaultValue;
            return value;
        }


        public static TValue SafeGet<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
                return default(TValue);
            return value;
        }


        public static TValue SafeGet<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> getDefaultFunc)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");
            if (getDefaultFunc == null)
                throw new ArgumentNullException("getDefaultFunc");
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
                return getDefaultFunc(key);
            return value;
        }
    }
}