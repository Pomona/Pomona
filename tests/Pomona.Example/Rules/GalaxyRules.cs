#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using Pomona.Common;
using Pomona.Example.Models.Existence;
using Pomona.FluentMapping;

namespace Pomona.Example.Rules
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
               .Include(x => x.Planets,
                        o => o.ExposedAsRepository().Allow(HttpMethod.Post).ItemsAllow(HttpMethod.Delete))
               .HasChild(x => x.Star, x => x.PlanetarySystem)
               .AsChildResourceOf(x => x.Galaxy, x => x.PlanetarySystems);
        }


        public void Map(ITypeMappingConfigurator<Galaxy> map)
        {
            map.AsUriBaseType()
               .Include(x => x.PlanetarySystems, o => o.ExposedAsRepository())
               .HasChild(x => x.Info, x => x.Galaxy);
        }


        public void Map(ITypeMappingConfigurator<Star> map)
        {
            map.AsChildResourceOf(x => x.PlanetarySystem, x => x.Star);
        }


        public void Map(ITypeMappingConfigurator<Planet> map)
        {
            map.HasChildren(x => x.Moons,
                            x => x.Planet,
                            x => x.AsUriBaseType()
                                  .ConstructedUsing(y => new Moon(y.Requires().Name, y.Parent<Planet>())),
                            x => x.Writable());
            map.ConstructedUsing(x => new Planet(x.Requires().Name, x.Parent<PlanetarySystem>()));
        }


        public void Map(ITypeMappingConfigurator<Planemo> map)
        {
            map.AsUriBaseType();
            map.AsChildResourceOf(x => x.PlanetarySystem, x => x.Planets);
            map.PostAllowed();
            map.DeleteAllowed();
        }
    }
}

