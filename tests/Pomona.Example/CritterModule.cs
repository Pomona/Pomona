#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Pomona.Nancy;

namespace Pomona.Example
{
    [PomonaConfiguration(typeof(CritterPomonaConfiguration))]
    public class CritterModule : PomonaModule
    {
        public CritterModule(IPomonaSessionFactory pomonaSessionFactory) : base(pomonaSessionFactory)
        {
        }
    }
}