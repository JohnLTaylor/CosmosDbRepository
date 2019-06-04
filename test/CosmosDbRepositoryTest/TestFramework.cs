using CosmosDbRepository;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace CosmosDbRepositoryTest
{
    [TestClass]
    public static class TestFramework
    {
        public static ServiceProvider Services;

        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testsettings.json"))
            .AddEnvironmentVariables()
            .Build();

            ServiceCollection services = new ServiceCollection();
            services.Configure<CosmosDbConfig>(configuration.GetSection("CosmosDbConfig"));
            services.Configure<TestConfig>(configuration.GetSection("TestConfig"));
            services.Configure<EnvironmentConfig>(configuration.GetSection("EnvironmentConfig"));
            Services = services.BuildServiceProvider();

            var envConfig = Services.GetRequiredService<IOptions<EnvironmentConfig>>().Value;

            if (envConfig.RandomizeDbName)
            {
                var dbConfig = Services.GetRequiredService<IOptions<CosmosDbConfig>>().Value ;
                dbConfig.DbName = $"{dbConfig.DbName}!{Guid.NewGuid()}";
            }
        }


        [AssemblyCleanup]
        public static void Cleanup()
        {
            if (Services != null)
            {
                var envConfig = Services.GetRequiredService<IOptions<EnvironmentConfig>>().Value;

                if (envConfig.DeleteDatabaseOnClose)
                {
                    var dbConfig = Services.GetRequiredService<IOptions<CosmosDbConfig>>().Value;

                    var client = new DocumentClient(new Uri(dbConfig.DbEndPoint), dbConfig.DbKey);
                    var repo = new CosmosDbBuilder()
                        .WithId(dbConfig.DbName)
                        .WithDefaultThroughput(400)
                        .Build(client);

                    repo.DeleteAsync().Wait();
                }
            }
        }
    }
}
