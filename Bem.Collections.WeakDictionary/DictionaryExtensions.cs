// --------------------------------------
// <copyright file="DictionaryExtensions.cs" company="Daniel Balogh">
//     Copyright (c) Daniel Balogh. All rights reserved.
//     Licensed under the GNU Generic Public License 3.0 license.
//     See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Bem.Collections.WeakDictionary
{
    internal static class DictionaryExtensions
    {
        internal static TValue AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
            where TKey : notnull
        {
            TValue newValue;

            if (@this.TryGetValue(key, out var oldValue))
            {
                newValue = updateValueFactory(key, oldValue);
                @this[key] = newValue;
            }
            else
            {
                newValue = addValueFactory(key);
                @this.Add(key, newValue);
            }

            return newValue;
        }

        internal static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TValue> valueFactory)
            where TKey : notnull
        {
            if (!@this.TryGetValue(key, out var value))
            {
                value = valueFactory(key);
                @this.Add(key, value);
            }

            return value;
        }

        internal static bool TryRemove<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, [MaybeNullWhen(false)] out TValue value)
            where TKey : notnull
        {
            return @this.Remove(key, out value);
        }
    }
}
