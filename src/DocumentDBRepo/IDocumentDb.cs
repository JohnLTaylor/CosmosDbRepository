using System.Threading.Tasks;

namespace DocumentDBRepo
{
    public interface IDocumentDb
    {
        Task<string> SelfLinkAsync { get; }
        IDbCollection<T> Repository<T>(string id = null);
    }
}
