using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CosmosDbRepository.Implementation
{
    internal class CosmosDbRepositoryBuilder<T>
        : ICosmosDbRepositoryBuilder<T>
    {
        private List<IncludedPath> _includePaths = new List<IncludedPath>();
        private List<ExcludedPath> _excludePaths = new List<ExcludedPath>();
        private List<StoredProcedure> _storedProcedure = new List<StoredProcedure>();
        private IndexingMode _indexingMode = IndexingMode.Consistent;
        private bool _createOnMissing = true;
        private int? _throughput;
        private Func<T, object> _partitionkeySelector;
        private List<string> _partitionKeyPaths = new List<string>();
        public string Id { get; private set; }

        public ICosmosDbRepositoryBuilder<T> NoCreate()
        {
            _createOnMissing = false;
            return this;
        }

        public ICosmosDbRepositoryBuilder<T> WithId(string id)
        {
            Id = id;
            return this;
        }

        public ICosmosDbRepositoryBuilder<T> WithThroughput(int? throughput)
        {
            _throughput = throughput;
            return this;
        }

        public ICosmosDbRepositoryBuilder<T> IncludeIndexPath(string path, params Index[] indexes)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Invalid Include Path", nameof(path));
            };

            _includePaths.Add(new IncludedPath
            {
                Path = path,
                Indexes = (indexes?.Any() ?? false)
                    ? new Collection<Index>(indexes)
                    : null
            });

            return this;
        }

        public ICosmosDbRepositoryBuilder<T> IncludePartitionkeySelector(Func<T, object> partitionkeySelector)
        {
            _partitionkeySelector = partitionkeySelector;
            return this;
        }

        public ICosmosDbRepositoryBuilder<T> IncludePartitionkeyPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Invalid Include Path", nameof(path));
            };

            _partitionKeyPaths.Add(path);

            return this;
        }

        public ICosmosDbRepositoryBuilder<T> ExcludeIndexPath(params string[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }

            if (paths.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Invalid Exclude Path", nameof(paths));
            }

            if (paths.Any())
            {
                _excludePaths.AddRange(paths.Select(path => new ExcludedPath { Path = path }));
            };

            return this;
        }

        public ICosmosDbRepositoryBuilder<T> StoredProcedure(string id, string body)
        {
            _storedProcedure.Add(new StoredProcedure { Id = id, Body = body });
            return this;
        }

        public ICosmosDbRepository Build(IDocumentClient client, ICosmosDb documentDb, int? defaultThroughput)
        {
            if (string.IsNullOrWhiteSpace(Id)) throw new InvalidOperationException("Id not specified");

            var indexingPolicy = new IndexingPolicy
            {
                IndexingMode = _indexingMode
            };


            PartitionKeyDefinition partitionkeyDefinition = null;

            if (_partitionKeyPaths.Any())
            {
                partitionkeyDefinition = new PartitionKeyDefinition
                {
                    Paths = new Collection<string>(_partitionKeyPaths)
                };
            }           

            if (_includePaths.Any())
            {
                indexingPolicy.IncludedPaths = new Collection<IncludedPath>(_includePaths);
            }

            if (_excludePaths.Any())
            {
                indexingPolicy.ExcludedPaths = new Collection<ExcludedPath>(_excludePaths);
            }

            return new CosmosDbRepository<T>(client, documentDb, Id, indexingPolicy, _partitionkeySelector, partitionkeyDefinition, _throughput ?? defaultThroughput, _storedProcedure, _createOnMissing);
        }
    }
}
