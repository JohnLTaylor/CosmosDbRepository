using System.Collections;
using System.Collections.Generic;

namespace CosmosDbRepository
{
    public class CosmosDbRepositoryResults<T>
        : IEnumerable<T>
    {
        public string ContinuationToken { get; set; }

        public List<T> Results { get; set; }

        public IEnumerator<T> GetEnumerator() => Results.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Results.GetEnumerator();
    }
}