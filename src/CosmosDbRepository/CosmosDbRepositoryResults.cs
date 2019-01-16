using System.Collections;
using System.Collections.Generic;

namespace CosmosDbRepository
{
    public class CosmosDbRepositoryResults<T>
        : IEnumerable<T>
    {
        public string ContinuationToken { get; set; }

        public List<T> Items { get; set; } = new List<T>();

        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
    }
}