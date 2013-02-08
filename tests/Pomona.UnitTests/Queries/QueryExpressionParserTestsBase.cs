using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Pomona.Queries;

namespace Pomona.UnitTests.Queries
{
    public class QueryExpressionParserTestsBase
    {
        protected QueryExpressionParser parser;

        [SetUp]
        public void SetUp()
        {
            parser = new QueryExpressionParser(new SimpleQueryPropertyResolver());
        }

        public class SimpleQueryPropertyResolver : IQueryTypeResolver
        {
            #region IQueryPropertyResolver Members

            public Expression ResolveProperty(Expression rootInstance, string propertyPath)
            {
                return Expression.Property(
                    rootInstance,
                    rootInstance.Type.GetProperties().First(x => x.Name.ToLower() == propertyPath.ToLower()));
            }


            public Type ResolveType(string typeName)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        public class Dummy
        {
            public bool and { get; set; }
            public IDictionary<string, string> Attributes { get; set; }
            public IEnumerable<Dummy> Children { get; set; }
            public Dummy Friend { get; set; }
            public Guid Guid { get; set; }
            public int Number { get; set; }
            public Dummy Parent { get; set; }
            public IEnumerable<string> SomeStrings { get; set; }
            public string Text { get; set; }
            public DateTime Time { get; set; }
        }
    }
}