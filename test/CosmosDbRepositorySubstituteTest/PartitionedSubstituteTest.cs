using CosmosDbRepository;
using FluentAssertions;
using Microsoft.Azure.Documents.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CosmosDbRepositorySubstituteTest
{
    [TestClass]
    public class PartitionedSubstituteTest
        : CosmosDbRepositoryPartitionedTests<TestData<Guid>>
    {
        private const string IgnoreGeneratedFields = "Item2.ETag|Item2.UpdateEpoch";
        private const string IgnoreExceptionFields = "Item1.InnerException.TargetSite|Item1.InnerException.StackTrace|Item1.InnerException.Source|Item1.InnerException.IPForWatsonBuckets";
        private const string IgnoreExceptionMessageFields = IgnoreExceptionFields + "|Item1.Message|Item1.InnerException.Message|Item1.InnerException._message|Item1.InnerException.HResult|Item1.InnerException._HResult";

        private IServiceProvider _services;

        [TestInitialize]
        public void TestInitialize()
        {
            _services = TestFramework.Initialize();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            TestFramework.Cleanup(_services);
        }

        [TestMethod]
        public async Task AddTheSameIdDIfferentPartitions()
        {
            var data = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "One"
            };

            (Exception Exception, TestData<Guid> Result) repoResult;

            using (var context = CreateContext(_services, repoBuilderCallback: b => b.IncludePartitionkeyPath("/data").IncludePartitionkeySelector(i => i.Data)))
            {
                await context.Repo.AddAsync(data);
                var tmp = new TestData<Guid>
                {
                    Id = data.Id,
                    Data = "Two"
                };

                repoResult = await context.Repo.AddAsync(tmp).ContinueWith(CaptureResult);

                if (repoResult.Exception == default(Exception))
                {
                    var count = await context.Repo.FindAsync(feedOptions: new FeedOptions { EnableCrossPartitionQuery = true });
                    count.Count.Should().Be(2);
                }
            }

            (Exception Exception, TestData<Guid>) subResult;

            using (var context = CreateSubstituteContext(i => i.Data))
            {
                await context.Repo.AddAsync(data);
                var tmp = new TestData<Guid>
                {
                    Id = data.Id,
                    Data = "Two"
                };

                subResult = await context.Repo.AddAsync(tmp).ContinueWith(CaptureResult);

                if (subResult.Exception == default(Exception))
                {
                    var count = await context.Repo.FindAsync(feedOptions: new FeedOptions { EnableCrossPartitionQuery = true });
                    count.Count.Should().Be(2);
                }
            }

            subResult.Should().BeEquivalentTo(repoResult, opt => opt.Excluding(su =>
                    Regex.IsMatch(su.SelectedMemberPath, IgnoreGeneratedFields)));
        }

        [TestMethod]
        public async Task GetWithoutPartitions()
        {
            var data = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "One"
            };

            (Exception Exception, TestData<Guid> Result) repoResult;

            using (var context = CreateContext(_services, repoBuilderCallback: b => b.IncludePartitionkeyPath("/data").IncludePartitionkeySelector(i => i.Data)))
            {
                await context.Repo.AddAsync(data);

                repoResult = await context.Repo.GetAsync(data.Id).ContinueWith(CaptureResult);
            }

            (Exception Exception, TestData<Guid>) subResult;

            using (var context = CreateSubstituteContext(i => i.Data))
            {
                await context.Repo.AddAsync(data);

                subResult = await context.Repo.GetAsync(data.Id).ContinueWith(CaptureResult);
            }

            subResult.Should().BeEquivalentTo(repoResult, opt => opt.Excluding(su =>
                    Regex.IsMatch(su.SelectedMemberPath, IgnoreExceptionFields)));
        }

        [TestMethod]
        public async Task GetWithWrongPartitions()
        {
            var data = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "One"
            };

            (Exception Exception, TestData<Guid> Result) repoResult;

            using (var context = CreateContext(_services, repoBuilderCallback: b => b.IncludePartitionkeyPath("/data").IncludePartitionkeySelector(i => i.Data)))
            {
                await context.Repo.AddAsync(data);
                repoResult = await context.Repo.GetAsync("Two", data.Id).ContinueWith(CaptureResult);
            }

            (Exception Exception, TestData<Guid>) subResult;

            using (var context = CreateSubstituteContext(i => i.Data))
            {
                await context.Repo.AddAsync(data);
                subResult = await context.Repo.GetAsync("Two", data.Id).ContinueWith(CaptureResult);
            }

            subResult.Should().BeEquivalentTo(repoResult);
        }

        [TestMethod]
        public async Task ReplaceWithSamePartition()
        {
            var data = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "One"
            };

            (Exception Exception, TestData<Guid> Result) repoResult;

            using (var context = CreateContext(_services, repoBuilderCallback: b => b.IncludePartitionkeyPath("/data").IncludePartitionkeySelector(i => i.Data)))
            {
                var tmp = await context.Repo.AddAsync(data);
                repoResult = await context.Repo.ReplaceAsync(tmp).ContinueWith(CaptureResult);
            }

            (Exception Exception, TestData<Guid>) subResult;

            using (var context = CreateSubstituteContext(i => i.Data))
            {
                var tmp = await context.Repo.AddAsync(data);
                subResult = await context.Repo.ReplaceAsync(tmp).ContinueWith(CaptureResult);
            }

            subResult.Should().BeEquivalentTo(repoResult, opt => opt.Excluding(su =>
                    Regex.IsMatch(su.SelectedMemberPath, IgnoreGeneratedFields)));
        }

        [TestMethod]
        public async Task ReplaceWithDifferentPartition()
        {
            var data = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "One"
            };

            (Exception Exception, TestData<Guid> Result) repoResult;

            using (var context = CreateContext(_services, repoBuilderCallback: b => b.IncludePartitionkeyPath("/data").IncludePartitionkeySelector(i => i.Data)))
            {
                var tmp = await context.Repo.AddAsync(data);
                tmp.Data = "Two";
                repoResult = await context.Repo.ReplaceAsync(tmp).ContinueWith(CaptureResult);
            }

            (Exception Exception, TestData<Guid>) subResult;

            using (var context = CreateSubstituteContext(i => i.Data))
            {
                var tmp = await context.Repo.AddAsync(data);
                tmp.Data = "Two";
                subResult = await context.Repo.ReplaceAsync(tmp).ContinueWith(CaptureResult);
            }

            subResult.Should().BeEquivalentTo(repoResult, opt => opt.Excluding(su =>
                    Regex.IsMatch(su.SelectedMemberPath, IgnoreExceptionMessageFields)));
        }

        private (Exception Exception, T Result) CaptureResult<T>(Task<T> task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    return (default, task.Result);

                case TaskStatus.Faulted:
                    return (task.Exception, default);

                default:
                    throw new IndexOutOfRangeException($"task.Status '{task.Status}' is not handled");
            }
        }
    }
}
