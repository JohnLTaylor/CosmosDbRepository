using FluentAssertions;
using Microsoft.Azure.Documents.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CosmosDbRepositorySubstituteTest
{
    [TestClass]
    public class SubstituteTest
        : CosmosDbRepositoryTests<TestData<Guid>>
    {
        [TestMethod]
        public async Task AddWithId()
        {
            var data = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "My Data"
            };

            (Exception Exception, TestData<Guid> Result) repoResult;

            using (var context = CreateContext())
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

            using (var context = CreateContext())
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

            using (var context = CreateContext())
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

            using (var context = CreateContext())
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
        public async Task UpsertItemWithETagChange()
        {
            var data = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = "My Data"
            };

            (Exception Exception, TestData<Guid> Result) repoResult;

            using (var context = CreateContext())
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

            repoResult.Should().BeEquivalentTo(subResult, opt => opt.Excluding(su =>
                    Regex.IsMatch(su.SelectedMemberPath, "Item1.Message|Item1.InnerException")));
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
