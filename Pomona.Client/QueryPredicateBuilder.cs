#region License

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

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Internals;

namespace Pomona.Client
{
    public abstract class QueryPredicateBuilder
    {
        protected static readonly ReadOnlyDictionary<ExpressionType, string> binaryExpressionNodeDict;


        static QueryPredicateBuilder()
        {
            var binExprDict = new Dictionary<ExpressionType, string>()
            {
                { ExpressionType.AndAlso, "and" },
                { ExpressionType.OrElse, "or" },
                { ExpressionType.Equal, "eq" },
                { ExpressionType.GreaterThan, "gt" },
                { ExpressionType.GreaterThanOrEqual, "ge" },
                { ExpressionType.LessThan, "lt" },
                { ExpressionType.LessThanOrEqual, "le" }
            };

            binaryExpressionNodeDict = new ReadOnlyDictionary<ExpressionType, string>(binExprDict);
        }
    }

    public class QueryPredicateBuilder<T> : QueryPredicateBuilder
    {
        private readonly Expression<Func<T, bool>> lambda;


        public QueryPredicateBuilder(Expression<Func<T, bool>> lambda)
        {
            if (lambda == null)
                throw new ArgumentNullException("expr");
            this.lambda = lambda;
        }


        private ParameterExpression InstanceParameter
        {
            get { return this.lambda.Parameters[0]; }
        }


        public override string ToString()
        {
            return Build(this.lambda.Body);
        }


        private static string DateTimeToString(DateTime dt)
        {
            var roundedDt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Kind);
            if (roundedDt == dt)
            {
                if (dt.Kind == DateTimeKind.Utc)
                    return dt.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                return dt.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
            }
            return dt.ToString("O");
        }


        private static string GetJsonTypeName(Type typeOperand)
        {
            var resourceInfoAttribute =
                typeOperand.GetCustomAttributes(typeof(ResourceInfoAttribute), false).
                    OfType<ResourceInfoAttribute>().First();
            var jsonTypeName = resourceInfoAttribute.JsonTypeName;
            return jsonTypeName;
        }


        private static string GetMemberName(MemberInfo member)
        {
            // Do it JSON style camelCase
            return member.Name.Substring(0, 1).ToLower() + member.Name.Substring(1);
        }


        private static object GetMemberValue(object obj, MemberInfo member)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (member == null)
                throw new ArgumentNullException("member");
            var propertyInfo = member as PropertyInfo;
            if (propertyInfo != null)
                return propertyInfo.GetValue(obj, null);

            var fieldInfo = member as FieldInfo;
            if (fieldInfo != null)
                return fieldInfo.GetValue(obj);

