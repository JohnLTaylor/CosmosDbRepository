using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace CosmosDbRepository.Implementation
{
    internal abstract class StoreProcedureImpl
    {
        protected readonly IDocumentClient Client;
        protected readonly AsyncLazy<Uri> StoredProcUri;

        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id)
        {
            Client = client;
            StoredProcUri = new AsyncLazy<Uri>(() => GetStoredProcUri(repository, id));
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
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id)
            : base(client, repository, id)
        {
        }

        public async Task<TResult> ExecuteAsync()
        {
            var result = await Client.ExecuteStoredProcedureAsync<string>(await StoredProcUri.Value);
            return JsonConvert.DeserializeObject<TResult>(result);
        }
    }

    internal class StoreProcedureImpl<TParam, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id)
            : base(client, repository, id)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam param)
        {
            var result = await Client.ExecuteStoredProcedureAsync<string>(await StoredProcUri.Value, param);
            return JsonConvert.DeserializeObject<TResult>(result);
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id)
            : base(client, repository, id)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2)
        {
            var result = await Client.ExecuteStoredProcedureAsync<string>(await StoredProcUri.Value, param1, param2);
            return JsonConvert.DeserializeObject<TResult>(result);
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id)
            : base(client, repository, id)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3)
        {
            var result = await Client.ExecuteStoredProcedureAsync<string>(await StoredProcUri.Value, param1, param2, param3);
            return JsonConvert.DeserializeObject<TResult>(result);
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id)
            : base(client, repository, id)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4)
        {
            var result = await Client.ExecuteStoredProcedureAsync<string>(await StoredProcUri.Value, param1, param2, param3, param4);
            return JsonConvert.DeserializeObject<TResult>(result);
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id)
            : base(client, repository, id)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5)
        {
            var result = await Client.ExecuteStoredProcedureAsync<string>(await StoredProcUri.Value, param1, param2, param3, param4, param5);
            return JsonConvert.DeserializeObject<TResult>(result);
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id)
            : base(client, repository, id)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6)
        {
            var result = await Client.ExecuteStoredProcedureAsync<string>(await StoredProcUri.Value, param1, param2, param3, param4, param5, param6);
            return JsonConvert.DeserializeObject<TResult>(result);
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id)
            : base(client, repository, id)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7)
        {
            var result = await Client.ExecuteStoredProcedureAsync<string>(await StoredProcUri.Value, param1, param2, param3, param4, param5, param6, param7);
            return JsonConvert.DeserializeObject<TResult>(result);
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id)
            : base(client, repository, id)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8)
        {
            var result = await Client.ExecuteStoredProcedureAsync<string>(await StoredProcUri.Value, param1, param2, param3, param4, param5, param6, param7, param8);
            return JsonConvert.DeserializeObject<TResult>(result);
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id)
            : base(client, repository, id)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9)
        {
            var result = await Client.ExecuteStoredProcedureAsync<string>(await StoredProcUri.Value, param1, param2, param3, param4, param5, param6, param7, param8, param9);
            return JsonConvert.DeserializeObject<TResult>(result);
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id)
            : base(client, repository, id)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10)
        {
            var result = await Client.ExecuteStoredProcedureAsync<string>(await StoredProcUri.Value, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10);
            return JsonConvert.DeserializeObject<TResult>(result);
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id)
            : base(client, repository, id)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11)
        {
            var result = await Client.ExecuteStoredProcedureAsync<string>(await StoredProcUri.Value, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11);
            return JsonConvert.DeserializeObject<TResult>(result);
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id)
            : base(client, repository, id)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12)
        {
            var result = await Client.ExecuteStoredProcedureAsync<string>(await StoredProcUri.Value, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12);
            return JsonConvert.DeserializeObject<TResult>(result);
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id)
            : base(client, repository, id)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12, TParam13 param13)
        {
            var result = await Client.ExecuteStoredProcedureAsync<string>(await StoredProcUri.Value, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13);
            return JsonConvert.DeserializeObject<TResult>(result);
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id)
            : base(client, repository, id)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12, TParam13 param13, TParam14 param14)
        {
            var result = await Client.ExecuteStoredProcedureAsync<string>(await StoredProcUri.Value, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14);
            return JsonConvert.DeserializeObject<TResult>(result);
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id)
            : base(client, repository, id)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12, TParam13 param13, TParam14 param14, TParam15 param15)
        {
            var result = await Client.ExecuteStoredProcedureAsync<string>(await StoredProcUri.Value, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15);
            return JsonConvert.DeserializeObject<TResult>(result);
        }
    }

    internal class StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TParam16, TResult>
        : StoreProcedureImpl
        , IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TParam16, TResult>
    {
        public StoreProcedureImpl(IDocumentClient client, ICosmosDbRepository repository, string id)
            : base(client, repository, id)
        {
        }

        public async Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12, TParam13 param13, TParam14 param14, TParam15 param15, TParam16 param16)
        {
            var result = await Client.ExecuteStoredProcedureAsync<string>(await StoredProcUri.Value, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16);
            return JsonConvert.DeserializeObject<TResult>(result);
        }
    }
}