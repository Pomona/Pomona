#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;

namespace Pomona.Samples.MiscSnippets
{
    // SAMPLE: misc-config-fluent-rule-objects-overrides
    public class Configuration : PomonaConfigurationBase
    {
        public override IEnumerable<object> FluentRuleObjects
        {
            get { yield return new FluentRules();}
        }
    }
    // ENDSAMPLE
}
