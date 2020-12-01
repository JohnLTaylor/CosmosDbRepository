using CosmosDbRepository;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace CosmosDbRepositoryTest.SQL
{
    [TestClass]
    public class CosmosDbRepositoryStoredProcedureTests
        : CosmosDbRepositoryTests<ComplexTestData<Guid>>
    {
        private const string StoredProcedureBody =
@"function()
{
    var context = getContext();
    var response = context.getResponse();

    response.setBody(""Hello, World"");
}";

        [TestMethod]
        public async Task Create_FromBlankWithCreate_Success()
        {
            const string spId = "spHelloWorldFromBlankWithCreate";

            using (var context = CreateContext(repoBuilderCallback: builder => RepoBuilderCallback(builder, spId, "1.0")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);


                var result = await spHelloWorld.ExecuteAsync();
                result.Should().Be("Hello, World");
            }
        }

        [TestMethod]
        public async Task Updated_MajorDowngradeWithCreate_Failure()
        {
            const string spId = "spHelloWorldMajorDowngradeWithCreate";

            using (var context = CreateContext(repoBuilderCallback: builder => RepoBuilderCallback(builder, spId, "2.0")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                var result = await spHelloWorld.ExecuteAsync();
            }

            using (var context = CreateContext(repoBuilderCallback: builder => RepoBuilderCallback(builder, spId, "1.0")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                Func<Task<string>> action = async () => await spHelloWorld.ExecuteAsync();
                action.Should().ThrowExactly<InvalidOperationException>();
            }
        }

        [TestMethod]
        public async Task Updated_MajorUpgradeWithCreate_Success()
        {
            const string spId = "spHelloWorldMajorUpgradeWithCreate";

            using (var context = CreateContext(repoBuilderCallback: builder => RepoBuilderCallback(builder, spId, "1.0")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                var result = await spHelloWorld.ExecuteAsync();
            }

            using (var context = CreateContext(repoBuilderCallback: builder => RepoBuilderCallback(builder, spId, "2.0")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                var result = await spHelloWorld.ExecuteAsync();
                result.Should().Be("Hello, World");
            }
        }

        [TestMethod]
        public async Task Updated_MinorDowngradeWithCreate_Failure()
        {
            const string spId = "spHelloWorldMinorDowngradeWithCreate";

            using (var context = CreateContext(repoBuilderCallback: builder => RepoBuilderCallback(builder, spId, "1.1")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                var result = await spHelloWorld.ExecuteAsync();
            }

            using (var context = CreateContext(repoBuilderCallback: builder => RepoBuilderCallback(builder, spId, "1.0")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                Func<Task<string>> action = async () => await spHelloWorld.ExecuteAsync();
                action.Should().ThrowExactly<InvalidOperationException>();
            }
        }

        [TestMethod]
        public async Task Updated_MinorUpgradeWithCreate_Success()
        {
            const string spId = "spHelloWorldMinorUpgradeWithCreate";

            using (var context = CreateContext(repoBuilderCallback: builder => RepoBuilderCallback(builder, spId, "1.0")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                var result = await spHelloWorld.ExecuteAsync();
            }

            using (var context = CreateContext(repoBuilderCallback: builder => RepoBuilderCallback(builder, spId, "1.1")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                var result = await spHelloWorld.ExecuteAsync();
                result.Should().Be("Hello, World");
            }
        }

        [TestMethod]
        public async Task Updated_NoChangeWithCreate_Success()
        {
            const string spId = "spHelloWorldNoChangeWithCreate";

            using (var context = CreateContext(repoBuilderCallback: builder => RepoBuilderCallback(builder, spId, "1.0")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                var result = await spHelloWorld.ExecuteAsync();
            }

            using (var context = CreateContext(repoBuilderCallback: builder => RepoBuilderCallback(builder, spId, "1.0")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                var result = await spHelloWorld.ExecuteAsync();
                result.Should().Be("Hello, World");
            }
        }

        [TestMethod]
        public async Task Create_FromBlankWithoutCreate_Failure()
        {
            const string spId = "spHelloWorldFromBlankWithoutCreate";

            using (var context = CreateContext())
            {
                await context.Repo.AddAsync(new ComplexTestData<Guid> { });
            }

            using (var context = CreateContext(builder => builder.NoCreate(), builder => RepoBuilderCallback(builder, spId, "1.0")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);


                Func<Task<string>> action = async () => await spHelloWorld.ExecuteAsync();
                action.Should().ThrowExactly<InvalidOperationException>();
            }
        }

        [TestMethod]
        public async Task Updated_MajorDowngradeWithoutCreate_Failure()
        {
            const string spId = "spHelloWorldMajorDowngradeWithoutCreate";

            using (var context = CreateContext(repoBuilderCallback: builder => RepoBuilderCallback(builder, spId, "2.0")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                var result = await spHelloWorld.ExecuteAsync();
            }

            using (var context = CreateContext(builder => builder.NoCreate(), builder => RepoBuilderCallback(builder, spId, "1.0")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                Func<Task<string>> action = async () => await spHelloWorld.ExecuteAsync();
                action.Should().ThrowExactly<InvalidOperationException>();
            }
        }

        [TestMethod]
        public async Task Updated_MajorUpgradeWithoutCreate_Failure()
        {
            const string spId = "spHelloWorldMajorUpgradeWithoutCreate";

            using (var context = CreateContext(repoBuilderCallback: builder => RepoBuilderCallback(builder, spId, "1.0")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                var result = await spHelloWorld.ExecuteAsync();
            }

            using (var context = CreateContext(builder => builder.NoCreate(), builder => RepoBuilderCallback(builder, spId, "2.0")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                Func<Task<string>> action = async () => await spHelloWorld.ExecuteAsync();
                action.Should().ThrowExactly<InvalidOperationException>();
            }
        }

        [TestMethod]
        public async Task Updated_MinorDowngradeWithoutCreate_Failure()
        {
            const string spId = "spHelloWorldMinorDowngradeWithoutCreate";

            using (var context = CreateContext(repoBuilderCallback: builder => RepoBuilderCallback(builder, spId, "1.1")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                var result = await spHelloWorld.ExecuteAsync();
            }

            using (var context = CreateContext(builder => builder.NoCreate(), builder => RepoBuilderCallback(builder, spId, "1.0")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                Func<Task<string>> action = async () => await spHelloWorld.ExecuteAsync();
                action.Should().ThrowExactly<InvalidOperationException>();
            }
        }

        [TestMethod]
        public async Task Updated_MinorUpgradeWithoutCreate_Success()
        {
            const string spId = "spHelloWorldMinorUpgradeWithoutCreate";

            using (var context = CreateContext(repoBuilderCallback: builder => RepoBuilderCallback(builder, spId, "1.0")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                var result = await spHelloWorld.ExecuteAsync();
            }

            using (var context = CreateContext(builder => builder.NoCreate(), builder => RepoBuilderCallback(builder, spId, "1.1")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                Func<Task<string>> action = async () => await spHelloWorld.ExecuteAsync();
                action.Should().ThrowExactly<InvalidOperationException>();
            }
        }

        [TestMethod]
        public async Task Updated_NoChangeWithoutCreate_Success()
        {
            const string spId = "spHelloWorldNoChangeWithoutCreate";

            using (var context = CreateContext(repoBuilderCallback: builder => RepoBuilderCallback(builder, spId, "1.0")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                var result = await spHelloWorld.ExecuteAsync();
            }

            using (var context = CreateContext(builder => builder.NoCreate(), builder => RepoBuilderCallback(builder, spId, "1.0")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<string>(spId);

                var result = await spHelloWorld.ExecuteAsync();
                result.Should().Be("Hello, World");
            }
        }

        private void RepoBuilderCallback<T>(ICosmosDbRepositoryBuilder<T> builder, string spId, string version)
        {
            var body = $"// Version: {version}\n{StoredProcedureBody}";

            builder.StoredProcedure(spId, body);
        }
    }
}
