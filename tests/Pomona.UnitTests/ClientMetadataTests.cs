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

namespace Pomona.UnitTests
{
    [TestFixture]
    public class ClientMetadataTests
    {
        private class TestableClientMetadata : ClientMetadata
        {
            public TestableClientMetadata(string assemblyName = null,
                                          string name = null,
                                          string interfaceName = null,
                                          string @namespace = null,
                                          string informationalVersion = null)
                : base(assemblyName, name, interfaceName, @namespace, informationalVersion)
            {
            }
        }


        [Test]
        public void Constructor_AssemblyName()
        {
            ClientMetadata clientMetadata = new TestableClientMetadata("My.Client");

            Assert.That(clientMetadata.AssemblyName, Is.EqualTo("My.Client"), "AssemblyName");
            Assert.That(clientMetadata.Name, Is.EqualTo("Client"), "Name");
            Assert.That(clientMetadata.InterfaceName, Is.EqualTo("IClient"), "InterfaceName");
            Assert.That(clientMetadata.Namespace, Is.EqualTo("My.Client"), "Namespace");
            Assert.That(clientMetadata.InformationalVersion, Is.EqualTo("1.0.0.0"), "InformationalVersion");
        }


        [Test]
        public void Constructor_AssemblyNameAndClientName()
        {
            ClientMetadata clientMetadata = new TestableClientMetadata("My.Awesome.Client", "MyClient");

            Assert.That(clientMetadata.AssemblyName, Is.EqualTo("My.Awesome.Client"), "AssemblyName");
            Assert.That(clientMetadata.Name, Is.EqualTo("MyClient"), "Name");
            Assert.That(clientMetadata.InterfaceName, Is.EqualTo("IMyClient"), "InterfaceName");
            Assert.That(clientMetadata.Namespace, Is.EqualTo("My.Awesome.Client"), "Namespace");
            Assert.That(clientMetadata.InformationalVersion, Is.EqualTo("1.0.0.0"), "InformationalVersion");
        }


        [Test]
        public void Constructor_Default_AllValuesAreDefault()
        {
            ClientMetadata clientMetadata = new TestableClientMetadata();

            Assert.That(clientMetadata.AssemblyName, Is.EqualTo("Client"), "AssemblyName");
            Assert.That(clientMetadata.Name, Is.EqualTo("Client"), "Name");
            Assert.That(clientMetadata.InterfaceName, Is.EqualTo("IClient"), "InterfaceName");
            Assert.That(clientMetadata.Namespace, Is.EqualTo("Client"), "Namespace");
            Assert.That(clientMetadata.InformationalVersion, Is.EqualTo("1.0.0.0"), "InformationalVersion");
        }


        [Test]
        public void Constructor_EmptyString_AllValuesAreDefault()
        {
            ClientMetadata clientMetadata = new TestableClientMetadata(String.Empty,
                                                                       String.Empty,
                                                                       String.Empty,
                                                                       String.Empty);

            Assert.That(clientMetadata.AssemblyName, Is.EqualTo("Client"), "AssemblyName");
            Assert.That(clientMetadata.Name, Is.EqualTo("Client"), "Name");
            Assert.That(clientMetadata.InterfaceName, Is.EqualTo("IClient"), "InterfaceName");
            Assert.That(clientMetadata.Namespace, Is.EqualTo("Client"), "Namespace");
            Assert.That(clientMetadata.InformationalVersion, Is.EqualTo("1.0.0.0"), "InformationalVersion");
        }


        [Test]
        public void Constructor_InvalidClientInterfaceName_ThrowsArgumentException()
        {
            TestDelegate throwing = () => new TestableClientMetadata(interfaceName : "My.Awesome.Client");

            var exception = Assert.Throws<ArgumentException>(throwing);

            Console.WriteLine(exception);

            Assert.That(exception.ParamName, Is.EqualTo("interfaceName"));
        }


