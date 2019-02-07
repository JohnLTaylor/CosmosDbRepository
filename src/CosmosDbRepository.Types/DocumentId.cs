using System;
using System.Collections.Generic;

namespace CosmosDbRepository.Types
{
    public struct DocumentId : IEquatable<DocumentId>
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

        public static bool operator ==(DocumentId left, DocumentId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DocumentId left, DocumentId right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return obj is DocumentId id && Equals(id);
        }

        public bool Equals(DocumentId other)
        {
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return 2108858624 + EqualityComparer<string>.Default.GetHashCode(Id);
        }
    }
}