#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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