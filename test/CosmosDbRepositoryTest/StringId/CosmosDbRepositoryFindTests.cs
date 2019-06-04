using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosDbRepositoryTest.StringId
{
    [TestClass]
    public class CosmosDbRepositoryFindTests
        : CosmosDbRepositoryStringTests
    {
        [TestMethod]
        public async Task Find_Expect_Success()
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

                var dataList = await context.Repo.FindAsync(d => d.Data == uniqueData);

                dataList.Should().NotBeNull();
                dataList.Should().BeEquivalentTo(new[] { data });
            }
        }

        [TestMethod]
        public async Task Find_Expect_Success_WithNoData()
        {
            using (var context = CreateContext())
            {
                var uniqueData = Guid.NewGuid().ToString();
                var dataList = await context.Repo.FindAsync(d => d.Data == uniqueData);
                dataList.Should().BeEmpty();
            }
        }

        [TestMethod]
        public async Task Find_Expect_Success_OrderedAscending()
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

                var dataList = await context.Repo.FindAsync(d => d.Data == uniqueData, q => q.OrderBy(d => d.Rank));

                dataList.Should().NotBeNull();
                dataList.Should().BeEquivalentTo(new[] { data, data2 });
                dataList.Should().BeInAscendingOrder(d => d.Rank);
            }
        }

        [TestMethod]
        public async Task Find_Expect_Success_OrderedDescending()
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

                var dataList = await context.Repo.FindAsync(d => d.Data == uniqueData, q => q.OrderByDescending(d => d.Rank));

                dataList.Should().NotBeNull();
                dataList.Should().BeEquivalentTo(new[] { data, data2 });
                dataList.Should().BeInDescendingOrder(d => d.Rank);
            }
        }

        [TestMethod]
        public async Task Find_WithSkipTake_Expect_Success()
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

                var dataList = await context.Repo.FindAsync(d => d.Data == uniqueData, q => q.OrderBy(d => d.Rank).Skip(0).Take(1));

                dataList.Should().NotBeNull();
                dataList.Should().BeEquivalentTo(new[] { data });

                dataList = await context.Repo.FindAsync(d => d.Data == uniqueData, q => q.OrderBy(d => d.Rank).Skip(1).Take(1));

                dataList.Should().NotBeNull();
                dataList.Should().BeEquivalentTo(new[] { data2 });
            }
        }
    }
}
