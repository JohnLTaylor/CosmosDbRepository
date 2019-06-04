using CosmosDbRepository;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace CosmosDbRepositoryTest
{
    public class TestingContext<T>
        : IDisposable
    {
        public readonly DocumentClient DbClient;
        public readonly ICosmosDb CosmosDb;
        public readonly CosmosDbConfig DbConfig;
        public readonly TestConfig TestConfig;
        public readonly EnvironmentConfig EnvConfig;
        public readonly ICosmosDbRepository<T> Repo;

        private bool _disposed;

        public TestingContext(Action<ICosmosDbBuilder> builderCallback, Action<ICosmosDbRepositoryBuilder> repoBuilderCallback)
        {
            var services = TestFramework.Services;
            DbConfig = services.GetRequiredService<IOptions<CosmosDbConfig>>().Value;
            TestConfig = services.GetRequiredService<IOptions<TestConfig>>().Value.Clone();
            EnvConfig = services.GetRequiredService<IOptions<EnvironmentConfig>>().Value;

            if (EnvConfig.RandomizeCollectionName)
            {
                TestConfig.CollectionName = $"{TestConfig.CollectionName}{Guid.NewGuid()}";
            }

            DbClient = new DocumentClient(new Uri(DbConfig.DbEndPoint), DbConfig.DbKey);
            var builder = new CosmosDbBuilder()
                .WithId(DbConfig.DbName)
                .AddCollection<T>(TestConfig.CollectionName, repoBuilderCallback);

            builderCallback?.Invoke(builder);

            CosmosDb = builder.Build(DbClient);

            Repo = CosmosDb.Repository<T>();
        }

        public void Dispose()
        {
            if (!_disposed && EnvConfig.DeleteCollectionsOnClose)
            {
                Repo.DeleteAsync();
                DbClient.Dispose();
                _disposed = true;
            }
        }
    }
}