// --------------------------------------
// <copyright file="SetBenchmark.cs" company="Daniel Balogh">
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

public class SetBenchmark : BenchmarkBase
{
    private object[]? _keys;

    [IterationSetup]
    public void Setup()
    {
        _concurrentDictionary = new ConcurrentDictionary<object, int>();
        _weakDictionary = new WeakDictionary<object, int>();
        _dictionary = new Dictionary<object, int>();
        _keys = new object[N];
        _table = new ConditionalWeakTable<object, object>();

        foreach (var i in Enumerable.Range(0, N))
        {
            _keys[i] = new object();
            _concurrentDictionary.TryAdd(_keys[i], 0);
            _weakDictionary.Add(_keys[i], 0);
            _dictionary.Add(_keys[i], 0);
            _table.Add(_keys[i], _keys[i]);
        }
    }

    [Benchmark]
    public void WeakDictionary_Set()
    {
        for (int i = 0; i < N; i++)
        {
            _weakDictionary[_keys![i]] = i;
        }
    }

    [Benchmark]
    public void ConcurrentDictionary_Set()
    {
        for (int i = 0; i < N; i++)
        {
            _concurrentDictionary[_keys![i]] = i;
        }
    }

    [Benchmark]
    public void Dictionary_Set()
    {
        for (int i = 0; i < N; i++)
        {
            _concurrentDictionary[_keys![i]] = i;
        }
    }

    [Benchmark(Baseline = true)]
    public void ConditionalWeakTable_Set()
    {
        for (int i = 0; i < N; i++)
        {
            _table.AddOrUpdate(_keys![i], _keys[i]);
        }
    }
}
