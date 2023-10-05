#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

using Pomona.Common;

namespace Pomona.Schemas
{
    public class Schema
    {
        public Schema()
        {
            Types = new List<SchemaTypeEntry>();
        }


        public IList<SchemaTypeEntry> Types { get; set; }
        public string Version { get; set; }


        public static Schema FromJson(string jsonString)
        {
            var serializer = GetSerializer();
            var stringReader = new StringReader(jsonString);
            var schema = (Schema)serializer.Deserialize(stringReader, typeof(Schema));
            // Fix property names (they're not serialized as this would be redundant)..
            foreach (var propKvp in schema.Types.SelectMany(x => x.Properties))
                propKvp.Value.Name = propKvp.Key;
            return schema;
        }


        public bool IsBackwardsCompatibleWith(Schema oldSchema, TextWriter errorLog)
        {
            // Allowed changes:
            //   * Add a new type.
            //   * Add a new property that is not required.
            // Disallowed changes:
            //   * Remove a type.
            //   * Change inheritance hierarchy.
            //   * Add a new required property.
            //   * Change uri.
            // Undetectable breaking changes:
            //   * Logical changes in what stuff means.

            var isBackwardsCompatible = true;
            var allTypeNames = Types.Select(x => x.Name).Union(oldSchema.Types.Select(x => x.Name));

            foreach (var typeName in allTypeNames)
            {
                var oldType = oldSchema.Types.FirstOrDefault(x => x.Name == typeName);
                var newType = Types.FirstOrDefault(x => x.Name == typeName);

                if (oldType == null)
                {
                    if (newType == null)
                        throw new InvalidOperationException("newType should not be null at this point.");
                    // New type, accept this change!
                    // TODO: (maybe not if it inherits from an existing type though?)
                    continue;
                    ;
                }
                if (newType == null)
                {
                    // Removal of type not allowed.
                    isBackwardsCompatible = false;
                    errorLog.Write("ERROR: Removal of type {0} breaks backwards compability.\r\n", typeName);
                    continue;
                }

                if (newType.Uri != oldType.Uri)
                {
                    isBackwardsCompatible = false;
                    errorLog.Write(
                        "Change of uri from {0} to {1} for type {2} breaks backwards compability.\r\n",
                        oldType.Uri, newType.Uri, typeName);
                }

                if (newType.Extends != oldType.Extends)
                {
                    isBackwardsCompatible = false;
                    errorLog.Write(
                        "Change of baseclass from {0} to {1} for type {2} breaks backwards compability.\r\n",
                        oldType.Extends, newType.Extends, typeName);
                }

                PropertiesAreBackwardsCompatible(errorLog, oldType, newType, ref isBackwardsCompatible, typeName);
            }

            return isBackwardsCompatible;
        }


        public string ToJson()
        {
            var serializer = GetSerializer();
            var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, this);
            return stringWriter.ToString();
        }


        internal static string[] HttpAccessModeToMethodsArray(HttpMethod httpMethod)
        {
            if (httpMethod == 0)
                return new string[] { };
            return httpMethod.ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(
                x => x.Trim().ToUpperInvariant()).ToArray();
        }


        internal static HttpMethod MethodsArrayToHttpAccessMode(string[] methods)
        {
            if (methods == null)
                return default(HttpMethod);

            return methods.Select(x => (HttpMethod)Enum.Parse(typeof(HttpMethod), x, true))
                          .Aggregate(
                              default(HttpMethod),
                              (a, b) => a | b);
        }


        private static JsonSerializer GetSerializer()
        {
            var serializer =
                JsonSerializer.Create(
                    new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                        Converters = { new StringEnumConverter() }
                    });
            serializer.Formatting = Formatting.Indented;
            return serializer;
        }


        private static void PropertiesAreBackwardsCompatible(TextWriter errorLog,
                                                             SchemaTypeEntry oldType,
                                                             SchemaTypeEntry newType,
                                                             ref bool isBackwardsCompatible,
                                                             string typeName)
        {
            var propertyNames =
                oldType.Properties.Select(x => x.Key).Concat(newType.Properties.Select(x => x.Key)).Distinct();

            foreach (var propName in propertyNames)
            {
                SchemaPropertyEntry oldPropEntry, newPropEntry;

                oldType.Properties.TryGetValue(propName, out oldPropEntry);
                newType.Properties.TryGetValue(propName, out newPropEntry);

                if (oldPropEntry == null && newPropEntry == null)
                {
                    throw new InvalidOperationException("Should have found a property named " + propName +
                                                        " on either old or new type. WTF?");
                }

                if (newPropEntry == null)
                {
                    // Removal of property, not allowed!
                    isBackwardsCompatible = false;
                    errorLog.Write(
                        "Removal of property {0} from type {1} breaks backwards compability.\r\n", propName,
                        typeName);
                    continue;
                }
                if (oldPropEntry == null)
                {
                    if (newPropEntry.Required)
                    {
                        isBackwardsCompatible = false;
                        errorLog.Write(
                            "Introducing a new required property {0} to type {1} breaks backwards compability.\r\n",
                            propName, typeName);
                    }
                    continue;
                }

                var lostRights = oldPropEntry.Access ^ (newPropEntry.Access & oldPropEntry.Access);

                if (lostRights != 0)
                {
                    isBackwardsCompatible = false;
                    errorLog.Write(
                        "The property access rights {0} has been removed from property, which breaks backwards compatibility.",
                        lostRights);
                }

                if (!oldPropEntry.Required && newPropEntry.Required)
                {
                    isBackwardsCompatible = false;
                    errorLog.Write(
                        "Making previosly optional property {0} of type {1} required breaks compability.\r\n",
                        propName, typeName);
                }

                if (newPropEntry.Type != oldPropEntry.Type)
                {
                    isBackwardsCompatible = false;
                    errorLog.Write(
                        "Changing type of property {0} declared by {1} from type {2} to type {3} breaks compability\r\n",
                        propName, typeName, oldPropEntry.Type, newPropEntry.Type);
                }
            }
        }
    }
}

