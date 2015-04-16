#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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
using Pomona.Example.Models;
using Pomona.Example.Models.Existence;

namespace Pomona.UnitTests
{
    [TestFixture]
    public class UriResolverTests : IBaseUriProvider
    {
        private TypeMapper typeMapper;
        private UriResolver uriResolver;


        [Test]
        public void GetUriFor_entity_having_reserved_characters_in_path_segment_encodes_url_correctly()
        {
            var galaxy = new Galaxy() { Name = "this is it!?~_--- :;" };
            var url = this.uriResolver.GetUriFor(galaxy);
            Assert.That(url, Is.EqualTo("http://whateva/galaxies/this%20is%20it%21%3F%7E_---%20%3A%3B"));
        }


        [TestCase("http://whateva/", "http://whateva/farms/1234/critters")]
        [TestCase("http://whateva/boo", "http://whateva/boo/farms/1234/critters")]
        [Test]
        public void GetUriFor_property_of_resource_returns_correct_url(string baseUrl, string expectedResourceUrl)
        {
            BaseUri = new Uri(baseUrl);
            var farm = new Farm("the farm") { Id = 1234 };
            var url = this.uriResolver.GetUriFor(this.typeMapper.FromType(typeof(Farm)).GetPropertyByName("Critters", true), farm);
            Assert.That(url, Is.EqualTo(expectedResourceUrl));
        }


        [TestCase("http://whateva/", "http://whateva/critters/1234")]
        [TestCase("http://whateva/boo", "http://whateva/boo/critters/1234")]
        [TestCase("http://whateva/boo/", "http://whateva/boo/critters/1234")]
        [Test]
        public void GetUriFor_resource_of_collection_returns_correct_url(string baseUrl, string expectedResourceUrl)
        {
            BaseUri = new Uri(baseUrl);
            var critter = new Critter() { Id = 1234 };
            var url = this.uriResolver.GetUriFor(critter);
            Assert.That(url, Is.EqualTo(expectedResourceUrl));
        }


        [SetUp]
        public void SetUp()
        {
            BaseUri = new Uri("http://whateva/");
            this.typeMapper = new TypeMapper(new CritterPomonaConfiguration());
            this.uriResolver = new UriResolver(this.typeMapper, this);
        }


        public Uri BaseUri { get; set; }
    }
}