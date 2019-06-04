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
    public class CosmosDbRepositoryReplaceTests
        : CosmosDbRepositoryTests<TestData<Guid>>
    {
        [TestMethod]
        public async Task Replace_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<Guid>
                {
                    Id = Guid.NewGuid(),
                    Data = "Old Data"
                };

                data = await context.Repo.AddAsync(data);

                data.Data = "New Data";

                await context.Repo.ReplaceAsync(data);
            }
        }

        [TestMethod]
        public async Task Replace_Expect_PreconditionFailed()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<Guid>
                {
                    Id = Guid.NewGuid(),
                    Data = "Old Data"
                };

                data = await context.Repo.AddAsync(data);
                await context.Repo.ReplaceAsync(data);
                var faultedTask = context.Repo.ReplaceAsync(data);
                await faultedTask.ShollowException();

                faultedTask.IsFaulted.Should().BeTrue();
                faultedTask.Exception.InnerExceptions.Should().HaveCount(1);
                var dce = faultedTask.Exception.InnerExceptions.Single() as DocumentClientException;
                dce.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
            }
        }
    }
}
