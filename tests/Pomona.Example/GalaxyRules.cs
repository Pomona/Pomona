using System.Linq;

using Nancy.Session;
using Pomona.Common;
using Pomona.Example.Models.Existence;
using Pomona.FluentMapping;

namespace Pomona.Example
{
    public class GalaxyRules
    {
        public void Map(ITypeMappingConfigurator<CelestialObject> map)
        {
            map.AsIndependentTypeRoot()
                .Include(x => x.Name, o => o.AsPrimaryKey())
                .Include(x => x.ETag, o => o.AsEtag().ReadOnly());
        }


        public void Map(ITypeMappingConfigurator<PlanetarySystem> map)
        {
            map.AsUriBaseType()
                .Include(x => x.Planets, o => o.ExposedAsRepository().Allow(HttpMethod.Post).ItemsAllow(HttpMethod.Delete))
                .AsChildResourceOf(x => x.Galaxy, x => x.PlanetarySystems);
        }

        public void Map(ITypeMappingConfigurator<Galaxy> map)
        {
            map.AsUriBaseType()
                .Include(x => x.PlanetarySystems, o => o.ExposedAsRepository());
        }


        public void Map(ITypeMappingConfigurator<Star> map)
        {
            map.AsChildResourceOf(x => x.PlanetarySystem, x => x.Star);
        }

        public void Map(ITypeMappingConfigurator<Planet> map)
        {
            map.AsUriBaseType();
            map.HasChildren(x => x.Moons,
                x => x.Planet,
                x => x.AsUriBaseType()
                    .ConstructedUsing(y => new Moon(y.Requires().Name, y.Parent<Planet>())), x => x.Writable());
            map.AsChildResourceOf(x => x.PlanetarySystem, x => x.Planets)
                .ConstructedUsing(x => new Planet(x.Requires().Name, x.Parent<PlanetarySystem>()));
            map.PostAllowed();
            map.DeleteAllowed();

        }
    }
}