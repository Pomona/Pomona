using System;
using System.Linq;
using System.Reflection;

using NHibernate;
using NHibernate.Linq;

using Pomona;
using Pomona.Common;

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
            getEntityByIdMethod =
                ReflectionHelper.GetGenericMethodDefinition<TestPomonaDataSource>(x => x.GetEntityById<EntityBase>(0));
            queryMethod = ReflectionHelper.GetGenericMethodDefinition<TestPomonaDataSource>(x => x.Query<object>(null));
        }

        public TestPomonaDataSource(ISessionFactory sessionFactory)
        {
            this.session = sessionFactory.OpenSession();
        }

        #region Implementation of IPomonaDataSource

        public T GetById<T>(object id)
        {
            return (T) getEntityByIdMethod.MakeGenericMethod(typeof (T)).Invoke(this, new object[] {Convert.ToInt32(id)});
        }


        public QueryResult Query(IPomonaQuery query)
        {
            return
                (QueryResult)
                queryMethod.MakeGenericMethod(query.TargetType.MappedTypeInstance).Invoke(this, new object[] {query});
        }


        public object Post<T>(T newObject)
        {
            throw new NotImplementedException();
        }

        public T GetEntityById<T>(int id)
            where T : EntityBase
        {
            return LinqExtensionMethods.Query<T>(this.session).First(x => x.Id == id);
        }

        private QueryResult Query<T>(IPomonaQuery query)
        {
            var pq = (PomonaQuery) query;
            return pq.ApplyAndExecute(LinqExtensionMethods.Query<T>(this.session));
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            this.session.Dispose();
        }

        #endregion
    }
}