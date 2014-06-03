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
using System.Linq.Expressions;

using Nancy;

using Pomona.Common;
using Pomona.Common.TypeSystem;

namespace Pomona.FluentMapping
{
    internal abstract class PropertyOptionsBuilderBase<TDeclaring, TProperty> : IPropertyOptionsBuilder<TDeclaring, TProperty>
    {
        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> AlwaysExpanded()
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> AsAttributes()
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> AsEtag()
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> AsPrimaryKey()
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> ExposedAsRepository()
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> Named(string name)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> ReadOnly()
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> UsingDecompiledFormula()
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> WithItemAccessMode(HttpMethod method)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> Allow(HttpMethod method)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> Deny(HttpMethod method)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> ItemsAllow(HttpMethod method)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> ItemsDeny(HttpMethod method)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> UsingFormula(
            Expression<Func<TDeclaring, TProperty>> formula)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> OnSet<TContext>(Action<TDeclaring, TProperty, TContext> setter)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> OnSet(Action<TDeclaring, TProperty> setter)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> OnGet<TContext>(Func<TDeclaring, TContext, TProperty> getter)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> OnGet(Func<TDeclaring, TProperty> getter)
        {
            return this;
        }


        IPropertyOptionsBuilder<TDeclaring, TProperty> IPropertyOptionsBuilder<TDeclaring, TProperty>.OnQuery(Expression<Func<TDeclaring, TProperty>> getter)
        {
            return UsingFormula(getter);
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> WithAccessMode(HttpMethod method)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> WithCreateMode(PropertyCreateMode createMode)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> Writable()
        {
            return this;
        }
    }
}