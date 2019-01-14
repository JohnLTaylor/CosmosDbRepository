using Microsoft.Azure.Documents;

namespace DocumentDBRepo
{
    public interface IDbCollectionBuilder
    {
        IDbCollectionBuilder WithId(string name);
        IDbCollectionBuilder IncludeIndexPath(string path, params Index[] indexes);
        IDbCollectionBuilder ExcludeIndexPath(params string[] paths);
        IDbCollection Build(IDocumentClient client, IDocumentDb documentDb);
    }
}
