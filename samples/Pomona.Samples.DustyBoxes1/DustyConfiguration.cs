#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;

using Pomona.FluentMapping;
using Pomona.Samples.DustyBoxes.Models;

namespace Pomona.Samples.DustyBoxes
{
    // SAMPLE: dusty-configuration
    public class DustyConfiguration : PomonaConfigurationBase
    {
        public override IEnumerable<object> FluentRuleObjects => new object[] { new DustyRules() };
        public override IEnumerable<Type> SourceTypes => new[] { typeof(GameConsole) };

        public class DustyRules
        {
            public void Map(ITypeMappingConfigurator<GameConsole> gameConsole)
            {
                gameConsole.HandledBy<GameConsoleHandler>();
            }
        }
    }

    // ENDSAMPLE
}