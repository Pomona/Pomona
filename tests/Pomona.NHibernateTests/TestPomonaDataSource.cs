using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NHibernate;
using NHibernate.Linq;
using Pomona;
using Pomona.CodeGen;
using Pomona.Common;
using Pomona.Fetcher;
using PomonaNHibernateTest.Models;
using ReflectionHelper = Pomona.Internals.ReflectionHelper;

namespace PomonaNHibernateTest
{
    public class TestPomonaDataSource : IPomonaDataSource, IDisposable
    {
        private static readonly MethodInfo queryMethod;
        private static readonly MethodInfo getEntityByIdMethod;
        private readonly ISession session;


        static TestPomonaDataSource()
        {
            // REMOVE CODE BELOW!!
            var testAnonTypes = Enumerable.Range(0, 4).Select(x => new {key = x, count = x}).ToList();
            Console.Write(testAnonTypes.ToString());
            AnonymousTypeBuilder.ScanAssemblyForExistingAnonymousTypes(typeof(TestPomonaDataSource).Assembly);

            getEntityByIdMethod =
                ReflectionHelper.GetMethodDefinition<TestPomonaDataSource>(x => x.GetEntityById<EntityBase>(0));
            queryMethod = ReflectionHelper.GetMethodDefinition<TestPomonaDataSource>(x => x.Query<object>(null));
        }

        public TestPomonaDataSource(ISessionFactory sessionFactory)
        {
            session = sessionFactory.OpenSession();
        }

        #region Implementation of IPomonaDataSource

        public T GetById<T>(object id)
        {
            return
                (T) getEntityByIdMethod.MakeGenericMethod(typeof (T)).Invoke(this, new object[] {Convert.ToInt32(id)});
        }


        public PomonaResponse Query(PomonaQuery query)
        {
            return
                (PomonaResponse)
                queryMethod.MakeGenericMethod(query.TargetType.MappedTypeInstance).Invoke(this, new object[] {query});
        }


        public object Post<T>(T newObject)
        {
            throw new NotImplementedException();
        }

        public object Patch<T>(T updatedObject)
        {
            session.SaveOrUpdate(updatedObject);
            return updatedObject;
        }

        public T GetEntityById<T>(int id)
            where T : EntityBase
        {
            return session.Query<T>().First(x => x.Id == id);
        }

        private PomonaResponse Query<T>(PomonaQuery query)
        {
            Console.WriteLine("ORIG FETCH START");
            var qres = query.ApplyAndExecute(session.Query<T>());
            Console.WriteLine("ORIG FETCH STOP");
            if (!string.IsNullOrEmpty(query.ExpandedPaths))
            {
                var batchFetcher = new BatchFetcher(new NHibernateBatchFetchDriver(session), query.ExpandedPaths);
                batchFetcher.Expand(qres, query.SelectExpression != null ? query.SelectExpression.ReturnType : query.TargetType.MappedTypeInstance);
            }
            return qres;
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            session.Dispose();
        }

        #endregion
    }
}