        [Test]
        public void Constructor_InvalidClientName_ThrowsArgumentException()
        {
            TestDelegate throwing = () => new TestableClientMetadata(name : "My.Awesome.Client");

            var exception = Assert.Throws<ArgumentException>(throwing);

            Console.WriteLine(exception);

            Assert.That(exception.ParamName, Is.EqualTo("name"));
        }


        [Test]
        public void Constructor_InvalidClientNamespace_ThrowsArgumentException()
        {
            TestDelegate throwing = () => new TestableClientMetadata(@namespace : "My#Awesome#Client");

            var exception = Assert.Throws<ArgumentException>(throwing);

            Console.WriteLine(exception);

            Assert.That(exception.ParamName, Is.EqualTo("namespace"));
        }


        [Test]
        public void Constructor_Null_AllValuesAreDefault()
        {
            ClientMetadata clientMetadata = new TestableClientMetadata(null, null, null, null);

            Assert.That(clientMetadata.AssemblyName, Is.EqualTo("Client"), "AssemblyName");
            Assert.That(clientMetadata.Name, Is.EqualTo("Client"), "Name");
            Assert.That(clientMetadata.InterfaceName, Is.EqualTo("IClient"), "InterfaceName");
            Assert.That(clientMetadata.Namespace, Is.EqualTo("Client"), "Namespace");
            Assert.That(clientMetadata.InformationalVersion, Is.EqualTo("1.0.0.0"), "InformationalVersion");
        }


        [Test]
        public void Constructor_Whitespace_AllValuesAreDefault()
        {
            const string whitespace = "   ";
            ClientMetadata clientMetadata = new TestableClientMetadata(whitespace, whitespace, whitespace, whitespace);

            Assert.That(clientMetadata.AssemblyName, Is.EqualTo("Client"), "AssemblyName");
            Assert.That(clientMetadata.Name, Is.EqualTo("Client"), "Name");
            Assert.That(clientMetadata.InterfaceName, Is.EqualTo("IClient"), "InterfaceName");
            Assert.That(clientMetadata.Namespace, Is.EqualTo("Client"), "Namespace");
            Assert.That(clientMetadata.InformationalVersion, Is.EqualTo("1.0.0.0"), "InformationalVersion");
        }


        [Test]
        public void With_AllValuesAreSet()
        {
            var clientMetadata = new TestableClientMetadata();
            var overriddenMetadata = clientMetadata.With("My.Awesome.Client",
                                                         "MyClient",
                                                         "IMyClientInterace",
                                                         "My.Awesome.Client.Namespace",
                                                         "1.2.3.4");

            Assert.That(overriddenMetadata.AssemblyName, Is.EqualTo("My.Awesome.Client"), "AssemblyName");
            Assert.That(overriddenMetadata.Name, Is.EqualTo("MyClient"), "Name");
            Assert.That(overriddenMetadata.InterfaceName, Is.EqualTo("IMyClientInterace"), "InterfaceName");
            Assert.That(overriddenMetadata.Namespace, Is.EqualTo("My.Awesome.Client.Namespace"), "Namespace");
            Assert.That(overriddenMetadata.InformationalVersion, Is.EqualTo("1.2.3.4"), "InformationalVersion");
        }


        [Test]
        public void With_ClientName()
        {
            var clientMetadata = new TestableClientMetadata();
            var overriddenMetadata = clientMetadata.With(name : "MyAwesomeClient");

            Assert.That(overriddenMetadata.AssemblyName, Is.EqualTo("Client"), "AssemblyName");
            Assert.That(overriddenMetadata.Name, Is.EqualTo("MyAwesomeClient"), "Name");
            Assert.That(overriddenMetadata.InterfaceName, Is.EqualTo("IMyAwesomeClient"), "InterfaceName");
            Assert.That(overriddenMetadata.Namespace, Is.EqualTo("Client"), "Namespace");
            Assert.That(overriddenMetadata.InformationalVersion, Is.EqualTo("1.0.0.0"), "InformationalVersion");
        }
    }
}