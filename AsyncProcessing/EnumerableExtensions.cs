using System.Collections.Generic;
using System.Linq;

namespace AsyncProcessingBenchmarks
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
        {
            return source.Select((item, index) => (item, index));
        }

        public static IAsyncEnumerable<(T item, int index)> WithIndex<T>(this IAsyncEnumerable<T> source)
        {
            return source.Select((item, index) => (item, index));
        }
    }
}
