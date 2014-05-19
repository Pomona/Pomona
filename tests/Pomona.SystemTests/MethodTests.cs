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
using NUnit.Framework;
using Pomona.Common;
using Pomona.Example.Models;
using Pomona.Example.Models.Existence;
using Pomona.RequestProcessing;

namespace Pomona.SystemTests
{
    [TestFixture]
    public class MethodTests : ClientTestsBase
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
            return new List<PlanetarySystem>().AsQueryable();
        }

        public IQueryable<PlanetarySystem> QueryPlanetarySystems( /* No Galaxy here, no Sir*/)
        {
            return new List<PlanetarySystem>().AsQueryable();
        }

        [Test]
        public void ChildResource_Requires_ParentResource()
        {
            var handlerMethod = new Method(typeof (MethodTests).GetMethod("QueryPlanetarySystems"), TypeMapper);
            Assert.That(
                handlerMethod.Match(HttpMethod.Get, PathNodeType.Collection,
                    TypeMapper.GetClassMapping(typeof (PlanetarySystem))), Is.False);
        }

        [Test]
        public void ChildResource_Takes_ParentResource()
        {
            var handlerMethod = new Method(typeof (MethodTests).GetMethod("GetPlanetarySystems"), TypeMapper);
            Assert.That(
                handlerMethod.Match(HttpMethod.Get, PathNodeType.Collection,
                    TypeMapper.GetClassMapping(typeof (PlanetarySystem))), Is.True);
        }


        [Test]
        public void QueryMethod_Does_Not_Match_IncorrectMethodName()
        {
            var handlerMethod = new Method(typeof (MethodTests).GetMethod("GetCrayons"), TypeMapper);
            Assert.That(
                handlerMethod.Match(HttpMethod.Get, PathNodeType.Collection,
                    TypeMapper.GetClassMapping(typeof (Critter))), Is.False);
        }

        [Test]
        public void QueryMethod_Does_Not_Match_IncorrectSignature()
        {
            var handlerMethod = new Method(typeof (MethodTests).GetMethod("GetCritters"), TypeMapper);
            Assert.That(
                handlerMethod.Match(HttpMethod.Get, PathNodeType.Collection,
                    TypeMapper.GetClassMapping(typeof (MusicalCritter))), Is.False);
        }

        [Test]
        public void QueryMethod_Matches_CorrectSignature()
        {
            var handlerMethod = new Method(typeof (MethodTests).GetMethod("GetCritters"), TypeMapper);
            Assert.That(
                handlerMethod.Match(HttpMethod.Get, PathNodeType.Collection,
                    TypeMapper.GetClassMapping(typeof (Critter))), Is.True);
        }
    }
}