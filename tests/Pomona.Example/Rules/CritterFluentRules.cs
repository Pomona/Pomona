#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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

using System;

using Nancy;

using Pomona.Common;
using Pomona.Example.Handlers;
using Pomona.Example.Models;
using Pomona.FluentMapping;

namespace Pomona.Example.Rules
{
    public class CritterFluentRules
    {
        public void Map(ITypeMappingConfigurator<IExposedInterface> map)
        {
            map
                .Named("ExposedInterface")
                .AsUriBaseType()
                .WithPluralName("ExposedInterfaces")
                .Include(x => x.PropertyFromInheritedInterface)
                .AsConcrete()
                .ConstructedUsing(c => new ExposedInterfaceInternalImplementation());
        }


        public void Map(ITypeMappingConfigurator<HandledThing> map)
        {
            map
                .HasChildren(x => x.Children,
                    x => x.Parent,
                    t =>
                    {
                        return
                            t.ConstructedUsing(c => new HandledChild(c.Parent<HandledThing>()))
                                .HandledBy<HandledThingsHandler>();
                    },
                    o => o.ExposedAsRepository())
                .Include(x => x.ETag, o => o.AsEtag())
                .AsUriBaseType()
                .DeleteAllowed().HandledBy<HandledThingsHandler>();
        }


        public void Map(ITypeMappingConfigurator<HandledSingleChild> map)
        {
            map.AsChildResourceOf(x => x.HandledThing, x => x.SingleChild);
        }


        public void Map(ITypeMappingConfigurator<RenamedThing> map)
        {
            map.Named("GotNewName").WithPluralName("ThingsWithNewName").ExposedAt("renamings");
        }


        public void Map(ITypeMappingConfigurator<UnhandledThing> map)
        {
            // HandledThingsHandler got not matching methods for unhandled thing. Which means request should be passed to the DataSource instead.
            map.HandledBy<HandledThingsHandler>();
        }


        public void Map(ITypeMappingConfigurator<UnpostableThing> map)
        {
            map.PostDenied();
        }


        public void Map(ITypeMappingConfigurator<AbstractAnimal> map)
        {
            map.Include(x => x.PublicAndReadOnlyThroughApi, o => o.ReadOnly());
        }


        public void Map(ITypeMappingConfigurator<UnpostableThingOnServer> map)
        {
            map.WithPluralName("UnpostableThingsOnServer");
            map.PostDenied();
        }


        public void Map(ITypeMappingConfigurator<UnpatchableThing> map)
        {
            map.PatchDenied();
        }


        public void Map(ITypeMappingConfigurator<MusicalCritter> map)
        {
            map.ConstructedUsing((c) => new MusicalCritter(c.Optional().OnlyWritableByInheritedResource));
        }


        public void Map(ITypeMappingConfigurator<JunkWithRenamedProperty> map)
        {
            map.Include(x => x.ReallyUglyPropertyName, o => o.Named("BeautifulAndExposed"));
        }


        public void Map(ITypeMappingConfigurator<ThingIndependentFromBase> map)
        {
            map.AsIndependentTypeRoot();
        }


        public void Map(ITypeMappingConfigurator<StringToObjectDictionaryContainer> map)
        {
            map.Include(x => x.Map, o => o.AsAttributes());
            map.Include(x => x.ETag, o => o.AsEtag());
        }


        public void Map(ITypeMappingConfigurator<DictionaryContainer> map)
        {
            map.Include(x => x.Map, o => o.AsAttributes());
        }


        public void Map(ITypeMappingConfigurator<ThingWithRenamedProperties> map)
        {
            map.Include(x => x.Junky, o => o.Named("DiscoFunky"));
            map.Include(x => x.RelatedJunks, o => o.Named("PrettyThings"));
        }


        public void Map(ITypeMappingConfigurator<Order> map)
        {
            map.PostReturns<OrderResponse>();
            map.AsUriBaseType();
        }


        public void Map(ITypeMappingConfigurator<ArgNullThrowingThing> map)
        {
            // This should throw ArgumentNullException on creation if Incoming property is null,
            // for testing that this turns into a ValidationException.
            map.ConstructedUsing(x => new ArgNullThrowingThing(x.Optional().Incoming));
        }

        public void Map(ITypeMappingConfigurator<OrderResponse> map)
        {
            map.AsValueObject();
        }


        public void Map(ITypeMappingConfigurator<Loner> map)
        {
            map.ConstructedUsing(
                (c) =>
                    new Loner(c.Requires().Name,
                        c.Requires().Strength,
                        c.Optional().OptionalInfo,
                        c.Optional().OptionalDate));
        }


        public void Map(ITypeMappingConfigurator<ErrorStatus> map)
        {
            map.AsValueObject();
        }


        public void Map(ITypeMappingConfigurator<Subscription> map)
        {
            map.AsValueObject();
            map.Exclude(x => x.Critter);
        }


        public void Map(ITypeMappingConfigurator<Critter> map)
        {
            map.AsUriBaseType()
                .Include(x => x.CrazyValue)
                .Include(x => x.CreatedOn)
                .Include(x => x.Subscriptions, o => o.Expand().Writable())
                .Include(x => x.HandledGeneratedProperty, o => o.OnQuery(x => x.Id % 6))
                .Include(x => x.DecompiledGeneratedProperty, o => o.UsingDecompiledFormula())
                .Include(x => x.Password, o => o.WithAccessMode(HttpMethod.Post | HttpMethod.Put))
                .Include(x => x.PublicAndReadOnlyThroughApi, o => o.ReadOnly())
                .Include(x => x.PropertyWithAttributeAddedFluently, o => o.HasAttribute(new ObsoleteAttribute()))
                .IncludeAs<string>(x => x.IntExposedAsString,
                    o =>
                        o.OnGet(x => x.IntExposedAsString.ToString()).OnQuery(x => x.IntExposedAsString.ToString())
                            .OnSet((c, v) => c.IntExposedAsString = int.Parse(v)))
                .Include(x => x.Weapons, o => o.Writable())
                .Include(x => x.RelativeImageUrl,
                    o => o.Named("AbsoluteImageUrl")
                        .OnGet<NancyContext>((critter, ctx) =>
                        {
                            var absUrl = ctx.Request.Url.Clone();
                            absUrl.Path = critter.RelativeImageUrl;
                            absUrl.Query = null;
                            return absUrl.ToString();
                        })
                        .OnSet<NancyContext>(
                            (critter, value, ctx) =>
                            {
                                critter.RelativeImageUrl =
                                    new Uri(value).AbsolutePath.Substring((ctx.Request.Url.BasePath ?? "").Length);
                            }))
                .Include(x => x.Enemies, o => o.ExpandShallow())
                .HandledBy<CritterHandler>()
                .OnDeserialized(c => c.FixParentReferences());
        }


        public void Map(ITypeMappingConfigurator<HasReadOnlyDictionaryProperty> map)
        {
            map.Include(x => x.Map,
                o =>
                    o.AsAttributes().WithAccessMode(HttpMethod.Get | HttpMethod.Patch | HttpMethod.Post));
        }


        public void Map(ITypeMappingConfigurator<EtaggedEntity> map)
        {
            map.Include(x => x.ETag, o => o.AsEtag());
        }


        public void Map(ITypeMappingConfigurator<CritterCaptureCommand> map)
        {
            map.AsValueObject();
        }


        public void Map(ITypeMappingConfigurator<Gun> map)
        {
            map.ConstructedUsing(x => new Gun(x.Requires().Model))
                .Include(x => x.ExplosionFactor);
        }
    }
}