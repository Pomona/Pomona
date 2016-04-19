#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Internals;

namespace Pomona.Common.TypeSystem
{
    public class ConstructorSpec
    {
        private static readonly MethodInfo maybeMethod =
            ReflectionHelper.GetMethodDefinition<IConstructorControl<object>>(x => x.Optional());

        private static readonly MethodInfo requiresMethod =
            ReflectionHelper.GetMethodDefinition<IConstructorControl<object>>(x => x.Requires());


        public ConstructorSpec(LambdaExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            ConstructorExpression = expression;
        }


        public LambdaExpression ConstructorExpression { get; }

        public LambdaExpression InjectingConstructorExpression
        {
            get
            {
                var visitor = new CallConstructorPropertySourceVisitor();
                return (LambdaExpression)visitor.Visit(ConstructorExpression);
            }
        }

        public IEnumerable<ParameterSpec> ParameterSpecs
        {
            get
            {
                var visitor = new FindRequiredPropertiesVisitor();
                visitor.Visit(ConstructorExpression);
                return visitor.ParameterSpecs;
            }
        }


        public static ConstructorSpec FromConstructorInfo(ConstructorInfo constructorInfo,
                                                          Type convertedType = null,
                                                          Func<ConstructorSpec> defaultFactory = null)
        {
            if (constructorInfo == null)
                throw new ArgumentNullException(nameof(constructorInfo));
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
            if (
                mappedProperties.Any(
                    x => x.Property == null || !x.Param.ParameterType.IsAssignableFrom(x.Property.PropertyType)))
            {
                if (defaultFactory != null)
                    return defaultFactory();

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
            visitor.Visit(ConstructorExpression);
            return visitor.ParameterSpecs.FirstOrDefault(x => PropertiesAreEquivalent(propertyInfo, x));
        }


        private static bool PropertiesAreEquivalent(PropertyInfo propertyInfo, ParameterSpec x)
        {
            return propertyInfo == x.PropertyInfo
                   || propertyInfo.GetBaseDefinition() == x.PropertyInfo.GetBaseDefinition();
        }

        #region Nested type: CallConstructorPropertySourceVisitor

        public class CallConstructorPropertySourceVisitor : FindRequiredPropertiesVisitor
        {
            private MethodInfo getValueMethod;
            private ParameterExpression newParam;
            private ParameterExpression oldParam;


            protected override Expression VisitArgumentBinding(Expression node,
                                                               PropertyInfo property,
                                                               bool isRequired,
                                                               int position,
                                                               Type convertedToType)
            {
                base.VisitArgumentBinding(node, property, isRequired, position, convertedToType);

                var defaultFactory = !isRequired
                    ? (Expression)Expression.Lambda(Expression.Default(convertedToType))
                    : Expression.Constant(null, typeof(Func<>).MakeGenericType(convertedToType));
                return Expression.Call(this.newParam,
                                       this.getValueMethod.MakeGenericMethod(convertedToType),
                                       Expression.Constant(property),
                                       defaultFactory);
            }


            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                if (this.oldParam != null)
                    return base.VisitLambda(node);
                Type[] paramGenArgs;
                if (node.Parameters.Count != 1
                    || !(this.oldParam = node.Parameters[0]).Type.TryExtractTypeArguments(
                        typeof(IConstructorControl<>),
                        out paramGenArgs))
                    throw new InvalidOperationException("Lambda is not of correct type");
                var propSourceGenericTypeInstance = typeof(IConstructorPropertySource);
                this.newParam = Expression.Parameter(propSourceGenericTypeInstance);
                this.getValueMethod = propSourceGenericTypeInstance.GetMethod("GetValue");
                return Expression.Lambda(Visit(node.Body), this.newParam);
            }


            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node == this.oldParam)
                    return this.newParam;
                return base.VisitParameter(node);
            }
        }

        #endregion

        #region Nested type: FindRequiredPropertiesVisitor

        public class FindRequiredPropertiesVisitor : ExpressionVisitor
        {
            private readonly List<ParameterSpec> parameterSpecs = new List<ParameterSpec>();

            public IList<ParameterSpec> ParameterSpecs => this.parameterSpecs;


            protected virtual Expression VisitArgumentBinding(Expression node,
                                                              PropertyInfo property,
                                                              bool isRequired,
                                                              int position,
                                                              Type convertedToType)
            {
                this.parameterSpecs.Add(new ParameterSpec(isRequired, property, position));
                return node;
            }


            protected virtual Expression VisitContextReference(MemberExpression node)
            {
                return node;
            }


            protected override Expression VisitMember(MemberExpression node)
            {
                Expression result;
                if (TryRecognizeMemberExpression(node, out result))
                    return result;

                return base.VisitMember(node);
            }


            protected virtual Expression VisitParentReference(MemberExpression node)
            {
                return node;
            }


            protected override Expression VisitUnary(UnaryExpression node)
            {
                Expression result;
                if (TryRecognizeMemberExpression(node, out result))
                    return result;

                return base.VisitUnary(node);
            }


            private bool TryRecognizeMemberExpression(Expression node, out Expression result)
            {
                result = null;
                var outerNode = node;
                var convertedToType = node.Type;

                while (node.NodeType == ExpressionType.Convert)
                    node = ((UnaryExpression)node).Operand;

                var memberExpr = node as MemberExpression;
                if (memberExpr == null)
                    return false;

                var propInfo = memberExpr.Member as PropertyInfo;
                var declaringType = memberExpr.Member.DeclaringType;
                var objExpr = memberExpr.Expression as MethodCallExpression;

                var isRequired = objExpr != null && objExpr.Method.UniqueToken() == requiresMethod.UniqueToken();
                var isMaybe = objExpr != null && objExpr.Method.UniqueToken() == maybeMethod.UniqueToken();

                if (propInfo != null && declaringType != null && objExpr != null && (isRequired || isMaybe)
                    && objExpr.Object is ParameterExpression)
                {
                    result = VisitArgumentBinding(outerNode,
                                                  propInfo,
                                                  isRequired,
                                                  this.parameterSpecs.Count,
                                                  convertedToType);
                    return true;
                }
                return false;
            }
        }

        #endregion

        #region Nested type: ParameterSpec

        public class ParameterSpec
        {
            internal ParameterSpec(bool isRequired, PropertyInfo propertyInfo, int position)
            {
                if (propertyInfo == null)
                    throw new ArgumentNullException(nameof(propertyInfo));
                IsRequired = isRequired;
                PropertyInfo = propertyInfo;
                Position = position;
            }


            public bool IsRequired { get; }

            public int Position { get; }

            public PropertyInfo PropertyInfo { get; }
        }

        #endregion
    }
}