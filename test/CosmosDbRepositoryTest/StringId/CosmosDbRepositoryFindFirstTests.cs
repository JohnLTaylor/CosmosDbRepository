using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosDbRepositoryTest.StringId
{
    [TestClass]
    public class CosmosDbRepositoryFindFirstTests
        : CosmosDbRepositoryStringTests
    {
        [TestMethod]
        public async Task FindFirst_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var uniqueData = Guid.NewGuid().ToString();

                var data = new TestData<string>
                {
                    Id = GetNewId(),
                    Data = uniqueData
                };

                data = await context.Repo.AddAsync(data);

                var foundData = await context.Repo.FindFirstOrDefaultAsync(d => d.Data == uniqueData);

                foundData.Should().NotBeNull();
                foundData.Should().BeEquivalentTo(data);
            }
        }

        [TestMethod]
        public async Task FindFirst_Expect_Success_WithNoData()
        {
            using (var context = CreateContext())
            {
                var uniqueData = Guid.NewGuid().ToString();
                var foundData = await context.Repo.FindFirstOrDefaultAsync(d => d.Data == uniqueData);
                foundData.Should().BeNull();
            }
        }

        [TestMethod]
        public async Task FindFirst_MultipleRecords_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var uniqueData = Guid.NewGuid().ToString();

                var data = new TestData<string>
                {
                    Id = GetNewId(),
                    Data = uniqueData,
                    Rank = 1
                };

                data = await context.Repo.AddAsync(data);

                var data2 = new TestData<string>
                {
                    Id = GetNewId(),
                    Data = uniqueData,
                    Rank = 2
                };

                data2 = await context.Repo.AddAsync(data2);

                var foundData = await context.Repo.FindFirstOrDefaultAsync(d => d.Data == uniqueData, q => q.OrderByDescending(d => d.Rank));

                foundData.Should().NotBeNull();
                foundData.Should().BeEquivalentTo(data2);

                foundData = await context.Repo.FindFirstOrDefaultAsync(d => d.Data == uniqueData, q => q.OrderBy(d => d.Rank));

                foundData.Should().NotBeNull();
                foundData.Should().BeEquivalentTo(data);
            }
        }

    }
}
