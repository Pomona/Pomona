#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Linq.Expressions;

using Pomona.Common.Internals;

namespace Pomona.Common.Linq.NonGeneric
{
    public abstract class QueryProjection
    {
        public static readonly QueryProjection Any = Create(x => x.Any());
        public static readonly QueryProjection Count = Create(x => x.Count());
        public static readonly QueryProjection AsEnumerable = new AsEnumerableQueryProjection();
        public static readonly QueryProjection First = Create(x => x.First());
        public static readonly QueryProjection FirstOrDefault = Create(x => x.FirstOrDefault());
        public static readonly QueryProjection Last = Create(x => x.Last());
        public static readonly QueryProjection LastOrDefault = Create(x => x.LastOrDefault());
        public static readonly QueryProjection Max = Create(x => x.Max());
        public static readonly QueryProjection Min = Create(x => x.Min());
        public static readonly QueryProjection Single = Create(x => x.Single());
        public static readonly QueryProjection SingleOrDefault = Create(x => x.SingleOrDefault());
        public static readonly QueryProjection Sum = CreateNonGeneric("Sum");


        internal QueryProjection()
        {
        }


        public abstract string Name { get; }
        public abstract Expression Apply(IQueryable queryable);


        public virtual object Execute(IQueryable queryable)
        {
            if (queryable == null)
                throw new ArgumentNullException(nameof(queryable));
            return queryable.Provider.Execute(Apply(queryable));
        }


        public T Execute<T>(IQueryable queryable)
        {
            return (T)Execute(queryable);
        }


        /// <summary>
        /// Gets the resulting type given an IQueryable with specified element type.
        /// </summary>
        /// <param name="elementType">The element type</param>
        /// <returns>The type the projection would produce.</returns>
        public abstract Type GetResultType(Type elementType);


        public override string ToString()
        {
            return Name;
        }


        private static QueryProjection Create(Expression<Func<IQueryable<object>, object>> func, string name = null)
        {
            var method = func.ExtractMethodInfo();
            return
                new QueryProjectionUsingGenericMethod(method, name ?? method.Name);
        }


        private static QueryProjection CreateNonGeneric(string name)
        {
            return new QueryProjectionUsingNonGenericMethod(name);
        }
    }
}