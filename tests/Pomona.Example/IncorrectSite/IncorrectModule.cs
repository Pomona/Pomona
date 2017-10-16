#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Pomona.Nancy;

namespace Pomona.Example.IncorrectSite
{
    [PomonaConfiguration(typeof(IncorrectPomonaConfiguration))]
    public class IncorrectModule : PomonaModule
    {
        public IncorrectModule()
            : base("/incorrect")
        {
        }
    }
}