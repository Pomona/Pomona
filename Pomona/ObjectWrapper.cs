using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

namespace Pomona
{
    public class ObjectWrapper
    {
        private readonly object target;
        private readonly string path;
        private readonly PomonaContext context;
        private readonly IMappedType expectedBaseType;
        private readonly IMappedType targetType;


        public ObjectWrapper(object target, string path, PomonaContext context, IMappedType expectedBaseType)
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
            this.targetType = context.ClassMappingFactory.GetClassMapping(target.GetType());
        }

        public virtual void ToJson(TextWriter textWriter)
        {
            var jsonWriter = new JsonTextWriter(textWriter);
            jsonWriter.Formatting = Formatting.Indented;
            ToJson(jsonWriter);
            jsonWriter.Flush();
        }

        private bool IsIList(object obj)
        {
            return
                IsIList(obj.GetType());
        }

        private static Type[] knownGenericCollectionTypes = new[] {typeof(IList<>), typeof(ICollection<>), typeof(List<>)};

        static bool TryGetCollectionElementType(IMappedType type, out IMappedType elementType, bool searchInterfaces = true)
        {
            elementType = null;

            var sharedType = type as SharedType;
            if (sharedType == null)
                return false;

            // First look if we're dealing directly with a known collection type
            if (sharedType.IsGenericType && knownGenericCollectionTypes.Any(x => x.IsAssignableFrom(sharedType.TargetType)))
            {
                elementType = sharedType.GenericArguments[0];
            }
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



        private bool IsIList(Type t)
        {
            return
                t.GetInterfaces().Any(
                    x => x.IsGenericType &&
                         x.GetGenericTypeDefinition() == typeof(IList<>)
                    /* && typeof(EntityBase).IsAssignableFrom(x.GetGenericArguments()[0])*/);
        }
        public virtual void ToJson(JsonWriter writer)
        {
            IMappedType collectionElementType;
            if (TryGetCollectionElementType(targetType, out collectionElementType))
            {
                writer.WriteStartArray();
                foreach (var child in ((IEnumerable)this.target))
                {
                    WriteJsonExpandedOrReference(writer, child, path, context.ClassMappingFactory.GetClassMapping(child.GetType()), collectionElementType);
                }
                writer.WriteEndArray();
            }
            else
            {
                var transformedType = (TransformedType) this.targetType;
                writer.WriteStartObject();

                if (expectedBaseType != targetType)
                {
                    writer.WritePropertyName("_type");
                    writer.WriteValue(targetType.Name);
                }

                foreach (var propDef in transformedType.Properties)
                {
                    var subPath = this.path + "." + propDef.JsonName;
                    var value = propDef.Getter(this.target);
                    
                    writer.WritePropertyName(propDef.JsonName);
                    if (value == null)
                    {
                        writer.WriteNull();
                        continue;
                    }

                    var valueType = value.GetType();
                    var valueTypeMapping = context.ClassMappingFactory.GetClassMapping(valueType);

                    var serializeAsArray = IsIList(valueType);

                    if (this.context.IsWrittenAsObject(valueType) || serializeAsArray)
                    {
                        var propertyValue = propDef.Getter(this.target);

                        if (propertyValue == null)
                            writer.WriteNull();
                        else
                        {
                            WriteJsonExpandedOrReference(writer, propertyValue, subPath, valueTypeMapping, propDef.PropertyType);
                        }
                    }
                    else
                    {
                        writer.WriteValue(propDef.Getter(this.target));
                    }
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


        private void WriteJsonExpandedOrReference(JsonWriter writer, object propertyValue, string subPath, IMappedType valueType, IMappedType expectedBaseType)
        {
            if (this.context.PathToBeExpanded(subPath) || IsIList(valueType))
            {
                var wrapper = this.context.CreateWrapperFor(propertyValue, subPath, expectedBaseType);
                wrapper.ToJson(writer);
            }
            else
            {
                writer.WriteStartObject();
                writer.WritePropertyName("_uri");
                writer.WriteValue(this.context.GetUri(propertyValue));
                
                if (context.DebugMode)
                {
                    writer.WritePropertyName("_path");
                    writer.WriteValue(subPath);
                }

                writer.WriteEndObject();
            }
        }


        public string Path
        {
            get { return this.path; }
        }
    }
}