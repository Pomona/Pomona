#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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

using Nancy.Extensions;

using Pomona.Common;
using Pomona.Common.TypeSystem;

namespace Pomona.FluentMapping
{
    internal class PropertyOptionsBuilder<TDeclaringType, TPropertyType>
        : PropertyOptionsBuilderBase<TDeclaringType, TPropertyType>
    {
        private readonly PropertyMappingOptions options;


        public PropertyOptionsBuilder(PropertyMappingOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");
            this.options = options;
        }

        #region Implementation of IPropertyOptionsBuilder<TDeclaringType,TPropertyType>

        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> Allow(HttpMethod method)
        {
            this.options.SetAccessModeFlag(method);
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AsAttributes()
        {
            this.options.IsAttributesProperty = true;
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AsEtag()
        {
            this.options.IsEtagProperty = true;
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AsPrimaryKey()
        {
            this.options.IsPrimaryKey = true;
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> Deny(HttpMethod method)
        {
            this.options.ClearAccessModeFlag(method);
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> Expand(ExpandMode expandMode)
        {
            this.options.PropertyExpandMode = expandMode;
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> ExposedAsRepository()
        {
            this.options.ExposedAsRepository = true;
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> HasAttribute(Attribute attribute)
        {
            this.options.AddedAttributes.Add(attribute);
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> ItemsAllow(HttpMethod method)
        {
            this.options.SetItemAccessModeFlag(method);
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> ItemsDeny(HttpMethod method)
        {
            this.options.ClearItemAccessModeFlag(method);
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> Named(string name)
        {
            this.options.Name = name;
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> OnGet(
            Func<TDeclaringType, TPropertyType> getter)
        {
            this.options.OnGetDelegate = (target, contextResolver) => getter((TDeclaringType)target);
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> OnGet<TContext>(
            Func<TDeclaringType, TContext, TPropertyType> getter)
        {
            this.options.OnGetDelegate =
                (target, contextResolver) => getter((TDeclaringType)target, contextResolver.GetInstance<TContext>());
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> OnSet(
            Action<TDeclaringType, TPropertyType> setter)
        {
            this.options.OnSetDelegate =
                (target, value, contextResolver) =>
                    setter((TDeclaringType)target, (TPropertyType)value);
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> OnSet<TContext>(
            Action<TDeclaringType, TPropertyType, TContext> setter)
        {
            this.options.OnSetDelegate =
                (target, value, contextResolver) =>
                    setter((TDeclaringType)target, (TPropertyType)value, contextResolver.GetInstance<TContext>());
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> ReadOnly()
        {
            this.options.CreateMode = PropertyCreateMode.Excluded;

            var allMutatingMethods = HttpMethod.Patch | HttpMethod.Post | HttpMethod.Delete
                                     | HttpMethod.Put;

            this.options.SetAccessModeFlag(HttpMethod.Get);
            this.options.ClearAccessModeFlag(allMutatingMethods);
            this.options.SetItemAccessModeFlag(HttpMethod.Get);
            this.options.ClearItemAccessModeFlag(allMutatingMethods);
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> UsingFormula(
            Expression<Func<TDeclaringType, TPropertyType>> formula)
        {
            if (formula == null)
                throw new ArgumentNullException("formula");
            this.options.Formula = formula;
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> WithAccessMode(HttpMethod method)
        {
            this.options.MethodMask = ~(default(HttpMethod));
            this.options.Method = method;
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> WithCreateMode(
            PropertyCreateMode createMode)
        {
            this.options.CreateMode = createMode;
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> WithItemAccessMode(HttpMethod method)
        {
            this.options.ItemMethodMask = ~(default(HttpMethod));
            this.options.ItemMethod = method;
            return this;
        }


        public override IPropertyOptionsBuilder<TDeclaringType, TPropertyType> Writable()
        {
            this.options.CreateMode = PropertyCreateMode.Optional;
            if (typeof(TPropertyType).IsCollection())
                this.options.SetItemAccessModeFlag(HttpMethod.Patch | HttpMethod.Post | HttpMethod.Delete);
            else
            {
                if (this.options.PropertyInfo.CanWrite)
                    this.options.SetAccessModeFlag(HttpMethod.Put);
            }
            this.options.SetAccessModeFlag(HttpMethod.Patch | HttpMethod.Post);
            return this;
        }

        #endregion
    }
}