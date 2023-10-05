#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.IO;

using Nancy.Testing;

using Pomona.Schemas;

namespace Pomona.TestHelpers
{
    public class ApiChangeVerifier
    {
        private readonly string schemaDirectory;


        public ApiChangeVerifier(string schemaDirectory)
        {
            if (schemaDirectory == null)
                throw new ArgumentNullException(nameof(schemaDirectory));
            this.schemaDirectory = schemaDirectory;
        }


        public void MarkApiVersion(Schema schema)
        {
            var schemaFilename = Path.Combine(this.schemaDirectory, schema.Version + ".json");
            File.WriteAllText(schemaFilename, schema.ToJson());
        }


        public void VerifyCompatibility(Schema changedSchema)
        {
            foreach (var schemaFilename in Directory.GetFiles(this.schemaDirectory, "*.json"))
            {
                var content = File.ReadAllText(schemaFilename);
                Console.WriteLine(content);
                var oldSchema = Schema.FromJson(content);
                bool breaks;
                using (var errorWriter = new StringWriter())
                {
                    breaks = !changedSchema.IsBackwardsCompatibleWith(oldSchema, errorWriter);
                    errorWriter.Flush();
                    if (breaks)
                    {
                        throw new AssertException("Schema " + changedSchema.Version + " breaks compatibility with " +
                                                  schemaFilename + ": " + errorWriter);
                    }
                }
            }
        }
    }
}

