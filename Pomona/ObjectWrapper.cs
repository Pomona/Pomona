// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pomona
{
    public class ObjectWrapper
    {
        private static readonly Type[] knownGenericCollectionTypes = new[]
                                                                         {
                                                                             typeof (IList<>), typeof (ICollection<>),
                                                                             typeof (List<>)
                                                                         };

        private readonly FetchContext context;
        private readonly IMappedType expectedBaseType;
        private readonly string path;
        private readonly object target;
        private readonly IMappedType targetType;


        public ObjectWrapper(object target, string path, FetchContext context, IMappedType expectedBaseType)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (path == null)
                throw new ArgumentNullException("path");
            if (context == null)
                throw new ArgumentNullException("context");
            if (expectedBaseType == null)
                throw new ArgumentNullException("expectedBaseType");
            this.target = target;
            this.path = path;
            this.context = context;
            this.expectedBaseType = expectedBaseType;
            targetType = context.TypeMapper.GetClassMapping(target.GetType());
        }


        public string Path
        {
            get { return path; }
        }


        public virtual void ToJson(TextWriter textWriter)
        {
            var jsonWriter = new JsonTextWriter(textWriter) {Formatting = Formatting.Indented};
            ToJson(jsonWriter);
            jsonWriter.Flush();
        }


        public virtual void ToJson(JsonWriter writer)
        {
            IMappedType collectionElementType;
            if (TryGetCollectionElementType(targetType, out collectionElementType))
            {
                writer.WriteStartArray();
                foreach (var child in ((IEnumerable) target))
                {
                    WriteJsonExpandedOrReference(
                        writer,
                        child,
                        path,
                        context.TypeMapper.GetClassMapping(child.GetType()),
                        collectionElementType);
                }
                writer.WriteEndArray();
            }
            else
            {
                var transformedType = (TransformedType) targetType;
                writer.WriteStartObject();

                if (!transformedType.MappedAsValueObject)
                {
                    writer.WritePropertyName("_uri");
                    writer.WriteValue(context.GetUri(target));
                }

                if (expectedBaseType != targetType)
                {
                    writer.WritePropertyName("_type");
                    writer.WriteValue(targetType.Name);
                }

                foreach (var propDef in transformedType.Properties)
                {
                    var subPath = path + "." + propDef.JsonName;
                    var value = propDef.Getter(target);

                    writer.WritePropertyName(propDef.JsonName);
                    if (value == null)
                    {
                        writer.WriteNull();
                        continue;
                    }

                    var valueTypeMapping = context.TypeMapper.GetClassMapping(value.GetType());

                    if (context.TypeMapper.IsSerializedAsObject(valueTypeMapping) ||
                        context.TypeMapper.IsSerializedAsArray(valueTypeMapping))
                    {
                        var propertyValue = propDef.Getter(target);

                        if (propertyValue == null)
                            writer.WriteNull();
                        else
                        {
                            WriteJsonExpandedOrReference(
                                writer, propertyValue, subPath, valueTypeMapping, propDef.PropertyType);
                        }
                    }
                    else
                        writer.WriteValue(propDef.Getter(target));
                }

                // Write path for debug purposes
                if (context.DebugMode)
                {
                    writer.WritePropertyName("_path");
                    writer.WriteValue(path);
                }

                writer.WriteEndObject();
            }
        }


        public virtual void UpdateFromJson(TextReader textReader)
        {
            var jsonObject = JObject.Load(new JsonTextReader(textReader));
            UpdateFromJson(jsonObject);
        }


        protected virtual void UpdateFromJson(JObject jsonObject)
        {
            if (!(targetType is TransformedType))
                throw new InvalidOperationException("Update object not supported on type " + targetType.Name);

            var transformedType = targetType as TransformedType;

            foreach (var jsonProperty in jsonObject.Properties())
            {
                var targetProperty = transformedType.GetPropertyByJsonName(jsonProperty.Name);

                // TODO: Check whether update is allowed here!

                // Only supports value type properties for now, ie. shared types!
                // Should support "new" types in the future?
                var propertyType = targetProperty.PropertyType as SharedType;
                if (propertyType == null)
                {
                    throw new InvalidOperationException(
                        "Unable to set property of type " +
                        targetProperty.PropertyType.GetType().Name);
                }

                targetProperty.Setter(
                    target,
                    Convert.ChangeType(
                        ((JValue) jsonProperty.Value).Value,
                        propertyType.TargetType));
            }
        }


        private bool IsIList(IMappedType mappedType)
        {
            var sharedType = mappedType as SharedType;
            if (sharedType == null)
                return false;

            return IsIList(sharedType.TargetType);
        }


        private bool IsIList(object obj)
        {
            return
                IsIList(obj.GetType());
        }


        private bool IsIList(Type t)
        {
            return
                t.GetInterfaces().Any(
                    x => x.IsGenericType &&
                         x.GetGenericTypeDefinition() == typeof (IList<>)
                    /* && typeof(EntityBase).IsAssignableFrom(x.GetGenericArguments()[0])*/);
        }


        private bool TryGetCollectionElementType(
            IMappedType type, out IMappedType elementType, bool searchInterfaces = true)
        {
            elementType = null;

            var sharedType = type as SharedType;
            if (sharedType == null)
                return false;

            // First look if we're dealing directly with a known collection type

            var collectionInterface =
                sharedType.TargetType.GetInterfaces().FirstOrDefault(
                    x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof (ICollection<>));

            if (collectionInterface == null)
                return false;

            var genargs = collectionInterface.GetGenericArguments().First();

            if (genargs.IsGenericParameter)
            {
                // This means that we have to get the element type from one of the type arguments in TargetType
                IList<Type> targetTypeGenericArgs = sharedType.TargetType.GetGenericArguments();
                var indexOfElementType = targetTypeGenericArgs.IndexOf(genargs);
                elementType = sharedType.GenericArguments[indexOfElementType];
            }
            else
            {
                elementType = context.TypeMapper.GetClassMapping(genargs);
                //throw new NotImplementedException("Don't recognize collection type where element type is not specified in a generic parameter yet, like in SomeClass : ICollection<Blah>");
            }

            //if (sharedType.IsGenericType
            //    && knownGenericCollectionTypes.Any(x => x.IsAssignableFrom(sharedType.TargetType)))
            //{
            //    elementType = sharedType.GenericArguments[0];
            //}
            /*
            if (elementType == null && searchInterfaces)
            {
                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (TryGetCollectionElementType(interfaceType, out elementType, false))
                        break;
                }
            }*/

            return elementType != null;
        }


        private void WriteJsonExpandedOrReference(
            JsonWriter writer, object propertyValue, string subPath, IMappedType valueType, IMappedType expectedBaseType)
        {
            var isCollection = IsIList(valueType);

            if (context.PathToBeExpanded(subPath) || valueType.IsAlwaysExpanded
                || (isCollection && context.PathToBeExpanded(subPath + "!")))
            {
                var wrapper = context.CreateWrapperFor(propertyValue, subPath, expectedBaseType);
                wrapper.ToJson(writer);
            }
            else
            {
                writer.WriteStartObject();
                writer.WritePropertyName("_ref");

                if (isCollection)
                {
                    // HACK: Resolving URL like this is not the correct way
                    // Collections are exposed on uri http://host/entity/{id}/collection-name
                    var propertyName = subPath.Split(new char[] {'.'}, StringSplitOptions.RemoveEmptyEntries).Last();
                    writer.WriteValue(context.GetUri(target) + "/" + propertyName);
                }
                else
                {
                    writer.WriteValue(context.GetUri(propertyValue));

                    if (expectedBaseType != valueType)
                    {
                        writer.WritePropertyName("_type");
                        // TODO: This only works as long as mapped name equals source name
                        writer.WriteValue(propertyValue.GetType().Name);
                    }
                }

                if (context.DebugMode)
                {
                    writer.WritePropertyName("_path");
                    writer.WriteValue(subPath);
                }

                writer.WriteEndObject();
            }
        }
    }
}