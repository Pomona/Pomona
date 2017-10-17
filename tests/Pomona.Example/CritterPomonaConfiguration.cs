﻿#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Pomona.Example.Models;
using Pomona.Example.Rules;

namespace Pomona.Example
{
    public class CritterPomonaConfiguration : PomonaConfigurationBase
    {
        public override IEnumerable<object> FluentRuleObjects
        {
            get
            {
                yield return new CritterFluentRules();
                yield return new GalaxyRules();
                yield return new GuidThingFluentRules();
                yield return new HandledThingRules();
            }
        }

        public override IEnumerable<Type> SourceTypes => CritterRepository.GetEntityTypes().Concat(new[] { typeof(GenericBaseClass<int>) });

        public override ITypeMappingFilter TypeMappingFilter => new CritterTypeMappingFilter(SourceTypes);
    }
}