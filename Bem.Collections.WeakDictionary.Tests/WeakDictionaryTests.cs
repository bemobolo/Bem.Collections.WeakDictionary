// --------------------------------------
// <copyright file="WeakDictionaryTests.cs" company="Daniel Balogh">
//     Copyright (c) Daniel Balogh. All rights reserved.
//     Licensed under the GNU Generic Public License 3.0 license.
//     See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Bem.Collections.WeakDictionary.Tests;

public class WeakDictionaryTests
{
    [Fact]
    public void Constructor_Shallow_Copies_From_IEnumerableOfKeyValuePair()
    {
        var key = new object();
        var dict = new WeakDictionary<object, int>(new[] { KeyValuePair.Create(key, 1) });

        Assert.NotEmpty(dict);
        Assert.Equal(new[] { KeyValuePair.Create(key, 1) }, dict);
    }

    [Fact]
    public void Constructor_Parameters_Null_Throws_ArgumentNullException()
    {
        var key = new object();
        Assert.Throws<ArgumentNullException>("collection", () => new WeakDictionary<object, int>(null!, EqualityComparer<object>.Default));
        Assert.Throws<ArgumentNullException>("comparer", () => new WeakDictionary<object, int>(new[] { KeyValuePair.Create(key, 1) }, null!));
    }

    [Fact]
    public void Clear_Clears_Dictionary()
    {
        var key1 = new Custom(1);
        var key2 = new Custom(2);
        var key3 = new Custom(3);

        var dict = new WeakDictionary<Custom, int>
        {
            { key1, 1 },
            { key2, 2 },
            { key3, 3 }
        };

        dict.Clear();

        Assert.Empty(dict);
    }

    [Fact]
    public void GetOrAdd_Key_Is_Null_Throws_ArgumentNullException()
    {
        var dict = new WeakDictionary<object, object>();

        Assert.Throws<ArgumentNullException>("key", () => dict.GetOrAdd(null!, _ => new object()));
    }

    [Fact]
    public void GetOrAdd_Value_Factory_Is_Null_Throws_ArgumentNullException()
    {
        var dict = new WeakDictionary<object, object>();

        Assert.Throws<ArgumentNullException>("valueFactory", () => dict.GetOrAdd(new object(), null!));
    }

    [Fact]
    public void GetOrAdd_Creates_New_Instance_If_Not_Existing()
    {
        var dict = new WeakDictionary<object, object>();

        var created = false;
        var key = new object();
        var value = new object();

        var returned = dict.GetOrAdd(key, _ =>
        {
            created = true;
            return value;
        });

        Assert.True(created);
        Assert.Equal(value, returned);
    }

    [Fact]
    public void GetOrAdd_Returns_Existing_Value()
    {
        var dict = new WeakDictionary<object, object>();

        var key = new object();
        var value = new object();

        var returned = dict.GetOrAdd(key, _ => value);
        var returned2 = dict.GetOrAdd(key, _ => throw new XunitException("Add factory should not be called again!"));

        Assert.Equal(value, returned);
        Assert.Equal(returned, returned2);
    }

    [Fact]
    public void TryGetValue_Gets_Existing_Value_And_Returns_True()
    {
        var key = new object();

        var dict = new WeakDictionary<object, int>
        {
            { key, 1 }
        };

        var success = dict.TryGetValue(key, out var item);

        Assert.True(success);
        Assert.Equal(1, item);
    }

    [Fact]
    public void TryGetValue_When_Key_Not_Found_Returns_False()
    {
        var dict = new WeakDictionary<object, int>();

        var success = dict.TryGetValue(new object(), out var item);

        Assert.False(success);
        Assert.Equal(default, item);
    }

    [Fact]
    public void TryGetValue_When_Key_Is_Null_Throws_ArgumentNullException()
    {
        var dict = new WeakDictionary<object, int>();

        Assert.Throws<ArgumentNullException>("key", () => dict.TryGetValue(null!, out _));
    }

