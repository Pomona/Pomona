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

using Pomona.Common;
using Pomona.Common.TypeSystem;

namespace Pomona.FluentMapping
{
    public interface IPropertyOptionsBuilder<TDeclaringType, TPropertyType>
    {
        /// <summary>
        /// Allow given methods for the property, which is combined with default convention-based permissions.
        /// </summary>
        /// <param name="method">Methods to allow.</param>
        /// <returns>The builder</returns>
        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> Allow(HttpMethod method);


        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AlwaysExpanded();


        /// <summary>
        /// Property defines the attributes of the resource.
        /// By doing this the property will have ResourceAttributesPropertyAttribute
        /// attached to it.
        /// </summary>
        /// <returns>the builder</returns>
        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AsAttributes();


        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AsEtag();

        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AsPrimaryKey();


        /// <summary>
        /// Deny methods for property, removing permissions that would be given by default.
        /// </summary>
        /// <param name="method">Methods to allow.</param>
        /// <returns>The builder</returns>
        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> Deny(HttpMethod method);


        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> ExposedAsRepository();
        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> HasAttribute(Attribute attribute);


        /// <summary>
        /// Allow given methods for items of the property, which is combined with default convention-based permissions.
        /// Note that this only applies to properties of type <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="method">Methods to deny.</param>
        /// <returns>The builder</returns>
        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> ItemsAllow(HttpMethod method);


        /// <summary>
        /// Deny methods for items of a property, removing permissions that would be given by default.
        /// Note that this only applies to properties of type <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="method">Methods to deny.</param>
        /// <returns>The builder</returns>
        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> ItemsDeny(HttpMethod method);


        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> Named(string name);


        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> OnGet<TContext>(
            Func<TDeclaringType, TContext, TPropertyType> getter);


        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> OnGet(Func<TDeclaringType, TPropertyType> getter);


        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> OnQuery(
            Expression<Func<TDeclaringType, TPropertyType>> getter);


        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> OnSet<TContext>(
            Action<TDeclaringType, TPropertyType, TContext> setter);


        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> OnSet(Action<TDeclaringType, TPropertyType> setter);
        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> ReadOnly();
        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> UsingDecompiledFormula();


        [Obsolete("Use OnQuery instead")]
        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> UsingFormula(
            Expression<Func<TDeclaringType, TPropertyType>> formula);


        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> WithAccessMode(HttpMethod method);
        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> WithCreateMode(PropertyCreateMode createMode);


        /// <summary>
        /// Specify what methods are allowed for items of a property, replacing all default permissions.
        /// This also overrides any permissions set by using ItemsAllow and ItemsDeny method at earlier point.
        /// Note that this only applies to properties of type <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="method">The methods to be allowed for items of property.</param>
        /// <returns>The builder.</returns>
        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> WithItemAccessMode(HttpMethod method);


        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> Writable();
    }
}