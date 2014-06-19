using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Pomona.Example.SimpleExtraSite
{
    class SimpleDataSource : IPomonaDataSource
    {
        private static IList<SimpleExtraData> repository = new List<SimpleExtraData>()
                {
                    new SimpleExtraData() {Id=0,TheString = "What"},
                    new SimpleExtraData() {Id=1,TheString = "The"},
                    new SimpleExtraData() {Id=2,TheString = "BLEEP"}
                };

        public IQueryable<T> Query<T>() where T : class
        {
            return repository.Cast<T>().AsQueryable();
        }
        
        public object Post<T>(T newObject) where T : class
        {
            var simpleData = newObject as SimpleExtraData;
            simpleData.Id = repository.Count;
            repository.Add(simpleData);
            return simpleData;
        }

        public object Patch<T>(T updatedObject) where T : class
        {
            var simpleData = updatedObject as SimpleExtraData;
            repository[simpleData.Id]=simpleData;
            return simpleData;
        }
    }
}
