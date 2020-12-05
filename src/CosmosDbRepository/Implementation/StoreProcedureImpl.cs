using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _scriptLogger;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector, ILogger scriptLogger)
        {
            Id = id;
            Client = client;
            StoredProcUri = new AsyncLazy<Uri>(() => GetStoredProcUri(repository, id));
            StatsCollector = statsCollector;
            _scriptLogger = scriptLogger;
        }

        protected async Task<TResult> ExecutorAsync<TResult>(RequestOptions requestOptions, params dynamic[] parameters)
        {
            bool enableScriptLogging = _scriptLogger != default;

            requestOptions = requestOptions.ShallowCopy() ?? new RequestOptions();
            requestOptions.EnableScriptLogging = enableScriptLogging;

            var result = await Client.ExecuteStoredProcedureAsync<TResult>(await StoredProcUri.Value, requestOptions, new dynamic[]{ parameters });
            StatsCollector?.Collect(new CosmosDbQueryStats<TResult>(result, $"ExecuteAsync({Id})"));

            if (enableScriptLogging && !string.IsNullOrWhiteSpace(result.ScriptLog))
            {
                _scriptLogger.LogInformation(result.ScriptLog);
            }

            return result;
        }

        protected async Task<TResult> PolymorphicExecutor<TResult>(Func<Document, TResult> deserializer, RequestOptions requestOptions, params dynamic[] parameters)
        {
            var result = await Client.ExecuteStoredProcedureAsync<Document>(await StoredProcUri.Value, requestOptions, new dynamic[]{ parameters });
            StatsCollector?.Collect(new CosmosDbQueryStats<Document>(result, $"ExecuteAsync({Id})"));
            return deserializer(result);
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
        private Func<RequestOptions, dynamic[], Task<TResult>> _executor;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector, ILogger scriptLogger, Func<Document, TResult> deserializer)
            : base(client, repository, id, statsCollector, scriptLogger)
        {
            _executor = (deserializer != default)
				? (Func<RequestOptions, dynamic[], Task<TResult>>)((requestOptions, parameters) => PolymorphicExecutor(deserializer, requestOptions, parameters))
				: (requestOptions, parameters) => ExecutorAsync<TResult>(requestOptions, parameters);
        }

        public async Task<TResult> ExecuteAsync(RequestOptions requestOptions)
        {
            var result = await _executor(requestOptions, new dynamic[]{ });
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam, TResult>
    {
        private Func<RequestOptions, dynamic[], Task<TResult>> _executor;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector, ILogger scriptLogger, Func<Document, TResult> deserializer)
            : base(client, repository, id, statsCollector, scriptLogger)
        {
            _executor = (deserializer != default)
				? (Func<RequestOptions, dynamic[], Task<TResult>>)((requestOptions, parameters) => PolymorphicExecutor(deserializer, requestOptions, parameters))
				: (requestOptions, parameters) => ExecutorAsync<TResult>(requestOptions, parameters);
        }

        public async Task<TResult> ExecuteAsync(TParam param, RequestOptions requestOptions)
        {
            var result = await _executor(requestOptions, new dynamic[]{ param });
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TResult>
    {
        private Func<RequestOptions, dynamic[], Task<TResult>> _executor;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector, ILogger scriptLogger, Func<Document, TResult> deserializer)
            : base(client, repository, id, statsCollector, scriptLogger)
        {
            _executor = (deserializer != default)
				? (Func<RequestOptions, dynamic[], Task<TResult>>)((requestOptions, parameters) => PolymorphicExecutor(deserializer, requestOptions, parameters))
				: (requestOptions, parameters) => ExecutorAsync<TResult>(requestOptions, parameters);
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, RequestOptions requestOptions)
        {
            var result = await _executor(requestOptions, new dynamic[]{ param1, param2 });
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TResult>
    {
        private Func<RequestOptions, dynamic[], Task<TResult>> _executor;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector, ILogger scriptLogger, Func<Document, TResult> deserializer)
            : base(client, repository, id, statsCollector, scriptLogger)
        {
            _executor = (deserializer != default)
				? (Func<RequestOptions, dynamic[], Task<TResult>>)((requestOptions, parameters) => PolymorphicExecutor(deserializer, requestOptions, parameters))
				: (requestOptions, parameters) => ExecutorAsync<TResult>(requestOptions, parameters);
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, RequestOptions requestOptions)
        {
            var result = await _executor(requestOptions, new dynamic[]{ param1, param2, param3 });
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TResult>
    {
        private Func<RequestOptions, dynamic[], Task<TResult>> _executor;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector, ILogger scriptLogger, Func<Document, TResult> deserializer)
            : base(client, repository, id, statsCollector, scriptLogger)
        {
            _executor = (deserializer != default)
				? (Func<RequestOptions, dynamic[], Task<TResult>>)((requestOptions, parameters) => PolymorphicExecutor(deserializer, requestOptions, parameters))
				: (requestOptions, parameters) => ExecutorAsync<TResult>(requestOptions, parameters);
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, RequestOptions requestOptions)
        {
            var result = await _executor(requestOptions, new dynamic[]{ param1, param2, param3, param4 });
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>
    {
        private Func<RequestOptions, dynamic[], Task<TResult>> _executor;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector, ILogger scriptLogger, Func<Document, TResult> deserializer)
            : base(client, repository, id, statsCollector, scriptLogger)
        {
            _executor = (deserializer != default)
				? (Func<RequestOptions, dynamic[], Task<TResult>>)((requestOptions, parameters) => PolymorphicExecutor(deserializer, requestOptions, parameters))
				: (requestOptions, parameters) => ExecutorAsync<TResult>(requestOptions, parameters);
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, RequestOptions requestOptions)
        {
            var result = await _executor(requestOptions, new dynamic[]{ param1, param2, param3, param4, param5 });
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>
    {
        private Func<RequestOptions, dynamic[], Task<TResult>> _executor;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector, ILogger scriptLogger, Func<Document, TResult> deserializer)
            : base(client, repository, id, statsCollector, scriptLogger)
        {
            _executor = (deserializer != default)
				? (Func<RequestOptions, dynamic[], Task<TResult>>)((requestOptions, parameters) => PolymorphicExecutor(deserializer, requestOptions, parameters))
				: (requestOptions, parameters) => ExecutorAsync<TResult>(requestOptions, parameters);
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, RequestOptions requestOptions)
        {
            var result = await _executor(requestOptions, new dynamic[]{ param1, param2, param3, param4, param5, param6 });
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>
    {
        private Func<RequestOptions, dynamic[], Task<TResult>> _executor;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector, ILogger scriptLogger, Func<Document, TResult> deserializer)
            : base(client, repository, id, statsCollector, scriptLogger)
        {
            _executor = (deserializer != default)
				? (Func<RequestOptions, dynamic[], Task<TResult>>)((requestOptions, parameters) => PolymorphicExecutor(deserializer, requestOptions, parameters))
				: (requestOptions, parameters) => ExecutorAsync<TResult>(requestOptions, parameters);
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, RequestOptions requestOptions)
        {
            var result = await _executor(requestOptions, new dynamic[]{ param1, param2, param3, param4, param5, param6, param7 });
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>
    {
        private Func<RequestOptions, dynamic[], Task<TResult>> _executor;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector, ILogger scriptLogger, Func<Document, TResult> deserializer)
            : base(client, repository, id, statsCollector, scriptLogger)
        {
            _executor = (deserializer != default)
				? (Func<RequestOptions, dynamic[], Task<TResult>>)((requestOptions, parameters) => PolymorphicExecutor(deserializer, requestOptions, parameters))
				: (requestOptions, parameters) => ExecutorAsync<TResult>(requestOptions, parameters);
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, RequestOptions requestOptions)
        {
            var result = await _executor(requestOptions, new dynamic[]{ param1, param2, param3, param4, param5, param6, param7, param8 });
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>
    {
        private Func<RequestOptions, dynamic[], Task<TResult>> _executor;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector, ILogger scriptLogger, Func<Document, TResult> deserializer)
            : base(client, repository, id, statsCollector, scriptLogger)
        {
            _executor = (deserializer != default)
				? (Func<RequestOptions, dynamic[], Task<TResult>>)((requestOptions, parameters) => PolymorphicExecutor(deserializer, requestOptions, parameters))
				: (requestOptions, parameters) => ExecutorAsync<TResult>(requestOptions, parameters);
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, RequestOptions requestOptions)
        {
            var result = await _executor(requestOptions, new dynamic[]{ param1, param2, param3, param4, param5, param6, param7, param8, param9 });
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult>
    {
        private Func<RequestOptions, dynamic[], Task<TResult>> _executor;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector, ILogger scriptLogger, Func<Document, TResult> deserializer)
            : base(client, repository, id, statsCollector, scriptLogger)
        {
            _executor = (deserializer != default)
				? (Func<RequestOptions, dynamic[], Task<TResult>>)((requestOptions, parameters) => PolymorphicExecutor(deserializer, requestOptions, parameters))
				: (requestOptions, parameters) => ExecutorAsync<TResult>(requestOptions, parameters);
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, RequestOptions requestOptions)
        {
            var result = await _executor(requestOptions, new dynamic[]{ param1, param2, param3, param4, param5, param6, param7, param8, param9, param10 });
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult>
    {
        private Func<RequestOptions, dynamic[], Task<TResult>> _executor;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector, ILogger scriptLogger, Func<Document, TResult> deserializer)
            : base(client, repository, id, statsCollector, scriptLogger)
        {
            _executor = (deserializer != default)
				? (Func<RequestOptions, dynamic[], Task<TResult>>)((requestOptions, parameters) => PolymorphicExecutor(deserializer, requestOptions, parameters))
				: (requestOptions, parameters) => ExecutorAsync<TResult>(requestOptions, parameters);
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, RequestOptions requestOptions)
        {
            var result = await _executor(requestOptions, new dynamic[]{ param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11 });
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult>
    {
        private Func<RequestOptions, dynamic[], Task<TResult>> _executor;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector, ILogger scriptLogger, Func<Document, TResult> deserializer)
            : base(client, repository, id, statsCollector, scriptLogger)
        {
            _executor = (deserializer != default)
				? (Func<RequestOptions, dynamic[], Task<TResult>>)((requestOptions, parameters) => PolymorphicExecutor(deserializer, requestOptions, parameters))
				: (requestOptions, parameters) => ExecutorAsync<TResult>(requestOptions, parameters);
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12, RequestOptions requestOptions)
        {
            var result = await _executor(requestOptions, new dynamic[]{ param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12 });
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult>
    {
        private Func<RequestOptions, dynamic[], Task<TResult>> _executor;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector, ILogger scriptLogger, Func<Document, TResult> deserializer)
            : base(client, repository, id, statsCollector, scriptLogger)
        {
            _executor = (deserializer != default)
				? (Func<RequestOptions, dynamic[], Task<TResult>>)((requestOptions, parameters) => PolymorphicExecutor(deserializer, requestOptions, parameters))
				: (requestOptions, parameters) => ExecutorAsync<TResult>(requestOptions, parameters);
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12, TParam13 param13, RequestOptions requestOptions)
        {
            var result = await _executor(requestOptions, new dynamic[]{ param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13 });
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult>
    {
        private Func<RequestOptions, dynamic[], Task<TResult>> _executor;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector, ILogger scriptLogger, Func<Document, TResult> deserializer)
            : base(client, repository, id, statsCollector, scriptLogger)
        {
            _executor = (deserializer != default)
				? (Func<RequestOptions, dynamic[], Task<TResult>>)((requestOptions, parameters) => PolymorphicExecutor(deserializer, requestOptions, parameters))
				: (requestOptions, parameters) => ExecutorAsync<TResult>(requestOptions, parameters);
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12, TParam13 param13, TParam14 param14, RequestOptions requestOptions)
        {
            var result = await _executor(requestOptions, new dynamic[]{ param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14 });
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult>
    {
        private Func<RequestOptions, dynamic[], Task<TResult>> _executor;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector, ILogger scriptLogger, Func<Document, TResult> deserializer)
            : base(client, repository, id, statsCollector, scriptLogger)
        {
            _executor = (deserializer != default)
				? (Func<RequestOptions, dynamic[], Task<TResult>>)((requestOptions, parameters) => PolymorphicExecutor(deserializer, requestOptions, parameters))
				: (requestOptions, parameters) => ExecutorAsync<TResult>(requestOptions, parameters);
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12, TParam13 param13, TParam14 param14, TParam15 param15, RequestOptions requestOptions)
        {
            var result = await _executor(requestOptions, new dynamic[]{ param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15 });
            return result;
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TParam16, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TParam16, TResult>
    {
        private Func<RequestOptions, dynamic[], Task<TResult>> _executor;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id, ICosmosDbQueryStatsCollector statsCollector, ILogger scriptLogger, Func<Document, TResult> deserializer)
            : base(client, repository, id, statsCollector, scriptLogger)
        {
            _executor = (deserializer != default)
				? (Func<RequestOptions, dynamic[], Task<TResult>>)((requestOptions, parameters) => PolymorphicExecutor(deserializer, requestOptions, parameters))
				: (requestOptions, parameters) => ExecutorAsync<TResult>(requestOptions, parameters);
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12, TParam13 param13, TParam14 param14, TParam15 param15, TParam16 param16, RequestOptions requestOptions)
        {
            var result = await _executor(requestOptions, new dynamic[]{ param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16 });
            return result;
        }
    }
}