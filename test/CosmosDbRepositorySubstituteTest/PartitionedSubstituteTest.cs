using CosmosDbRepository;
using FluentAssertions;
using Microsoft.Azure.Documents.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CosmosDbRepositorySubstituteTest
{
    [TestClass]
    public class PartitionedSubstituteTest
        : CosmosDbRepositoryTests<TestData<Guid>>
    {
        private const string IgnoreGeneratedFields = "Item2.ETag|Item2.UpdateEpoch";
        private const string IgnoreExceptionFields = "Item1.InnerException.TargetSite|Item1.InnerException.StackTrace|Item1.InnerException.Source|Item1.InnerException.IPForWatsonBuckets";
        private const string IgnoreExceptionMessageFields = IgnoreExceptionFields + "|Item1.Message|Item1.InnerException.Message|Item1.InnerException._message|Item1.InnerException.HResult|Item1.InnerException._HResult";
        private const string IgnoreArrayGeneratedFields = @"Item2\[.\]\.ETag|Item2\[.\]\.UpdateEpoch";
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

        [TestMethod]
        public async Task AddAndGetWithPartitionKey()
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
                repoResult = await context.Repo.GetAsync(tmp).ContinueWith(CaptureResult);
            }

            (Exception Exception, TestData<Guid>) subResult;

            using (var context = CreateSubstituteContext(i => i.Data))
            {
                var tmp = await context.Repo.AddAsync(data);
                subResult = await context.Repo.GetAsync(tmp).ContinueWith(CaptureResult);
            }

            subResult.Should().BeEquivalentTo(repoResult, opt => opt.Excluding(su =>
                    Regex.IsMatch(su.SelectedMemberPath, IgnoreGeneratedFields)));
        }

        [TestMethod]
        public async Task FindItemsCrossPartition()
        {
            var dataOne = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "One"
            };

            var dataTwo = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "Two"
            };

            (Exception Exception, IList<TestData<Guid>> Result) repoResult;

            using (var context = CreateContext(_services, repoBuilderCallback: b => b.IncludePartitionkeyPath("/data").IncludePartitionkeySelector(i => i.Data)))
            {
                await context.Repo.AddAsync(dataOne);
                await context.Repo.AddAsync(dataTwo);

                repoResult = await context.Repo.CrossPartitionFindAsync().ContinueWith(CaptureResult);
            }

            (Exception Exception, IList<TestData<Guid>>) subResult;

            using (var context = CreateSubstituteContext(i => i.Data))
            {
                await context.Repo.AddAsync(dataOne);
                await context.Repo.AddAsync(dataTwo);

                subResult = await context.Repo.CrossPartitionFindAsync().ContinueWith(CaptureResult);
            }

            subResult.Should().BeEquivalentTo(repoResult, opt => opt.Excluding(su =>
                    Regex.IsMatch(su.SelectedMemberPath, IgnoreArrayGeneratedFields)));
        }

        [TestMethod]
        public async Task FindItemsTwoPartitions()
        {
            var dataOne = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "One"
            };

            var dataTwo = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "Two"
            };

            (Exception Exception, IList<TestData<Guid>> Result) repoResultOne;
            (Exception Exception, IList<TestData<Guid>> Result) repoResultTwo;

            using (var context = CreateContext(_services, repoBuilderCallback: b => b.IncludePartitionkeyPath("/data").IncludePartitionkeySelector(i => i.Data)))
            {
                await context.Repo.AddAsync(dataOne.Data, dataOne);
                await context.Repo.AddAsync(dataTwo.Data, dataTwo);

                repoResultOne = await context.Repo.FindAsync(dataOne.Data).ContinueWith(CaptureResult);
                repoResultTwo = await context.Repo.FindAsync(dataTwo.Data).ContinueWith(CaptureResult);
            }

            (Exception Exception, IList<TestData<Guid>>) subResultOne;
            (Exception Exception, IList<TestData<Guid>>) subResultTwo;

            using (var context = CreateSubstituteContext(i => i.Data))
            {
                await context.Repo.AddAsync(dataOne.Data, dataOne);
                await context.Repo.AddAsync(dataTwo.Data, dataTwo);

                subResultOne = await context.Repo.FindAsync(dataOne.Data).ContinueWith(CaptureResult);
                subResultTwo = await context.Repo.FindAsync(dataTwo.Data).ContinueWith(CaptureResult);
            }

            subResultOne.Should().BeEquivalentTo(repoResultOne, opt => opt.Excluding(su =>
                    Regex.IsMatch(su.SelectedMemberPath, IgnoreArrayGeneratedFields)));

            subResultTwo.Should().BeEquivalentTo(repoResultTwo, opt => opt.Excluding(su =>
                    Regex.IsMatch(su.SelectedMemberPath, IgnoreArrayGeneratedFields)));
        }

        [TestMethod]
        public async Task SelectItemsTwoPartitions()
        {
            var dataOneA = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "One"
            };

            var dataOneB = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "One"
            };

            var dataTwoA = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "Two"
            };

            var dataTwoB = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "Two"
            };

            (Exception Exception, IList<Guid> Result) repoResultOne;
            (Exception Exception, IList<Guid> Result) repoResultTwo;

            using (var context = CreateContext(_services, repoBuilderCallback: b => b.IncludePartitionkeyPath("/data").IncludePartitionkeySelector(i => i.Data)))
            {
                await context.Repo.AddAsync(dataOneA);
                await context.Repo.AddAsync(dataOneB);
                await context.Repo.AddAsync(dataTwoA);
                await context.Repo.AddAsync(dataTwoB);

                repoResultOne = await context.Repo.SelectAsync(partitionKey: dataOneA.Data, selector: d => d.Id, selectClauses: d => d).ContinueWith(CaptureResult);
                repoResultTwo = await context.Repo.SelectAsync(partitionKey: dataTwoA.Data, selector: d => d.Id, selectClauses: d => d).ContinueWith(CaptureResult);
            }

            (Exception Exception, IList<Guid>) subResultOne;
            (Exception Exception, IList<Guid>) subResultTwo;

            using (var context = CreateSubstituteContext(i => i.Data))
            {
                await context.Repo.AddAsync(dataOneA);
                await context.Repo.AddAsync(dataOneB);
                await context.Repo.AddAsync(dataTwoA);
                await context.Repo.AddAsync(dataTwoB);

                subResultOne = await context.Repo.SelectAsync(partitionKey: dataOneA.Data, selector: d => d.Id, selectClauses: d => d).ContinueWith(CaptureResult);
                subResultTwo = await context.Repo.SelectAsync(partitionKey: dataTwoA.Data, selector: d => d.Id, selectClauses: d => d).ContinueWith(CaptureResult);
            }

            subResultOne.Should().BeEquivalentTo(repoResultOne, opt => opt.Excluding(su =>
                    Regex.IsMatch(su.SelectedMemberPath, IgnoreArrayGeneratedFields)));

            subResultTwo.Should().BeEquivalentTo(repoResultTwo, opt => opt.Excluding(su =>
                    Regex.IsMatch(su.SelectedMemberPath, IgnoreArrayGeneratedFields)));
        }

        [TestMethod]
        public async Task FindItemsWrongPartition()
        {
            var dataOne = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "One"
            };

            var dataTwo = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "Two"
            };

            (Exception Exception, IList<TestData<Guid>> Result) repoResult;

            using (var context = CreateContext(_services, repoBuilderCallback: b => b.IncludePartitionkeyPath("/data").IncludePartitionkeySelector(i => i.Data)))
            {
                await context.Repo.AddAsync(dataOne);
                await context.Repo.AddAsync(dataTwo);

                repoResult = await context.Repo.FindAsync("Three").ContinueWith(CaptureResult);
            }

            (Exception Exception, IList<TestData<Guid>>) subResult;

            using (var context = CreateSubstituteContext(i => i.Data))
            {
                await context.Repo.AddAsync(dataOne);
                await context.Repo.AddAsync(dataTwo);

                subResult = await context.Repo.FindAsync("Three").ContinueWith(CaptureResult);
            }

            subResult.Should().BeEquivalentTo(repoResult);
        }

        [TestMethod]
        public async Task FindItemsNoPartition()
        {
            var dataOne = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "One"
            };

            var dataTwo = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "Two"
            };

            (Exception Exception, IList<TestData<Guid>> Result) repoResult;

            using (var context = CreateContext(_services, repoBuilderCallback: b => b.IncludePartitionkeyPath("/data").IncludePartitionkeySelector(i => i.Data)))
            {
                await context.Repo.AddAsync(dataOne);
                await context.Repo.AddAsync(dataTwo);

                repoResult = await context.Repo.FindAsync().ContinueWith(CaptureResult);
            }

            (Exception Exception, IList<TestData<Guid>>) subResult;

            using (var context = CreateSubstituteContext(i => i.Data))
            {
                await context.Repo.AddAsync(dataOne);
                await context.Repo.AddAsync(dataTwo);

                subResult = await context.Repo.FindAsync().ContinueWith(CaptureResult);
            }

            subResult.Should().BeEquivalentTo(repoResult, opt => opt.Excluding(su =>
                    Regex.IsMatch(su.SelectedMemberPath, IgnoreExceptionFields)));
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
