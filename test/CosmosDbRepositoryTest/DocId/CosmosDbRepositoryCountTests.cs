using CosmosDbRepository.Types;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace CosmosDbRepositoryTest.DocId
{
    [TestClass]
    public class CosmosDbRepositoryCountTests
        : CosmosDbRepositoryTests<TestData<DocumentId>>
    {
        [TestMethod]
        public async Task Count_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var uniqueData = Guid.NewGuid().ToString();

                var data = new TestData<DocumentId>
                {
                    Id = Guid.NewGuid(),
                    Data = uniqueData
                };

                data = await context.Repo.AddAsync(data);

                int count = await context.Repo.CountAsync(d => d.Data == uniqueData);

                count.Should().Be(1);
            }
        }

        [TestMethod]
        public async Task Count_Expect_Success_WithNoData()
        {
            using (var context = CreateContext())
            {
                var uniqueData = Guid.NewGuid().ToString();
                int count = await context.Repo.CountAsync(d => d.Data == uniqueData);
                count.Should().Be(0);
            }
        }

        [TestMethod]
        public async Task Count_MultipleRecords_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var uniqueData = Guid.NewGuid().ToString();

                var data = new TestData<DocumentId>
                {
                    Id = Guid.NewGuid(),
                    Data = uniqueData,
                    Rank = 1
                };

                data = await context.Repo.AddAsync(data);

                var data2 = new TestData<DocumentId>
                {
                    Id = Guid.NewGuid(),
                    Data = uniqueData,
                    Rank = 2
                };

                data2 = await context.Repo.AddAsync(data2);

                int count = await context.Repo.CountAsync(d => d.Data == uniqueData);

                count.Should().Be(2);
            }
        }

    }
}
