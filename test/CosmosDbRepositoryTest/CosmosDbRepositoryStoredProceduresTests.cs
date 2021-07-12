using CosmosDbRepository;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
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
        public async Task TestReturingABool_Success()
        {
            const string spId = "spTestReturingABool";

            using (var context = CreateContext(repoBuilderCallback: builder => RepoBuilderCallback(builder, spId, "1.0",
@"function()
{
    var context = getContext();
    var response = context.getResponse();
    response.setBody(true);
}")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<bool>(spId);


                var result = await spHelloWorld.ExecuteAsync();
                result.Should().Be(true);
            }
        }

        [TestMethod]
        public async Task TestReturingAnArrayOfInt_Success()
        {
            const string spId = "spTestReturingAnArrayOfInt";

            using (var context = CreateContext(repoBuilderCallback: builder => RepoBuilderCallback(builder, spId, "1.0",
@"function()
{
    var context = getContext();
    var response = context.getResponse();
    response.setBody([ ...Array(10).keys() ]);
}")))
            {
                var spHelloWorld = context.Repo.StoredProcedure<int[]>(spId);


                var result = await spHelloWorld.ExecuteAsync();
                result.Should().HaveCount(10);
                result.Should().StartWith(0);
                result.Should().EndWith(9);
                result.Should().ContainInOrder(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
            }
        }

        [TestMethod]
        public async Task TestReturingAnArrayOfObjects_Success()
        {
            const string spId = "spTestReturingAnArrayOfObjects";

            using (var context = CreateContext(repoBuilderCallback: builder =>
                {
                    builder.EnablePolymorphism(r => r.Rank, new(int, Type)[]
                    {
                        (1, typeof(OneClass)),
                        (2, typeof(TwoClass)),
                        (3, typeof(ThreeClass)),
                        (4, typeof(FourClass))
                    });
                    RepoBuilderCallback(builder, spId, "1.0",
    @"function()
{
    var context = getContext();
    var response = context.getResponse();
    response.setBody([ { rank: 1, type: 'one'}, { rank: 2, type: 'two'}, { rank: 3, type: 'three'}, { rank: 4, type: 'four'} ]);
}");
                }))
            {
                var spHelloWorld = context.Repo.StoredProcedure<ComplexTestData<Guid>[]>(spId);

                var result = (await spHelloWorld.ExecuteAsync()).OfType<IBaseClass>().ToArray();
                result.Should().HaveCount(4);
                result[0].type.Should().Be("one");
                result[1].type.Should().Be("two");
                result[2].type.Should().Be("three");
                result[3].type.Should().Be("four");
            }
        }

        [TestMethod]
        public async Task TestReturingAnEmptyObject_Success()
        {
            const string spId = "TestReturingAnEmptyObject";

            using (var context = CreateContext(repoBuilderCallback: builder =>
            {
                RepoBuilderCallback(builder, spId, "1.0",
@"function()
{
    var context = getContext();
    var response = context.getResponse();
    response.setBody('');
}");
            }))
            {
                var spHelloWorld = context.Repo.StoredProcedure<ComplexTestData<Guid>>(spId);

                var result = await spHelloWorld.ExecuteAsync();
                result.Should().Be(default);
            }
        }

        [TestMethod]
        public async Task TestReturingANullObject_Success()
        {
            const string spId = "TestReturingANullObject";

            using (var context = CreateContext(repoBuilderCallback: builder =>
            {
                RepoBuilderCallback(builder, spId, "1.0",
@"function()
{
    var context = getContext();
    var response = context.getResponse();
    response.setBody(null);
}");
            }))
            {
                var spHelloWorld = context.Repo.StoredProcedure<ComplexTestData<Guid>>(spId);

                var result = await spHelloWorld.ExecuteAsync();
                result.Should().Be(default);
            }
        }

        [TestMethod]
        public async Task TestReturingAnArrayOfObjects_NoRecords()
        {
            const string spId = "spTestReturingAnArrayOfObjectsNoRecords";

            using (var context = CreateContext(repoBuilderCallback: builder =>
            {
                builder.EnablePolymorphism(r => r.Rank, new (int, Type)[]
                {
                        (1, typeof(OneClass)),
                        (2, typeof(TwoClass)),
                        (3, typeof(ThreeClass)),
                        (4, typeof(FourClass))
                });
                RepoBuilderCallback(builder, spId, "1.0",
@"function()
{
}");
            }))
            {
                var spHelloWorld = context.Repo.StoredProcedure<ComplexTestData<Guid>[]>(spId);

                var result = (await spHelloWorld.ExecuteAsync())?.OfType<IBaseClass>().ToArray();
                result.Should().BeNullOrEmpty();
            }
        }

        private interface IBaseClass
        {
            public string type { get; set; }
        }

        private class OneClass:
            ComplexTestData<Guid>,
            IBaseClass
        {
            public string type { get; set; }
        }

        private class TwoClass :
            ComplexTestData<Guid>,
            IBaseClass
        {
            public string type { get; set; }
        }

        private class ThreeClass :
            ComplexTestData<Guid>,
            IBaseClass
        {
            public string type { get; set; }
        }

        private class FourClass :
            ComplexTestData<Guid>,
            IBaseClass
        {
            public int id { get; set; }
            public string type { get; set; }
        }

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
        public async Task Updated_MinorDowngradeWithoutCreate_Success()
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

                var result = await spHelloWorld.ExecuteAsync();
                result.Should().Be("Hello, World");
            }
        }

        [TestMethod]
        public async Task Updated_MinorUpgradeWithoutCreate_Failure()
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

        private void RepoBuilderCallback<T>(ICosmosDbRepositoryBuilder<T> builder, string spId, string version, string spBody = default)
        {
            var body = $"// Version: {version}\n{spBody ?? StoredProcedureBody}";

            builder.StoredProcedure(spId, body);
        }
    }
}
