#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

#endregion

using System;
using System.Linq;
using System.Reflection;

using NHibernate;
using NHibernate.Linq;

using Pomona;
using Pomona.CodeGen;
using Pomona.Fetcher;

using PomonaNHibernateTest.Models;

using ReflectionHelper = Pomona.Internals.ReflectionHelper;

namespace PomonaNHibernateTest
{
    public class TestPomonaDataSource : IPomonaDataSource, IDisposable
    {
        private static readonly MethodInfo getEntityByIdMethod;
        private readonly ISession session;


        static TestPomonaDataSource()
        {
            // REMOVE CODE BELOW!!
            var testAnonTypes = Enumerable.Range(0, 4).Select(x => new { key = x, count = x }).ToList();
            Console.Write(testAnonTypes.ToString());
            AnonymousTypeBuilder.ScanAssemblyForExistingAnonymousTypes(typeof(TestPomonaDataSource).Assembly);

            getEntityByIdMethod =
                ReflectionHelper.GetMethodDefinition<TestPomonaDataSource>(x => x.GetEntityById<EntityBase>(0));
        }


        public TestPomonaDataSource(ISessionFactory sessionFactory)
        {
            this.session = sessionFactory.OpenSession();
        }

        #region Implementation of IPomonaDataSource

        public PomonaModule Module { get; set; }


        public T GetEntityById<T>(int id)
            where T : EntityBase
        {
            return LinqExtensionMethods.Query<T>(this.session).First(x => x.Id == id);
        }


        public object Patch<T>(T updatedObject) where T : class
        {
            this.session.SaveOrUpdate(updatedObject);
            return updatedObject;
        }


        public object Post<T>(T newObject) where T : class
        {
            throw new NotImplementedException();
        }


        public IQueryable<T> Query<T>() where T : class
        {
            throw new NotImplementedException();
        }


        private PomonaResponse Query<T>(PomonaQuery query)
        {
            throw new NotImplementedException();
            Console.WriteLine("ORIG FETCH START");
            var qres = query.ApplyAndExecute(LinqExtensionMethods.Query<T>(this.session));
            Console.WriteLine("ORIG FETCH STOP");
            if (!string.IsNullOrEmpty(query.ExpandedPaths))
            {
                var batchFetcher = new BatchFetcher(new NHibernateBatchFetchDriver(this.session), query.ExpandedPaths);
                batchFetcher.Expand(qres,
                    query.SelectExpression != null
                        ? query.SelectExpression.ReturnType
                        : query.OfType.MappedTypeInstance);
            }
            return qres;
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