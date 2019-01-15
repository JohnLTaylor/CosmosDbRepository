using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DocDbRepo.Implementation
{
    internal class DbCollectionBuilder<T>
        : IDbCollectionBuilder
    {
        private List<IncludedPath> _includePaths = new List<IncludedPath>();
        private List<ExcludedPath> _excludePaths = new List<ExcludedPath>();
        private IndexingMode _indexingMode = IndexingMode.Consistent;

        public string Id { get; private set; }

        public IDbCollectionBuilder WithId(string id)
        {
            Id = id;
            return this;
        }

        public IDbCollectionBuilder IncludeIndexPath(string path, params Index[] indexes)
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

        public IDbCollectionBuilder ExcludeIndexPath(params string[] paths)
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

        public IDbCollection Build(IDocumentClient client, IDocumentDb documentDb)
        {
            if (string.IsNullOrWhiteSpace(Id)) throw new InvalidOperationException("Id not specified");

            var indexingPolicy = new IndexingPolicy
            {
                IncludedPaths = (_includePaths?.Any() ?? false) ? new Collection<IncludedPath>(_includePaths) : null,
                ExcludedPaths = (_excludePaths?.Any() ?? false) ? new Collection<ExcludedPath>(_excludePaths) : null,
                IndexingMode = _indexingMode
            };

            return new DbCollection<T>(client, documentDb, Id, indexingPolicy);
        }
    }
}
