using System.Linq;

using Nancy.Session;

using Pomona.Example.Models.Existence;
using Pomona.FluentMapping;

namespace Pomona.Example
{
    public class GalaxyRules
    {
        public void Map(ITypeMappingConfigurator<CelestialObject> map)
        {
            map.AsIndependentTypeRoot()
                .Include(x => x.Name, o => o.AsPrimaryKey());
        }


        public void Map(ITypeMappingConfigurator<PlanetarySystem> map)
        {
            map.AsUriBaseType()
                .AsChildResourceOf(x => x.Galaxy, x => x.PlanetarySystems);
        }


        public void Map(ITypeMappingConfigurator<Moon> map)
        {
            map.AsUriBaseType();
        }

        public void Map(ITypeMappingConfigurator<Galaxy> map)
        {
            map.AsUriBaseType();
        }

        public void Map(ITypeMappingConfigurator<Planet> map)
        {
            map.AsUriBaseType();
            map.AsChildResourceOf(x => x.PlanetarySystem, x => x.Planets);
        }
    }
}