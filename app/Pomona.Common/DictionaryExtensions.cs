#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pomona.Common
{
    public static class DictionaryExtensions
    {
        public static ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            return new ReadOnlyDictionary<TKey, TValue>(dict);
        }


        public static bool Contains<TKey, TValue>(
            this IDictionary<TKey, TValue> dict,
            TKey key,
            Func<TValue, bool> predicate)
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
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TValue> getDefaultFunc)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));
            if (getDefaultFunc == null)
                throw new ArgumentNullException(nameof(getDefaultFunc));
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
                return getDefaultFunc();
            return value;
        }


        public static TValue SafeGet<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
                return defaultValue;
            return value;
        }


        public static TValue SafeGet<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
                return default(TValue);
            return value;
        }


        public static TValue SafeGet<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TKey, TValue> getDefaultFunc)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));
            if (getDefaultFunc == null)
                throw new ArgumentNullException(nameof(getDefaultFunc));
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
                return getDefaultFunc(key);
            return value;
        }


        public static bool TryGetValueAsType<TKey, TDictValue, TCastValue>(
            this IDictionary<TKey, TDictValue> dict,
            TKey key,
            out TCastValue result)
        {
            TDictValue resultObj;
            result = default(TCastValue);
            if (dict.TryGetValue(key, out resultObj))
            {
                if (resultObj is TCastValue)
                {
                    result = (TCastValue)((object)resultObj);
                    return true;
                }
            }
            return false;
        }
    }
}

