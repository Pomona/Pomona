#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;
using System.Linq;

namespace Pomona.Example.SimpleExtraSite
{
    // SAMPLE: simple-data-source
    internal class SimpleDataSource : IPomonaDataSource
    {
        private static readonly IList<SimpleExtraData> repository = new List<SimpleExtraData>()
        {
            new SimpleExtraData { Id = 0, TheString = "What" },
            new SimpleExtraData { Id = 1, TheString = "The" },
            new SimpleExtraData { Id = 2, TheString = "BLEEP" }
        };


        public object Patch<T>(T updatedObject) where T : class
        {
            var simpleData = updatedObject as SimpleExtraData;
            repository[simpleData.Id] = simpleData;
            return simpleData;
        }


        public object Post<T>(T newObject) where T : class
        {
            var simpleData = newObject as SimpleExtraData;
            simpleData.Id = repository.Count;
            repository.Add(simpleData);
            return simpleData;
        }


        public IQueryable<T> Query<T>() where T : class
        {
            return repository.Cast<T>().AsQueryable();
        }
    }
    // ENDSAMPLE
}