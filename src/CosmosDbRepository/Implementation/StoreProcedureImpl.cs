using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Threading.Tasks;

namespace CosmosDbRepository.Implementation
{
    internal abstract class StoreProcedureImpl
    {
        protected readonly string Id;
        protected readonly IDocumentClient Client;
        protected readonly AsyncLazy<Uri> StoredProcUri;
        protected readonly ICosmosDbQueryStatsCollector StatsCollector;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector)
        {
            Id = id;
            Client = client;
            StoredProcUri = new AsyncLazy<Uri>(() => GetStoredProcUri(repository, id));
            StatsCollector = statsCollector;
        }

        private async Task<Uri> GetStoredProcUri(ICosmosDbRepository repository, string id)
        {
            return new Uri($"{await repository.AltLink}/sprocs/{Uri.EscapeUriString(id)}", UriKind.Relative);
        }
    }

    internal class StoreProcedureImpl<TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector)
            : base(client, repository, id, statsCollector)
        {
        }

        public async Task<TResult> ExecuteAsync(RequestOptions requestOptions)
        {
            var result = await Client.ExecuteStoredProcedureAsync<TResult>(await StoredProcUri.Value, requestOptions, requestOptions);
            StatsCollector?.Collect(new CosmosDbQueryStats<TResult>(result, $"ExecuteAsync({Id})"));
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector)
            : base(client, repository, id, statsCollector)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam param, RequestOptions requestOptions)
        {
            var result = await Client.ExecuteStoredProcedureAsync<TResult>(await StoredProcUri.Value, requestOptions, param);
            StatsCollector?.Collect(new CosmosDbQueryStats<TResult>(result, $"ExecuteAsync({Id})"));
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector)
            : base(client, repository, id, statsCollector)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, RequestOptions requestOptions)
        {
            var result = await Client.ExecuteStoredProcedureAsync<TResult>(await StoredProcUri.Value, requestOptions, param1, param2);
            StatsCollector?.Collect(new CosmosDbQueryStats<TResult>(result, $"ExecuteAsync({Id})"));
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector)
            : base(client, repository, id, statsCollector)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, RequestOptions requestOptions)
        {
            var result = await Client.ExecuteStoredProcedureAsync<TResult>(await StoredProcUri.Value, requestOptions, param1, param2, param3);
            StatsCollector?.Collect(new CosmosDbQueryStats<TResult>(result, $"ExecuteAsync({Id})"));
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector)
            : base(client, repository, id, statsCollector)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, RequestOptions requestOptions)
        {
            var result = await Client.ExecuteStoredProcedureAsync<TResult>(await StoredProcUri.Value, requestOptions, param1, param2, param3, param4);
            StatsCollector?.Collect(new CosmosDbQueryStats<TResult>(result, $"ExecuteAsync({Id})"));
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector)
            : base(client, repository, id, statsCollector)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, RequestOptions requestOptions)
        {
            var result = await Client.ExecuteStoredProcedureAsync<TResult>(await StoredProcUri.Value, requestOptions, param1, param2, param3, param4, param5);
            StatsCollector?.Collect(new CosmosDbQueryStats<TResult>(result, $"ExecuteAsync({Id})"));
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector)
            : base(client, repository, id, statsCollector)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, RequestOptions requestOptions)
        {
            var result = await Client.ExecuteStoredProcedureAsync<TResult>(await StoredProcUri.Value, requestOptions, param1, param2, param3, param4, param5, param6);
            StatsCollector?.Collect(new CosmosDbQueryStats<TResult>(result, $"ExecuteAsync({Id})"));
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector)
            : base(client, repository, id, statsCollector)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, RequestOptions requestOptions)
        {
            var result = await Client.ExecuteStoredProcedureAsync<TResult>(await StoredProcUri.Value, requestOptions, param1, param2, param3, param4, param5, param6, param7);
            StatsCollector?.Collect(new CosmosDbQueryStats<TResult>(result, $"ExecuteAsync({Id})"));
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector)
            : base(client, repository, id, statsCollector)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, RequestOptions requestOptions)
        {
            var result = await Client.ExecuteStoredProcedureAsync<TResult>(await StoredProcUri.Value, requestOptions, param1, param2, param3, param4, param5, param6, param7, param8);
            StatsCollector?.Collect(new CosmosDbQueryStats<TResult>(result, $"ExecuteAsync({Id})"));
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector)
            : base(client, repository, id, statsCollector)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, RequestOptions requestOptions)
        {
            var result = await Client.ExecuteStoredProcedureAsync<TResult>(await StoredProcUri.Value, requestOptions, param1, param2, param3, param4, param5, param6, param7, param8, param9);
            StatsCollector?.Collect(new CosmosDbQueryStats<TResult>(result, $"ExecuteAsync({Id})"));
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector)
            : base(client, repository, id, statsCollector)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, RequestOptions requestOptions)
        {
            var result = await Client.ExecuteStoredProcedureAsync<TResult>(await StoredProcUri.Value, requestOptions, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10);
            StatsCollector?.Collect(new CosmosDbQueryStats<TResult>(result, $"ExecuteAsync({Id})"));
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector)
            : base(client, repository, id, statsCollector)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, RequestOptions requestOptions)
        {
            var result = await Client.ExecuteStoredProcedureAsync<TResult>(await StoredProcUri.Value, requestOptions, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11);
            StatsCollector?.Collect(new CosmosDbQueryStats<TResult>(result, $"ExecuteAsync({Id})"));
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector)
            : base(client, repository, id, statsCollector)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12, RequestOptions requestOptions)
        {
            var result = await Client.ExecuteStoredProcedureAsync<TResult>(await StoredProcUri.Value, requestOptions, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12);
            StatsCollector?.Collect(new CosmosDbQueryStats<TResult>(result, $"ExecuteAsync({Id})"));
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector)
            : base(client, repository, id, statsCollector)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12, TParam13 param13, RequestOptions requestOptions)
        {
            var result = await Client.ExecuteStoredProcedureAsync<TResult>(await StoredProcUri.Value, requestOptions, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13);
            StatsCollector?.Collect(new CosmosDbQueryStats<TResult>(result, $"ExecuteAsync({Id})"));
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector)
            : base(client, repository, id, statsCollector)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12, TParam13 param13, TParam14 param14, RequestOptions requestOptions)
        {
            var result = await Client.ExecuteStoredProcedureAsync<TResult>(await StoredProcUri.Value, requestOptions, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14);
            StatsCollector?.Collect(new CosmosDbQueryStats<TResult>(result, $"ExecuteAsync({Id})"));
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector)
            : base(client, repository, id, statsCollector)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12, TParam13 param13, TParam14 param14, TParam15 param15, RequestOptions requestOptions)
        {
            var result = await Client.ExecuteStoredProcedureAsync<TResult>(await StoredProcUri.Value, requestOptions, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15);
            StatsCollector?.Collect(new CosmosDbQueryStats<TResult>(result, $"ExecuteAsync({Id})"));
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TParam16, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TParam16, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector)
            : base(client, repository, id, statsCollector)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12, TParam13 param13, TParam14 param14, TParam15 param15, TParam16 param16, RequestOptions requestOptions)
        {
            var result = await Client.ExecuteStoredProcedureAsync<TResult>(await StoredProcUri.Value, requestOptions, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16);
            StatsCollector?.Collect(new CosmosDbQueryStats<TResult>(result, $"ExecuteAsync({Id})"));
            return result;
        }
    }
}