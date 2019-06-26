using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CosmosDbRepositoryTest.SQL
{
    [TestClass]
    public class CosmosDbRepositoryJoinTests
        : CosmosDbRepositoryTests<ComplexTestData<Guid>>
    {
        private const string _viewModelQuery =
            "SELECT t.id, t.xRefId, t.date, g.dataType, g.dataCategory, g.numericValue " +
            "FROM TestData t " +
            "JOIN c IN t.childItems " +
            "JOIN g IN c.grandchildItems " +
            "WHERE c.booleanValue";

        [TestMethod]
        public async Task Find_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var data = JsonConvert.DeserializeObject<ComplexTestData<Guid>>(_testData);
                data = await context.Repo.UpsertAsync(data);

                //var dataList = await context.Repo.FindAsync<ViewModel>(_viewModelQuery);
                var dataList = (await context.Repo.SelectManyAsync(
                    t => t.ChildItems.Where(c => c.BooleanValue).SelectMany(c => c.GrandchildItems.Select(g => new { t.Id, t.XRefId, t.Date, g.DataType, g.DataCategory, g.NumericValue })))).ToArray();

                dataList.Should().HaveCount(3);

                dataList[0].DataType.Should().Be(data.ChildItems[0].GrandchildItems[0].DataType);
                dataList[0].DataCategory.Should().Be(data.ChildItems[0].GrandchildItems[0].DataCategory);
                dataList[0].NumericValue.Should().Be(data.ChildItems[0].GrandchildItems[0].NumericValue);

                dataList[1].DataType.Should().Be(data.ChildItems[0].GrandchildItems[1].DataType);
                dataList[1].DataCategory.Should().Be(data.ChildItems[0].GrandchildItems[1].DataCategory);
                dataList[1].NumericValue.Should().Be(data.ChildItems[0].GrandchildItems[1].NumericValue);

                dataList[2].DataType.Should().Be(data.ChildItems[2].GrandchildItems[0].DataType);
                dataList[2].DataCategory.Should().Be(data.ChildItems[2].GrandchildItems[0].DataCategory);
                dataList[2].NumericValue.Should().Be(data.ChildItems[2].GrandchildItems[0].NumericValue);
            }
        }

        [TestMethod]
        public async Task Find_Expect_Success_WithNoData()
        {
            using (var context = CreateContext())
            {
                await context.Repo.DeleteAsync();
                //                var dataList = await context.Repo.FindAsync<ViewModel>(_viewModelQuery);
                var dataList = (await context.Repo.SelectManyAsync(
                    t => t.ChildItems.Where(c => c.BooleanValue).SelectMany(c => c.GrandchildItems.Select(g => new { t.Id, t.XRefId, t.Date, g.DataType, g.DataCategory, g.NumericValue })))).ToArray();
                dataList.Should().BeEmpty();
            }
        }

        [TestMethod]
        public async Task Find_WithSkipTake_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var data = JsonConvert.DeserializeObject<ComplexTestData<Guid>>(_testData);
                data = await context.Repo.UpsertAsync(data);

                //                var dataList = await context.Repo.FindAsync<ViewModel>(_viewModelQuery + " OFFSET 2 LIMIT 2");
                var dataList = (await context.Repo.SelectManyAsync(
                    t => t.ChildItems.Where(c => c.BooleanValue).SelectMany(c => c.GrandchildItems.Select(g => new { t.Id, t.XRefId, t.Date, g.DataType, g.DataCategory, g.NumericValue })),
                     selectClauses: q => q.Skip(2).Take(2))).ToArray();

                dataList.Should().HaveCount(1);

                dataList[0].DataType.Should().Be(data.ChildItems[2].GrandchildItems[0].DataType);
                dataList[0].DataCategory.Should().Be(data.ChildItems[2].GrandchildItems[0].DataCategory);
                dataList[0].NumericValue.Should().Be(data.ChildItems[2].GrandchildItems[0].NumericValue);
            }
        }

        [TestMethod]
        public async Task Find_WithPageLimit_Expect_Success()
        {
            using (var context = CreateContext())
            {
                var data = JsonConvert.DeserializeObject<ComplexTestData<Guid>>(_testData);
                data = await context.Repo.UpsertAsync(data);

                //var first = await context.Repo.FindAsync<ViewModel>(1, null, _viewModelQuery);
                var first = await context.Repo.SelectManyAsync(1, null,
                    t => t.ChildItems.Where(c => c.BooleanValue).SelectMany(c => c.GrandchildItems.Select(g => new { t.Id, t.XRefId, t.Date, g.DataType, g.DataCategory, g.NumericValue })));

                first.Items.Should().HaveCount(1);

                first.Items[0].DataType.Should().Be(data.ChildItems[0].GrandchildItems[0].DataType);
                first.Items[0].DataCategory.Should().Be(data.ChildItems[0].GrandchildItems[0].DataCategory);
                first.Items[0].NumericValue.Should().Be(data.ChildItems[0].GrandchildItems[0].NumericValue);

                //var second = await context.Repo.FindAsync<ViewModel>(2, first.ContinuationToken, _viewModelQuery);
                var second = await context.Repo.SelectManyAsync(2, first.ContinuationToken,
                    t => t.ChildItems.Where(c => c.BooleanValue).SelectMany(c => c.GrandchildItems.Select(g => new { t.Id, t.XRefId, t.Date, g.DataType, g.DataCategory, g.NumericValue })));

                second.Items.Should().HaveCount(2);

                second.Items[0].DataType.Should().Be(data.ChildItems[0].GrandchildItems[1].DataType);
                second.Items[0].DataCategory.Should().Be(data.ChildItems[0].GrandchildItems[1].DataCategory);
                second.Items[0].NumericValue.Should().Be(data.ChildItems[0].GrandchildItems[1].NumericValue);

                second.Items[1].DataType.Should().Be(data.ChildItems[2].GrandchildItems[0].DataType);
                second.Items[1].DataCategory.Should().Be(data.ChildItems[2].GrandchildItems[0].DataCategory);
                second.Items[1].NumericValue.Should().Be(data.ChildItems[2].GrandchildItems[0].NumericValue);
            }
        }

        public class ViewModel
        {
            public Guid Id { get; set; }
            public string XRefId { get; set; }
            public DateTimeOffset Date { get; set; }
            public string DataType { get; set; }
            public string DataCategory { get; set; }
            public float NumericValue { get; set; }
        }

        private const string _testData =
    @"{
    ""id"": ""02f1dfe9-38b1-274e-2d8c-154a82b84e49"",
    ""tag"": ""ComplexTestData"",
    ""xRefId"": ""6b017a0d-7fb3-4aa7-bd03-b75f1f4ed5b2"",
    ""date"": ""2019-06-15T04:48:00+00:00"",
    ""childItems"": [
        {
            ""booleanValue"": true,
            ""grandchildItems"": [
                {
                    ""dataType"": ""Type1"",
                    ""dataCategory"": ""Category1"",
                    ""createdAt"": ""2019-06-16T05:05:00+00:00"",
                    ""numericValue"": 1.11,
                },
                {
                    ""dataType"": ""Type1"",
                    ""dataCategory"": ""Category2"",
                    ""createdAt"": ""2019-06-16T05:05:00+00:00"",
                    ""numericValue"": 1.12,
                }
            ]
        },
        {
            ""booleanValue"": false,
            ""grandchildItems"": [
                {
                    ""dataType"": ""Type1"",
                    ""dataCategory"": ""Category1"",
                    ""createdAt"": ""2019-06-16T05:05:00+00:00"",
                    ""numericValue"": 2.11
                }
            ]
        },
        {
            ""booleanValue"": true,
            ""grandchildItems"": [
                {
                    ""dataType"": ""Type2"",
                    ""dataCategory"": ""Category1"",
                    ""createdAt"": ""2019-06-16T05:05:00+00:00"",
                    ""numericValue"": 3.11
                }
            ]
        },
        {
            ""booleanValue"": true,
            ""grandchildItems"": []
        }
    ]
}";
    }
}
