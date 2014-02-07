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

using NUnit.Framework;

using Pomona.Example;
using Pomona.Example.Models.Existence;

namespace Pomona.UnitTests
{
    [TestFixture]
    public class UriResolverTests
    {
        private class DummyBaseUriProvider : IBaseUriProvider
        {
            public Uri BaseUri { get; set; }
        }


        [Test]
        public void GetUriFor_WithEntityWithSpaceInPath_EncodesUrlTheRightWay()
        {
            var typeMapper = new TypeMapper(new CritterPomonaConfiguration());
            var uriResolver = new UriResolver(typeMapper,
                new DummyBaseUriProvider() { BaseUri = new Uri("http://whateva/") });
            var galaxy = new Galaxy() { Name = "this is it" };
            var url = uriResolver.GetUriFor(galaxy);
            Assert.That(url, Is.EqualTo("http://whateva/galaxies/this%20is%20it"));
        }
    }
}