    [Fact]
    public void CopyTo_Array_Is_Null_Throws_ArgumentNullException()
    {
        ICollection<KeyValuePair<object, int>> dict = new WeakDictionary<object, int>();

        Assert.Throws<ArgumentNullException>("array", () => dict.CopyTo(null!, 0));
    }

    [Fact]
    public void CopyTo_Array_Index_Is_Negative_Throws_ArgumentOutOfRangeException()
    {
        ICollection<KeyValuePair<object, int>> dict = new WeakDictionary<object, int>();

        var array = new KeyValuePair<object, int>[1];

        Assert.Throws<ArgumentOutOfRangeException>("arrayIndex", () => dict.CopyTo(array, -1));
    }

    [Fact]
    public void CopyTo_Array_Capacity_Is_Insufficient_ArgumentException()
    {
        var key = new object();
        var key2 = new object();

        ICollection<KeyValuePair<object, int>> dict = new WeakDictionary<object, int>
        {
            { key, 1 },
            { key2, 2 }
        };

        var array = new KeyValuePair<object, int>[2];

        Assert.Throws<ArgumentException>("array", () => dict.CopyTo(array, 1));
    }

    [Fact]
    public void CopyTo_Copies_To_Array()
    {
        var key = new object();

        ICollection<KeyValuePair<object, int>> dict = new WeakDictionary<object, int>
        {
            { key, 1 }
        };

        var array = new KeyValuePair<object, int>[1];

        dict.CopyTo(array, 0);

        Assert.Equal(dict.First(), array[0]);
    }

    [Fact]
    public async Task Entries_Removed_After_GC()
    {
        var key1 = new Custom(1);
        var key2 = new Custom(2);
        var key3 = new Custom(3);

        var dict = new WeakDictionary<Custom, int>
        {
            { key1, 1 },
            { key2, 2 },
            { key3, 3 }
        };

        var weakKey1 = new WeakReference(key1);
        key1 = null;

        await Collect();

        Assert.False(weakKey1.IsAlive);
        Assert.Equal(2, dict.Count);
        Assert.Equal(new[] { key2, key3 }, dict.Keys.OrderBy(k => k.HashCode));
    }

    [Fact]
    public async Task Entry_Not_Removed_After_GC_When_Key_Used_As_Value_Too()
    {
        var dict = new WeakDictionary<object, object>();

        var key = new object();

        _ = dict.GetOrAdd(key, k => k);

        var weakKey = new WeakReference(key);
        key = null;

        await Collect();

        Assert.NotNull(weakKey.Target);
        Assert.Single(dict);
    }

    [Fact]
    public async Task Entry_Removed_After_GC_When_Key_Used_As_Weak_Referenced_Value()
    {
        var dict = new WeakDictionary<object, object>();

        var key = new object();

        _ = dict.GetOrAdd(key, k => new WeakReference(k));

        var weakKey = new WeakReference(key);
        key = null;

        await Collect();

        Assert.Null(weakKey.Target);
        Assert.Empty(dict);
    }

    [Fact]
    public async Task Entry_Removed_After_GC_When_Key_Referenced_In_A_Weak_Referenced_Value()
    {
        var dict = new WeakDictionary<object, WeakReference<Tuple<int, object>>>();

        var key = new object();
        var value = new WeakReference<Tuple<int, object>>(new Tuple<int, object>(0, key));

        _ = dict.GetOrAdd(key, _ => value);

        var weakKey = new WeakReference(key);
        key = null;

        await Collect();

        Assert.Null(weakKey.Target);
        Assert.Empty(dict);
    }

    [Fact]
    public void Indexer_Sets_Item()
    {
        var key = new object();

        var dict = new WeakDictionary<object, int>();

        dict[key] = 1;

        Assert.Equal(1, dict[key]);
    }

    [Fact]
    public void Indexer_Updates_Item_For_Existing_Key()
    {
        var key = new object();

        var dict = new WeakDictionary<object, int>
        {
            { key, 1 }
        };

        dict[key] = 2;

        Assert.Equal(2, dict[key]);
    }

