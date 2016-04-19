#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;

using NUnit.Framework;

using Pomona.SystemTests;

namespace CritterClientTests.ConsoleAppRunner
{
    /// <summary>
    /// This is for testing performance
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            var testRunCount = 1000;

            var critterTests = new CritterTests();

            critterTests.FixtureSetUp();

            var tests = typeof(CritterTests).GetMethods().Select(
                x => new
                {
                    TestInfo =
                    x.GetCustomAttributes(typeof(TestAttribute), false).OfType<TestAttribute>().FirstOrDefault(),
                    Method = x
                }).Where(x => x.TestInfo != null).ToList();

            for (var i = 0; i < testRunCount; i++)
            {
                foreach (var test in tests)
                {
                    Console.WriteLine("Running test " + test.Method.Name + "(" + test.TestInfo.Description + ")");
                    try
                    {
                        critterTests.SetUp();
                        test.Method.Invoke(critterTests, null);
                    }
                    catch (AssertionException assertionException)
                    {
                        Console.WriteLine("Test failed: " + assertionException);
                    }
                }
            }

            critterTests.FixtureTearDown();
        }
    }
}