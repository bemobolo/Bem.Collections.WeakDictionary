// --------------------------------------
// <copyright file="WeakDictionary.cs" company="Daniel Balogh">
//     Copyright (c) Daniel Balogh. All rights reserved.
//     Licensed under the GNU Generic Public License 3.0 license.
//     See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Bem.Collections.WeakDictionary;

/// <summary>
/// Represents a thread-safe collection of keys and values where keys stored as <see cref="WeakReference{TKey}"/> instances.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
/// <remarks>
/// Weakly referenced keys - and therefore entries - are subject of garbage collection.
/// Collected entries are removed during key finalization.
/// All public and protected members of ConcurrentDictionary{TKey,TValue} are thread-safe
/// and may be used concurrently from multiple threads. However, members accessed through
/// the interfaces the WeakDictionary{TKey,TValue} implements are not guaranteed to b
/// thread safe and may need to be synchronized by the caller.
/// </remarks>
[DebuggerDisplay("Count = {Count}")]
public sealed class WeakDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    where TKey : class
{
    private readonly IEqualityComparer<TKey> _comparer;
    private readonly Dictionary<HashMemoWeakReference<TKey>, TValue> _dict;
    private readonly ConditionalWeakTable<TKey, FinalizableWeakReference<TKey>> _table;
    private readonly object _syncRoot = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="WeakDictionary{TKey, TValue}"/> class.
    /// </summary>
    /// <remarks>
    /// All public and protected members of <see cref="WeakDictionary{TKey,TValue}"/> are thread-safe and may be used
    /// concurrently from multiple threads.
    /// </remarks>
    public WeakDictionary()
        : this(EqualityComparer<TKey>.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeakDictionary{TKey, TValue}"/>
    /// class that uses the specified <see cref="IEqualityComparer{TKey}"/>.
    /// </summary>
    /// <param name="comparer">The <see cref="IEqualityComparer{TKey}"/>
    /// implementation to use when comparing keys.</param>
    public WeakDictionary(IEqualityComparer<TKey> comparer)
        : this(Enumerable.Empty<KeyValuePair<TKey, TValue>>(), comparer)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeakDictionary{TKey,TValue}"/>
    /// class that contains elements copied from the specified <see
    /// cref="IEnumerable{KeyValuePair}"/>, has the default concurrency
    /// level, has the default initial capacity, and uses the default comparer for the key type.
    /// </summary>
    /// <param name="collection">The <see
    /// cref="IEnumerable{KeyValuePair}"/> whose elements are copied to
    /// the new
    /// <see cref="WeakDictionary{TKey,TValue}"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is a null reference
    /// (Nothing in Visual Basic).</exception>
    /// <exception cref="ArgumentException"><paramref name="collection"/> contains one or more
    /// duplicate keys.</exception>
    public WeakDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        : this(collection, EqualityComparer<TKey>.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeakDictionary{TKey,TValue}"/>
    /// class that contains elements copied from the specified <see
    /// cref="IEnumerable{KeyValuePair}"/>, has the default concurrency level, has the default
    /// initial capacity, and uses the specified
    /// <see cref="IEqualityComparer{TKey}"/>.
    /// </summary>
    /// <param name="collection">The <see
    /// cref="IEnumerable{KeyValuePair}"/> whose elements are copied to
    /// the new
    /// <see cref="WeakDictionary{TKey,TValue}"/>.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{TKey}"/>
    /// implementation to use when comparing keys.</param>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is a null reference
    /// (Nothing in Visual Basic).</exception>
    public WeakDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

        _table = new();
        _dict = new(new HashMemoWeakReferenceEqualityComparer<TKey>(comparer));

        foreach (var kvp in collection)
        {
            Add(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Gets a collection containing the keys in the <see
    /// cref="WeakDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <value>An <see cref="ICollection{TKey}"/> containing the keys in the
    /// <see cref="WeakDictionary{TKey,TValue}"/>.</value>
    public ICollection<TKey> Keys => _dict.Keys
                .Select(h => (IsAlive: h.TryGetTarget(out var key), Key: key))
                .Where(t => t.IsAlive)
                .Select(t => t.Key!)
                .ToList();

    /// <summary>
    /// Gets a collection containing the values in the <see
    /// cref="WeakDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <value>An
    ///     <see cref="ICollection{TValue}"/>
    /// containing the values in the
    ///     <see cref="WeakDictionary{TKey,TValue}"/>
    /// .
    /// </value>
    public ICollection<TValue> Values => _dict.Values;

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    /// <summary>
    /// Gets the number of key/value pairs contained in the <see
    /// cref="WeakDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <exception cref="OverflowException">The dictionary contains too many
    /// elements.</exception>
    /// <value>The number of key/value pairs contained in the <see
    /// cref="WeakDictionary{TKey,TValue}"/>.</value>
    /// <remarks>Count has snapshot semantics and represents the number of items in the <see
    /// cref="WeakDictionary{TKey,TValue}"/>
    /// at the moment when Count was accessed.</remarks>
    public int Count => _dict.Count;

    /// <inheritdoc cref="ConcurrentDictionary{Tkey,TValue}.this" />
    public TValue this[TKey key]
    {
        get => _dict[new HashMemoWeakReference<TKey>(key, _comparer)];
        set
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            lock (_syncRoot)
            {
                _ = _dict.AddOrUpdate(
                    new HashMemoWeakReference<TKey>(key, _comparer),
                    _ =>
                    {
                        AddToTable(key);
                        return value;
                    },
                    (_, _) =>
                    {
                        _ = TryRemoveFromTable(key, out _);

                        AddToTable(key);
                        return value;
                    });
            }
        }
    }

    /// <summary>
    /// Attempts to add the specified key and value to the <see cref="WeakDictionary{TKey,
    /// TValue}"/>.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add. The value can be a null reference (Nothing
    /// in Visual Basic) for reference types.</param>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null reference
    /// (Nothing in Visual Basic).</exception>
    /// /// <exception cref="ArgumentException"><paramref name="key"/> already exists.</exception>
    /// <exception cref="OverflowException">The <see cref="WeakDictionary{TKey, TValue}"/>
    /// contains too many elements.</exception>
    public void Add(TKey key, TValue value)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        lock (_syncRoot)
        {
            _ = _dict.AddOrUpdate(
                new HashMemoWeakReference<TKey>(key, _comparer),
                _ =>
                {
                    AddToTable(key);
                    return value;
                },
                (key, _) => throw new ArgumentException("An element with the same key already exists", nameof(key)));
        }
    }

    /// <summary>
    /// Adds a key/value pair to the <see cref="WeakDictionary{TKey,TValue}"/>
    /// if the key does not already exist.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="valueFactory">The function used to generate a value for the key.</param>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is a null reference
    /// (Nothing in Visual Basic).</exception>
    /// <exception cref="ArgumentNullException"><paramref name="valueFactory"/> is a null reference
    /// (Nothing in Visual Basic).</exception>
    /// <exception cref="OverflowException">The dictionary contains too many
    /// elements.</exception>
    /// <returns>The value for the key.  This will be either the existing value for the key if the
    /// key is already in the dictionary, or the new value for the key as returned by valueFactory
    /// if the key was not in the dictionary.</returns>
    public TValue? GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (valueFactory == null)
        {
            throw new ArgumentNullException(nameof(valueFactory));
        }

        lock (_syncRoot)
        {
            return _dict.GetOrAdd(
                new HashMemoWeakReference<TKey>(key, _comparer),
                _ =>
                {
                    var value = valueFactory(key);
                    AddToTable(key);
                    return value;
                });
        }
    }

    /// <summary>
    /// Removes all keys and values from the <see cref="WeakDictionary{TKey,TValue}"/>.
    /// </summary>
    public void Clear()
    {
        lock (_syncRoot)
        {
            _dict.Clear();
            _table.Clear();
        }
    }

    /// <summary>
    /// Determines whether the <see cref="WeakDictionary{TKey,TValue}"/>
    /// contains a specific key and value.
    /// </summary>
    /// <param name="item">The <see cref="KeyValuePair{TKey,TValue}"/>
    /// structure to locate in the <see cref="WeakDictionary{TKey,TValue}"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
    /// <returns>true if the <paramref name="item"/> is found in the <see
    /// cref="WeakDictionary{TKey,TValue}"/>; otherwise, false.</returns>
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        if (item.Key == null)
        {
            throw new ArgumentNullException(nameof(item), "Key must not be null!");
        }

        return ((IDictionary<HashMemoWeakReference<TKey>, TValue>)_dict).Contains(
            new KeyValuePair<HashMemoWeakReference<TKey>, TValue>(
                new HashMemoWeakReference<TKey>(item.Key, _comparer),
                item.Value));
    }

