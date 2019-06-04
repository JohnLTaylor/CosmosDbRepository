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
    public class CosmosDbRepositoryDeleteTests
        : CosmosDbRepositoryTests<TestData<Guid>>
    {
        [TestMethod]
        public async Task Delete_Expect_True()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<Guid>
                {
                    Id = Guid.NewGuid(),
                    Data = "MyData"
                };

                data = await context.Repo.AddAsync(data);

                bool deleted = await context.Repo.DeleteDocumentAsync(data);
                deleted.Should().BeTrue();
            }
        }

        [TestMethod]
        public async Task Delete_ById_Expect_True()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<Guid>
                {
                    Id = Guid.NewGuid(),
                    Data = "MyData"
                };

                data = await context.Repo.AddAsync(data);

                bool deleted = await context.Repo.DeleteDocumentAsync(data.Id);
                deleted.Should().BeTrue();
            }
        }

        [TestMethod]
        public async Task Delete_Expect_PreconditionFailed()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<Guid>
                {
                    Id = Guid.NewGuid(),
                    Data = "MyData"
                };

                data = await context.Repo.AddAsync(data);
                await context.Repo.ReplaceAsync(data);

                var faultedTask = context.Repo.DeleteDocumentAsync(data);
                await faultedTask.ShollowException();

                faultedTask.IsFaulted.Should().BeTrue();
                faultedTask.Exception.InnerExceptions.Should().HaveCount(1);
                var dce = faultedTask.Exception.InnerExceptions.Single() as DocumentClientException;
                dce.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
            }
        }

        [TestMethod]
        public async Task Delete_Expect_NotFound()
        {
            using (var context = CreateContext())
            {
                var faultedTask = context.Repo.DeleteDocumentAsync(Guid.NewGuid());
                await faultedTask.ShollowException();

                faultedTask.IsFaulted.Should().BeTrue();
                faultedTask.Exception.InnerExceptions.Should().HaveCount(1);
                var dce = faultedTask.Exception.InnerExceptions.Single() as DocumentClientException;
                dce.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
