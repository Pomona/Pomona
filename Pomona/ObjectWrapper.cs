using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

namespace Pomona
{
    public abstract class ObjectWrapper
    {
        public abstract void ToJson(TextWriter textWriter);
        public abstract void ToJson(JsonWriter writer);
    }


    public class ObjectWrapper<T> : ObjectWrapper
    {
        private readonly T target;
        private readonly string path;
        private readonly PomonaContext context;


        public ObjectWrapper(T target, string path, PomonaContext context)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (path == null)
                throw new ArgumentNullException("path");
            if (context == null)
                throw new ArgumentNullException("context");
            this.target = target;
            this.path = path;
            this.context = context;
        }

        public override void ToJson(TextWriter textWriter)
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


        private bool IsIList(Type t)
        {
            return
                t.GetInterfaces().Any(
                    x => x.IsGenericType &&
                         x.GetGenericTypeDefinition() == typeof(IList<>)
                    /* && typeof(EntityBase).IsAssignableFrom(x.GetGenericArguments()[0])*/);
        }
        public override void ToJson(JsonWriter writer)
        {
            var classDef = this.context.GetClassMapping<T>();

            if (IsIList(this.target))
            {
                writer.WriteStartArray();
                foreach (var child in ((IEnumerable)this.target))
                {
                    WriteJsonExpandedOrReference(writer, child, path, child.GetType());
                }
                writer.WriteEndArray();
            }
            else
            {

                writer.WriteStartObject();
                foreach (var propDef in classDef.Properties)
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

                    var serializeAsArray = IsIList(valueType);

                    if (this.context.IsWrittenAsObject(valueType) || serializeAsArray)
                    {
                        var propertyValue = propDef.Getter(this.target);

                        if (propertyValue == null)
                            writer.WriteNull();
                        else
                        {
                            WriteJsonExpandedOrReference(writer, propertyValue, subPath, valueType);
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


        private void WriteJsonExpandedOrReference(JsonWriter writer, object propertyValue, string subPath, Type valueType)
        {
            if (this.context.PathToBeExpanded(subPath) || IsIList(valueType))
            {
                var wrapper = this.context.CreateWrapperFor(propertyValue, subPath);
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