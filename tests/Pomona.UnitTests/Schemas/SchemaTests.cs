// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Pomona.Schemas;

namespace Pomona.UnitTests.Schemas
{
    [TestFixture]
    public class SchemaTests
    {
        private void AssertIsBackwardsCompatible(Action<Schema> changeAction)
        {
            Assert.IsTrue(IsBackwardsCompatible(changeAction));
        }

        private void AssertBreaksBackwardsCompability(Action<Schema> changeAction)
        {
            Assert.IsFalse(IsBackwardsCompatible(changeAction));
        }

        private bool IsBackwardsCompatible(Action<Schema> changeAction)
        {
            var oldSchema = CreateSchema();
            var newSchema = CreateSchema();

            changeAction(newSchema);

            using (var stringWriter = new StringWriter())
            {
                var result = newSchema.IsBackwardsCompatibleWith(oldSchema, stringWriter);
                Console.WriteLine(stringWriter.ToString());
                return result;
            }
        }

        public static Schema CreateSchema()
        {
            var schema = new Schema
                {
                    Version = "1.3.3.7"
                };

            schema.Types.Add(new SchemaTypeEntry
                {
                    Extends = "Parent",
                    Name = "Class",
                    Properties =
                        {
                            {
                                "fooRequired",
                                new SchemaPropertyEntry
                                    {
                                        Generated = false,
                                        Name = "fooRequired",
                                        Required = true,
                                        Type = "string"
                                    }
                            },
                            {
                                "barOptional",
                                new SchemaPropertyEntry
                                    {
                                        Generated = false,
                                        Name = "barOptional",
                                        Required = false,
                                        Type = "string"
                                    }
                            },
                        }
                });
            return schema;
        }


        [Test]
        public void IsBackwardsCompatibleWith_OnSchemaHavingOptionalPropertyAdded_ReturnsTrue()
        {
            AssertIsBackwardsCompatible(s => s.Types.First(x => x.Name == "Class")
                                              .Properties.Add("AddedProp",
                                                              new SchemaPropertyEntry
                                                                  {
                                                                      Name = "AddedProp",
                                                                      Type = "string",
                                                                      Required = false
                                                                  }));
        }

        [Test]
        public void IsBackwardsCompatibleWith_OnSchemaHavingOptionalPropertyChangedToRequired_ReturnsFalse()
        {
            AssertBreaksBackwardsCompability(
                s =>
                s.Types.First(x => x.Name == "Class").Properties["barOptional"].Required = true);
        }

        [Test]
        public void IsBackwardsCompatibleWith_OnSchemaHavingPropertyRemoved_ReturnsFalse()
        {
            AssertBreaksBackwardsCompability(
                s =>
                s.Types.First(x => x.Name == "Class").Properties.Remove("barOptional"));
        }

        [Test]
        public void IsBackwardsCompatibleWith_OnSchemaHavingPropertyTypeChanged_ReturnsFalse()
        {
            AssertBreaksBackwardsCompability(
                s =>
                s.Types.First(x => x.Name == "Class").Properties["barOptional"].Type = "number");
        }

        [Test]
        public void IsBackwardsCompatibleWith_OnSchemaHavingRequiredPropertyAdded_ReturnsFalse()
        {
            AssertBreaksBackwardsCompability(
                s =>
                s.Types.First(x => x.Name == "Class")
                 .Properties.Add("AddedProp",
                                 new SchemaPropertyEntry {Name = "AddedProp", Type = "string", Required = true}));
        }

        [Test]
        public void IsBackwardsCompatibleWith_OnSchemaHavingTypeRemoved_ReturnsFalse()
        {
            AssertBreaksBackwardsCompability(
                s =>
                s.Types.Clear());
        }
    }
}