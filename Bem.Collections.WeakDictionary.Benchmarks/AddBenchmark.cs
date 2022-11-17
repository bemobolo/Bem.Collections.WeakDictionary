// --------------------------------------
// <copyright file="AddBenchmark.cs" company="Daniel Balogh">
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

public class AddBenchmark : BenchmarkBase
{
    private object[]? _keys;

    [IterationSetup]
    public void IterationSetup()
    {
        _keys = Enumerable.Range(0, N).Select(_ => new object()).ToArray();
        _concurrentDictionary = new ConcurrentDictionary<object, int>();
        _weakDictionary = new WeakDictionary<object, int>();
        _dictionary = new Dictionary<object, int>();
        _table = new ConditionalWeakTable<object, object>();
    }

    [Benchmark]
    public void WeakDictionary_Add()
    {
        for (int i = 0; i < N; i++)
        {
            _weakDictionary.Add(_keys![i], i);
        }
    }

    [Benchmark]
    public void ConcurrentDictionary_Add()
    {
        for (int i = 0; i < N; i++)
        {
            _concurrentDictionary.TryAdd(_keys![i], i);
        }
    }

    [Benchmark]
    public void Dictionary_Add()
    {
        for (int i = 0; i < N; i++)
        {
            _dictionary.Add(_keys![i], i);
        }
    }

    [Benchmark(Baseline = true)]
    public void ConditionalWeakTable_Add()
    {
        for (int i = 0; i < N; i++)
        {
            _table.Add(_keys![i], _keys![i]);
        }
    }
}
