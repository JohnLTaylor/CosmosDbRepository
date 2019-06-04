using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CosmosDbRepositoryTest.GuidId
{
    [TestClass]
    public class CosmosDbRepositoryUpsertTests
        : CosmosDbRepositoryTests<TestData<Guid>>
    {
        [TestMethod]
        public async Task Upsert_New_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<Guid>
                {
                    Id = Guid.NewGuid(),
                    Data = "My Data"
                };

                await context.Repo.UpsertAsync(data);
            }
        }

        [TestMethod]
        public async Task Upsert_Added_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<Guid>
                {
                    Id = Guid.NewGuid(),
                    Data = "My Data"
                };

                data = await context.Repo.AddAsync(data);
                await context.Repo.UpsertAsync(data);
            }
        }

        [TestMethod]
        public async Task Upsert_Expect_PreconditionFailed()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<Guid>
                {
                    Id = Guid.NewGuid(),
                    Data = "My Data"
                };

                data = await context.Repo.AddAsync(data);
                await context.Repo.UpsertAsync(data);
                var faultedTask = context.Repo.UpsertAsync(data);
                await faultedTask.ShollowException();

                faultedTask.IsFaulted.Should().BeTrue();
                faultedTask.Exception.InnerExceptions.Should().HaveCount(1);
                var dce = faultedTask.Exception.InnerExceptions.Single() as DocumentClientException;
                dce.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
            }
        }
    }
}
