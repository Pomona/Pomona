#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Reflection;

namespace Pomona.Common.Linq.NonGeneric
{
    internal class QueryProjectionUsingNonGenericMethod : QueryProjectionMethodBase
    {
        private readonly string name;


        public QueryProjectionUsingNonGenericMethod(string name)
        {
            this.name = name;
        }


        public override string Name
        {
            get { return this.name; }
        }


        protected override MethodInfo GetMethod(Type elementType)
        {
            var method = typeof(Queryable).GetMethod(this.name,
                                                     BindingFlags.Public | BindingFlags.Static,
                                                     null,
                                                     new Type[] { typeof(IQueryable<>).MakeGenericType(elementType) },
                                                     null);
            if (method == null)
                throw new NotSupportedException("Unable to apply " + this.name + " to " + elementType);
            return method;
        }
    }
}