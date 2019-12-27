# experiments


## Async Processing Benchmarks
Create a ravendb database on a sevrer with high latency from you, such as in the AWS when running the benchmark from your computer.

Add database information to appsettings.json or create an appsettings.local.json

Seed database with Sample data. This is available in the UI with RavenDB 4

Compile the project for Release, and run.

Blog post explaining what this is about: https://scatteredcode.net/parallel-foreach-async-in-c/

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

### Results using 9130 base records

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18363
AMD Ryzen Threadripper 2950X, 1 CPU, 32 logical and 16 physical cores
.NET Core SDK=3.1.100
  [Host]     : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT
  Job-WEHRKA : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT

InvocationCount=1  UnrollFactor=1

|                         Method |     Mean |   Error |  StdDev | Ratio | RatioSD |
|------------------------------- |---------:|--------:|--------:|------:|--------:|
|                         Linear | 244.07 s | 5.703 s | 8.879 s |  1.00 |    0.00 |
|                   ForEachAsync |  30.84 s | 1.899 s | 5.324 s |  0.11 |    0.01 |
|                   AsyncForEach | 272.10 s | 5.435 s | 8.620 s |  1.12 |    0.05 |
|           ParallelForEachAsync |  26.53 s | 0.339 s | 0.317 s |  0.11 |    0.00 |
|           AsyncParallelForEach |  25.79 s | 0.490 s | 0.564 s |  0.10 |    0.00 |
| AsyncEnumerableParallelForEach |  24.67 s | 0.436 s | 0.408 s |  0.10 |    0.00 |
