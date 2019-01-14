using System;

namespace DocDbRepo
{
    [AttributeUsage(validOn: AttributeTargets.Class)]
    public class DocumentCollectionNameAttribute
        : Attribute
    {
        public string Name { get; }

        public DocumentCollectionNameAttribute(string name)
        {
            Name = name;
        }
    }
}