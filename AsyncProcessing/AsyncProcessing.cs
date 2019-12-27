using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using Raven.Client.Documents;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Operations;
using System.Collections.Generic;
using Raven.Client.Documents.Session;
using System.Threading;

namespace AsyncProcessingBenchmarks
{
    [RPlotExporter]
    public class AsyncProcessing
    {
        private DocumentStore _documentStore;

        [GlobalSetup]
        public async Task Setup()
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", false, true)
              .AddJsonFile("appsettings.local.json", true, true)
              .Build();

            _documentStore = new DocumentStore
            {
                Urls = new[] { config["raven:server"] },
                Database = config["raven:database"],
                Certificate = new X509Certificate2(config["raven:certificate"], config["raven:certificatePassword"])
            };
            _documentStore.Initialize();

            await SeedDatabase();
        }

        public async Task SeedDatabase()
        {
            // Let's create a healthy data set, about a 9k records should be good
            // We'll create the database with samples in the UI, and here we'll duplicate the Orders
            var operation = await _documentStore.Operations.SendAsync(new PatchByQueryOperation(new IndexQuery
            {
                Query = @"from Orders 
                        update { 
                            for (var i = 0; i < 10; i++) {
                                this.DocumentId = 'orders/';
                                this.DocumentCollection = this[""@metadata""][""@collection""];
                                put('orders/', this);
                            }
                        }"
            }));
            await operation.WaitForCompletionAsync();
        }

        [IterationCleanup]
        public async void Cleanup()
        {
            var operation = await _documentStore.Operations.SendAsync(new DeleteByQueryOperation(new IndexQuery
            {
                Query = "from ProcessedOrders o"
            }));
            await operation.WaitForCompletionAsync();
        }

        [Benchmark(Baseline = true)]
        public async Task Linear()
        {
            using (var session = _documentStore.OpenAsyncSession())
            {
                session.Advanced.MaxNumberOfRequestsPerSession = int.MaxValue;
                var skip = 0;
                do
                {
                    var entries = await session.Query<Order>().OrderByDescending(x => x.Id).Skip(skip).Take(100).ToListAsync();
                    foreach (var entry in entries)
                    {
                        Console.WriteLine($"Processing entry 1 '{entry.Id}'");
                        
                        // This is the most expensive way I can think of doing this, obviously don't do this if you want performance
                        using (var tempSession = _documentStore.OpenAsyncSession())
                        {
                            await tempSession.StoreAsync(new ProcessedOrder { OrderId = entry.Id, Id = "ProcessedOrders/" });
                            await tempSession.SaveChangesAsync();
                        }
                    }
                    skip += 100;
                    if (entries.Count < 100)
                        break;
                } while (true);
            }
        }

        [Benchmark]
        public async Task ForEachAsync()
        {
            using (var session = _documentStore.OpenSession())
            {
                session.Advanced.MaxNumberOfRequestsPerSession = int.MaxValue;

                await GetDocumentsFromDatabase(session).ForEachAsync(dop: Environment.ProcessorCount, body: async entry =>
                {
                    Console.WriteLine($"Processing entry 2 '{entry.Id}'");

                    // This is the most expensive way I can think of doing this, obviously don't do this if you want performance
                    using (var tempSession = _documentStore.OpenAsyncSession())
                    {
                        await tempSession.StoreAsync(new ProcessedOrder { OrderId = entry.Id });
                        await tempSession.SaveChangesAsync();
                    }
                });
            }
        }

        [Benchmark]
        public async Task AsyncForEach()
        {
            using (var session = _documentStore.OpenAsyncSession())
            {
                session.Advanced.MaxNumberOfRequestsPerSession = int.MaxValue;

                await foreach (var entry in GetDocumentsFromDatabase2(session))
                {
                    Console.WriteLine($"Processing entry 3 '{entry.Id}'");

                    // This is the most expensive way I can think of doing this, obviously don't do this if you want performance
                    using (var tempSession = _documentStore.OpenAsyncSession())
                    {
                        await tempSession.StoreAsync(new ProcessedOrder { OrderId = entry.Id });
                        await tempSession.SaveChangesAsync();
                    }
                }
            }
        }

        [Benchmark]
        public async Task ParallelForEachAsync()
        {
            using (var session = _documentStore.OpenSession())
            {
                session.Advanced.MaxNumberOfRequestsPerSession = int.MaxValue;

                await GetDocumentsFromDatabase(session).ParallelForEachAsync(dop: Environment.ProcessorCount, body: async entry =>
                {
                    Console.WriteLine($"Processing entry 4 '{entry.Id}'");

                    // This is the most expensive way I can think of doing this, obviously don't do this if you want performance
                    using (var tempSession = _documentStore.OpenAsyncSession())
                    {
                        await tempSession.StoreAsync(new ProcessedOrder { OrderId = entry.Id });
                        await tempSession.SaveChangesAsync();
                    }
                });
            }
        }

        [Benchmark]
        public async Task AsyncParallelForEach()
        {
            using (var session = _documentStore.OpenSession())
            {
                session.Advanced.MaxNumberOfRequestsPerSession = int.MaxValue;

                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

                await GetDocumentsFromDatabase(session).AsyncParallelForEach(async entry =>
                {
                    Console.WriteLine($"Processing entry 5 '{entry.Id}'");

                    // This is the most expensive way I can think of doing this, obviously don't do this if you want performance
                    using (var tempSession = _documentStore.OpenAsyncSession())
                    {
                        await tempSession.StoreAsync(new ProcessedOrder { OrderId = entry.Id });
                        await tempSession.SaveChangesAsync();
                    }
                }, Environment.ProcessorCount, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        [Benchmark]
        public async Task AsyncEnumerableParallelForEach()
        {
            using (var session = _documentStore.OpenAsyncSession())
            {
                session.Advanced.MaxNumberOfRequestsPerSession = int.MaxValue;

                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

                await GetDocumentsFromDatabase2(session).AsyncParallelForEach(async entry => {
                    Console.WriteLine($"Processing entry 6'{entry.Id}'");

                    // This is the most expensive way I can think of doing this, obviously don't do this if you want performance
                    using (var tempSession = _documentStore.OpenAsyncSession())
                    {
                        await tempSession.StoreAsync(new ProcessedOrder { OrderId = entry.Id });
                        await tempSession.SaveChangesAsync();
                    }
                }, Environment.ProcessorCount, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        public static IEnumerable<Order> GetDocumentsFromDatabase(IDocumentSession session)
        {
            var skip = 0;
            do
            {
                var entries = session.Query<Order>().OrderByDescending(x => x.Id).Skip(skip).Take(10).ToList();
                foreach (var entry in entries)
                    yield return entry;
                skip += 10;
                if (entries.Count < 10)
                    break;
            } while (true);
        }

        public static async IAsyncEnumerable<Order> GetDocumentsFromDatabase2(IAsyncDocumentSession session)
        {
            var skip = 0;
            do
            {
                var entries = await session.Query<Order>().OrderByDescending(x => x.Id).Skip(skip).Take(10).ToListAsync();
                foreach (var entry in entries)
                    yield return entry;
                skip += 10;
                if (entries.Count < 10)
                    break;
            } while (true);
        }
    }
}
