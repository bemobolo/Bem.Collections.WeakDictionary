// --------------------------------------
// <copyright file="RemoveBenchmark.cs" company="Daniel Balogh">
//     Copyright (c) Daniel Balogh. All rights reserved.
//     Licensed under the GNU Generic Public License 3.0 license.
//     See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------

using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace Bem.Collections.WeakDictionary.Benchmarks;

public class RemoveBenchmark : BenchmarkBase
{
    private object[]? _keys;

    [IterationSetup]
    public void IterationSetup()
    {
        _keys = Enumerable.Range(0, N).Select(_ => new object()).ToArray();
        _dictionary = _keys.ToDictionary(o => o, _ => 0);
        _concurrentDictionary = new ConcurrentDictionary<object, int>(_dictionary);
        _weakDictionary = new WeakDictionary<object, int>(_concurrentDictionary);
        _table = new ConditionalWeakTable<object, object>();

        foreach (var key in _keys)
        {
            _table.Add(key, key);
        }
    }

    [Benchmark]
    public void WeakDictionary_Remove()
    {
        for (int i = 0; i < N; i++)
        {
            _weakDictionary.Remove(_keys![i]);
        }
    }

    [Benchmark]
    public void ConcurrentDictionary_Remove()
    {
        for (int i = 0; i < N; i++)
        {
            _concurrentDictionary.TryRemove(_keys![i], out _);
        }
    }

    [Benchmark]
    public void Dictionary_Remove()
    {
        for (int i = 0; i < N; i++)
        {
            _dictionary.Remove(_keys![i]);
        }
    }

    [Benchmark(Baseline = true)]
    public void ConditionalWeakTable_Remove()
    {
        for (int i = 0; i < N; i++)
        {
            _table.Remove(_keys![i]);
        }
    }
}
