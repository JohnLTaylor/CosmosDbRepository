using CosmosDbRepository;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace CosmosDbRepositorySubstituteTest
{
    [TestClass]
    public static class TestFramework
    {
        public static IServiceProvider Initialize()
        {
            var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testsettings.json"))
            .AddEnvironmentVariables()
            .Build();

            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.Configure<CosmosDbConfig>(configuration.GetSection("CosmosDbConfig"));
            serviceCollection.Configure<TestConfig>(configuration.GetSection("TestConfig"));
            serviceCollection.Configure<EnvironmentConfig>(configuration.GetSection("EnvironmentConfig"));
            var services = serviceCollection.BuildServiceProvider();

            var envConfig = services.GetRequiredService<IOptions<EnvironmentConfig>>().Value;

            if (envConfig.RandomizeDbName)
            {
                var dbConfig = services.GetRequiredService<IOptions<CosmosDbConfig>>().Value ;
                dbConfig.DbName = $"{dbConfig.DbName}!{Guid.NewGuid()}";
            }

            return services;
        }

        public static void Cleanup(IServiceProvider services)
        {
            if (services != null)
            {
                var envConfig = services.GetRequiredService<IOptions<EnvironmentConfig>>().Value;

                if (envConfig.DeleteDatabaseOnClose)
                {
                    var dbConfig = services.GetRequiredService<IOptions<CosmosDbConfig>>().Value;

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
