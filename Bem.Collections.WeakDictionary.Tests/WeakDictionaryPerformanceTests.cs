// --------------------------------------
// <copyright file="WeakDictionaryPerformanceTests.cs" company="Daniel Balogh">
//     Copyright (c) Daniel Balogh. All rights reserved.
//     Licensed under the GNU Generic Public License 3.0 license.
//     See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------

using Bem.Collections.WeakDictionary.Benchmarks;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using Xunit.Abstractions;

namespace Bem.Collections.WeakDictionary.Tests;

[Trait("Category", "Benchmark")]
public class WeakDictionaryPerformanceTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly AccumulationLogger _logger;
    private readonly BenchmarkConfig _config;

    public WeakDictionaryPerformanceTests(ITestOutputHelper testOutputHelper)
    {
        _logger = new AccumulationLogger();
        _config = new BenchmarkConfig(_logger);
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Benchmark_TryGetValue()
    {
        _ = BenchmarkRunner.Run<TryGetValueBenchmark>(_config);

        _testOutputHelper.WriteLine(_logger.GetLog());
    }

    [Fact]
    public void Benchmark_Add()
    {
        _ = BenchmarkRunner.Run<AddBenchmark>(_config);

        _testOutputHelper.WriteLine(_logger.GetLog());
    }

    [Fact]
    public void Benchmark_Set()
    {
        _ = BenchmarkRunner.Run<SetBenchmark>(_config);

        _testOutputHelper.WriteLine(_logger.GetLog());
    }

    [Fact]
    public void Benchmark_Remove()
    {
        _ = BenchmarkRunner.Run<RemoveBenchmark>(_config);

        _testOutputHelper.WriteLine(_logger.GetLog());
    }
}
