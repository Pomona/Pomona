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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Persister.Collection;
using NHibernate.Type;
using Pomona.Fetcher;

namespace PomonaNHibernateTest
{
    public class NHibernateBatchFetchDriver : IBatchFetchDriver
    {
        private readonly ISession session;
        private readonly ISessionFactory sessionFactory;

        public NHibernateBatchFetchDriver(ISession session)
        {
            sessionFactory = session.SessionFactory;
            this.session = session;
        }

        public ISession Session
        {
            get { return session; }
        }

        public void PopulateCollections<TParentEntity, TCollectionElement>(
            IEnumerable<KeyValuePair<TParentEntity, IEnumerable<TCollectionElement>>> bindings, PropertyInfo property,
            Type elementType)
        {
            var cm = sessionFactory.GetClassMetadata(property.DeclaringType);
            var collectionType = cm.GetPropertyType(property.Name) as CollectionType;
            if (collectionType == null)
                throw new InvalidOperationException(
                    "Unable to recognize collection property as NHibernate collection type.");

            var collectionPersister =
                sessionFactory.GetCollectionMetadata(collectionType.Role) as AbstractCollectionPersister;

            if (collectionPersister == null)
                throw new InvalidOperationException("Unable to get persister for collection.");

            foreach (var kvp in bindings)
            {
                var wrapper = collectionPersister.CollectionType.Wrap(session.GetSessionImplementation(),
                                                                      kvp.Value.ToList());
                property.SetValue(kvp.Key, wrapper, null);
            }
        }

        public bool IsManyToOne(PropertyInfo prop)
        {
            var cm = sessionFactory.GetClassMetadata(prop.DeclaringType);
            return cm.GetPropertyType(prop.Name) is ManyToOneType;
        }

        public IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            return type.GetProperties();
        }

        public bool PathIsExpanded(string path, PropertyInfo property)
        {
            if (path == "Father")
                return true;
            if (path == "Children")
                return true;

            return false;
        }

        public PropertyInfo GetIdProperty(Type type)
        {
            var cm = sessionFactory.GetClassMetadata(type);
            if (!cm.HasIdentifierProperty)
                throw new InvalidOperationException("Could not find nhibernate-bound identifier property for type");
            var propIdName = cm.IdentifierPropertyName;

            return type.GetProperty(propIdName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        public bool IsLoaded(object obj)
        {
            return false;
        }

        public IQueryable<TEntity> Query<TEntity>()
        {
            return session.Query<TEntity>();
        }
    }
}