using System;
using System.Linq.Expressions;

namespace Pomona.Queries
{
    public interface IQueryTypeResolver
    {
        Type Resolve(string typeName);
        Expression Resolve<T>(Expression rootInstance, string propertyPath);
    }
}