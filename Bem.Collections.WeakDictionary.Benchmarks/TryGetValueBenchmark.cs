// --------------------------------------
// <copyright file="TryGetValueBenchmark.cs" company="Daniel Balogh">
//     Copyright (c) Daniel Balogh. All rights reserved.
//     Licensed under the GNU Generic Public License 3.0 license.
//     See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace Bem.Collections.WeakDictionary.Benchmarks;

public class TryGetValueBenchmark : BenchmarkBase
{
    private object[]? _keys;

    [IterationSetup]
    public void Setup()
    {
        _concurrentDictionary = new ConcurrentDictionary<object, int>();
        _weakDictionary = new WeakDictionary<object, int>();
        _dictionary = new Dictionary<object, int>();
        _table = new ConditionalWeakTable<object, object>();

        for (int i = 0; i < N; i++)
        {
            var key = new object();
            _concurrentDictionary.TryAdd(key, i);
            _weakDictionary.Add(key, i);
            _dictionary.Add(key, i);
            _table.Add(key, key);
        }

        _keys = _concurrentDictionary.Keys.ToArray();
    }

    [Benchmark]
    public void WeakDictionary_TryGetValue()
    {
        for (int i = 0; i < N; i++)
        {
            _weakDictionary.TryGetValue(_keys![i], out _);
        }
    }

    [Benchmark]
    public void ConcurrentDictionary_TryGetValue()
    {
        for (int i = 0; i < N; i++)
        {
            _concurrentDictionary.TryGetValue(_keys![i], out _);
        }
    }

    [Benchmark]
    public void Dictionary_TryGetValue()
    {
        for (int i = 0; i < N; i++)
        {
            _dictionary.TryGetValue(_keys![i], out _);
        }
    }

    [Benchmark(Baseline = true)]
    public void ConditionalWeakTable_TryGetValue()
    {
        for (int i = 0; i < N; i++)
        {
            _table.TryGetValue(_keys![i], out _);
        }
    }
}
