#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Linq;

using Pomona.Samples.DustyBoxes.Models;

namespace Pomona.Samples.DustyBoxes
{
    // SAMPLE: dusty-game-console-handler
    public class GameConsoleHandler
    {
        public IQueryable<GameConsole> Query()
        {
            return
                new[]
                {
                    new GameConsole() { Id = "a2600", Name = "Atari 2600" },
                    new GameConsole() { Id = "gameboy", Name = "Game Boy" },
                    new GameConsole() { Id = "nes", Name = "Nintendo Entertainment System" },
                }.AsQueryable();
        }
    }
    // ENDSAMPLE
}
