#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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

namespace Pomona.Common.TypeSystem
{
    public class ComplexType : RuntimeTypeSpec
    {
        private readonly Lazy<ComplexTypeDetails> complexTypeDetails;
        private readonly Lazy<IEnumerable<ComplexType>> subTypes;
        private Func<IDictionary<PropertySpec, object>, object> createFunc;
        private Delegate createUsingPropertySourceFunc;


        public ComplexType(IExportedTypeResolver typeResolver,
            Type type,
            Func<IEnumerable<TypeSpec>> genericArguments = null)
            : base(typeResolver, type, genericArguments)
        {
            this.subTypes = CreateLazy(() => (IEnumerable<ComplexType>)typeResolver.GetAllTransformedTypes()
                .Where(x => x.BaseType == this)
                .SelectMany(x => x.SubTypes.Append(x)).ToList());
            this.complexTypeDetails = CreateLazy(() => typeResolver.LoadComplexTypeDetails(this));
        }


        public virtual HttpMethod AllowedMethods
        {
            get { return this.ComplexTypeDetails.AllowedMethods; }
        }

        public virtual ComplexProperty PrimaryId
        {
            get { return this.ComplexTypeDetails.PrimaryId; }
        }

        public new virtual IEnumerable<ComplexProperty> Properties
        {
            get { return base.Properties.Cast<ComplexProperty>(); }
        }

        public virtual ResourceInfoAttribute ResourceInfo
        {
            get { return DeclaredAttributes.OfType<ResourceInfoAttribute>().FirstOrDefault(); }
        }

        public bool DeleteAllowed
        {
            get { return this.ComplexTypeDetails.AllowedMethods.HasFlag(HttpMethod.Delete); }
        }

        public ComplexProperty ETagProperty
        {
            get { return this.ComplexTypeDetails.ETagProperty; }
        }

        public override bool IsAbstract
        {
            get { return this.ComplexTypeDetails.IsAbstract; }
        }

        public override bool IsAlwaysExpanded
        {
            get { return this.ComplexTypeDetails.AlwaysExpand; }
        }

        public bool MappedAsValueObject
        {
            get { return this.ComplexTypeDetails.MappedAsValueObject; }
        }

        public Action<object> OnDeserialized
        {
            get { return this.ComplexTypeDetails.OnDeserialized; }
        }

        public bool PatchAllowed
        {
            get { return this.ComplexTypeDetails.AllowedMethods.HasFlag(HttpMethod.Patch); }
        }

        public string PluralName
        {
            get { return this.ComplexTypeDetails.PluralName; }
        }

        public bool PostAllowed
        {
            get { return this.ComplexTypeDetails.AllowedMethods.HasFlag(HttpMethod.Post); }
        }

        public IEnumerable<ComplexType> SubTypes
        {
            get { return this.subTypes.Value; }
        }

        public new IExportedTypeResolver TypeResolver
        {
            get { return (IExportedTypeResolver)base.TypeResolver; }
        }

        protected ComplexTypeDetails ComplexTypeDetails
        {
            get { return this.complexTypeDetails.Value; }
        }


        public virtual object Create<T>(IConstructorPropertySource<T> propertySource)
        {
            if (typeof(T) != Type)
                throw new InvalidOperationException(string.Format("T ({0}) does not match Type property", typeof(T)));

            if (IsAbstract)
            {
                throw new PomonaSerializationException("Pomona was unable to instantiate type " + Name
                                                       + ", as it's an abstract type.");
            }

            if (Constructor == null)
            {
                throw new PomonaSerializationException("Pomona was unable to instantiate type " + Name
                                                       + ", Constructor property was null.");
            }

            if (this.createUsingPropertySourceFunc == null)
            {
                var param = Expression.Parameter(typeof(IConstructorPropertySource<T>));
                var expr =
                    Expression.Lambda<Func<IConstructorPropertySource<T>, T>>(
                        Expression.Invoke(Constructor.InjectingConstructorExpression, param),
                        param);
                this.createUsingPropertySourceFunc = expr.Compile();
            }

            return ((Func<IConstructorPropertySource<T>, T>)this.createUsingPropertySourceFunc)(propertySource);
        }


        public override object Create(IDictionary<PropertySpec, object> args)
        {
            if (this.createFunc == null)
            {
                var argsParam = Expression.Parameter(typeof(IDictionary<PropertySpec, object>));
                var makeGenericType = typeof(ConstructorPropertySource<>).MakeGenericType(
                    Constructor.InjectingConstructorExpression.ReturnType);
                var constructorInfo = makeGenericType.GetConstructor(new[]
                { typeof(IDictionary<PropertySpec, object>) });

                if (constructorInfo == null)
                {
                    throw new InvalidOperationException(
                        "Unable to find constructor for ConstructorPropertySource (should not get here).");
                }

                this.createFunc =
                    Expression.Lambda<Func<IDictionary<PropertySpec, object>, object>>(
                        Expression.Convert(
                            Expression.Invoke(Constructor.InjectingConstructorExpression,
                                Expression.New(
                                    constructorInfo,
                                    argsParam)),
                            typeof(object)),
                        argsParam).Compile();
            }
            return this.createFunc(args);
        }


        protected override TypeSerializationMode OnLoadSerializationMode()
        {
            return TypeSerializationMode.Complex;
        }


        protected internal override PropertySpec OnWrapProperty(PropertyInfo property)
        {
            return new ComplexProperty(TypeResolver, property, () => this);
        }

        #region Nested type: ConstructorPropertySource

        private class ConstructorPropertySource<T> : IConstructorPropertySource<T>
        {
            private readonly IDictionary<PropertySpec, object> args;


            public ConstructorPropertySource(IDictionary<PropertySpec, object> args)
            {
                this.args = args;
            }


            public TContext Context<TContext>()
            {
                throw new NotImplementedException();
            }


            public TProperty GetValue<TProperty>(PropertyInfo propertyInfo, Func<TProperty> defaultFactory)
            {
                defaultFactory = defaultFactory
                                 ?? (() => { throw new InvalidOperationException("Unable to get required property."); });

                // TODO: Optimize a lot!!!
                return
                    this.args.Where(x => x.Key.PropertyInfo.Name == propertyInfo.Name).Select(x => (TProperty)x.Value)
                        .MaybeFirst()
                        .OrDefault(defaultFactory);
            }


            public T Optional()
            {
                throw new NotImplementedException();
            }


            public TParentType Parent<TParentType>()
            {
                throw new NotImplementedException();
            }


            public T Requires()
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}