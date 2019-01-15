using System;

namespace CosmosDbRepository
{
    [AttributeUsage(validOn: AttributeTargets.Class)]
    public class CosmosDbRepositoryNameAttribute
        : Attribute
    {
        public string Name { get; }

        public CosmosDbRepositoryNameAttribute(string name)
        {
            Name = name;
        }
    }
}