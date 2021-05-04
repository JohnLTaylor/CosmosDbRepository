using FluentAssertions;
using Microsoft.Azure.Documents;
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
                    q => q.Where(d => d.Data == uniqueData),
                    q => q.Distinct());

                results.Should().NotBeNull();
                results.Count().Should().Be(3);

                results = await context.Repo.SelectAsync(
                    d => d.Rank,
                    whereClauses: q => q.Where(d => d.Data == uniqueData && d.Rank == 1));

                results.Should().NotBeNull();
                results.Count().Should().Be(1);

                results = await context.Repo.SelectAsync(
                    d => d.Rank,
                    q => q.Where(d => d.Data == uniqueData && d.Rank == 1),
                    q => q.Distinct());

                results.Should().NotBeNull();
                results.Count().Should().Be(1);

                results = await context.Repo.SelectAsync(
                    d => d.Rank,
                    whereClauses: q => q.Where(d => d.Data == uniqueData && d.Rank == 2));

                results.Should().NotBeNull();
                results.Count().Should().Be(2);

                results = await context.Repo.SelectAsync(
                    d => d.Rank,
                    q => q.Where(d => d.Data == uniqueData && d.Rank == 2),
                    q => q.Distinct());

                results.Should().NotBeNull();
                results.Count().Should().Be(1);

                results = await context.Repo.SelectAsync(
                    d => d.Rank,
                    whereClauses: q => q.Where(d => d.Data == uniqueData && d.Rank == 3));

                results.Should().NotBeNull();
                results.Count().Should().Be(3);

                results = await context.Repo.SelectAsync(
                    d => d.Rank,
                    q => q.Where(d => d.Data == uniqueData && d.Rank == 3),
                    q => q.Distinct());

                results.Should().NotBeNull();
                results.Count().Should().Be(1);
            }
        }

        [TestMethod]
        public async Task Select_DistinctCount_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var uniqueData = Guid.NewGuid().ToString();

                var data1 = await GetTestData(context, uniqueData, 1, CreateSubData);
                var data2a = await GetTestData(context, uniqueData, 2, CreateSubData);
                var data2b = await GetTestData(context, uniqueData, 2, CreateSubData);
                var data3a = await GetTestData(context, uniqueData, 3, CreateSubData);
                var data3b = await GetTestData(context, uniqueData, 3, CreateSubData);
                var data3c = await GetTestData(context, uniqueData, 3, CreateSubData);

                var results = await context.Repo.SelectAsync(
                    d => d.Rank,
                    whereClauses: q => q.Where(d => d.Data == uniqueData));

                results.Should().NotBeNull();
                results.Count().Should().Be(6);

                var groupResults = await context.Repo.SelectManyAsync(
                    d => d.Subdata.SelectMany(e => e.SubSubData).Select(f => new { d.Id, d.Data, d.Rank, fId = f.Id, f.Value }),
                    q => q.Where(d => d.Data == uniqueData));


                groupResults.Should().NotBeNull();

                var count = new[] { data1, data2a, data2b, data3a, data3b, data3c }.SelectMany(a => a.Subdata.SelectMany(b => b.SubSubData)).Count();

                groupResults.Count().Should().Be(count);
            }
        }

        [TestMethod]
        public async Task Select_MinMax_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var uniqueData = Guid.NewGuid().ToString();

                var data1 = await GetTestData(context, uniqueData, 1, CreateSubData);
                var data2a = await GetTestData(context, uniqueData, 2, CreateSubData);
                var data2b = await GetTestData(context, uniqueData, 2, CreateSubData);
                var data3a = await GetTestData(context, uniqueData, 3, CreateSubData);
                var data3b = await GetTestData(context, uniqueData, 3, CreateSubData);
                var data3c = await GetTestData(context, uniqueData, 3, CreateSubData);

                var result = await context.Repo.SelectAsync<Document>($"SELECT MIN(c.rank) AS Min, MAX(c.rank) AS Max FROM c WHERE c.data = '{uniqueData}'");

                result.Should().NotBeNull();
                result.Count().Should().Be(1);

                int min = result[0].GetPropertyValue<int>("Min");
                int max = result[0].GetPropertyValue<int>("Max");

                min.Should().Be(1);
                max.Should().Be(3);
            }
        }

        private void CreateSubData(TestData<Guid> data)
        {
            var rand = new Random();

            data.Subdata = Enumerable.Range(0, rand.Next(0, 5))
                .Select(i =>
                    new TestSubData
                    {
                        SubSubData = Enumerable.Range(0, rand.Next(0, 5))
                            .Select(j => new TestSubSubData
                            {
                                Id = Guid.NewGuid(),
                                Value = rand.Next(256).ToString()
                            })
                            .ToArray()
                    })
                .ToArray();
        }
    }
}
