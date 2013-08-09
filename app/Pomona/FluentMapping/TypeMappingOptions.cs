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
using System.Linq.Expressions;
using System.Reflection;
using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.Internals;

namespace Pomona.FluentMapping
{
    internal sealed class TypeMappingOptions
    {
        private static readonly MethodInfo getConfiguratorGenericMethod =
            ReflectionHelper.GetMethodDefinition<TypeMappingOptions>(x => x.GetConfigurator<object>());

        private readonly Type declaringType;

        private readonly IDictionary<string, PropertyMappingOptions> propertyOptions =
            new Dictionary<string, PropertyMappingOptions>();

        private ConstructorInfo constructor;

        private DefaultPropertyInclusionMode defaultPropertyInclusionMode =
            DefaultPropertyInclusionMode.AllPropertiesAreIncludedByDefault;

        private bool? isExposedAsRepository;

        private bool? isIndependentTypeRoot;

        private bool? isUriBaseType;
        private bool? isValueObject;

        private string pluralName;
        private Type postResponseType;

        public TypeMappingOptions(Type declaringType)
        {
            this.declaringType = declaringType;
        }

        public bool? IsIndependentTypeRoot
        {
            get { return isIndependentTypeRoot; }
        }

        public ConstructorInfo Constructor
        {
            get { return constructor; }
        }

        public DefaultPropertyInclusionMode DefaultPropertyInclusionMode
        {
            get { return defaultPropertyInclusionMode; }
            set { defaultPropertyInclusionMode = value; }
        }

        public bool? IsUriBaseType
        {
            get { return isUriBaseType; }
        }

        public bool? IsValueObject
        {
            get { return isValueObject; }
        }

        public string PluralName
        {
            get { return pluralName; }
        }


        public bool? IsExposedAsRepository
        {
            get { return isExposedAsRepository; }
        }

        public Type PostResponseType
        {
            get { return postResponseType; }
        }

        public IDictionary<string, PropertyMappingOptions> PropertyOptions
        {
            get { return propertyOptions; }
        }


        internal PropertyMappingOptions GetPropertyOptions(string name)
        {
            var propInfo = declaringType.GetProperty(name,
                                                     BindingFlags.Instance | BindingFlags.Public |
                                                     BindingFlags.NonPublic);
            if (propInfo == null)
            {
                throw new InvalidOperationException(
                    "No property with name " + name + " found on type " + declaringType.FullName);
            }

            return propertyOptions.GetOrCreate(propInfo.Name, () => new PropertyMappingOptions(propInfo));
        }


        private PropertyMappingOptions GetPropertyOptions(Expression propertyExpr)
        {
            if (propertyExpr == null)
                throw new ArgumentNullException("propertyExpr");
            var propInfo = propertyExpr.ExtractPropertyInfo();
            var propOptions = propertyOptions.GetOrCreate(
                propInfo.Name, () => new PropertyMappingOptions(propInfo));
            return propOptions;
        }

        internal object GetConfigurator(Type exposedAsType)
        {
            if (exposedAsType == null) throw new ArgumentNullException("exposedAsType");
            return getConfiguratorGenericMethod.MakeGenericMethod(exposedAsType).Invoke(this, null);
        }

        internal ITypeMappingConfigurator<TDeclaringType> GetConfigurator<TDeclaringType>()
        {
            return new Configurator<TDeclaringType>(this);
        }

        private class Configurator<TDeclaringType> : ITypeMappingConfigurator<TDeclaringType>
        {
            private static MethodInfo optionalMethod =
                ReflectionHelper.GetMethodDefinition<IConstructorControl>(x => x.Optional(0));

            private readonly TypeMappingOptions owner;

            public Configurator(TypeMappingOptions owner)
            {
                this.owner = owner;
            }

            public ITypeMappingConfigurator<TDeclaringType> AllPropertiesAreExcludedByDefault()
            {
                owner.defaultPropertyInclusionMode = DefaultPropertyInclusionMode.AllPropertiesAreExcludedByDefault;
                return this;
            }


            public ITypeMappingConfigurator<TDeclaringType> AllPropertiesAreIncludedByDefault()
            {
                owner.defaultPropertyInclusionMode = DefaultPropertyInclusionMode.AllPropertiesAreIncludedByDefault;
                return this;
            }


            public ITypeMappingConfigurator<TDeclaringType> AllPropertiesRequiresExplicitMapping()
            {
                owner.defaultPropertyInclusionMode = DefaultPropertyInclusionMode.AllPropertiesRequiresExplicitMapping;
                return this;
            }


