#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2016 Karsten Nikolai Strand
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
using System.Collections.Generic;
using System.Linq;

using Critters.Client;

using Mono.Cecil;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Common.Web;

namespace Pomona.SystemTests.ClientCompatibility
{
    /// <summary>
    /// These tests checks client compatibility with old generated clients.
    /// 
    /// This was introduced during transition to the async HttpClient.
    /// </summary>
    [TestFixture]
    public class ClientCompatibilityTests
    {
        [Test]
        public void All_references_from_old_assembly_are_valid()
        {
            var exceptions = new List<AssertionException>();
            try
            {
                var clientAssembly = AssemblyDefinition.ReadAssembly(typeof(CritterClient).Assembly.Location);
                var pomonaCommonModule = AssemblyDefinition.ReadAssembly(typeof(IPomonaClient).Assembly.Location).MainModule;

                var pomonaCommonAssemblyName = "Pomona.Common";
                foreach (
                    var typeReference in clientAssembly.MainModule.GetTypeReferences().Where(x => x.Scope.Name == pomonaCommonAssemblyName))
                {
                    var resolved = pomonaCommonModule.MetadataResolver.Resolve(typeReference);
                    Assert.That(resolved, Is.Not.Null,
                                $"Required type {typeReference} needed for backwards compatibility is missing from {pomonaCommonAssemblyName}.");
                }

                foreach (
                    var memberReference in
                        clientAssembly.MainModule.GetMemberReferences().Where(x => x.DeclaringType.Scope.Name == pomonaCommonAssemblyName))
                {
                    var methodReference = memberReference as MethodReference;
                    var fieldReference = memberReference as FieldReference;
                    if (methodReference != null)
                    {
                        var resolved = pomonaCommonModule.MetadataResolver.Resolve(methodReference);
                        Assert.That(resolved, Is.Not.Null,
                                    $"Required method {methodReference} needed for backwards compatibility is missing from {pomonaCommonAssemblyName}.");
                    }
                    else if (fieldReference != null)
                    {
                        var resolved = pomonaCommonModule.MetadataResolver.Resolve(fieldReference);
                        Assert.That(resolved, Is.Not.Null,
                                    $"Required method {fieldReference} needed for backwards compatibility is missing from {pomonaCommonAssemblyName}.");
                    }
                    else
                    {
                        Assert.Fail(
                            $"Don't know how to check member reference {memberReference} of type {memberReference.GetType().FullName}");
                    }
                }
            }
            catch (AssertionException ex)
            {
                exceptions.Add(ex);
            }

            if (exceptions.Count > 0)
                throw new AggregateException("One or more assertions failed", exceptions);
        }


        [Test]
        public void Ctor_taking_uri_does_not_throw_exception()
        {
            Assert.DoesNotThrow(() => new CritterClient("http://test"));
        }


        [Test]
        public void Ctor_taking_webclient_does_not_throw_exception()
        {
            Assert.DoesNotThrow(() => new CritterClient("http://test", new HttpWebClient()));
        }
    }
}