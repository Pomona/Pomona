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

using Pomona.Common.Internals;
using Pomona.Internals;

namespace Pomona.Common.TypeSystem
{
    public class TransformedType : RuntimeTypeSpec
    {
        private readonly Lazy<ExportedTypeDetails> exportedTypeDetails;


        public TransformedType(IExportedTypeResolver typeResolver,
            Type type,
            Func<IEnumerable<TypeSpec>> genericArguments = null)
            : base(typeResolver, type, genericArguments)
        {
            this.exportedTypeDetails = CreateLazy(() => typeResolver.LoadExportedTypeDetails(this));
        }

        public virtual ResourceInfoAttribute ResourceInfo
        {
            get { return DeclaredAttributes.OfType<ResourceInfoAttribute>().FirstOrDefault(); }
        }

        private class ConstructorPropertySource<T> : IConstructorPropertySource<T>
        {
            private IDictionary<PropertySpec, object> args;


            public ConstructorPropertySource(IDictionary<PropertySpec, object> args)
            {
                this.args = args;
            }


            public T Requires()
            {
                throw new NotImplementedException();
            }

            public T Optional()
            {
                throw new NotImplementedException();
            }

            public TParentType Parent<TParentType>()
            {
                throw new NotImplementedException();
            }

            public TContext Context<TContext>()
            {
                throw new NotImplementedException();
            }

            public TProperty GetValue<TProperty>(PropertyInfo propertyInfo, Func<TProperty> defaultFactory)
            {
                defaultFactory = defaultFactory ?? (() =>
                {
                    throw new InvalidOperationException("Unable to get required property.");
                });

                // TODO: Optimize a lot!!!
                return
                    this.args.Where(x => x.Key.PropertyInfo.Name == propertyInfo.Name).Select(x => (TProperty)x.Value).MaybeFirst()
                        .OrDefault(defaultFactory);
            }
        }

        protected override TypeSerializationMode OnLoadSerializationMode()
        {
            return TypeSerializationMode.Complex;
        }


        private Delegate createUsingPropertySourceFunc;

        public virtual object Create<T>(IConstructorPropertySource<T> propertySource)
        {
            if (typeof(T) != Type)
                throw new InvalidOperationException(string.Format("T ({0}) does not match Type property", typeof(T)));
            if (createUsingPropertySourceFunc == null)
            {
                var param = Expression.Parameter(typeof (IConstructorPropertySource<T>));
                var expr = Expression.Lambda<Func<IConstructorPropertySource<T>, T>>(Expression.Invoke(Constructor.InjectingConstructorExpression, param), param);
                createUsingPropertySourceFunc = expr.Compile();
            }

            return ((Func<IConstructorPropertySource<T>,T>)createUsingPropertySourceFunc)(propertySource);
        }

        private Func<IDictionary<PropertySpec, object>, object> createFunc;

        public override object Create(IDictionary<PropertySpec, object> args)
        {
            if (createFunc == null)
            {
                var argsParam = Expression.Parameter(typeof(IDictionary<PropertySpec, object>));
                Type makeGenericType = typeof(ConstructorPropertySource<>).MakeGenericType(
                    Constructor.InjectingConstructorExpression.ReturnType);
                ConstructorInfo constructorInfo = makeGenericType.GetConstructor(new[]
                    { typeof(IDictionary<PropertySpec, object>) });

                if (constructorInfo == null)
                    throw new InvalidOperationException("Unable to find constructor for ConstructorPropertySource (should not get here).");

                this.createFunc =
                    Expression.Lambda<Func<IDictionary<PropertySpec, object>, object>>(
                        Expression.Convert(
                            Expression.Invoke(Constructor.InjectingConstructorExpression,
                                Expression.New(
                                    constructorInfo, argsParam)),
                            typeof(object)),
                        argsParam).Compile();
            }
            return createFunc(args);
        }