    [Fact]
    public void Indexer_Gets_Existing_Item()
    {
        var key = new object();

        var dict = new WeakDictionary<object, int>
        {
            { key, 1 }
        };

        Assert.Equal(1, dict[key]);
    }

    [Fact]
    public void Indexer_Getting_Non_Existing_Item_Throws_KeyNotFoundException()
    {
        var key = new object();

        var dict = new WeakDictionary<object, int>
        {
            { key, 1 }
        };

        Assert.Throws<KeyNotFoundException>(() => dict[new object()]);
    }

    [Fact]
    public void Indexer_Key_Is_Null_Throws_ArgumentNullException()
    {
        var dict = new WeakDictionary<object, int>();

        Assert.Throws<ArgumentNullException>("key", () => dict[null!] = 1);
    }

    [Fact]
    public async Task Values_Returns_Values_For_Entries_Where_Key_Is_Not_GCd()
    {
        var key1 = new Custom(1);
        var key2 = new Custom(2);

        var dict = new WeakDictionary<Custom, int>
        {
            { key1, 1 },
            { key2, 2 }
        };

        key1 = null;

        await Collect();

        Assert.Equal(new[] { 2 }, dict.Values);
        Assert.NotNull(key2);
    }

    [Fact]
    public async Task Keys_Returns_Keys_For_Entries_Where_Key_Is_Not_GCd()
    {
        var key1 = new Custom(1);
        var key2 = new Custom(2);

        var dict = new WeakDictionary<Custom, int>
        {
            { key1, 1 },
            { key2, 2 }
        };

        key1 = null;

        await Collect();

        Assert.Equal(new[] { key2 }, dict.Keys);
    }

    [Fact]
    public void IsReadOnly_Returns_False()
    {
        var dict = new WeakDictionary<Custom, int>();

        Assert.False(((ICollection<KeyValuePair<Custom, int>>)dict).IsReadOnly);
    }

    [Fact]
    public void Add_Using_Custom_Key_Comparer_Throws_ArgumentException_On_Equality()
    {
        var key1 = new Custom(1);
        var key2 = new Custom(2);
        var key2a = new Custom(2);
        var key3 = new Custom(3);

        var dict = new WeakDictionary<Custom, int>(new CustomComparer())
        {
            { key1, 1 },
            { key2, 2 },
            { key3, 3 }
        };

        Assert.Throws<ArgumentException>("key", () => dict.Add(key2a, 24));
    }

    [Fact]
    public void Add_Key_And_Value_Adds()
    {
        var key = new object();

        var dict = new WeakDictionary<object, int>();

        dict.Add(key, 2);

        Assert.Equal(2, dict[key]);
    }

    [Fact]
    public void Add_KeyValuePair_Adds()
    {
        var key = new object();

        var dict = new WeakDictionary<object, int>();

        dict.Add(new KeyValuePair<object, int>(key, 2));

        Assert.Equal(2, dict[key]);
    }

    [Fact]
    public void Add_Key_Is_Null_Throws_ArgumentNullException()
    {
        var dict = new WeakDictionary<object, int>();

        Assert.Throws<ArgumentNullException>("key", () => dict.Add(null!, 2));
    }

    [Fact]
    public void Remove_Key_Not_Found_Returns_False()
    {
        var dict = new WeakDictionary<object, int>();

        var success = dict.Remove(new object());

        Assert.False(success);
    }

    [Fact]
    public void Remove_Key_Found_Removes_And_Returns_True()
    {
        var key = new object();

        var dict = new WeakDictionary<object, int>
        {
            { key, 1 }
        };

        var success = dict.Remove(key);

        Assert.True(success);
        Assert.Empty(dict);
    }

    [Fact]
    public void Remove_Key_Is_Null_Throws_ArgumentNullException()
    {
        var dict = new WeakDictionary<object, int>();

        Assert.Throws<ArgumentNullException>("key", () => dict.Remove(null!));
    }