    /// <summary>
    /// Determines whether the <see cref="WeakDictionary{TKey, TValue}"/> contains the specified
    /// key.
    /// </summary>
    /// <param name="key">The key to locate in the <see cref="WeakDictionary{TKey,
    /// TValue}"/>.</param>
    /// <returns>true if the <see cref="WeakDictionary{TKey, TValue}"/> contains an element with
    /// the specified key; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is a null reference
    /// (Nothing in Visual Basic).</exception>
    public bool ContainsKey(TKey key)
    {
        return key == null
            ? throw new ArgumentNullException(nameof(key))
            : _dict.ContainsKey(new HashMemoWeakReference<TKey>(key, _comparer));
    }

    /// <summary>
    /// Removes the element with the specified key from the <see cref="WeakDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>true if the element is successfully remove; otherwise false. This method also returns
    /// false if <paramref name="key"/> was not found in the original <see cref="WeakDictionary{TKey,TValue}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is a null reference
    /// (Nothing in Visual Basic).</exception>
    public bool Remove(TKey key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        var success = false;

        lock (_syncRoot)
        {
            success = TryRemoveFromTable(key, out var finalizable) && _dict.TryRemove(finalizable, out _);
        }

        return success;
    }

    /// <summary>
    /// Attempts to get the value associated with the specified key from the <see
    /// cref="WeakDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">When this method returns, <paramref name="value"/> contains the object from
    /// the
    /// <see cref="WeakDictionary{TKey,TValue}"/> with the specified key or the default value of
    /// <typeparamref name="TValue"/>, if the operation failed.</param>
    /// <returns>true if the key was found in the <see cref="WeakDictionary{TKey,TValue}"/>;
    /// otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is a null reference
    /// (Nothing in Visual Basic).</exception>
    public bool TryGetValue(TKey key, out TValue value)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        return _dict.TryGetValue(new HashMemoWeakReference<TKey>(key, _comparer), out value);
    }

    /// <summary>
    /// Attempts to add the specified <see cref="KeyValuePair{TKey,TValue}" /> to the <see cref="WeakDictionary{TKey,
    /// TValue}"/>.
    /// </summary>
    /// <param name="item">The <see cref="KeyValuePair{TKey,TValue}" />
    /// structure representing the key and value to add to the <see
    /// cref="WeakDictionary{TKey,TValue}"/>.</param>
    /// <exception cref="ArgumentNullException">The Key property of <paramref
    /// name="item"/> is null.</exception>
    /// /// <exception cref="ArgumentException"><paramref name="item"/> already exists.</exception>
    /// <exception cref="OverflowException">The <see cref="WeakDictionary{TKey, TValue}"/>
    /// contains too many elements.</exception>
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    /// <inheritdoc/>
    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (arrayIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Index must be greater than or equal to zero!");
        }

        int i = 0;
        foreach (var kvp in _dict)
        {
            if (array.Length - Count < arrayIndex)
            {
                throw new ArgumentException("Array is not large enough!", nameof(array));
            }

            if (kvp.Key.TryGetTarget(out var key))
            {
                array[arrayIndex + i] = new KeyValuePair<TKey, TValue>(key!, kvp.Value);
                i++;
            }
        }
    }

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        return TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(item.Value, value) &&
               Remove(item.Key);
    }

    /// <inheritdoc/>
    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return _dict
            .Select(kvp => (IsAlive: kvp.Key.TryGetTarget(out var key), Key: key, kvp.Value))
            .Where(t => t.IsAlive)
            .Select(t => new KeyValuePair<TKey, TValue>(t.Key!, t.Value))
            .GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>)this).GetEnumerator();

    private void FinalizableOnFinalizing(HashMemoWeakReference<TKey> key)
    {
        _ = _dict.TryRemove(key, out _);
    }

    private void AddToTable(TKey key)
    {
        var finalizable = CreateFinalizable(key);
        _table.Add(key, finalizable);
    }

    private FinalizableWeakReference<TKey> CreateFinalizable(TKey key)
    {
        var finalizable = new FinalizableWeakReference<TKey>(key, _comparer);
        finalizable.FinalizingCallback += FinalizableOnFinalizing;
        return finalizable;
    }

    private bool TryRemoveFromTable(TKey key, out FinalizableWeakReference<TKey> value)
    {
        var success = false;

        if (_table.TryGetValue(key, out value))
        {
            value.FinalizingCallback -= FinalizableOnFinalizing;
            success = _table.Remove(key);
        }

        return success;
    }

    private class HashMemoWeakReference<T>
        where T : class
    {
        private readonly WeakReference<T> _item;
        private readonly int _hashCode;

        public HashMemoWeakReference(T item, IEqualityComparer<T> equalityComparer)
        {
            _item = new WeakReference<T>(item);
            _hashCode = equalityComparer.GetHashCode(item);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public bool TryGetTarget(out T? item)
        {
            return _item.TryGetTarget(out item);
        }
    }

    private sealed class FinalizableWeakReference<T> : HashMemoWeakReference<T>
        where T : class
    {
        public FinalizableWeakReference(T item, IEqualityComparer<T> equalityComparer)
            : base(item, equalityComparer)
        {
        }

        ~FinalizableWeakReference()
        {
            FinalizingCallback?.Invoke(this);
        }

        internal delegate void FinalizingDelegate(FinalizableWeakReference<T> reference);

        internal FinalizingDelegate? FinalizingCallback { get; set; }
    }

    private sealed class HashMemoWeakReferenceEqualityComparer<T> : IEqualityComparer<HashMemoWeakReference<T>>
        where T : class
    {
        private readonly IEqualityComparer<T> _equalityComparer;

        public HashMemoWeakReferenceEqualityComparer(IEqualityComparer<T> equalityComparer)
        {
            _equalityComparer = equalityComparer;
        }

        public bool Equals(HashMemoWeakReference<T> x, HashMemoWeakReference<T> y)
        {
            return (x.TryGetTarget(out var a) && y.TryGetTarget(out var b) && _equalityComparer.Equals(a, b)) ||
                   (!x.TryGetTarget(out _) && !y.TryGetTarget(out _)); // two not available (null) targets are equal
        }

        public int GetHashCode(HashMemoWeakReference<T> obj)
        {
            return obj.GetHashCode();
        }
    }
}
