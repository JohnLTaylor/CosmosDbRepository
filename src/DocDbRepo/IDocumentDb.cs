using System.Threading.Tasks;

namespace DocDbRepo
{
    public interface IDocumentDb
    {
        Task<string> SelfLinkAsync { get; }
        IDbCollection<T> Repository<T>(string id = null);
    }
}
