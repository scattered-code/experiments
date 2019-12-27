# experiments


## Async Processing Benchmarks
Create a ravendb database on a sevrer with high latency from you, such as in the AWS when running the benchmark from your computer.

Add database information to appsettings.json or create an appsettings.local.json

Seed database with Sample data. This is available in the UI with RavenDB 4

Compile the project for Release, and run.


### Results using 913 base records

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18363
AMD Ryzen Threadripper 2950X, 1 CPU, 32 logical and 16 physical cores
.NET Core SDK=3.1.100
  [Host]     : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT
  DefaultJob : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT


|                         Method |     Mean |    Error |   StdDev | Ratio | RatioSD |
|------------------------------- |---------:|---------:|---------:|------:|--------:|
|                         Linear | 22.599 s | 0.4394 s | 0.5866 s |  1.00 |    0.00 |
|                   ForEachAsync |  2.499 s | 0.0491 s | 0.0947 s |  0.11 |    0.00 |
|           ParallelForEachAsync |  2.485 s | 0.0604 s | 0.0565 s |  0.11 |    0.00 |
|           AsyncParallelForEach |  2.157 s | 0.0423 s | 0.0578 s |  0.10 |    0.00 |
| AsyncEnumerableParallelForEach |  2.130 s | 0.0425 s | 0.0522 s |  0.09 |    0.00 |