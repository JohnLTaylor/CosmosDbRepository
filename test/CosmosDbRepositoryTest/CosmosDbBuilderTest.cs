using CosmosDbRepository;
using CosmosDbRepository.Types;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace CosmosDbRepositoryTest
{
    [TestClass]
    public class CosmosDbBuilderTest
    {
        [TestMethod]
        public void WithId_NullName_Success()
        {
            new CosmosDbBuilder().WithId("MyCollection");
        }

        [TestMethod]
        public void WithId_NullName_Expect_ArgumentException()
        {
            Func<ICosmosDbBuilder> action = () => new CosmosDbBuilder().WithId(null);
            action.Should().ThrowExactly<ArgumentException>();
        }

        [TestMethod]
        public void WithId_EmptyName_Expect_ArgumentException()
        {
            Func<ICosmosDbBuilder> action = () => new CosmosDbBuilder().WithId(string.Empty);
            action.Should().ThrowExactly<ArgumentException>();
        }

        [TestMethod]
        public void WithId_WhitespaceName_Expect_ArgumentException()
        {
            Func<ICosmosDbBuilder> action = () => new CosmosDbBuilder().WithId("   ");
            action.Should().ThrowExactly<ArgumentException>();
        }

        [TestMethod]
        public void WithId_DoubleSet_Expect_InvalidOperationException()
        {
            Func<ICosmosDbBuilder> action = () => new CosmosDbBuilder().WithId("1").WithId("2");
            action.Should().ThrowExactly<InvalidOperationException>();
        }

        [TestMethod]
        public void AddCollection_Success()
        {
            new CosmosDbBuilder().AddCollection<TestData<Guid>>();
        }

        [TestMethod]
        public void AddCollection_WithCallback_Success()
        {
            new CosmosDbBuilder().AddCollection<TestData<Guid>>("MyCollection", _ => { });
        }

        [TestMethod]
        public void AddCollection_EmptyName_Expect_ArgumentException()
        {
            Func<ICosmosDbBuilder> action = () => new CosmosDbBuilder().AddCollection<TestData<Guid>>(string.Empty);
            action.Should().ThrowExactly<ArgumentException>();
        }

        [TestMethod]
        public void AddCollection_WhitespaceName_Expect_ArgumentException()
        {
            Func<ICosmosDbBuilder> action = () => new CosmosDbBuilder().AddCollection<TestData<Guid>>("   ");
            action.Should().ThrowExactly<ArgumentException>();
        }

        [TestMethod]
        public void AddCollection_IncludeIndexPath_NullIndexPath_Expect_ArgumentException()
        {
            Func<ICosmosDbBuilder> action = () => new CosmosDbBuilder()
                .AddCollection<TestData<Guid>>(null, bld => bld.IncludeIndexPath(null, Index.Range(DataType.String)));
            action.Should().ThrowExactly<ArgumentException>();
        }

        [TestMethod]
        public void AddCollection_IncludeIndexPath_EmptyIndexPath_Expect_ArgumentException()
        {
            Func<ICosmosDbBuilder> action = () => new CosmosDbBuilder()
                .AddCollection<TestData<Guid>>(null, bld => bld.IncludeIndexPath(string.Empty, Index.Range(DataType.String)));
            action.Should().ThrowExactly<ArgumentException>();
        }

        [TestMethod]
        public void AddCollection_IncludeIndexPath_Success()
        {
            new CosmosDbBuilder()
                .AddCollection<TestData<Guid>>(null, bld => bld.IncludeIndexPath("/id", Index.Range(DataType.String)));
        }

        [TestMethod]
        public void AddCollection_ExcludeIndexPath_NullIndexPath_Expect_ArgumentNullException()
        {
            Func<ICosmosDbBuilder> action = () => new CosmosDbBuilder()
                .AddCollection<TestData<Guid>>(null, bld => bld.ExcludeIndexPath(null));
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        public void AddCollection_ExcludeIndexPath_EmptyIndexPath_Expect_ArgumentException()
        {
            Func<ICosmosDbBuilder> action = () => new CosmosDbBuilder()
                .AddCollection<TestData<Guid>>(null, bld => bld.ExcludeIndexPath(""));
            action.Should().ThrowExactly<ArgumentException>();
        }

        [TestMethod]
        public void AddCollection_ExcludeIndexPath_Success()
        {
            new CosmosDbBuilder()
                .AddCollection<TestData<Guid>>(null, bld => bld.ExcludeIndexPath("/data"));
        }

        [TestMethod]
        public void Build_Success()
        {
            var services = TestFramework.Services;
            var dbConfig = services.GetRequiredService<IOptions<CosmosDbConfig>>().Value;
            using (var dbClient = new DocumentClient(new Uri(dbConfig.DbEndPoint), dbConfig.DbKey))
            {
               new CosmosDbBuilder().WithId("MyDatabase").Build(dbClient);
            }
        }

        [TestMethod]
        public void Build_NoId_Expect_InvalidOperationException()
        {
            Func<ICosmosDb> action = () => new CosmosDbBuilder().Build(null);
            action.Should().ThrowExactly<InvalidOperationException>();
        }

        [TestMethod]
        public void Build_NullClient_Expect_InvalidOperationException()
        {
            Func<ICosmosDb> action = () => new CosmosDbBuilder().WithId("MyDatabase").Build(null);
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        public void Build_WithCollection_Success()
        {
            var services = TestFramework.Services;
            var dbConfig = services.GetRequiredService<IOptions<CosmosDbConfig>>().Value;
            using (var dbClient = new DocumentClient(new Uri(dbConfig.DbEndPoint), dbConfig.DbKey))
            {
                new CosmosDbBuilder()
                    .WithId("MyDatabase")
                    .WithDefaultThroughput(400)
                    .AddCollection<TestData<Guid>>()
                    .Build(dbClient);
            }
        }

        [TestMethod]
        public void Build_WithNamedCollection_Success()
        {
            var services = TestFramework.Services;
            var dbConfig = services.GetRequiredService<IOptions<CosmosDbConfig>>().Value;
            using (var dbClient = new DocumentClient(new Uri(dbConfig.DbEndPoint), dbConfig.DbKey))
            {
                new CosmosDbBuilder()
                    .WithId("MyDatabase")
                    .WithDefaultThroughput(400)
                    .AddCollection<TestClass>("MyTestClass")
                    .Build(dbClient);
            }
        }

        [TestMethod]
        public void Build_WithAttributeNamedCollection_Success()
        {
            var services = TestFramework.Services;
            var dbConfig = services.GetRequiredService<IOptions<CosmosDbConfig>>().Value;
            using (var dbClient = new DocumentClient(new Uri(dbConfig.DbEndPoint), dbConfig.DbKey))
            {
                new CosmosDbBuilder()
                    .WithId("MyDatabase")
                    .WithDefaultThroughput(400)
                    .AddCollection<TestClass>("TestClass")
                    .Build(dbClient);
            }
        }

        [TestMethod]
        public void Build_WithClassNamedCollection_Success()
        {
            var services = TestFramework.Services;
            var dbConfig = services.GetRequiredService<IOptions<CosmosDbConfig>>().Value;
            using (var dbClient = new DocumentClient(new Uri(dbConfig.DbEndPoint), dbConfig.DbKey))
            {
                new CosmosDbBuilder()
                    .WithId("MyDatabase")
                    .WithDefaultThroughput(400)
                    .AddCollection<TestClass2>()
                    .Build(dbClient);
            }
        }

        [TestMethod]
        public void Repository_WithNamedCollection_Success()
        {
            var services = TestFramework.Services;
            var dbConfig = services.GetRequiredService<IOptions<CosmosDbConfig>>().Value;
            using (var dbClient = new DocumentClient(new Uri(dbConfig.DbEndPoint), dbConfig.DbKey))
            {
                var db = new CosmosDbBuilder()
                    .WithId("MyDatabase")
                    .WithDefaultThroughput(400)
                    .AddCollection<TestClass>("MyTestClass")
                    .Build(dbClient);

                db.Repository<TestClass>("MyTestClass");
            }
        }

        [TestMethod]
        public void Repository_WithAttributeNamedCollection_Success()
        {
            var services = TestFramework.Services;
            var dbConfig = services.GetRequiredService<IOptions<CosmosDbConfig>>().Value;
            using (var dbClient = new DocumentClient(new Uri(dbConfig.DbEndPoint), dbConfig.DbKey))
            {
                var db = new CosmosDbBuilder()
                    .WithId("MyDatabase")
                    .WithDefaultThroughput(400)
                    .AddCollection<TestClass>()
                    .Build(dbClient);

                db.Repository<TestClass>();
            }
        }

        [TestMethod]
        public void Repository_WithClassNamedCollection_Success()
        {
            var services = TestFramework.Services;
            var dbConfig = services.GetRequiredService<IOptions<CosmosDbConfig>>().Value;
            using (var dbClient = new DocumentClient(new Uri(dbConfig.DbEndPoint), dbConfig.DbKey))
            {
                var db = new CosmosDbBuilder()
                    .WithId("MyDatabase")
                    .WithDefaultThroughput(400)
                    .AddCollection<TestClass2>()
                    .Build(dbClient);

                db.Repository<TestClass2>();
            }
        }

        [CosmosDbRepositoryName("TheTestClass")]
        private class TestClass
        {
        }

        private class TestClass2
        {
        }

        //[TestMethod]
        //public async Task WithId_NullName_ExpectException()
        //{
        //    var services = TestFramework.Services;
        //    DbConfig = services.GetRequiredService<IOptions<CosmosDbConfig>>().Value;
        //    TestConfig = services.GetRequiredService<IOptions<TestConfig>>().Value.Clone();
        //    EnvConfig = services.GetRequiredService<IOptions<EnvironmentConfig>>().Value;

        //    if (EnvConfig.RandomizeCollectionName)
        //    {
        //        TestConfig.CollectionName = $"{TestConfig.CollectionName}{Guid.NewGuid()}";
        //    }

        //    DbClient = new DocumentClient(new Uri(DbConfig.DbEndPoint), DbConfig.DbKey);
        //    var builder = new CosmosDbBuilder()
        //        .WithId(DbConfig.DbName)
        //        .AddCollection<T>(TestConfig.CollectionName, repoBuilderCallback);

        //    builderCallback?.Invoke(builder);

        //    CosmosDb = builder.Build(DbClient);

        //    Repo = CosmosDb.Repository<T>();
        //}
    }
}
