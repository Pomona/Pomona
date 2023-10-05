#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using Pomona.Example.Models;

namespace Pomona.Example.Handlers
{
    public class CritterHandler
    {
        public object Post(Critter critter, CritterExplodeCommand explodeCommand)
        {
            critter.Name = explodeCommand.Noops + " EXPLOSION!";
            return critter;
        }


        public object Post(Critter critter, CritterCaptureCommand captureCommand)
        {
            critter.IsCaptured = true;
            return critter;
        }
    }
}

