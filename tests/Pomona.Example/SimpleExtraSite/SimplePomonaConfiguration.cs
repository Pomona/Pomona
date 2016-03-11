#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;

namespace Pomona.Example.SimpleExtraSite
{
    public class SimplePomonaConfiguration : PomonaConfigurationBase
    {
        public override IEnumerable<Type> SourceTypes
        {
            get { return new[] { typeof(SimpleExtraData) }; }
        }

        public override ITypeMappingFilter TypeMappingFilter
        {
            get { return new SimpleTypeMappingFilter(SourceTypes); }
        }

        protected override Type DataSource
        {
            get { return typeof(SimpleDataSource); }
        }
    }
}