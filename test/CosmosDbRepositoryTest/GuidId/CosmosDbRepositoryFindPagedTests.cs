using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosDbRepositoryTest.GuidId
{
    [TestClass]
    public class CosmosDbRepositoryFindPagedTests
        : CosmosDbRepositoryTests<TestData<Guid>>
    {
        [TestMethod]
        public async Task FindPaged_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var uniqueData = Guid.NewGuid().ToString();

                var data = new TestData<Guid>
                {
                    Id = Guid.NewGuid(),
                    Data = uniqueData
                };

                data = await context.Repo.AddAsync(data);

                var dataList = await context.Repo.FindAsync(1, null, d => d.Data == uniqueData);

                dataList.Should().NotBeNull();
                dataList.Items.Should().NotBeNull();
                dataList.Items.Should().BeEquivalentTo(new[] { data });
                dataList.ContinuationToken.Should().BeNull();
            }
        }

        [TestMethod]
        public async Task FindPaged_Expect_Success_WithNoData()
        {
            using (var context = CreateContext())
            {
                var uniqueData = Guid.NewGuid().ToString();
                var dataList = await context.Repo.FindAsync(1, null, d => d.Data == uniqueData);
                dataList.Should().NotBeNull();
                dataList.Items.Should().NotBeNull();
                dataList.Items.Should().BeEmpty();
                dataList.ContinuationToken.Should().BeNull();
            }
        }

        [TestMethod]
        public async Task FindPaged_WithContinuation_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var uniqueData = Guid.NewGuid().ToString();

                var data = new TestData<Guid>
                {
                    Id = Guid.NewGuid(),
                    Data = uniqueData,
                    Rank = 1
                };

                data = await context.Repo.AddAsync(data);

                var data2 = new TestData<Guid>
                {
                    Id = Guid.NewGuid(),
                    Data = uniqueData,
                    Rank = 2
                };

                data2 = await context.Repo.AddAsync(data2);

                var dataList = await context.Repo.FindAsync(1, null, d => d.Data == uniqueData, q => q.OrderBy(d => d.Rank));

                dataList.Should().NotBeNull();
                dataList.Items.Should().NotBeNull();
                dataList.Items.Should().BeEquivalentTo(new[] { data });
                dataList.ContinuationToken.Should().NotBeNull();

                dataList = await context.Repo.FindAsync(1, dataList.ContinuationToken, d => d.Data == uniqueData, q => q.OrderBy(d => d.Rank));

                dataList.Should().NotBeNull();
                dataList.Items.Should().NotBeNull();
                dataList.Items.Should().BeEquivalentTo(new[] { data2 });
                dataList.ContinuationToken.Should().BeNull();
            }
        }
    }
}
