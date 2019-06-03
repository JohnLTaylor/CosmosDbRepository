using System.Threading.Tasks;

namespace CosmosDbRepositoryTest
{
    public static class ExceptionExtensions
    {
        public static Task ShollowException(this Task task) => task.ContinueWith(_ => { }, TaskContinuationOptions.ExecuteSynchronously);
    }
}