            throw new InvalidOperationException("Don't know how to get value from member of type " + member.GetType());
        }


        private string Build(Expression expr)
        {
            var binaryExpr = expr as BinaryExpression;
            if (binaryExpr != null)
                return BuildFromBinaryExpression(binaryExpr);

            var memberExpr = expr as MemberExpression;
            if (memberExpr != null)
                return BuildFromMemberExpression(memberExpr);

            var constantExpr = expr as ConstantExpression;
            if (constantExpr != null)
                return BuildFromConstantExpression(constantExpr);

            var callExpr = expr as MethodCallExpression;
            if (callExpr != null)
                return BuildFromMethodCallExpression(callExpr);

            var typeBinaryExpression = expr as TypeBinaryExpression;
            if (typeBinaryExpression != null)
                return BuildFromTypeBinaryExpression(typeBinaryExpression);

            var unaryExpression = expr as UnaryExpression;
            if (unaryExpression != null)
                return BuildFromUnaryExpression(unaryExpression);

            throw new NotImplementedException("NodeType " + expr.NodeType + " not yet handled.");
        }


        private string BuildFromBinaryExpression(BinaryExpression binaryExpr)
        {
            string opString;
            if (!binaryExpressionNodeDict.TryGetValue(binaryExpr.NodeType, out opString))
            {
                throw new NotImplementedException(
                    "BinaryExpression NodeType " + binaryExpr.NodeType + " not yet handled.");
            }
            return string.Format("({0} {1} {2})", Build(binaryExpr.Left), opString, Build(binaryExpr.Right));
        }


        private string BuildFromConstantExpression(ConstantExpression constantExpr)
        {
            var valueType = constantExpr.Type;
            var value = constantExpr.Value;
            return GetEncodedConstant(valueType, value);
        }


        private string BuildFromMemberExpression(MemberExpression memberExpr)
        {
            // This gets weird with closures, see:
            // http://blog.nexterday.com/post/Automatic-compilation-of-Linq-to-SQL-queries.aspx

            object value;
            if (IsClosureMemberAccess(memberExpr, out value))
                return GetEncodedConstant(value.GetType(), value);

            if (memberExpr.Expression != InstanceParameter)
                return string.Format("{0}.{1}", Build(memberExpr.Expression), GetMemberName(memberExpr.Member));
            return GetMemberName(memberExpr.Member);
        }


        private string BuildFromMethodCallExpression(MethodCallExpression callExpr)
        {
            if (callExpr.Method == ReflectionHelper.GetInstanceMethodInfo<IDictionary<string, string>>(x => x[null]))
            {
                var indexKeyExpression = Build(callExpr.Arguments[0]);
                /*
                 * 
                 * TODO: Simplify dictionary member access like in JS.
                 * So instead of writing dict['boo'] we can write dict.boo
                if (indexKeyExpression.StartsWith("'") && indexKeyExpression.EndsWith("'"))
                {
                    // TODO: Decode!
                    return string.Format("{0}.{1}", Build(callExpr.Object), );
                }*/

                return string.Format("{0}[{1}]", Build(callExpr.Object), indexKeyExpression);
            }

            if (callExpr.Method == ReflectionHelper.GetInstanceMethodInfo<string>(s => s.StartsWith(null)))
                return string.Format("startswith({0},{1})", Build(callExpr.Object), Build(callExpr.Arguments[0]));

            if (callExpr.Method == ReflectionHelper.GetInstanceMethodInfo<string>(s => s.Contains(null)))
                return string.Format("substringof({0},{1})", Build(callExpr.Arguments[0]), Build(callExpr.Object));

            throw new NotImplementedException(
                "Don't know what to do with method " + callExpr.Method.Name + " declared in "
                + callExpr.Method.DeclaringType.FullName);
        }


        private string BuildFromTypeBinaryExpression(TypeBinaryExpression typeBinaryExpression)
        {
            switch (typeBinaryExpression.NodeType)
            {
                case ExpressionType.TypeIs:
                    var typeOperand = typeBinaryExpression.TypeOperand;
                    if (!typeOperand.IsInterface || !typeof(IClientResource).IsAssignableFrom(typeOperand))
                    {
                        throw new InvalidOperationException(
                            typeOperand.FullName
                            + " is not an interface and/or does not implement type IClientResource.");
                    }
                    if (typeBinaryExpression.Expression != InstanceParameter)
                    {
                        throw new NotImplementedException(
                            "Only know how to do TypeIs when target is instance parameter for now..");
                    }

                    // TODO: Proper typename resolving

                    var jsonTypeName = GetJsonTypeName(typeOperand);
                    return string.Format("isof({0})", jsonTypeName);
                    break;
                default:
                    throw new NotImplementedException(
                        "Don't know how to handle TypeBinaryExpression with NodeType " + typeBinaryExpression.NodeType);
            }
        }


        private string BuildFromUnaryExpression(UnaryExpression unaryExpression)
        {
            switch (unaryExpression.NodeType)
            {
                case ExpressionType.Convert:
                    if (unaryExpression.Operand != InstanceParameter)
                        throw new NotImplementedException("Only know how to cast `this` to something else");

                    return "cast(" + GetJsonTypeName(unaryExpression.Type) + ")";
                default:
                    throw new NotImplementedException(
                        "NodeType " + unaryExpression.NodeType + " in UnaryExpression not yet handled.");
            }
        }


        private string EncodeString(string text)
        {
            // TODO: IMPORTANT! Proper encoding!!

            return "'" + text + "'";
        }


        private string GetEncodedConstant(Type valueType, object value)
        {
            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.String:
                    return EncodeString((string)value);
                case TypeCode.Int32:
                    return value.ToString();
                case TypeCode.DateTime:
                    return string.Format("datetime'{0}'", DateTimeToString((DateTime)value));
                case TypeCode.Object:
                    if (value == null)
                        return "null";
                    if (value is Guid)
                        return "guid'" + ((Guid)value).ToString() + "'";
                    break;
                case TypeCode.Boolean:
                    return ((bool)value) ? "true" : "false";
                default:
                    break;
            }
            throw new NotImplementedException(
                "Don't know how to send constant of type " + valueType.FullName + " yet..");
        }


        private bool IsClosureMemberAccess(MemberExpression memberExpression, out object value)
        {
            value = null;
            var member = memberExpression.Member;
            if (member.DeclaringType.Name.StartsWith("<>c__") && member.MemberType == MemberTypes.Field)
            {
                var field = (FieldInfo)member;
                var obj = ((ConstantExpression)memberExpression.Expression).Value;

                //Add the value to the extraction list
                value = field.GetValue(obj);
                return true;
            }

            var innerMemberExpr = memberExpression.Expression as MemberExpression;

            if (innerMemberExpr == null)
                return false;

            object innerValue;
            if (IsClosureMemberAccess(innerMemberExpr, out innerValue))
            {
                value = GetMemberValue(innerValue, member);
                return true;
            }

            return false;
        }
    }
}