using CosmosDbRepository.Types;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace CosmosDbRepositoryTest.DocId
{
    [TestClass]
    public class CosmosDbRepositoryGetTests
        : CosmosDbRepositoryTests<TestData<DocumentId>>
    {
        [TestMethod]
        public async Task Get_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<DocumentId>
                {
                    Id = Guid.NewGuid(),
                    Data = "Old Data"
                };

                data = await context.Repo.AddAsync(data);

                var data2 = await context.Repo.GetAsync(data);

                data2.Should().BeEquivalentTo(data);
            }
        }

        [TestMethod]
        public async Task Get_ById_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<DocumentId>
                {
                    Id = Guid.NewGuid(),
                    Data = "Old Data"
                };

                data = await context.Repo.AddAsync(data);

                var data2 = await context.Repo.GetAsync(data.Id);

                data2.Should().BeEquivalentTo(data);
            }
        }

        [TestMethod]
        public async Task Get_NotFound_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<DocumentId>
                {
                    Id = Guid.NewGuid(),
                    Data = "Old Data"
                };

                var data2 = await context.Repo.GetAsync(data);

                data2.Should().Be(default);
            }
        }

        [TestMethod]
        public async Task Get_ById_NotFound_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<DocumentId>
                {
                    Id = Guid.NewGuid(),
                    Data = "Old Data"
                };

                var data2 = await context.Repo.GetAsync(data.Id);

                data2.Should().Be(default);
            }
        }
    }
}
