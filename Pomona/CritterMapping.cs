using Pomona.TestModel;

namespace Pomona
{
    public class CritterMapping : MappingBase<Critter>
    {
        public CritterMapping()
        {
            Hide(x => x.Subscriptions, x => x.Enemies);
            Rename(x => x.Name, "CritterName");


        }
    }
}