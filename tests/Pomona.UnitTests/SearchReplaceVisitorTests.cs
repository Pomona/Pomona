#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using NUnit.Framework;

using Pomona.Common.Expressions;
using Pomona.TestHelpers;

namespace Pomona.UnitTests
{
    [TestFixture]
    public class SearchReplaceVisitorTests
    {
        public void AssertVisitor<T>(SearchReplaceVisitor visitor,
                                     Expression<Func<T, object>> searchIn,
                                     Expression<Func<T, object>> expected)
        {
            var actual = visitor.Visit(searchIn);
            Console.WriteLine("SearchIn: {0}\r\nExpected:{1}\r\nActual:{2}", searchIn, expected, actual);
            actual.AssertEquals(expected);
        }


        [Test]
        public void Match_WithBinaryExpression_MatchesAndReplacesExpression()
        {
            var visitor =
                SearchReplaceVisitor
                    .Create()
                    .For<int>()
                    .Replace(x => x * 2, x => x << 1)
                    .Build();

            AssertVisitor<Customer>(visitor,
                                    x => x.GetHashCode() * 2,
                                    x => x.GetHashCode() << 1);
        }


        [Test]
        public void Match_WithConditionalExpression_MatchesAndReplacesExpression()
        {
            // ReSharper disable ConvertConditionalTernaryToNullCoalescing
            var visitor =
                SearchReplaceVisitor
                    .Create()
                    .For<string>()
                    .Replace(x => (x != null ? x : "(nothing)"), x => x ?? "(nothing)")
                    .Build();

            AssertVisitor<Customer>(visitor,
                                    c => (c.PrimaryPerson.FullName != null ? c.PrimaryPerson.FullName : "(nothing)"),
                                    c => c.PrimaryPerson.FullName ?? "(nothing)");
            // ReSharper restore ConvertConditionalTernaryToNullCoalescing
        }


        [Test]
        public void Match_WithNewArrayExpression_MatchesAndReplacesExpression()
        {
            var visitor =
                SearchReplaceVisitor
                    .Create()
                    .For<object>()
                    .Replace(x => (IEnumerable<int>)(new int[] { 0, 1, 2, 3, 4, 5 }), x => Enumerable.Range(0, 6))
                    .Build();

            AssertVisitor<Customer>(visitor,
                                    x => (IEnumerable<int>)(new int[] { 0, 1, 2, 3, 4, 5 }),
                                    x => Enumerable.Range(0, 6));
        }


        [Test]
        public void Match_WithNewExpressionAndCaptureGroup_MatchesAndReplacesExpression()
        {
            var visitor =
                SearchReplaceVisitor
                    .Create()
                    .For<string>()
                    .Replace((x, c) => new Guid(c.Group<string>()), (x, c) => Guid.Parse(c.Group<string>()))
                    .Build();

            AssertVisitor<Customer>(visitor,
                                    x => new Guid("93f37cd0-43a1-41e6-b2b8-35d131f39a49"),
                                    x => Guid.Parse("93f37cd0-43a1-41e6-b2b8-35d131f39a49"));
        }


        [Test]
        public void Match_WithNoCaptureGroup_MatchesAndReplacesExpression()
        {
            var visitor =
                SearchReplaceVisitor
                    .Create()
                    .For<Customer>()
                    .Replace(
                        x => x.PrimaryPerson.FullName,
                        x => x.Contacts
                              .Where(y => y.IsPrimary)
                              .Select(z => z.FirstName + " " + z.LastName)
                              .First()
                    )
                    .Build();

            AssertVisitor<Customer>(visitor,
                                    x => x.PrimaryPerson.FullName,
                                    x => x.Contacts
                                          .Where(y => y.IsPrimary)
                                          .Select(z => z.FirstName + " " + z.LastName)
                                          .First());

            AssertVisitor<Company>(visitor,
                                   x => x.Customers.Aggregate(string.Empty, (a, b) => a + "\r\n" + b.PrimaryPerson.FullName),
                                   x => x.Customers.Aggregate(string.Empty,
                                                              (a, b) => a + "\r\n" + b.Contacts
                                                                                      .Where(y => y.IsPrimary)
                                                                                      .Select(z => z.FirstName + " " + z.LastName)
                                                                                      .First()));
        }


        [Test]
        public void Match_WithUnaryExpression_MatchesAndReplacesExpression()
        {
            var visitor =
                SearchReplaceVisitor
                    .Create()
                    .For<int>()
                    .Replace(x => ~x, x => x ^ -1) // Unoptimizer!
                    .Build();

            AssertVisitor<Customer>(visitor,
                                    x => ~x.Contacts.Count,
                                    x => x.Contacts.Count ^ -1);
        }


        public class Address
        {
            public string Street { get; set; }
        }

        public class Company
        {
            public IList<Customer> Customers { get; set; }
        }

        public class Contact
        {
            public string FirstName { get; set; }
            public bool IsPrimary { get; set; }
            public string LastName { get; set; }
            public string Street { get; set; }
        }

        public class Customer
        {
            public IList<Contact> Contacts { get; set; }
            public Address PrimaryAddress { get; set; }
            public Person PrimaryPerson { get; set; }
        }

        public class Person
        {
            public string FullName { get; set; }
        }
    }
}

