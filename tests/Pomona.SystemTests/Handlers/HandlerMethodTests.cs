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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nancy;
using NUnit.Framework;
using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;
using Pomona.Common.TypeSystem;
using Pomona.Example.Models;
using Pomona.Example.Models.Existence;
using Pomona.RequestProcessing;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class HandlerMethodTests : ClientTestsBase
    {
        public IQueryable<Critter> GetCritters()
        {
            return new List<Critter>().AsQueryable(); // Why not?
        }

        public IQueryable<Critter> GetCrayons()
        {
            return new List<Critter>().AsQueryable();
        }

        public IQueryable<PlanetarySystem> GetPlanetarySystems(Galaxy theGalaxy)
        {
            if (theGalaxy != null)
                return new List<PlanetarySystem>().AsQueryable();
            else
                throw new PomonaException("A test called GetPlanetarySystems in HandlerMethodTests.cs without a Galaxy.");
        }

        public IQueryable<PlanetarySystem> QueryPlanetarySystems(
            /* This deliberately doesn't take a Galaxy although it should */)
        {
            return new List<PlanetarySystem>().AsQueryable();
        }

        public NancyContext nancyContext;
        public ITextSerializerFactory serializerFactory;

        [TestFixtureSetUp]
        public void Init()
        {
            nancyContext = new NancyContext();
            nancyContext.Request = new Request("Get", "http://test");
            serializerFactory = new PomonaJsonSerializerFactory(
                new ClientSerializationContextProvider(new ClientTypeMapper(Assembly.GetExecutingAssembly()), Client));
        }

        [Test]
        public void Invoke_Method_Handles_ParentResource()
        {
            var methodObject = new HandlerMethod(typeof (HandlerMethodTests).GetMethod("GetPlanetarySystems"),
                TypeMapper);

            var parentNode = new ResourceNode(TypeMapper, null, "Test", delegate() { return new Galaxy(); },
                TypeMapper.FromType(typeof (Galaxy)) as ResourceType);
            var pathNode = new ResourceNode(TypeMapper, parentNode, "Test",
                delegate() { return null; }, TypeMapper.FromType(typeof (PlanetarySystem)) as ResourceType);

            var pomonaRequest = new PomonaRequest(pathNode, nancyContext,
                serializerFactory);

            Assert.DoesNotThrow(() => methodObject.Invoke(this, pomonaRequest));
        }

        [Test]
        public void Invoke_Method_Requires_ParentResource()
        {
            var methodObject = new HandlerMethod(typeof (HandlerMethodTests).GetMethod("QueryPlanetarySystems"),
                TypeMapper);

            var pathNode = new ResourceNode(TypeMapper, null, "Test",
                delegate() { return null; }, TypeMapper.FromType(typeof (PlanetarySystem)) as ResourceType);

            var pomonaRequest = new PomonaRequest(pathNode, nancyContext,
                serializerFactory);

            var e = Assert.Throws<PomonaException>(() => methodObject.Invoke(this, pomonaRequest));
            Assert.AreEqual(
                "Type PlanetarySystem has the parent resource type Galaxy, but no parent element was specified.",
                e.Message);
        }

        [Test]
        public void Invoke_Method_Returns_Expected_Object()
        {
            var methodObject = new HandlerMethod(typeof (HandlerMethodTests).GetMethod("GetCritters"), TypeMapper);

            var pathNode = new ResourceNode(TypeMapper, null, "Test",
                delegate() { return null; }, TypeMapper.FromType(typeof (Critter)) as ResourceType);

            var pomonaRequest = new PomonaRequest(pathNode, nancyContext,
                serializerFactory);

            var returnedObject = methodObject.Invoke(this, pomonaRequest);

            Assert.IsInstanceOf(typeof (IQueryable<Critter>), returnedObject);
        }


        [Test]
        public void Match_ChildResource_Requires_ParentResource()
        {
            var methodObject = new HandlerMethod(typeof (HandlerMethodTests).GetMethod("QueryPlanetarySystems"),
                TypeMapper);
            Assert.That(
                methodObject.Match(HttpMethod.Get, PathNodeType.Collection,
                    TypeMapper.GetClassMapping(typeof (PlanetarySystem))), Is.False);
        }

        [Test]
        public void Match_ChildResource_Takes_ParentResource()
        {
            var methodObject = new HandlerMethod(typeof (HandlerMethodTests).GetMethod("GetPlanetarySystems"),
                TypeMapper);
            Assert.That(
                methodObject.Match(HttpMethod.Get, PathNodeType.Collection,
                    TypeMapper.GetClassMapping(typeof (PlanetarySystem))), Is.True);
        }


        [Test]
        public void Match_QueryMethod_Does_Not_Match_IncorrectMethodName()
        {
            var methodObject = new HandlerMethod(typeof (HandlerMethodTests).GetMethod("GetCrayons"), TypeMapper);
            Assert.That(
                methodObject.Match(HttpMethod.Get, PathNodeType.Collection,
                    TypeMapper.GetClassMapping(typeof (Critter))), Is.False);
        }

        [Test]
        public void Match_QueryMethod_Does_Not_Match_IncorrectSignature()
        {
            var methodObject = new HandlerMethod(typeof (HandlerMethodTests).GetMethod("GetCritters"), TypeMapper);
            Assert.That(
                methodObject.Match(HttpMethod.Get, PathNodeType.Collection,
                    TypeMapper.GetClassMapping(typeof (MusicalCritter))), Is.False);
        }

        [Test]
        public void Match_QueryMethod_Matches_CorrectSignature()
        {
            var methodObject = new HandlerMethod(typeof (HandlerMethodTests).GetMethod("GetCritters"), TypeMapper);
            Assert.That(
                methodObject.Match(HttpMethod.Get, PathNodeType.Collection,
                    TypeMapper.GetClassMapping(typeof (Critter))), Is.True);
        }
    }
}