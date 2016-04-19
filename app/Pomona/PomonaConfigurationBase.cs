#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Pomona.FluentMapping;
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
                    DataSourceRouteActionResolver,
                    QueryGetActionResolver
                }.Where(x => x != null);
            }
        }

        public virtual IEnumerable<Type> SourceTypes
        {
            get { return new Type[] { }; }
        }

        public virtual ITypeMappingFilter TypeMappingFilter
        {
            get { return new DefaultTypeMappingFilter(SourceTypes); }
        }

        protected virtual Type DataSource
        {
            get { return typeof(IPomonaDataSource); }
        }

        protected virtual IRouteActionResolver DataSourceRouteActionResolver
        {
            get { return new DataSourceRouteActionResolver(); }
        }

        protected virtual IRouteActionResolver QueryGetActionResolver
        {
            get { return new QueryGetActionResolver(new DefaultQueryProviderCapabilityResolver()); }
        }


        public IPomonaSessionFactory CreateSessionFactory()
        {
            var typeMapper = new TypeMapper(this);
            return CreateSessionFactory(typeMapper);
        }


        public virtual void OnMappingComplete(TypeMapper typeMapper)
        {
        }


        protected virtual Route OnCreateRootRoute(TypeMapper typeMapper)
        {
            return new DataSourceRootRoute(typeMapper, DataSource);
        }


        internal FluentTypeMappingFilter CreateMappingFilter()
        {
            var innerFilter = TypeMappingFilter;
            var fluentRuleObjects = FluentRuleObjects.ToArray();
            var fluentFilter = new FluentTypeMappingFilter(innerFilter, fluentRuleObjects, FluentRuleDelegates, SourceTypes);
            var wrappableFilter = innerFilter as IWrappableTypeMappingFilter;
            if (wrappableFilter != null)
                wrappableFilter.BaseFilter = fluentFilter;
            return fluentFilter;
        }


        internal IPomonaSessionFactory CreateSessionFactory(TypeMapper typeMapper)
        {
            var pomonaSessionFactory = new PomonaSessionFactory(typeMapper, OnCreateRootRoute(typeMapper),
                                                                new InternalRouteActionResolver(RouteActionResolvers));
            return pomonaSessionFactory;
        }
    }
}