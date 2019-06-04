using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace CosmosDbRepositoryTest.StringId
{
    [TestClass]
    public class CosmosDbRepositoryGetTests
        : CosmosDbRepositoryStringTests
    {
        [TestMethod]
        public async Task Get_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var data = new TestData<string>
                {
                    Id = GetNewId(),
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
                var data = new TestData<string>
                {
                    Id = GetNewId(),
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
                var data = new TestData<string>
                {
                    Id = GetNewId(),
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
                var data = new TestData<string>
                {
                    Id = GetNewId(),
                    Data = "Old Data"
                };

                var data2 = await context.Repo.GetAsync(data.Id);

                data2.Should().Be(default);
            }
        }
    }
}
