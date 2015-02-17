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
        private Action<JsonWriter, object, IContainer> writePropertiesFunc;


        public PomonaJsonSerializerTypeEntry(TypeSpec type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            this.type = type;

            BuildAcceleratedPropertyWritingAction();
        }


        public IEnumerable<PropertySpec> ManuallyWrittenProperties
        {
            get { return manuallyWrittenProperties; }
        }

        public Action<JsonWriter, object, IContainer> WritePropertiesFunc
        {
            get { return writePropertiesFunc; }
        }


        private static bool TryGetJsonWriterMethodForWritingType(TypeSpec type, out MethodInfo method)
        {
            method = null;
            if (type.SerializationMode != TypeSerializationMode.Value)
                return false;

            method = typeof (JsonWriter)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.Name == "WriteValue")
                .FirstOrDefault(
                    x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == type.Type);

            return method != null;
        }


        private void BuildAcceleratedPropertyWritingAction()
        {
            var writePropertyNameMethod = typeof (JsonWriter).GetMethod("WritePropertyName",
                                                                        new[] {typeof (string)});

            var expressions = new List<Expression>();
            var jsonWriterParam = Expression.Parameter(typeof (JsonWriter));
            var objValueParam = Expression.Parameter(typeof (object));
            var containerParam = Expression.Parameter(typeof(IContainer));
            var valueVariable = Expression.Variable(type.Type);

            expressions.Add(
                Expression.Assign(valueVariable, Expression.Convert(objValueParam, type.Type)));

            foreach (var prop in type.Properties.Where(p => p.IsSerialized))
            {
                MethodInfo method;
                if (TryGetJsonWriterMethodForWritingType(prop.PropertyType, out method))
                {
                    expressions.Add(
                        Expression.Call(
                            jsonWriterParam, writePropertyNameMethod, Expression.Constant(prop.JsonName)));
                    var getter = prop.GetterFunc;
                    expressions.Add(
                        Expression.Call(jsonWriterParam, method, Expression.Convert(Expression.Invoke(Expression.Constant(getter), valueVariable, containerParam), prop.PropertyType)));
                }
                else
                    manuallyWrittenProperties.Add(prop);
            }

            writePropertiesExpression = Expression.Lambda<Action<JsonWriter, object, IContainer>>(
                Expression.Block(new[] {valueVariable}, expressions), jsonWriterParam, objValueParam, containerParam);

            writePropertiesFunc = writePropertiesExpression.Compile();

            manuallyWrittenProperties.Capacity = manuallyWrittenProperties.Count;
        }
    }
}