#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Newtonsoft.Json;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Json
{
    /// <summary>
    /// This class contains code-gen func to write out JSON faster!
    /// </summary>
    internal class PomonaJsonSerializerTypeEntry
    {
        private readonly List<PropertySpec> manuallyWrittenProperties = new List<PropertySpec>();
        private readonly TypeSpec type;
        private Expression<Action<JsonWriter, object, IContainer>> writePropertiesExpression;


        public PomonaJsonSerializerTypeEntry(TypeSpec type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            this.type = type;

            BuildAcceleratedPropertyWritingAction();
        }


        public IEnumerable<PropertySpec> ManuallyWrittenProperties
        {
            get { return this.manuallyWrittenProperties; }
        }

        public Action<JsonWriter, object, IContainer> WritePropertiesFunc { get; private set; }


        private void BuildAcceleratedPropertyWritingAction()
        {
            var writePropertyNameMethod = typeof(JsonWriter).GetMethod("WritePropertyName",
                                                                       new[] { typeof(string) });

            var expressions = new List<Expression>();
            var jsonWriterParam = Expression.Parameter(typeof(JsonWriter));
            var objValueParam = Expression.Parameter(typeof(object));
            var containerParam = Expression.Parameter(typeof(IContainer));
            var valueVariable = Expression.Variable(this.type.Type);

            expressions.Add(
                Expression.Assign(valueVariable, Expression.Convert(objValueParam, this.type.Type)));

            foreach (var prop in this.type.Properties.Where(p => p.IsSerialized))
            {
                MethodInfo method;
                if (TryGetJsonWriterMethodForWritingType(prop.PropertyType, out method))
                {
                    expressions.Add(
                        Expression.Call(
                            jsonWriterParam, writePropertyNameMethod, Expression.Constant(prop.JsonName)));
                    var getter = prop.Getter;
                    expressions.Add(
                        Expression.Call(jsonWriterParam, method,
                                        Expression.Convert(Expression.Invoke(Expression.Constant(getter), valueVariable, containerParam),
                                                           prop.PropertyType)));
                }
                else
                    this.manuallyWrittenProperties.Add(prop);
            }

            this.writePropertiesExpression = Expression.Lambda<Action<JsonWriter, object, IContainer>>(
                Expression.Block(new[] { valueVariable }, expressions), jsonWriterParam, objValueParam, containerParam);

            WritePropertiesFunc = this.writePropertiesExpression.Compile();

            this.manuallyWrittenProperties.Capacity = this.manuallyWrittenProperties.Count;
        }


        private static bool TryGetJsonWriterMethodForWritingType(TypeSpec type, out MethodInfo method)
        {
            method = null;
            if (type.SerializationMode != TypeSerializationMode.Value)
                return false;

            method = typeof(JsonWriter)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.Name == "WriteValue")
                .FirstOrDefault(
                    x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == type.Type);

            return method != null;
        }
    }
}