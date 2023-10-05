#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Nancy;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;
using Pomona.Example.Models;
using Pomona.Example.Models.Existence;
using Pomona.RequestProcessing;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class HandlerMethodTests : ClientTestsBase
    {
        public NancyContext nancyContext;
        private ClientSerializationContextProvider serializationContextProvider;
        public ITextSerializerFactory serializerFactory;


        public IQueryable<Critter> GetCrayons()
        {
            return new List<Critter>().AsQueryable();
        }


        public IQueryable<Critter> GetCritters()
        {
            return new List<Critter>().AsQueryable(); // Why not?
        }


        public IQueryable<PlanetarySystem> GetPlanetarySystems(Galaxy theGalaxy)
        {
            if (theGalaxy != null)
                return new List<PlanetarySystem>().AsQueryable();
            else
                throw new PomonaServerException("A test called GetPlanetarySystems in HandlerMethodTests.cs without a Galaxy.");
        }


        [OneTimeSetUp]
        public void Init()
        {
            this.nancyContext = new NancyContext();
            this.nancyContext.Request = new Request("Get", "http://test");
            this.serializationContextProvider =
                new ClientSerializationContextProvider(new ClientTypeMapper(Assembly.GetExecutingAssembly()), Client, Client);
            this.serializerFactory = new PomonaJsonSerializerFactory();
        }


        [Test]
        public void Match_ChildResource_Requires_ParentResource()
        {
            var methodObject = new HandlerMethod(typeof(HandlerMethodTests).GetMethod("QueryPlanetarySystems"),
                                                 TypeMapper);
            Assert.That(
                methodObject.Match(HttpMethod.Get,
                                   PathNodeType.Collection,
                                   TypeMapper.FromType(typeof(PlanetarySystem))),
                Is.Null);
        }


        [Test]
        public void Match_ChildResource_Takes_ParentResource()
        {
            var methodObject = new HandlerMethod(typeof(HandlerMethodTests).GetMethod("GetPlanetarySystems"),
                                                 TypeMapper);
            Assert.That(
                methodObject.Match(HttpMethod.Get,
                                   PathNodeType.Collection,
                                   TypeMapper.FromType(typeof(PlanetarySystem))),
                Is.Not.Null);
        }


        [Test]
        public void Match_QueryMethod_Does_Not_Match_IncorrectMethodName()
        {
            var methodObject = new HandlerMethod(typeof(HandlerMethodTests).GetMethod("GetCrayons"), TypeMapper);
            Assert.That(
                methodObject.Match(HttpMethod.Get,
                                   PathNodeType.Collection,
                                   TypeMapper.FromType(typeof(Critter))),
                Is.Null);
        }


        [Test]
        public void Match_QueryMethod_Does_Not_Match_IncorrectSignature()
        {
            var methodObject = new HandlerMethod(typeof(HandlerMethodTests).GetMethod("GetCritters"), TypeMapper);
            Assert.That(
                methodObject.Match(HttpMethod.Get,
                                   PathNodeType.Collection,
                                   TypeMapper.FromType(typeof(MusicalCritter))),
                Is.Null);
        }


        [Test]
        public void Match_QueryMethod_PrefixedWithGet_Matches_CorrectSignature()
        {
            var methodObject = new HandlerMethod(typeof(HandlerMethodTests).GetMethod("GetCritters"), TypeMapper);
            Assert.That(
                methodObject.Match(HttpMethod.Get,
                                   PathNodeType.Collection,
                                   TypeMapper.FromType(typeof(Critter))),
                Is.Not.Null);
        }


        [Test]
        public void Match_QueryMethod_PrefixedWithQuery_Matches_CorrectSignature()
        {
            var methodObject = new HandlerMethod(typeof(HandlerMethodTests).GetMethod("QueryCritters"), TypeMapper);
            Assert.That(
                methodObject.Match(HttpMethod.Get,
                                   PathNodeType.Collection,
                                   TypeMapper.FromType(typeof(Critter))),
                Is.Not.Null);
        }


        public IQueryable<Critter> QueryCritters()
        {
            return new List<Critter>().AsQueryable(); // Why not?
        }


        public IQueryable<PlanetarySystem> QueryPlanetarySystems(
            /* This deliberately doesn't take a Galaxy although it should */)
        {
            return new List<PlanetarySystem>().AsQueryable();
        }
    }
}

