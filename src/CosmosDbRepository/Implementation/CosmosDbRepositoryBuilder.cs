using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
        private string _polymorphicField;
        private (string, Type type)[] _polymorphicValueTypes;

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


        public ICosmosDbRepositoryBuilder<T> EnablePolymorphism<TMember>(Expression<Func<T, TMember>> typeSelectMember, params (TMember Value, Type Type)[] valueTypes)
        {
            if (typeSelectMember == default)
            {
                throw new ArgumentNullException(nameof(typeSelectMember));
            }

            if (valueTypes?.Any() != true)
            {
                throw new ArgumentNullException(nameof(valueTypes));
            }

            var info = FindProperty(typeSelectMember);

            _polymorphicField = info.GetCustomAttribute<JsonPropertyAttribute>(true)?.PropertyName;

            if (string.IsNullOrWhiteSpace(_polymorphicField))
            {
                throw new InvalidOperationException("Required JsonPropertyAttribute missing");
            }

            _polymorphicValueTypes = valueTypes.Select(vt => (vt.Value.ToString(), vt.Type)).ToArray();

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

            return new CosmosDbRepository<T>(client,
                                             documentDb,
                                             Id,
                                             indexingPolicy,
                                             _partitionkeySelector,
                                             partitionkeyDefinition,
                                             _throughput ?? defaultThroughput,
                                             _storedProcedure,
                                             _createOnMissing,
                                             _polymorphicField,
                                             _polymorphicValueTypes);
        }

        static MemberInfo FindProperty(LambdaExpression lambdaExpression)
        {
            Expression expressionToCheck = lambdaExpression;

            while (true)
            {
                switch (expressionToCheck.NodeType)
                {
                    case ExpressionType.Convert:
                        expressionToCheck = ((UnaryExpression)expressionToCheck).Operand;
                        break;

                    case ExpressionType.Lambda:
                        expressionToCheck = ((LambdaExpression)expressionToCheck).Body;
                        break;

                    case ExpressionType.MemberAccess:
                        var memberExpression = ((MemberExpression)expressionToCheck);

                        if (memberExpression.Expression.NodeType != ExpressionType.Parameter &&
                            memberExpression.Expression.NodeType != ExpressionType.Convert)
                        {
                            throw new ArgumentException(
                                $"Expression '{lambdaExpression}' must resolve to top-level member and not any child object's properties. You can use ForPath, a custom resolver on the child type or the AfterMap option instead.",
                                nameof(lambdaExpression));
                        }

                        var member = memberExpression.Member;

                        return member;

                    default:
                        throw new InvalidOperationException("Custom configuration for members is only supported for top-level individual members on a type.");
                }
            }
        }
    }
}
