using Microsoft.Azure.Documents;
using System;

namespace DocDbRepo
{
    public interface IDocumentDbBuilder
    {
        IDocumentDbBuilder WithId(string name);
        IDocumentDbBuilder AddCollection<T>(string id = null, Action<IDbCollectionBuilder> func = null);
        IDocumentDb Build(IDocumentClient client);
    }
}
