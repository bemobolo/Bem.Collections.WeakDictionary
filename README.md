# Bem.Collections.WeakDictionary
![Nuget (with prereleases)](https://img.shields.io/nuget/v/Bem.Collections.WeakDictionary)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/bemobolo/Bem.Collections.WeakDictionary)
![Azure DevOps builds (branch)](https://img.shields.io/azure-devops/build/dbalogh/Bem.Collections.WeakDictionary/17/main)
![Azure DevOps tests](http://bemobolo.ddns.net/azure-devops/tests/dbalogh/Bem.Collections.WeakDictionary/17/main?compact_message)
![Azure DevOps coverage (branch)](http://bemobolo.ddns.net/azure-devops/coverage/dbalogh/Bem.Collections.WeakDictionary/17/main)
# Overview
A thread-safe dictionary implementation that stores values with weekly referenced keys. See [`WeakReference<T>`](https://learn.microsoft.com/en-us/dotnet/api/system.weakreference-1).  
Once a key is not strong-referenced anymore the entry will be removed automatically from `WeakDictionary<TKey,TValue>`.  
This collection can be used for similar purposes as [`ConditionalWeakTable<TKey,TValue>`](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.conditionalweaktable-2) except that `WeakDictionary<TKey,TValue>` can accept custom [`IEqualityComparer<T>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.iequalitycomparer-1) instance for key comparison. [`ConditionalWeakTable<TKey,TValue>`](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.conditionalweaktable-2) can only compare keys by reference no matter if [`GetHashCode()`](https://learn.microsoft.com/en-us/dotnet/api/system.object.gethashcode) and [`Equals()`](https://learn.microsoft.com/en-us/dotnet/api/system.object.equals) methods are overridden or not.  
Internally `WeakDictionary<TKey,TValue>` uses a combination of [`Dictionary<TKey,TValue>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2) and [`ConditionalWeakTable<TKey,TValue>`](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.conditionalweaktable-2) types to manage entries.
# Usage
Use the same way as a generic [`Dictionary<TKey,TValue>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2).  

```csharp
var dictionary = new WeakDictionary<MyReferenceType, AnyType>(optionalEqualityComparer);
```
Note that `WeakDictionary<TKey,TValue>` implements only [`IDictionary<TKey,TValue>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic), [`IEnumerable<T>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1) and [`IEnumerable`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.ienumerable) interfaces.  
Note that `TKey` must be reference type, but `TValue` can be anything you want.  
**Also note that `TValue` by default is not stored as a [`WeakReference<T>`](https://learn.microsoft.com/en-us/dotnet/api/system.weakreference-1). Therefore if key and value reference the same object GC can not collect the instance!**  
Consider this if you plan to use `WeakDictionary<TKey,TValue>` as a [`HashSet<T>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1) and avoid using the same instance for key and value.  
## Use Cases
The single use case of this library is when you need to use a custom [`IEqualityComparer<T>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.iequalitycomparer-1) for the weakly referenced keys. If this is not a requirement use the [`ConditionalWeakTable<TKey,Tvalue>`](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.conditionalweaktable-2) instead.
# Thread Safety
All public and protected members of `WeakDictionary<TKey,TValue>` are thread-safe and may be used concurrently from multiple threads. However, members accessed through one of the interfaces the `WeakDictionary<TKey,TValue>` implements, including extension methods, are not guaranteed to be thread safe and may need to be synchronized by the caller.  

# Benchmarks
The below results show that `WeakDictionary<TKey,TValue>` has some performance cost compared to the other data structures probably because of all the locks surrounding the logic of the internal data structure mutations.


## Runtime=.NET 7.0
### Populating
|                   Method |      N |      Mean |      Error |   StdDev | Ratio | RatioSD |
|------------------------- |------- |----------:|-----------:|---------:|------:|--------:|
|       WeakDictionary_Add | 100000 | 217.97 ms | 117.891 ms | 6.462 ms |  4.56 |    1.12 |
| ConcurrentDictionary_Add | 100000 |  49.66 ms |   9.680 ms | 0.531 ms |  1.04 |    0.24 |
|           Dictionary_Add | 100000 |  18.18 ms |  47.396 ms | 2.598 ms |  0.37 |    0.07 |
| ConditionalWeakTable_Add | 100000 |  49.44 ms | 177.786 ms | 9.745 ms |  1.00 |    0.00 |
### Getting items
|                           Method |      N |     Mean |    Error |   StdDev | Ratio | RatioSD |
|--------------------------------- |------- |---------:|---------:|---------:|------:|--------:|
|       WeakDictionary_TryGetValue | 100000 | 71.26 ms | 48.43 ms | 2.655 ms |  2.09 |    0.27 |
| ConcurrentDictionary_TryGetValue | 100000 | 12.10 ms | 36.76 ms | 2.015 ms |  0.35 |    0.04 |
|           Dictionary_TryGetValue | 100000 | 20.75 ms | 31.57 ms | 1.730 ms |  0.61 |    0.09 |
| ConditionalWeakTable_TryGetValue | 100000 | 34.57 ms | 89.00 ms | 4.879 ms |  1.00 |    0.00 |
### Updating items
|                   Method |      N |      Mean |     Error |    StdDev | Ratio | RatioSD |
|------------------------- |------- |----------:|----------:|----------:|------:|--------:|
|       WeakDictionary_Set | 100000 | 378.94 ms | 746.94 ms | 40.942 ms | 18.87 |    0.79 |
| ConcurrentDictionary_Set | 100000 |  26.72 ms |  32.10 ms |  1.759 ms |  1.33 |    0.04 |
|           Dictionary_Set | 100000 |  22.34 ms |  17.56 ms |  0.963 ms |  1.12 |    0.14 |
| ConditionalWeakTable_Set | 100000 |  20.07 ms |  33.44 ms |  1.833 ms |  1.00 |    0.00 |
### Removing items
|                      Method |      N |      Mean |     Error |    StdDev | Ratio | RatioSD |
|---------------------------- |------- |----------:|----------:|----------:|------:|--------:|
|       WeakDictionary_Remove | 100000 | 130.70 ms | 262.21 ms | 14.373 ms |  8.85 |    3.03 |
| ConcurrentDictionary_Remove | 100000 |  23.11 ms |  38.31 ms |  2.100 ms |  1.53 |    0.26 |
|           Dictionary_Remove | 100000 |  14.89 ms |  39.01 ms |  2.138 ms |  0.99 |    0.22 |
| ConditionalWeakTable_Remove | 100000 |  15.49 ms |  59.07 ms |  3.238 ms |  1.00 |    0.00 |
