using System.Collections.Generic;

namespace CosmosDbRepository.Types
{
    public class CosmosDbRepositoryPagedResults<T>
    {
        public string ContinuationToken { get; set; }

        public List<T> Items { get; set; } = new List<T>();
    }
}