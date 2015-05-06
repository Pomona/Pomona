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
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Pomona.Schemas;
using Pomona.TestHelpers;

namespace Pomona.UnitTests.Schemas
{
    [TestFixture]
    public class ApiChangeVerifierTests
    {
        private string tempDir;


        [SetUp]
        public void SetUp()
        {
            this.tempDir = Path.GetTempPath() + Guid.NewGuid().ToString("N");
            Directory.CreateDirectory(this.tempDir);
        }


        [TearDown]
        public void TearDown()
        {
            if (this.tempDir != null)
            {
                try
                {
                    Directory.Delete(this.tempDir, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("WARNING: Failed deleting temp dir {0}, got exception {1}", this.tempDir, ex);
                }
            }
        }


        [Test]
        public void VerifyCompatibility_WithBreakingChange_ThrowsException()
        {
            var verifier = new ApiChangeVerifier(this.tempDir);
            var schema = SchemaTests.CreateSchema();
            verifier.MarkApiVersion(schema);
            schema.Version = "1.3.3.8";
            schema.Types[0].Properties.Add(new KeyValuePair<string, SchemaPropertyEntry>("newProp",
                                                                                         new SchemaPropertyEntry
                                                                                         {
                                                                                             Name = "newProp",
                                                                                             Type = "string",
                                                                                             Required = true
                                                                                         }));
            Assert.That(() => verifier.VerifyCompatibility(schema), Throws.Exception);
        }


        [Test]
        public void VerifyCompatibility_WithNonBreakingChange_ThrowsNoException()
        {
            var verifier = new ApiChangeVerifier(this.tempDir);
            var schema = SchemaTests.CreateSchema();
            verifier.MarkApiVersion(schema);
            schema.Version = "1.3.3.8";
            schema.Types[0].Properties.Add(new KeyValuePair<string, SchemaPropertyEntry>("newProp",
                                                                                         new SchemaPropertyEntry
                                                                                         {
                                                                                             Name = "newProp",
                                                                                             Type = "string"
                                                                                         }));
            verifier.VerifyCompatibility(schema);
        }
    }
}