using Pomona.Example.Models;
using Pomona.FluentMapping;

namespace Pomona.Example
{
    public class CritterFluentRules
    {
                    //if (propertyInfo.DeclaringType == typeof(JunkWithRenamedProperty)
            //    && propertyInfo.Name == "ReallyUglyPropertyName")
            //    return "BeautifulAndExposed";

            //if (propertyInfo.DeclaringType == typeof(ThingWithRenamedReferenceProperty)
            //    && propertyInfo.Name == "Junky")
            //    return "DiscoFunky";
        public void Map(ITypeMappingConfigurator<JunkWithRenamedProperty> map)
        {
            map.Include(x => x.ReallyUglyPropertyName, o => o.Named("BeautifulAndExposed"));
        }

        public void Map(ITypeMappingConfigurator<ThingWithRenamedReferenceProperty> map)
        {
            map.Include(x => x.Junky, o => o.Named("DiscoFunky"));
        }

        public void Map(ITypeMappingConfigurator<Order> map)
        {
            map.PostReturns<OrderResponse>();
        }
        public void Map(ITypeMappingConfigurator<Critter> map)
        {
            map.AsUriBaseType()
                .Include(x => x.CrazyValue)
                .Include(x => x.CreatedOn);
        }

        public void Map(ITypeMappingConfigurator<Gun> map)
        {
            map.ConstructedUsing(x => new Gun(x.Critter, x.Model))
                .Include(x => x.ExplosionFactor);
        }
    }
}