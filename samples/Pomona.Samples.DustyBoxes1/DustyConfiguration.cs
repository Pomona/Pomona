#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;

using Pomona.Samples.DustyBoxes.Models;

namespace Pomona.Samples.DustyBoxes
{
    public class DustyConfiguration : PomonaConfigurationBase
    {
        public override IEnumerable<object> FluentRuleObjects => new object[] { new DustyRules() };
        public override IEnumerable<Type> SourceTypes => new[] { typeof(GameConsole) };
    }
}