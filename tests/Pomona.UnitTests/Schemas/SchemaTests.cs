#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.IO;
using System.Linq;

using NUnit.Framework;

using Pomona.Common;
using Pomona.Schemas;

namespace Pomona.UnitTests.Schemas
{
    [TestFixture]
    public class SchemaTests
    {
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
                        "wasReadonly",
                        new SchemaPropertyEntry()
                        {
                            Name = "wasReadonly",
                            Access = HttpMethod.Get,
                            Type = "string"
                        }
                    },
                    {
                        "wasWritable",
                        new SchemaPropertyEntry()
                        {
                            Name = "wasWritable",
                            Access = HttpMethod.Get | HttpMethod.Put | HttpMethod.Post | HttpMethod.Patch,
                            Type = "string"
                        }
                    },
                    {
                        "fooRequired",
                        new SchemaPropertyEntry
                        {
                            Name = "fooRequired",
                            Required = true,
                            Type = "string"
                        }
                    },
                    {
                        "barOptional",
                        new SchemaPropertyEntry
                        {
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
        public void IsBackwardsCompatibleWith_OnSchemaHavingReadOnlyPropertyMadeWritable_ReturnsTrue()
        {
            AssertIsBackwardsCompatible(s => s.Types.First(x => x.Name == "Class")
                                              .Properties["wasReadonly"].Access |= HttpMethod.Post);
        }


        [Test]
        public void IsBackwardsCompatibleWith_OnSchemaHavingRequiredPropertyAdded_ReturnsFalse()
        {
            AssertBreaksBackwardsCompability(
                s =>
                    s.Types.First(x => x.Name == "Class")
                     .Properties.Add("AddedProp",
                                     new SchemaPropertyEntry { Name = "AddedProp", Type = "string", Required = true }));
        }


        [Test]
        public void IsBackwardsCompatibleWith_OnSchemaHavingTypeRemoved_ReturnsFalse()
        {
            AssertBreaksBackwardsCompability(
                s =>
                    s.Types.Clear());
        }


        [Test]
        public void IsBackwardsCompatibleWith_OnSchemaHavingWritablePropertyMadeReadOnly_ReturnsFalse()
        {
            AssertBreaksBackwardsCompability(s =>
            {
                s.Types.First(x => x.Name == "Class")
                 .Properties["wasWritable"].Access = HttpMethod.Get;
            });
        }


        private void AssertBreaksBackwardsCompability(Action<Schema> changeAction)
        {
            Assert.IsFalse(IsBackwardsCompatible(changeAction));
        }


        private void AssertIsBackwardsCompatible(Action<Schema> changeAction)
        {
            Assert.IsTrue(IsBackwardsCompatible(changeAction));
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
    }
}