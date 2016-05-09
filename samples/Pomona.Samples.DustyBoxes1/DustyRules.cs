#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Pomona.FluentMapping;
using Pomona.Samples.DustyBoxes.Models;

namespace Pomona.Samples.DustyBoxes
{
    public class DustyRules
    {
        public void Map(ITypeMappingConfigurator<GameConsole> gameConsole)
        {
            gameConsole.HandledBy<GameConsoleHandler>();
        }
    }
}