        public new virtual IEnumerable<PropertyMapping> Properties
        {
            get { return base.Properties.Cast<PropertyMapping>(); }
        }

        public PropertyMapping ETagProperty
        {
            get { return ExportedTypeDetails.ETagProperty; }
        }

        [PendingRemoval]
        public bool MappedAsValueObject
        {
            get { return ExportedTypeDetails.MappedAsValueObject; }
        }

        public override bool IsAlwaysExpanded
        {
            get { return ExportedTypeDetails.AlwaysExpand; }
        }

        [PendingRemoval]
        public Action<object> OnDeserialized
        {
            get { return ExportedTypeDetails.OnDeserialized; }
        }

        public virtual HttpMethod AllowedMethods {get { return ExportedTypeDetails.AllowedMethods; }}

        [PendingRemoval]
        public bool PatchAllowed
        {
            get { return ExportedTypeDetails.AllowedMethods.HasFlag(HttpMethod.Patch); }
        }

        [PendingRemoval]
        public string PluralName
        {
            get { return ExportedTypeDetails.PluralName; }
        }

        [PendingRemoval]
        public bool PostAllowed
        {
            get { return ExportedTypeDetails.AllowedMethods.HasFlag(HttpMethod.Post); }
        }

        public virtual PropertyMapping PrimaryId
        {
            get { return ExportedTypeDetails.PrimaryId; }
        }

        public IEnumerable<TransformedType> SubTypes
        {
            get
            {
                return
                    TypeResolver.GetAllTransformedTypes()
                        .Where(x => x.BaseType == this)
                        .SelectMany(x => x.SubTypes.Concat(x));
            }
        }

        public new IExportedTypeResolver TypeResolver
        {
            get { return (IExportedTypeResolver)base.TypeResolver; }
        }

        protected ExportedTypeDetails ExportedTypeDetails
        {
            get { return this.exportedTypeDetails.Value; }
        }


        public Expression CreateExpressionForExternalPropertyPath(string externalPath)
        {
            var parameter = Expression.Parameter(Type, "x");
            var propertyAccessExpression = CreateExpressionForExternalPropertyPath(parameter, externalPath);

            return Expression.Lambda(propertyAccessExpression, parameter);
        }


        public Expression CreateExpressionForExternalPropertyPath(Expression instance, string externalPath)
        {
            string externalPropertyName, remainingExternalPath;
            TakeLeftmostPathPart(externalPath, out externalPropertyName, out remainingExternalPath);

            var prop =
                Properties.First(
                    x => String.Equals(x.Name, externalPropertyName, StringComparison.InvariantCultureIgnoreCase));

            var propertyAccessExpression = prop.CreateGetterExpression(instance);

            if (remainingExternalPath != null)
            {
                // TODO Error handling here when remaningpath does not represents a TransformedType
                var transformedPropType = prop.PropertyType as TransformedType;
                if (transformedPropType == null)
                {
                    throw new InvalidOperationException(
                        "Can not filter by subproperty when property is not TransformedType");
                }
                return transformedPropType.CreateExpressionForExternalPropertyPath(
                    propertyAccessExpression,
                    remainingExternalPath);
            }
            return propertyAccessExpression;
        }


        public object GetId(object entity)
        {
            return PrimaryId.Getter(entity);
        }


        private static void TakeLeftmostPathPart(string path, out string leftName, out string remainingPropPath)
        {
            var leftPathSeparatorIndex = path.IndexOf('.');
            if (leftPathSeparatorIndex == -1)
            {
                leftName = path;
                remainingPropPath = null;
            }
            else
            {
                leftName = path.Substring(0, leftPathSeparatorIndex);
                remainingPropPath = path.Substring(leftPathSeparatorIndex + 1);
            }
        }


        protected internal override PropertySpec OnWrapProperty(PropertyInfo property)
        {
            return new PropertyMapping(TypeResolver, property, () => this);
        }
    }
}