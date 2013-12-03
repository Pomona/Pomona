#region License

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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Internals;

namespace Pomona.Common.TypeSystem
{
    public interface IConstructorPropertySource<T> : IConstructorControl<T>
    {
        TProperty GetValue<TProperty>(PropertyInfo propertyInfo, Func<TProperty> defaultFactory);
    }


    public class ConstructorSpec
    {
        private static readonly MethodInfo maybeMethod =
            ReflectionHelper.GetMethodDefinition<IConstructorControl<object>>(x => x.Optional());

        private static readonly MethodInfo requiresMethod =
            ReflectionHelper.GetMethodDefinition<IConstructorControl<object>>(x => x.Requires());

        private readonly LambdaExpression expression;


        public ConstructorSpec(LambdaExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            this.expression = expression;
        }


        public LambdaExpression InjectingConstructorExpression
        {
            get
            {
                var visitor = new CallConstructorPropertySourceVisitor();
                return (LambdaExpression)visitor.Visit(expression);
            }
        }

        public LambdaExpression ConstructorExpression
        {
            get { return this.expression; }
        }

        public IEnumerable<ParameterSpec> ParameterSpecs
        {
            get
            {
                var visitor = new FindRequiredPropertiesVisitor();
                visitor.Visit(this.expression);
                return visitor.ParameterSpecs;
            }
        }


        public static ConstructorSpec FromConstructorInfo(ConstructorInfo constructorInfo, Type convertedType = null)
        {
            if (constructorInfo == null)
                throw new ArgumentNullException("constructorInfo");
            var type = convertedType ?? constructorInfo.DeclaringType;
            if (type == null)
                throw new InvalidOperationException("DeclaringType needs to be defined");
            var constructorControlType = typeof(IConstructorControl<>).MakeGenericType(type);
            var constructorControlParam = Expression.Parameter(constructorControlType);

            var properties =
                type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).ToDictionary(
                    x => x.Name,
                    x => x,
                    StringComparer.InvariantCultureIgnoreCase);

            var mappedProperties =
                constructorInfo.GetParameters().Select(
                    x => new { x.Name, Param = x, Property = properties.SafeGet(x.Name) }).ToList();
            if (mappedProperties.Any(x => x.Property == null))
            {
                throw new InvalidOperationException(
                    "Unable to map properties to ctor, could not find properties for the following arguments: "
                    + string.Join(", ", mappedProperties.Select(x => x.Name)));
            }

            var requiresMethodInstance = constructorControlType.GetMethod("Requires");
            var optionalMethodInstance = constructorControlType.GetMethod("Optional");
            var arguments =
                mappedProperties.Select(
                    x =>
                        Expression.Property(
                            Expression.Call(constructorControlParam,
                                x.Param.IsOptional ? optionalMethodInstance : requiresMethodInstance),
                            x.Property))
                    .ToList();

            Expression newExpression = Expression.New(constructorInfo, arguments);
            if (convertedType != null)
                newExpression = Expression.Convert(newExpression, convertedType);
            var expression = Expression.Lambda(newExpression, constructorControlParam);
            return new ConstructorSpec(expression);
        }

        public ParameterSpec GetParameterSpec(PropertyInfo propertyInfo)
        {
            var visitor = new FindRequiredPropertiesVisitor();
            visitor.Visit(this.expression);
            return visitor.ParameterSpecs.FirstOrDefault(x => PropertiesAreEquivalent(propertyInfo, x));
        }


        private static bool PropertiesAreEquivalent(PropertyInfo propertyInfo, ParameterSpec x)
        {
            return propertyInfo == x.PropertyInfo
                   || propertyInfo.GetBaseDefinition() == x.PropertyInfo.GetBaseDefinition();
        }

        #region Nested type: FindRequiredPropertiesVisitor

        public class CallConstructorPropertySourceVisitor : FindRequiredPropertiesVisitor
        {
            private ParameterExpression oldParam;
            private ParameterExpression newParam;
            private MethodInfo getValueMethod;


            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node == oldParam)
                    return newParam;
                return base.VisitParameter(node);
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                if (oldParam != null)
                    return base.VisitLambda(node);
                Type[] paramGenArgs;
                if (node.Parameters.Count != 1 || !(oldParam = node.Parameters[0]).Type.TryExtractTypeArguments(typeof(IConstructorControl<>), out paramGenArgs))
                    throw new InvalidOperationException("Lambda is not of correct type");
                Type propSourceGenericTypeInstance = typeof(IConstructorPropertySource<>).MakeGenericType(paramGenArgs);
                this.newParam = Expression.Parameter(propSourceGenericTypeInstance);
                this.getValueMethod = propSourceGenericTypeInstance.GetMethod("GetValue");
                return Expression.Lambda(Visit(node.Body), newParam);
            }


            protected override Expression VisitArgumentBinding(MemberExpression node, PropertyInfo property, bool isRequired, int position)
            {
                base.VisitArgumentBinding(node, property, isRequired, position);

                var defaultFactory = !isRequired ? (Expression)Expression.Lambda(Expression.Default(property.PropertyType)) : Expression.Constant(null, typeof(Func<>).MakeGenericType(property.PropertyType));
                return Expression.Call(newParam, getValueMethod.MakeGenericMethod(property.PropertyType),
                    Expression.Constant(property), defaultFactory);
            }
        }

        public class FindRequiredPropertiesVisitor : ExpressionVisitor
        {
            private List<ParameterSpec> parameterSpecs = new List<ParameterSpec>();

            public IList<ParameterSpec> ParameterSpecs
            {
                get { return this.parameterSpecs; }
            }


            protected virtual Expression VisitArgumentBinding(MemberExpression node,
                PropertyInfo property,
                bool isRequired,
                int position)
            {
                this.parameterSpecs.Add(new ParameterSpec(isRequired, property, position));
                return base.VisitMember(node);
            }


            protected virtual Expression VisitContextReference(MemberExpression node)
            {
                return node;
            }


            protected virtual Expression VisitParentReference(MemberExpression node)
            {
                return node;
            }


            protected override Expression VisitMember(MemberExpression node)
            {
                var propInfo = node.Member as PropertyInfo;
                var declaringType = node.Member.DeclaringType;
                var objExpr = node.Expression as MethodCallExpression;

                var isRequired = objExpr != null && objExpr.Method.UniqueToken() == requiresMethod.UniqueToken();
                var isMaybe = objExpr != null && objExpr.Method.UniqueToken() == maybeMethod.UniqueToken();

                if (propInfo != null && declaringType != null && objExpr != null && (isRequired || isMaybe)
                    && objExpr.Object is ParameterExpression)
                    return VisitArgumentBinding(node, propInfo, isRequired, this.parameterSpecs.Count);

                return base.VisitMember(node);
            }
        }

        #endregion

        #region Nested type: ParameterSpec

        public class ParameterSpec
        {
            private bool isRequired;
            private int position;
            private PropertyInfo propertyInfo;


            internal ParameterSpec(bool isRequired, PropertyInfo propertyInfo, int position)
            {
                if (propertyInfo == null)
                    throw new ArgumentNullException("propertyInfo");
                this.isRequired = isRequired;
                this.propertyInfo = propertyInfo;
                this.position = position;
            }


            public bool IsRequired
            {
                get { return this.isRequired; }
            }

            public int Position
            {
                get { return this.position; }
            }

            public PropertyInfo PropertyInfo
            {
                get { return this.propertyInfo; }
            }
        }

        #endregion
    }
}