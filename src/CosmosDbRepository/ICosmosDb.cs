using Microsoft.Azure.Documents.Client;
using System.Threading.Tasks;

namespace CosmosDbRepository
{
    public interface ICosmosDb
    {
        Task<string> SelfLinkAsync { get; }
        ICosmosDbRepository<T> Repository<T>();
        ICosmosDbRepository<T> Repository<T>(string id);
        Task<bool> DeleteAsync(RequestOptions options = null);

        // If you do not call init then the database and repositories
        // will be constructed as needed
        Task Init();
    }
}
