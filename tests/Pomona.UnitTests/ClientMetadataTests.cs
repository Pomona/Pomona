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
        public void Constructor_AssemblyName_AllValuesAreBasedOnAssemblyName()
        {
            ClientMetadata clientMetadata = new TestableClientMetadata("My.Client");

            Assert.That(clientMetadata.AssemblyName, Is.EqualTo("My.Client"), "AssemblyName");
            Assert.That(clientMetadata.Name, Is.EqualTo("Client"), "Name");
            Assert.That(clientMetadata.InterfaceName, Is.EqualTo("IClient"), "InterfaceName");
            Assert.That(clientMetadata.Namespace, Is.EqualTo("My.Client"), "Namespace");
        }


        [Test]
        public void Constructor_Default_AllValuesAreDefault()
        {
            ClientMetadata clientMetadata = new TestableClientMetadata();

            Assert.That(clientMetadata.AssemblyName, Is.EqualTo("Client"), "AssemblyName");
            Assert.That(clientMetadata.Name, Is.EqualTo("Client"), "Name");
            Assert.That(clientMetadata.InterfaceName, Is.EqualTo("IClient"), "InterfaceName");
            Assert.That(clientMetadata.Namespace, Is.EqualTo("Client"), "Namespace");
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
        }


        [Test]
        public void Constructor_Null_AllValuesAreDefault()
        {
            ClientMetadata clientMetadata = new TestableClientMetadata(null, null, null, null);

            Assert.That(clientMetadata.AssemblyName, Is.EqualTo("Client"), "AssemblyName");
            Assert.That(clientMetadata.Name, Is.EqualTo("Client"), "Name");
            Assert.That(clientMetadata.InterfaceName, Is.EqualTo("IClient"), "InterfaceName");
            Assert.That(clientMetadata.Namespace, Is.EqualTo("Client"), "Namespace");
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
        }
    }
}