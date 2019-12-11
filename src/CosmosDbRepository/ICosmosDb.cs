using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Threading.Tasks;

namespace CosmosDbRepository
{
    public interface ICosmosDb
    {
        Task<string> SelfLinkAsync { get; }
        Task<string> AltLinkAsync { get; }
        ICosmosDbRepository<T> Repository<T>();
        ICosmosDbRepository<T> Repository<T>(string id);
        Task<bool> DeleteAsync(RequestOptions options = null);
        Task<DocumentCollection[]> GetCollections(FeedOptions feedOptions = null);

        // If you do not call init then the database and repositories
        // will be constructed as needed
        Task Init();
    }
}
