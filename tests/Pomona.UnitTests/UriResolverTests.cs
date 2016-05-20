#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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

        [Test]
        public void GetUriFor_resource_without_primary_key_throws_invalidoperationexception()
        {
            BaseUri = new Uri("http://whateva");
            var noPrimaryKeyThing = new NoPrimaryKeyThing() { Foo = "bar" };
            TestDelegate throwing = () => this.uriResolver.GetUriFor(noPrimaryKeyThing);
            var exception = Assert.Throws<InvalidOperationException>(throwing);
            Assert.That(exception.Message, Is.EqualTo("NoPrimaryKeyThing has no Id property or primary key mapping"));
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