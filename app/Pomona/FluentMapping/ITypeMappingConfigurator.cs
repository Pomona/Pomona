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
    public interface ITypeMappingConfigurator<TDeclaringType>
    {
        ITypeMappingConfigurator<TDeclaringType> AllPropertiesAreExcludedByDefault();
        ITypeMappingConfigurator<TDeclaringType> AllPropertiesAreIncludedByDefault();
        ITypeMappingConfigurator<TDeclaringType> AllPropertiesRequiresExplicitMapping();
        ITypeMappingConfigurator<TDeclaringType> AsAbstract();

        ITypeMappingConfigurator<TDeclaringType> ExposedAt(string path);

        ITypeMappingConfigurator<TDeclaringType> AsChildResourceOf<TParent>(
            Expression<Func<TDeclaringType, TParent>> parentProperty,
            Expression<Func<TParent, IEnumerable<TDeclaringType>>> collectionProperty);


        ITypeMappingConfigurator<TDeclaringType> AsChildResourceOf<TParent>(
            Expression<Func<TDeclaringType, TParent>> parentProperty,
            Expression<Func<TParent, TDeclaringType>> childProperty);


        ITypeMappingConfigurator<TDeclaringType> AsConcrete();

        ITypeMappingConfigurator<TDeclaringType> AsEntity();
        ITypeMappingConfigurator<TDeclaringType> AsIndependentTypeRoot();
        ITypeMappingConfigurator<TDeclaringType> AsUriBaseType();
        ITypeMappingConfigurator<TDeclaringType> AsValueObject();
        ITypeMappingConfigurator<TDeclaringType> AsSingleton();


        ITypeMappingConfigurator<TDeclaringType> ConstructedUsing(
            Expression<Func<IConstructorControl<TDeclaringType>, TDeclaringType>> constructExpr);


        ITypeMappingConfigurator<TDeclaringType> ConstructedUsing(
            Expression<Func<TDeclaringType, IConstructorControl<TDeclaringType>, TDeclaringType>> constructExpr);


        ITypeMappingConfigurator<TDeclaringType> DeleteAllowed();
        ITypeMappingConfigurator<TDeclaringType> DeleteDenied();

        ITypeMappingConfigurator<TDeclaringType> Exclude(Expression<Func<TDeclaringType, object>> property);
        ITypeMappingConfigurator<TDeclaringType> ExposedAsRepository();

        ITypeMappingConfigurator<TDeclaringType> HandledBy<THandler>();


        ITypeMappingConfigurator<TDeclaringType> HasChildren<TItem>(
            Expression<Func<TDeclaringType, IEnumerable<TItem>>> collectionProperty,
            Expression<Func<TItem, TDeclaringType>> parentProperty,
            Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> typeOptions = null,
            Func
                <IPropertyOptionsBuilder<TDeclaringType, IEnumerable<TItem>>,
                    IPropertyOptionsBuilder<TDeclaringType, IEnumerable<TItem>>> propertyOptions = null);

        ITypeMappingConfigurator<TDeclaringType> HasChild<TItem>(
            Expression<Func<TDeclaringType, TItem>> childProperty,
            Expression<Func<TItem, TDeclaringType>> parentProperty,
            Func<ITypeMappingConfigurator<TItem>, ITypeMappingConfigurator<TItem>> typeOptions = null,
            Func
                <IPropertyOptionsBuilder<TDeclaringType, TItem>,
                    IPropertyOptionsBuilder<TDeclaringType, TItem>> propertyOptions = null);

        ITypeMappingConfigurator<TDeclaringType> Include<TPropertyType>(string name, Func
                <IPropertyOptionsBuilder<TDeclaringType, TPropertyType>,
                    IPropertyOptionsBuilder<TDeclaringType, TPropertyType>> options);
            
        ITypeMappingConfigurator<TDeclaringType> Include<TPropertyType>(
            Expression<Func<TDeclaringType, TPropertyType>> property,
            Func
                <IPropertyOptionsBuilder<TDeclaringType, TPropertyType>,
                    IPropertyOptionsBuilder<TDeclaringType, TPropertyType>> options = null);


        ITypeMappingConfigurator<TDeclaringType> IncludeAs<TPropertyType>(
            Expression<Func<TDeclaringType, object>> property,
            Func
                <IPropertyOptionsBuilder<TDeclaringType, TPropertyType>,
                    IPropertyOptionsBuilder<TDeclaringType, TPropertyType>> options = null);


        ITypeMappingConfigurator<TDeclaringType> Named(string exposedTypeName);

        ITypeMappingConfigurator<TDeclaringType> OnDeserialized(Action<TDeclaringType> action);

        ITypeMappingConfigurator<TDeclaringType> PatchAllowed();
        ITypeMappingConfigurator<TDeclaringType> PatchDenied();
        ITypeMappingConfigurator<TDeclaringType> PostAllowed();
        ITypeMappingConfigurator<TDeclaringType> PostDenied();
        ITypeMappingConfigurator<TDeclaringType> PostReturns<TPostResponseType>();
        ITypeMappingConfigurator<TDeclaringType> PostReturns(Type postResponseType);
        ITypeMappingConfigurator<TDeclaringType> WithPluralName(string pluralName);
    }
}