            public ITypeMappingConfigurator<TDeclaringType> AsEntity()
            {
                throw new NotImplementedException();
            }

            public ITypeMappingConfigurator<TDeclaringType> ExposedAsRepository()
            {
                owner.isExposedAsRepository = true;
                return this;
            }


            public ITypeMappingConfigurator<TDeclaringType> AsUriBaseType()
            {
                owner.isUriBaseType = true;
                return this;
            }


            public ITypeMappingConfigurator<TDeclaringType> AsValueObject()
            {
                owner.isValueObject = true;
                return this;
            }

            public ITypeMappingConfigurator<TDeclaringType> AsIndependentTypeRoot()
            {
                owner.isIndependentTypeRoot = true;
                return this;
            }


            public ITypeMappingConfigurator<TDeclaringType> ConstructedUsing(
                Expression<Func<TDeclaringType, TDeclaringType>> expr)
            {
                // Constructor fluent definitions should not be inherited to subclasses (because it wouldn't work).
                if (!owner.declaringType.IsAssignableFrom(typeof (TDeclaringType)))
                    return this;

                if (expr == null)
                    throw new ArgumentNullException("expr");

                var constructExpr = expr.Body as NewExpression;

                ConstructedUsing(constructExpr);

                return this;
            }

            public ITypeMappingConfigurator<TDeclaringType> ConstructedUsing(
                Expression<Func<TDeclaringType, IConstructorControl, TDeclaringType>> expr)
            {
                // Constructor fluent definitions should not be inherited to subclasses (because it wouldn't work).
                if (!owner.declaringType.IsAssignableFrom(typeof (TDeclaringType)))
                    return this;

                if (expr == null)
                    throw new ArgumentNullException("expr");

                var constructExpr = expr.Body as NewExpression;

                ConstructedUsing(constructExpr);

                return this;
            }

            public ITypeMappingConfigurator<TDeclaringType> WithPluralName(string pluralName)
            {
                owner.pluralName = pluralName;
                return this;
            }


            public ITypeMappingConfigurator<TDeclaringType> Exclude(Expression<Func<TDeclaringType, object>> property)
            {
                var propOptions = owner.GetPropertyOptions(property);
                propOptions.InclusionMode = PropertyInclusionMode.Excluded;
                return this;
            }


            public ITypeMappingConfigurator<TDeclaringType> Include<TPropertyType>(
                Expression<Func<TDeclaringType, TPropertyType>> property,
                Func
                    <IPropertyOptionsBuilder<TDeclaringType, TPropertyType>,
                    IPropertyOptionsBuilder<TDeclaringType, TPropertyType>> options = null)
            {
                var propOptions = owner.GetPropertyOptions(property);

                propOptions.InclusionMode = PropertyInclusionMode.Included;

                if (options != null)
                    options(new PropertyOptionsBuilder<TDeclaringType, TPropertyType>(propOptions));

                return this;
            }


            public ITypeMappingConfigurator<TDeclaringType> PostReturns<TPostResponseType>()
            {
                return PostReturns(typeof (TPostResponseType));
            }


            public ITypeMappingConfigurator<TDeclaringType> PostReturns(Type type)
            {
                owner.postResponseType = type;
                return this;
            }

            private void ConstructedUsing(NewExpression constructExpr)
            {
                var expressionGotWrongStructureMessage =
                    "Expression got wrong structure, expects expression which looks like this: x => new FooBar(x.Prop)" +
                    " where each property maps to the argument";

                if (constructExpr == null)
                    throw new ArgumentException(expressionGotWrongStructureMessage, "expr");

                var ctorArgCount = constructExpr.Arguments.Count;
                for (var ctorArgIndex = 0; ctorArgIndex < ctorArgCount; ctorArgIndex++)
                {
                    try
                    {
                        var ctorArg = constructExpr.Arguments[ctorArgIndex];
                        var isOptionalArg = false;

                        var methodCallExpr = ctorArg as MethodCallExpression;
                        if (methodCallExpr != null &&
                            methodCallExpr.Method.UniqueToken() == optionalMethod.UniqueToken())
                        {
                            isOptionalArg = true;
                            ctorArg = methodCallExpr.Arguments[0];
                        }

                        var propOptions = owner.GetPropertyOptions(ctorArg);
                        if (isOptionalArg)
                            propOptions.CreateMode = PropertyCreateMode.Optional;

                        propOptions.ConstructorArgIndex = ctorArgIndex;
                    }
                    catch (Exception exception)
                    {
                        throw new ArgumentException(expressionGotWrongStructureMessage, "expr", exception);
                    }
                }

                owner.constructor = constructExpr.Constructor;
            }
        }
    }
}