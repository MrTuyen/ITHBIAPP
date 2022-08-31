using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace ProductionApp.Helpers
{
    public static class IQueryableExtension
    {
        public static bool IsOrdered<T>(this IQueryable<T> queryable)
        {
            if (queryable == null)
            {
                throw new ArgumentNullException("queryable");
            }

            return queryable.Expression.Type == typeof(IOrderedQueryable<T>);
        }

        public static IQueryable<T> SmartOrderBy<T, TKey>(this IQueryable<T> queryable, Expression<Func<T, TKey>> keySelector)
        {
            if (queryable.IsOrdered())
            {
                var orderedQuery = queryable as IOrderedQueryable<T>;
                return orderedQuery.ThenBy(keySelector);
            }
            else
            {
                return queryable.OrderBy(keySelector);
            }
        }

        public static IQueryable<T> SmartOrderByDescending<T, TKey>(this IQueryable<T> queryable, Expression<Func<T, TKey>> keySelector)
        {
            if (queryable.IsOrdered())
            {
                var orderedQuery = queryable as IOrderedQueryable<T>;
                return orderedQuery.ThenByDescending(keySelector);
            }
            else
            {
                return queryable.OrderByDescending(keySelector);
            }
        }

        public static void AddRange<T>(this ConcurrentBag<T> @this, IEnumerable<T> toAdd)
        {
            foreach (var element in toAdd)
            {
                @this.Add(element);
            }
        }
    }
}