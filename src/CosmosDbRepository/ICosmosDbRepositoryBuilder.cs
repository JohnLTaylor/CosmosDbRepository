using Microsoft.Azure.Documents;

namespace CosmosDbRepository
{
    public interface ICosmosDbRepositoryBuilder
    {
        ICosmosDbRepositoryBuilder WithId(string name);
        ICosmosDbRepositoryBuilder IncludeIndexPath(string path, params Index[] indexes);
        ICosmosDbRepositoryBuilder ExcludeIndexPath(params string[] paths);
        ICosmosDbRepository Build(IDocumentClient client, ICosmosDb documentDb);
    }
}
