#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq.Expressions;

using Pomona.Common;
using Pomona.Common.TypeSystem;

namespace Pomona.FluentMapping
{
    internal abstract class PropertyOptionsBuilderBase<TDeclaring, TProperty>
        : IPropertyOptionsBuilder<TDeclaring, TProperty>
    {
        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> Allow(HttpMethod method)
        {
            return this;
        }


        public IPropertyOptionsBuilder<TDeclaring, TProperty> AlwaysExpanded()
        {
            return Expand(ExpandMode.Full);
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


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> Deny(HttpMethod method)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> Expand(ExpandMode expandMode)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> ExposedAsRepository()
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> HasAttribute(Attribute attribute)
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


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> Named(string name)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> OnGet<TContext>(
            Func<TDeclaring, TContext, TProperty> getter)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> OnGet(Func<TDeclaring, TProperty> getter)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> OnSet<TContext>(
            Action<TDeclaring, TProperty, TContext> setter)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> OnSet(Action<TDeclaring, TProperty> setter)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> ReadOnly()
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> UsingFormula(
            Expression<Func<TDeclaring, TProperty>> formula)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> WithAccessMode(HttpMethod method)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> WithCreateMode(PropertyCreateMode createMode)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> WithItemAccessMode(HttpMethod method)
        {
            return this;
        }


        public virtual IPropertyOptionsBuilder<TDeclaring, TProperty> Writable()
        {
            return this;
        }


        IPropertyOptionsBuilder<TDeclaring, TProperty> IPropertyOptionsBuilder<TDeclaring, TProperty>.OnQuery(
            Expression<Func<TDeclaring, TProperty>> getter)
        {
            return UsingFormula(getter);
        }
    }
}