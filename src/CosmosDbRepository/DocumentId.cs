using System;

namespace CosmosDbRepository
{
    public struct DocumentId
    {
        public string Id { get; }

        private DocumentId(string id)
        {
            Id = id;
        }

        public static implicit operator DocumentId(string id)
        {
            return new DocumentId(id);
        }

        public static implicit operator DocumentId(Guid id)
        {
            return new DocumentId(id.ToString());
        }

        public static implicit operator DocumentId(int id)
        {
            return new DocumentId(id.ToString());
        }
    }
}