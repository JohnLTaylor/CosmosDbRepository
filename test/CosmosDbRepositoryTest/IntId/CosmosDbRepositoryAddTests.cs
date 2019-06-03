using CosmosDbRepository.Types;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CosmosDbRepositoryTest.IntId
{
    [TestClass]
    public class CosmosDbRepositoryAddTests
        : CosmosDbRepositoryTests<TestData<DocumentId>>
    {
        [TestMethod]
        public async Task Add_ExpectSuccess_AsInt()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<DocumentId>
                {
                    Id = 1,
                    Data = "My Data"
                };

                await context.Repo.AddAsync(data);
            }
        }

        [TestMethod]
        public async Task Add_ExpectConflict_AsInt()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<DocumentId>
                {
                    Id = 1,
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

        [TestMethod]
        public async Task Add_ExpectSuccess_AsGuid()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<DocumentId>
                {
                    Id = Guid.NewGuid(),
                    Data = "My Data"
                };

                await context.Repo.AddAsync(data);
            }
        }

        [TestMethod]
        public async Task Add_ExpectConflict_AsGuid()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<DocumentId>
                {
                    Id = Guid.NewGuid(),
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

        [TestMethod]
        public async Task Add_ExpectSuccess_AsString()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<DocumentId>
                {
                    Id = "MyId",
                    Data = "My Data"
                };

                await context.Repo.AddAsync(data);
            }
        }

        [TestMethod]
        public async Task Add_ExpectConflict_AsString()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<DocumentId>
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
