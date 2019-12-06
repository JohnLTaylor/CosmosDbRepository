using CosmosDbRepository.Substitute;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CosmosDbRepositorySubstituteTest
{
    [TestClass]
    public class SubstituteTest
        : CosmosDbRepositoryTests<TestData<Guid>>
    {
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
        public async Task AddWithId()
        {
            var data = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "My Data"
            };

            (Exception Exception, TestData<Guid> Result) repoResult;

            using (var context = CreateContext(_services))
            {
                repoResult = await context.Repo.AddAsync(data).ContinueWith(CaptureResult);
            }

            (Exception Exception, TestData<Guid> Result) subResult;

            using (var context = CreateSubstituteContext())
            {
                subResult = await context.Repo.AddAsync(data).ContinueWith(CaptureResult);
            }

            repoResult.Should().BeEquivalentTo(subResult, opt => opt.Excluding(su =>
                    Regex.IsMatch(su.SelectedMemberPath, "Item2.ETag|Item2.UpdateEpoch")));

            subResult.Result.ETag.Should().NotBeNullOrEmpty();
            subResult.Result.UpdateEpoch.Should().NotBe(0);
        }

        [TestMethod]
        public async Task AddWithoutId()
        {
            var data = new TestData<Guid>
            {
                Data = "My Data"
            };

            (Exception Exception, TestData<Guid> Result) repoResult;

            using (var context = CreateContext(_services))
            {
                repoResult = await context.Repo.AddAsync(data).ContinueWith(CaptureResult);
            }

            (Exception Exception, TestData<Guid> Result) subResult;

            using (var context = CreateSubstituteContext())
            {
                subResult = await context.Repo.AddAsync(data).ContinueWith(CaptureResult);
            }

            repoResult.Should().BeEquivalentTo(subResult, opt => opt.Excluding(su =>
                    Regex.IsMatch(su.SelectedMemberPath, "Item2.ETag|Item2.UpdateEpoch")));

            subResult.Result.ETag.Should().NotBeNullOrEmpty();
            subResult.Result.UpdateEpoch.Should().NotBe(0);
        }

        [TestMethod]
        public async Task DeleteItem()
        {
            var data = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "My Data"
            };

            (Exception Exception, bool Result) repoResult;

            using (var context = CreateContext(_services))
            {
                var tmp = await context.Repo.AddAsync(data);
                repoResult = await context.Repo.DeleteDocumentAsync(tmp).ContinueWith(CaptureResult);
            }

            (Exception Exception, bool) subResult;

            using (var context = CreateSubstituteContext())
            {
                var tmp = await context.Repo.AddAsync(data);
                subResult = await context.Repo.DeleteDocumentAsync(tmp).ContinueWith(CaptureResult);
            }

            repoResult.Should().BeEquivalentTo(subResult);
        }

        [TestMethod]
        public async Task DeleteItemWithETagChange()
        {
            var data = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "My Data"
            };

            (Exception Exception, bool Result) repoResult;

            using (var context = CreateContext(_services))
            {
                var tmp = await context.Repo.AddAsync(data);
                await context.Repo.ReplaceAsync(tmp);
                repoResult = await context.Repo.DeleteDocumentAsync(tmp).ContinueWith(CaptureResult);
            }

            (Exception Exception, bool) subResult;

            using (var context = CreateSubstituteContext())
            {
                var tmp = await context.Repo.AddAsync(data);
                await context.Repo.ReplaceAsync(tmp);
                subResult = await context.Repo.DeleteDocumentAsync(tmp).ContinueWith(CaptureResult);
            }

            repoResult.Should().BeEquivalentTo(subResult, opt => opt.Excluding(su =>
                    Regex.IsMatch(su.SelectedMemberPath, "Item1.Message|Item1.InnerException")));
        }

        [TestMethod]
        public async Task DeleteItemByIdWithError()
        {
            var data = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "My Data"
            };

            (Exception Exception, bool) subResult;

            using (var context = CreateSubstituteContext())
            {
                var tmp = await context.Repo.AddAsync(data);
                context.Repo.GenerateExceptionOnDeleteWhen(id => data.Id == id, HttpStatusCode.ExpectationFailed);
                await context.Repo.ReplaceAsync(tmp);
                subResult = await context.Repo.DeleteDocumentAsync(tmp).ContinueWith(CaptureResult);
            }

            subResult.Exception.Should().NotBeNull();
            var aggException = subResult.Exception as AggregateException;
            aggException.Should().NotBeNull();
            aggException.InnerExceptions.Should().HaveCount(1);
            var exception = aggException.InnerExceptions[0] as DocumentClientException;
            exception.Should().NotBeNull();
            exception.StatusCode.Should().Be(HttpStatusCode.ExpectationFailed);
        }

        [TestMethod]
        public async Task DeleteItemByInstanceWithError()
        {
            var data = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "My Data"
            };

            (Exception Exception, bool) subResult;

            using (var context = CreateSubstituteContext())
            {
                var tmp = await context.Repo.AddAsync(data);
                context.Repo.GenerateExceptionOnDeleteWhen(inst => data.Id == inst.Id, HttpStatusCode.ExpectationFailed);
                await context.Repo.ReplaceAsync(tmp);
                subResult = await context.Repo.DeleteDocumentAsync(tmp).ContinueWith(CaptureResult);
            }

            subResult.Exception.Should().NotBeNull();
            var aggException = subResult.Exception as AggregateException;
            aggException.Should().NotBeNull();
            aggException.InnerExceptions.Should().HaveCount(1);
            var exception = aggException.InnerExceptions[0] as DocumentClientException;
            exception.Should().NotBeNull();
            exception.StatusCode.Should().Be(HttpStatusCode.ExpectationFailed);
        }

        [TestMethod]
        public async Task UpsertItemWithETagChange()
        {
            var data = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "My Data"
            };

            (Exception Exception, TestData<Guid> Result) repoResult;

            using (var context = CreateContext(_services))
            {
                var tmp = await context.Repo.AddAsync(data);
                await context.Repo.UpsertAsync(tmp);
                repoResult = await context.Repo.UpsertAsync(tmp).ContinueWith(CaptureResult);
            }

            (Exception Exception, TestData<Guid>) subResult;

            using (var context = CreateSubstituteContext())
            {
                var tmp = await context.Repo.AddAsync(data);
                await context.Repo.UpsertAsync(tmp);
                subResult = await context.Repo.UpsertAsync(tmp).ContinueWith(CaptureResult);
            }

            subResult.Should().BeEquivalentTo(repoResult, opt => opt.Excluding(su =>
                    Regex.IsMatch(su.SelectedMemberPath, "Item1.Message|Item1.InnerException")));
        }

        [TestMethod]
        public async Task UpsertItemWithoutETag()
        {
            var data = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "My Data"
            };

            (Exception Exception, TestData<Guid> Result) repoResult;

            using (var context = CreateContext(_services))
            {
                var tmp = await context.Repo.AddAsync(data);
                await context.Repo.UpsertAsync(tmp);
                tmp.ETag = default;
                repoResult = await context.Repo.UpsertAsync(tmp).ContinueWith(CaptureResult);
            }

            (Exception Exception, TestData<Guid>) subResult;

            using (var context = CreateSubstituteContext())
            {
                var tmp = await context.Repo.AddAsync(data);
                await context.Repo.UpsertAsync(tmp);
                tmp.ETag = default;
                subResult = await context.Repo.UpsertAsync(tmp).ContinueWith(CaptureResult);
            }

            subResult.Should().BeEquivalentTo(repoResult, opt => opt.Excluding(su =>
                    Regex.IsMatch(su.SelectedMemberPath, "Item2.ETag|Item2.UpdateEpoch")));
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
