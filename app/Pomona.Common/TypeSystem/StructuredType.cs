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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Pomona.Common.TypeSystem
{
    public abstract class StructuredType : RuntimeTypeSpec
    {
        private readonly Lazy<StructuredTypeDetails> structuredTypeDetails;
        private readonly Lazy<ReadOnlyCollection<StructuredType>> subTypes;
        private Func<IDictionary<PropertySpec, object>, object> createFunc;
        private Delegate createUsingPropertySourceFunc;


        protected StructuredType(IStructuredTypeResolver typeResolver,
                                 Type type,
                                 Func<IEnumerable<TypeSpec>> genericArguments = null)
            : base(typeResolver, type, genericArguments)
        {
            this.subTypes = CreateLazy(() => typeResolver.LoadSubTypes(this).ToList().AsReadOnly());
            this.structuredTypeDetails = CreateLazy(() => typeResolver.LoadStructuredTypeDetails(this));
        }


        public virtual HttpMethod AllowedMethods
        {
            get { return StructuredTypeDetails.AllowedMethods; }
        }

        public bool DeleteAllowed
        {
            get { return StructuredTypeDetails.AllowedMethods.HasFlag(HttpMethod.Delete); }
        }

        public override bool IsAbstract
        {
            get { return StructuredTypeDetails.IsAbstract; }
        }

        public override bool IsAlwaysExpanded
        {
            get { return StructuredTypeDetails.AlwaysExpand; }
        }

        public bool MappedAsValueObject
        {
            get { return StructuredTypeDetails.MappedAsValueObject; }
        }

        public Action<object> OnDeserialized
        {
            get { return StructuredTypeDetails.OnDeserialized; }
        }

        public bool PatchAllowed
        {
            get { return StructuredTypeDetails.AllowedMethods.HasFlag(HttpMethod.Patch); }
        }

        public string PluralName
        {
            get { return StructuredTypeDetails.PluralName; }
        }

        public bool PostAllowed
        {
            get { return StructuredTypeDetails.AllowedMethods.HasFlag(HttpMethod.Post); }
        }

        public virtual StructuredProperty PrimaryId
        {
            get { return StructuredTypeDetails.PrimaryId; }
        }
        
        public new virtual IEnumerable<StructuredProperty> Properties
        {
            get { return base.Properties.Cast<StructuredProperty>(); }
        }

        public virtual ResourceInfoAttribute ResourceInfo
        {
            get { return DeclaredAttributes.OfType<ResourceInfoAttribute>().FirstOrDefault(); }
        }

        public IEnumerable<StructuredType> SubTypes
        {
            get { return this.subTypes.Value; }
        }

        public new IResourceTypeResolver TypeResolver
        {
            get { return (IResourceTypeResolver)(((TypeSpec)this).TypeResolver); }
        }

        protected StructuredTypeDetails StructuredTypeDetails
        {
            get { return this.structuredTypeDetails.Value; }
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


        protected internal override PropertySpec OnWrapProperty(PropertyInfo property)
        {
            return new StructuredProperty(TypeResolver, property, this);
        }


        protected override TypeSerializationMode OnLoadSerializationMode()
        {
            return TypeSerializationMode.Structured;
        }


        protected internal override IEnumerable<PropertySpec> OnLoadProperties()
        {
            return
                Type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Select(x => TypeResolver.WrapProperty(this, x));
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
                defaultFactory = defaultFactory ?? (() =>
                {
                    throw new InvalidOperationException("Unable to get required property.");
                });

                // TODO: Optimize a lot!!!
                return this.args
                           .Where(x => x.Key.PropertyInfo.Name == propertyInfo.Name)
                           .Select(x => (TProperty)x.Value)
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