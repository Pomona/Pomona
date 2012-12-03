using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    /// <summary>
    /// This class contains code-gen func to write out JSON faster!
    /// </summary>
    internal class PomonaJsonSerializerTypeEntry
    {
        private readonly List<IPropertyInfo> manuallyWrittenProperties = new List<IPropertyInfo>();
        private readonly IMappedType type;
        private Expression<Action<JsonWriter, object>> writePropertiesExpression;
        private Action<JsonWriter, object> writePropertiesFunc;


        public PomonaJsonSerializerTypeEntry(IMappedType type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            this.type = type;

            BuildAcceleratedPropertyWritingAction();
        }


        public IEnumerable<IPropertyInfo> ManuallyWrittenProperties
        {
            get { return manuallyWrittenProperties; }
        }

        public Action<JsonWriter, object> WritePropertiesFunc
        {
            get { return writePropertiesFunc; }
        }


        private static bool TryGetJsonWriterMethodForWritingType(IMappedType type, out MethodInfo method)
        {
            method = null;
            if (type.SerializationMode != TypeSerializationMode.Value)
                return false;

            method = typeof (JsonWriter)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.Name == "WriteValue")
                .FirstOrDefault(
                    x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == type.MappedTypeInstance);

            return method != null;
        }


        private void BuildAcceleratedPropertyWritingAction()
        {
            var writePropertyNameMethod = typeof (JsonWriter).GetMethod("WritePropertyName");

            var expressions = new List<Expression>();
            var jsonWriterParam = Expression.Parameter(typeof (JsonWriter));
            var objValueParam = Expression.Parameter(typeof (object));
            var valueVariable = Expression.Variable(type.MappedTypeInstance);

            expressions.Add(
                Expression.Assign(valueVariable, Expression.Convert(objValueParam, type.MappedTypeInstance)));

            foreach (var prop in type.Properties)
            {
                MethodInfo method;
                if (TryGetJsonWriterMethodForWritingType(prop.PropertyType, out method))
                {
                    expressions.Add(
                        Expression.Call(
                            jsonWriterParam, writePropertyNameMethod, Expression.Constant(prop.JsonName)));
                    expressions.Add(
                        Expression.Call(jsonWriterParam, method, prop.CreateGetterExpression(valueVariable)));
                }
                else
                    manuallyWrittenProperties.Add(prop);
            }

            writePropertiesExpression = Expression.Lambda<Action<JsonWriter, object>>(
                Expression.Block(new[] {valueVariable}, expressions), jsonWriterParam, objValueParam);

            writePropertiesFunc = writePropertiesExpression.Compile();

            manuallyWrittenProperties.Capacity = manuallyWrittenProperties.Count;
        }
    }
}