#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Reflection;

namespace Pomona.Common.Linq.NonGeneric
{
    internal class QueryProjectionUsingGenericMethod : QueryProjectionMethodBase
    {
        private readonly MethodInfo methodDefinition;


        public QueryProjectionUsingGenericMethod(MethodInfo methodDefinition, string name)
        {
            this.methodDefinition = methodDefinition;
            Name = name;
        }


        public override string Name { get; }


        protected override MethodInfo GetMethod(Type elementType)
        {
            return this.methodDefinition.MakeGenericMethod(elementType);
        }
    }
}