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
using System.Linq.Expressions;

using Pomona.Common.TypeSystem;

namespace Pomona.FluentMapping
{
    internal abstract class TypeMappingConfiguratorBase<TDeclaring> : ITypeMappingConfigurator<TDeclaring>
    {
        public virtual ITypeMappingConfigurator<TDeclaring> AllPropertiesAreExcludedByDefault()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AllPropertiesAreIncludedByDefault()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AllPropertiesRequiresExplicitMapping()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AsChildResourceOf<TParent>(
            Expression<Func<TDeclaring, TParent>> parentProperty,
            Expression<Func<TParent, IEnumerable<TDeclaring>>> collectionProperty)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AsEntity()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AsIndependentTypeRoot()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AsUriBaseType()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AsValueObject()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> ConstructedUsing(
            Expression<Func<IConstructorControl<TDeclaring>, TDeclaring>> constructExpr)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> ConstructedUsing(
            Expression<Func<TDeclaring, IConstructorControl<TDeclaring>, TDeclaring>> constructExpr)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> DeleteAllowed()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> DeleteDenied()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> Exclude(Expression<Func<TDeclaring, object>> property)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> ExposedAsRepository()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> HandledBy<THandler>()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> HasChildren<TItem>(
            Expression<Func<TDeclaring, IEnumerable<TItem>>> property,
            Expression<Func<TItem, TDeclaring>> parentProperty,
            Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> typeOptions,
            Func
                <IPropertyOptionsBuilder<TDeclaring, IEnumerable<TItem>>,
                    IPropertyOptionsBuilder<TDeclaring, IEnumerable<TItem>>> propertyOptions)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> Include<TPropertyType>(
            Expression<Func<TDeclaring, TPropertyType>> property,
            Func<IPropertyOptionsBuilder<TDeclaring, TPropertyType>, IPropertyOptionsBuilder<TDeclaring, TPropertyType>>
                options = null)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> Named(string exposedTypeName)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> OnDeserialized(Action<TDeclaring> action)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> PatchAllowed()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> PatchDenied()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> PostAllowed()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> PostDenied()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> PostReturns<TPostResponseType>()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> PostReturns(Type postResponseType)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> WithPluralName(string pluralName)
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AsAbstract()
        {
            return this;
        }


        public virtual ITypeMappingConfigurator<TDeclaring> AsConcrete()
        {
            return this;
        }
    }
}