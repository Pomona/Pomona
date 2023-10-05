#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq;

using Pomona.Common.Internals;

namespace Pomona
{
    public static class PomonaDataSourceExtensions
    {
        private static readonly Func<Type, IPomonaDataSource, IQueryable> queryMethodInvoker =
            GenericInvoker.Instance<IPomonaDataSource>().CreateFunc1<IQueryable>(x => x.Query<object>());

        private static readonly Func<Type, IPomonaDataSource, object, object> postMethodInvoker =
            GenericInvoker.Instance<IPomonaDataSource>().CreateFunc1<object, object>(x => x.Post<object>(null));

        private static readonly Func<Type, IPomonaDataSource, object, object> patchMethodInvoker =
            GenericInvoker.Instance<IPomonaDataSource>().CreateFunc1<object, object>(x => x.Patch<object>(null));


        public static object Patch(this IPomonaDataSource dataSource, Type type, object patchedObject)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (patchedObject == null)
                throw new ArgumentNullException(nameof(patchedObject));

            return patchMethodInvoker(type, dataSource, patchedObject);
        }


        public static object Post(this IPomonaDataSource dataSource, Type type, object form)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            return postMethodInvoker(type, dataSource, form);
        }


        public static IQueryable Query(this IPomonaDataSource dataSource, Type type)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            return queryMethodInvoker(type, dataSource);
        }
    }
}

