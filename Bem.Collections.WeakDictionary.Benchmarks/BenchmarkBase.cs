// --------------------------------------
// <copyright file="BenchmarkBase.cs" company="Daniel Balogh">
//     Copyright (c) Daniel Balogh. All rights reserved.
//     Licensed under the GNU Generic Public License 3.0 license.
//     See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace Bem.Collections.WeakDictionary.Benchmarks;

public abstract class BenchmarkBase
{
    [Params(100_000)]
    public int N;

    protected ConcurrentDictionary<object, int> _concurrentDictionary;

    protected WeakDictionary<object, int> _weakDictionary;

    protected Dictionary<object, int> _dictionary;

    protected ConditionalWeakTable<object, object> _table;
}
