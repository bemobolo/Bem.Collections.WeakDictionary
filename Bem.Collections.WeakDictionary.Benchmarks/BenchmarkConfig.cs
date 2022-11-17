// --------------------------------------
// <copyright file="BenchmarkConfig.cs" company="Daniel Balogh">
//     Copyright (c) Daniel Balogh. All rights reserved.
//     Licensed under the GNU Generic Public License 3.0 license.
//     See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;

namespace Bem.Collections.WeakDictionary.Benchmarks;

public class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig(ILogger logger)
    {
        Add(DefaultConfig.Instance);
        AddLogger(logger);
        AddJob(Job.Default
            .WithRuntime(CoreRuntime.Core70)
            .WithLaunchCount(1)
            .WithIterationCount(3)
            .WithUnrollFactor(1)
            .WithInvocationCount(1)
            .WithWarmupCount(1));
    }
}
