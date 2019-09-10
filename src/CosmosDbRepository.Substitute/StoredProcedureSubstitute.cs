using System;
using System.Threading.Tasks;

namespace CosmosDbRepository.Substitute
{
    public class StoredProcedureSubstitute<TResult>
        : IStoredProcedure<TResult>
    {
        private readonly Func<object[], object> _func;

        public StoredProcedureSubstitute(Func<object[], object> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public Task<TResult> ExecuteAsync()
            => Task.FromResult((TResult)_func(Array.Empty<object>()));
    }

    public class StoredProcedureSubstitute<TParam, TResult>
        : IStoredProcedure<TParam, TResult>
    {
        private readonly Func<object[], object> _func;

        public StoredProcedureSubstitute(Func<object[], object> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public Task<TResult> ExecuteAsync(TParam param)
            => Task.FromResult((TResult)_func(new object[] { param }));
    }

    public class StoredProcedureSubstitute<TParam1, TParam2, TResult>
        : IStoredProcedure<TParam1, TParam2, TResult>
    {
        private readonly Func<object[], object> _func;

        public StoredProcedureSubstitute(Func<object[], object> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2)
            => Task.FromResult((TResult)_func(new object[] { param1, param2 }));
    }

    public class StoredProcedureSubstitute<TParam1, TParam2, TParam3, TResult>
        : IStoredProcedure<TParam1, TParam2, TParam3, TResult>
    {
        private readonly Func<object[], object> _func;

        public StoredProcedureSubstitute(Func<object[], object> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3)
            => Task.FromResult((TResult)_func(new object[] { param1, param2, param3 }));
    }

    public class StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TResult>
        : IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TResult>
    {
        private readonly Func<object[], object> _func;

        public StoredProcedureSubstitute(Func<object[], object> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4)
            => Task.FromResult((TResult)_func(new object[] { param1, param2, param3, param4 }));
    }

    public class StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>
        : IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>
    {
        private readonly Func<object[], object> _func;

        public StoredProcedureSubstitute(Func<object[], object> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5)
            => Task.FromResult((TResult)_func(new object[] { param1, param2, param3, param4, param5 }));
    }

    public class StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>
        : IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>
    {
        private readonly Func<object[], object> _func;

        public StoredProcedureSubstitute(Func<object[], object> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6)
            => Task.FromResult((TResult)_func(new object[] { param1, param2, param3, param4, param5, param6 }));
    }

    public class StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>
        : IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>
    {
        private readonly Func<object[], object> _func;

        public StoredProcedureSubstitute(Func<object[], object> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7)
            => Task.FromResult((TResult)_func(new object[] { param1, param2, param3, param4, param5, param6, param7 }));
    }

    public class StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>
        : IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>
    {
        private readonly Func<object[], object> _func;

        public StoredProcedureSubstitute(Func<object[], object> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8)
            => Task.FromResult((TResult)_func(new object[] { param1, param2, param3, param4, param5, param6, param7, param8 }));
    }

    public class StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>
        : IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>
    {
        private readonly Func<object[], object> _func;

        public StoredProcedureSubstitute(Func<object[], object> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9)
            => Task.FromResult((TResult)_func(new object[] { param1, param2, param3, param4, param5, param6, param7, param8, param9 }));
    }

    public class StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult>
        : IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult>
    {
        private readonly Func<object[], object> _func;

        public StoredProcedureSubstitute(Func<object[], object> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10)
            => Task.FromResult((TResult)_func(new object[] { param1, param2, param3, param4, param5, param6, param7, param8, param9, param10 }));
    }

    public class StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult>
        : IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult>
    {
        private readonly Func<object[], object> _func;

        public StoredProcedureSubstitute(Func<object[], object> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11)
            => Task.FromResult((TResult)_func(new object[] { param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11 }));
    }

    public class StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult>
        : IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult>
    {
        private readonly Func<object[], object> _func;

        public StoredProcedureSubstitute(Func<object[], object> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12)
            => Task.FromResult((TResult)_func(new object[] { param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12 }));
    }

    public class StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult>
        : IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult>
    {
        private readonly Func<object[], object> _func;

        public StoredProcedureSubstitute(Func<object[], object> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }
        public Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12, TParam13 param13)
            => Task.FromResult((TResult)_func(new object[] { param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13 }));

    }

    public class StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult>
        : IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult>
    {
        private readonly Func<object[], object> _func;

        public StoredProcedureSubstitute(Func<object[], object> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12, TParam13 param13, TParam14 param14)
            => Task.FromResult((TResult)_func(new object[] { param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14 }));
    }

    public class StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult>
        : IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult>
    {
        private readonly Func<object[], object> _func;

        public StoredProcedureSubstitute(Func<object[], object> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12, TParam13 param13, TParam14 param14, TParam15 param15)
            => Task.FromResult((TResult)_func(new object[] { param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15 }));
    }

    public class StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TParam16, TResult>
        : IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TParam16, TResult>
    {
        private readonly Func<object[], object> _func;

        public StoredProcedureSubstitute(Func<object[], object> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public Task<TResult> ExecuteAsync(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9, TParam10 param10, TParam11 param11, TParam12 param12, TParam13 param13, TParam14 param14, TParam15 param15, TParam16 param16)
            => Task.FromResult((TResult)_func(new object[] { param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16 }));
    }
}
