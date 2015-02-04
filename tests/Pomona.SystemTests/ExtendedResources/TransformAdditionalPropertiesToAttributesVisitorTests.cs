#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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

using Critters.Client;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.ExtendedResources;

namespace Pomona.SystemTests.ExtendedResources
{
    [TestFixture]
    public class TransformAdditionalPropertiesToAttributesVisitorTests : ClientTestsBase
    {
        public void AssertTransformIsCorrect<TServer, TExtended>(Func<IQueryable<TExtended>, IQueryable> origQuery,
                                                                 Func<IQueryable<TServer>, IQueryable> expectedFunc)
            where TExtended : TServer
        {
            var visitor = new TransformAdditionalPropertiesToAttributesVisitor(Client);

            var wrappedSource = Enumerable.Empty<TServer>().AsQueryable();

            ExtendedResourceInfo extendedResourceInfo;
            if (!ExtendedResourceInfo.TryGetExtendedResourceInfo(typeof(TExtended), out extendedResourceInfo))
                Assert.Fail("Unable to get ExtendedResourceInfo for " + typeof(TExtended));

            var originalQuery =
                origQuery(new ExtendedQueryableRoot<TExtended>(Client, wrappedSource, extendedResourceInfo));

            var expectedQuery = expectedFunc(wrappedSource);
            var expectedQueryExpr = expectedQuery.Expression;
            var actualQueryExpr = visitor.Visit(originalQuery.Expression);
            Assert.That(actualQueryExpr.ToString(), Is.EqualTo(expectedQueryExpr.ToString()));
        }


        [Test]
        public void Visit_TransformsPropertiesUnknownToServerToAttributeDictionaryAccess()
        {
            AssertTransformIsCorrect<IStringToObjectDictionaryContainer, ICustomUserEntity>(
                q => q.Where(x => x.CustomString == "Lalalala")
                    .Where(x => x.OtherCustom == "Blob rob")
                    .Select(x => x.OtherCustom),
                q => q.Where(x => (x.Map.SafeGet("CustomString") as string) == "Lalalala")
                    .Where(x => (x.Map.SafeGet("OtherCustom") as string) == "Blob rob")
                    .Select(x => (x.Map.SafeGet("OtherCustom") as string)));
        }


        [Test]
        public void Visit_TransformsNullableValuePropertyAccess_ToCorrectExpression()
        {
            AssertTransformIsCorrect<IStringToObjectDictionaryContainer, ICustomUserEntity>(
                q => q.Where(x => x.CustomInt.Value == 123),
                q => q.Where(x => (x.Map.SafeGet("CustomInt") as int?).Value == 123));
        }


        [Test]
        public void Visit_TransformsPropertyWithReferenceToExtendedTypeToServerKnownType()
        {
            AssertTransformIsCorrect<IMusicalCritter, IMusicalCritterOnHippieFarm>(
                q => q.Where(x => x.Farm.Name == "Wassup"),
                q => q.Where(x => x.Farm.Name == "Wassup"));
        }


        public interface ICustomUserEntity : IStringToObjectDictionaryContainer
        {
            string CustomString { get; set; }
            string OtherCustom { get; set; }
            int? CustomInt { get; set; }
        }

        public interface IHippieFarm : IFarm
        {
        }

        public interface IMusicalCritterOnHippieFarm : IMusicalCritter
        {
            new IHippieFarm Farm { get; }
        }
    }
}