using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CosmosDbRepository.Implementation
{
    internal static class ObjectExtensions
    {
        private static readonly ConcurrentDictionary<Type, Func<object, object>> _copiers = new ConcurrentDictionary<Type, Func<object, object>>();

        public static T ShallowCopy<T>(this T source)
             where T : class, new()
        {
            Func<object, object> Factory(Type type)
            {
                var properties = type.GetProperties().Where(i => i.CanRead && i.CanWrite).Cast<MemberInfo>()
                    .Concat(type.GetFields().Where(i => !i.IsInitOnly && !i.IsLiteral));

                var o = Expression.Parameter(typeof(object), "o");
                var src = Expression.Variable(type, "src");

                var body = Expression.Block(new[] { src },
                            Expression.Assign(src, Expression.Convert(o, type)),
                            Expression.MemberInit(Expression.New(type), properties.Select(i => Expression.Bind(i, Expression.PropertyOrField(src, i.Name)))));

                return Expression.Lambda<Func<object, object>>(body, o).Compile();
            }

            return source != default(T)
                ? (T)_copiers.GetOrAdd(source.GetType(), Factory)(source)
                : default(T);
        }
    }

}
