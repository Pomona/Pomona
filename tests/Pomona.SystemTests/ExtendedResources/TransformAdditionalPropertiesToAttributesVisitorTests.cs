#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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
            var extendedResourceMapper = new ExtendedResourceMapper(Client);
            var visitor = new TransformAdditionalPropertiesToAttributesVisitor(extendedResourceMapper);

            var wrappedSource = Enumerable.Empty<TServer>().AsQueryable();

            ExtendedResourceInfo extendedResourceInfo;
            if (!extendedResourceMapper.TryGetExtendedResourceInfo(typeof(TExtended), out extendedResourceInfo))
                Assert.Fail("Unable to get ExtendedResourceInfo for " + typeof(TExtended));

            var originalQuery =
                origQuery(new ExtendedQueryableRoot<TExtended>(Client, wrappedSource, extendedResourceInfo, extendedResourceMapper));

            var expectedQuery = expectedFunc(wrappedSource);
            var expectedQueryExpr = expectedQuery.Expression;
            var actualQueryExpr = visitor.Visit(originalQuery.Expression);
            Assert.That(actualQueryExpr.ToString(), Is.EqualTo(expectedQueryExpr.ToString()));
        }


        [Test]
        public void Visit_TransformsNullableValuePropertyAccess_ToCorrectExpression()
        {
            AssertTransformIsCorrect<IStringToObjectDictionaryContainer, ICustomUserEntity>(
                q => q.Where(x => x.CustomInt.Value == 123),
                q => q.Where(x => (x.Map.SafeGet("CustomInt") as int?).Value == 123));
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
        public void Visit_TransformsPropertyWithReferenceToExtendedTypeToServerKnownType()
        {
            AssertTransformIsCorrect<IMusicalCritter, IMusicalCritterOnHippieFarm>(
                q => q.Where(x => x.Farm.Name == "Wassup"),
                q => q.Where(x => x.Farm.Name == "Wassup"));
        }


        public interface ICustomUserEntity : IStringToObjectDictionaryContainer
        {
            int? CustomInt { get; set; }
            string CustomString { get; set; }
            string OtherCustom { get; set; }
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