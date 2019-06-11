using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosDbRepositoryTest.GuidId
{
    [TestClass]
    public class CosmosDbRepositorySelectTests
        : CosmosDbRepositoryTests<TestData<Guid>>
    {
        [TestMethod]
        public async Task Select_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var uniqueData = Guid.NewGuid().ToString();

                var data1 = await GetTestData(context, uniqueData, 1);
                var data2a = await GetTestData(context, uniqueData, 2);
                var data2b = await GetTestData(context, uniqueData, 2);
                var data3a = await GetTestData(context, uniqueData, 3);
                var data3b = await GetTestData(context, uniqueData, 3);
                var data3c = await GetTestData(context, uniqueData, 3);

                var results = await context.Repo.SelectAsync(
                    d => d.Rank, 
                    whereClauses: q => q.Where(d => d.Data == uniqueData));

                results.Should().NotBeNull();
                results.Count().Should().Be(6);

                results = await context.Repo.SelectAsync(
                    d => d.Rank,
                    whereClauses: q => q.Where(d => d.Data == uniqueData),
                    selectClauses: q => q.Distinct());

                results.Should().NotBeNull();
                results.Count().Should().Be(3);

                results = await context.Repo.SelectAsync(
                    d => d.Rank,
                    whereClauses: q => q.Where(d => d.Data == uniqueData && d.Rank == 1));

                results.Should().NotBeNull();
                results.Count().Should().Be(1);

                results = await context.Repo.SelectAsync(
                    d => d.Rank,
                    whereClauses: q => q.Where(d => d.Data == uniqueData && d.Rank == 1),
                    selectClauses: q => q.Distinct());

                results.Should().NotBeNull();
                results.Count().Should().Be(1);

                results = await context.Repo.SelectAsync(
                    d => d.Rank,
                    whereClauses: q => q.Where(d => d.Data == uniqueData && d.Rank == 2));

                results.Should().NotBeNull();
                results.Count().Should().Be(2);

                results = await context.Repo.SelectAsync(
                    d => d.Rank,
                    whereClauses: q => q.Where(d => d.Data == uniqueData && d.Rank == 2),
                    selectClauses: q => q.Distinct());

                results.Should().NotBeNull();
                results.Count().Should().Be(1);

                results = await context.Repo.SelectAsync(
                    d => d.Rank,
                    whereClauses: q => q.Where(d => d.Data == uniqueData && d.Rank == 3));

                results.Should().NotBeNull();
                results.Count().Should().Be(3);

                results = await context.Repo.SelectAsync(
                    d => d.Rank,
                    whereClauses: q => q.Where(d => d.Data == uniqueData && d.Rank == 3),
                    selectClauses: q => q.Distinct());

                results.Should().NotBeNull();
                results.Count().Should().Be(1);
            }
        }
    }
}
