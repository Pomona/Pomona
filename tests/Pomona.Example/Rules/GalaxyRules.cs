#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

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