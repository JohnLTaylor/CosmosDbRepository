using Microsoft.Azure.Documents;
using System;

namespace CosmosDbRepository
{
    public interface ICosmosDbBuilder
    {
        ICosmosDbBuilder WithId(string name);
        ICosmosDbBuilder AddCollection<T>(string id = null, Action<ICosmosDbRepositoryBuilder> func = null);
        ICosmosDb Build(IDocumentClient client);
    }
}
