using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CosmosDbRepositoryTest.StringId
{
    [TestClass]
    public class CosmosDbRepositoryAddTests
        : CosmosDbRepositoryTests<TestData<string>>
    {
        [TestMethod]
        public async Task Add_ExpectSuccess()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<string>
                {
                    Id = "MyId",
                    Data = "My Data"
                };

                await context.Repo.AddAsync(data);
            }
        }

        [TestMethod]
        public async Task Add_ExpectConflict()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<string>
                {
                    Id = "MyId",
                    Data = "My Data"
                };

                await context.Repo.AddAsync(data);
                var faultedTask = context.Repo.AddAsync(data);
                await faultedTask.ShollowException();

                faultedTask.IsFaulted.Should().BeTrue();
                faultedTask.Exception.InnerExceptions.Should().HaveCount(1);
                var dce = faultedTask.Exception.InnerExceptions.Single() as DocumentClientException;
                dce.StatusCode.Should().Be(HttpStatusCode.Conflict);
            }
        }
    }
}
