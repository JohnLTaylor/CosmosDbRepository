using System;
using System.Linq;
using System.Threading;

namespace CosmosDbRepositoryTest.StringId
{
    public class CosmosDbRepositoryStringTests
        : CosmosDbRepositoryTests<TestData<string>>
    {
        private readonly Random _random = new Random();
        private static int _serialnumber;

        protected string GetNewId()
        {
            var randomStr = new string(Enumerable.Range(0, 16).Select(_ => (char)_random.Next('A', 'Z' + 1)).ToArray());

            return $"{randomStr}{Interlocked.Increment(ref _serialnumber):0000}";
        }
    }
}