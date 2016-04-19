#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;

using Nancy.Validation;

using Pomona.Example.Models;
using Pomona.Queries;

namespace Pomona.Example
{
    public class CritterDataSource : IPomonaDataSource, IQueryExecutor
    {
        private readonly CritterRepository store;


        public CritterDataSource(CritterRepository store)
        {
            this.store = store;
        }


        public PomonaResponse ApplyAndExecute(IQueryable queryable, PomonaQuery query)
        {
            return this.store.ApplyAndExecute(queryable, query);
        }


        public object Patch<T>(T updatedObject) where T : class
        {
            return this.store.Patch(updatedObject);
        }


        public object Post<T>(T newObject) where T : class
        {
            if (typeof(T) == typeof(FailingThing))
                throw new Exception("Stupid exception from failing thing;");
            if (typeof(T) == typeof(HandledThing) || typeof(T) == typeof(HandledChild))
            {
                throw new InvalidOperationException(
                    "HandledThing and HandledChild should not be handled by DataSource because they have custom handlers.");
            }

            var newCritter = newObject as Critter;
            if (newCritter != null && newCritter.Name != null && newCritter.Name.Length > 50)
                throw new ModelValidationException("Critter can't have name longer than 50 characters.");

            return this.store.Post(newObject);
        }


        public IQueryable<T> Query<T>()
            where T : class
        {
            if (typeof(T) == typeof(HandledThing))
                throw new InvalidOperationException("Error: Should not call data source when querying HandledThing.");
            return this.store.Query<T>();
        }
    }
}