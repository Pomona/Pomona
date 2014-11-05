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

using Pomona.Routing;

namespace Pomona
{
    public abstract class PomonaConfigurationBase
    {
        public virtual IEnumerable<Delegate> FluentRuleDelegates
        {
            get { return Enumerable.Empty<Delegate>(); }
        }

        public virtual IEnumerable<object> FluentRuleObjects
        {
            get { return Enumerable.Empty<object>(); }
        }

        public virtual IEnumerable<Type> HandlerTypes
        {
            get { return Enumerable.Empty<Type>(); }
        }

        public virtual IEnumerable<IRouteActionResolver> RouteActionResolvers
        {
            get
            {
                return new[]
                {
                    new RequestHandlerActionResolver(),
                    DataSourceRootActionResolver,
                    QueryGetActionResolver
                }.Where(x => x != null);
            }
        }

        public abstract IEnumerable<Type> SourceTypes { get; }

        public abstract ITypeMappingFilter TypeMappingFilter { get; }

        protected virtual IRouteActionResolver DataSourceRootActionResolver
        {
            get { return new DataSourceRootActionResolver(); }
        }

        protected virtual IRouteActionResolver QueryGetActionResolver
        {
            get { return new QueryGetActionResolver(new DefaultQueryProviderCapabilityResolver()); }
        }


        public virtual void OnMappingComplete(TypeMapper typeMapper)
        {
        }
    }
}