    [Fact]
    public void Remove_KeyValuePair_Key_Is_Null_Throws_ArgumentNullException()
    {
        var dict = new WeakDictionary<object, int>();

        Assert.Throws<ArgumentNullException>("key", () => ((ICollection<KeyValuePair<object, int>>)dict).Remove(new KeyValuePair<object, int>(null!, 2)));
    }

    [Fact]
    public void Remove_KeyValuePair_Key_Not_Found_Returns_False()
    {
        var dict = new WeakDictionary<object, int>();

        var success = dict.Remove(new KeyValuePair<object, int>(null!, 2));

        Assert.False(success);
    }

    [Fact]
    public void Remove_KeyValuePair_Key_Found_And_Values_Match_Removes_And_Returns_True()
    {
        var key = new object();

        var dict = new WeakDictionary<object, int>
        {
            { key, 1 }
        };

        var success = ((ICollection<KeyValuePair<object, int>>)dict).Remove(new KeyValuePair<object, int>(key, 1));

        Assert.True(success);
    }

    [Fact]
    public void Remove_KeyValuePair_Key_Found_But_Values_Dont_Match_Keeps_And_Returns_False()
    {
        var key = new object();

        var dict = new WeakDictionary<object, int>
        {
            { key, 1 }
        };

        var success = dict.Remove(new KeyValuePair<object, int>(key, 2));

        Assert.False(success);
        Assert.Single(dict);
    }

    [Fact]
    public void ContainsKey_When_Key_Is_Not_GCd_Returns_True()
    {
        var key = new object();

        var dict = new WeakDictionary<object, int>
        {
            { key, 1 }
        };

        Assert.True(dict.ContainsKey(key));
    }

    [Fact]
    public void ContainsKey_When_Key_Is_Null_Throws_ArgumentNullException()
    {
        var dict = new WeakDictionary<object, int>();

        _ = Assert.Throws<ArgumentNullException>("key", () => dict.ContainsKey(null!));
    }

    [Fact]
    public void Contains_When_Key_Is_Not_GCd_Returns_True()
    {
        var kvp = KeyValuePair.Create(new object(), 1);

        var dict = new WeakDictionary<object, int>
        {
            { kvp.Key, kvp.Value }
        };

        Assert.Contains<KeyValuePair<object, int>>(kvp, dict);
    }

    [Fact]
    public void Contains_When_Value_Does_Not_Match_Returns_False()
    {
        var kvp = KeyValuePair.Create(new object(), 1);

        var dict = new WeakDictionary<object, int>
        {
            { kvp.Key, 2 }
        };

        Assert.DoesNotContain<KeyValuePair<object, int>>(kvp, dict);
    }

    [Fact]
    public void Contains_When_Key_Is_Null_Throws_ArgumentNullException()
    {
        var dict = new WeakDictionary<object, int>();

        _ = Assert.Throws<ArgumentNullException>("item", () => dict.Contains(new KeyValuePair<object, int>(null!, 1)));
    }

    [Fact]
    public void Contains_When_KeyValuePair_Not_Found_Returns_False()
    {
        var dict = new WeakDictionary<object, int>();

        var contains = dict.Contains(new KeyValuePair<object, int>(new object(), 1));

        Assert.False(contains);
    }

    [Fact]
    public void Contains_When_KeyValuePair_Exists_Returns_True()
    {
        var key = new object();

        var dict = new WeakDictionary<object, int>
        {
            { key, 1 }
        };

        var contains = dict.Contains(new KeyValuePair<object, int>(key, 1));

        Assert.True(contains);
    }

    private static async Task Collect()
    {
        for (int i = 0; i < 3; i++)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
            await Task.Delay(10);
        }
    }

    private sealed class Custom
    {
        public Custom(int hashCode)
        {
            HashCode = hashCode;
        }

        public int HashCode { get; }
    }

    private sealed class CustomComparer : IEqualityComparer<Custom>
    {
        public bool Equals(Custom? x, Custom? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null) || x.GetType() != y.GetType())
            {
                return false;
            }

            return x.HashCode == y.HashCode;
        }

        public int GetHashCode(Custom obj)
        {
            return obj.HashCode;
        }
    }
}
