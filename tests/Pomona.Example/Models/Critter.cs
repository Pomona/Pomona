#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;

namespace Pomona.Example.Models
{
    /// <summary>
    /// A small annoying animal, that come in many shapes and sizes.
    /// </summary>
    public class Critter : EntityBase, IHiddenInterface
    {
        public Critter()
        {
            Guid = Guid.NewGuid();
            Enemies = new List<Critter>();
            Weapons = new List<Weapon>();
            Subscriptions = new List<Subscription>();

            SimpleAttributes = new List<SimpleAttribute>
            {
                new SimpleAttribute { Key = "MeaningOfLife", Value = "42" },
                new SimpleAttribute { Key = "IsCat", Value = "maybe" }
            };

            OnlyWritableByInheritedResource = "blabla not writable";
            Hat = new Hat();
            Protected = Guid.NewGuid().ToString();
            RelativeImageUrl = "/photos/the-image.png";
        }


        /// <summary>
        /// This is a value object.
        /// </summary>
        public CrazyValueObject CrazyValue { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTimeOffset CreatedOnOffset { get; set; }

        /// <summary>
        /// List of <see cref="Critter"/> enemies.
        /// </summary>
        public IList<Critter> Enemies { get; set; }

        public Farm Farm { get; set; }
        public Guid Guid { get; set; }

        public int HandledGeneratedProperty => Id % 6;

        /// <summary>
        /// Something to put on its head to make it look silly.
        /// </summary>
        public Hat Hat { get; set; }

        /// <summary>
        /// An integer exposed as a string
        /// </summary>
        public int IntExposedAsString { get; set; }

        public bool IsCaptured { get; internal set; }

        /// <summary>
        /// This is a <see cref="System.String"/>
        /// </summary>
        public string IsNotAllowedInFilters => "bah bah";

        /// <summary>
        /// Name of the critter!
        /// </summary>
        public string Name { get; set; }

        [Obsolete("This is old and obsolete!")]
        public string ObsoletedProperty { get; set; }

        public string OnlyWritableByInheritedResource { get; protected set; }
        public string Password { get; set; }
        public string PropertyExcludedByGetAllPropertiesOfType { get; set; }
        public string PropertyWithAttributeAddedFluently { get; set; }
        public string Protected { get; protected set; }
        public bool PublicAndReadOnlyThroughApi { get; set; }
        public Critter ReferenceToAnotherCritter { get; set; }
        public string RelativeImageUrl { get; set; }
        public IList<SimpleAttribute> SimpleAttributes { get; set; }
        public IList<Subscription> Subscriptions { get; protected set; }

        public static bool TheIgnoredStaticProperty
        {
            get { throw new NotImplementedException("Should not be gotten!"); }
        }

        [NotKnownToDataSource]
        public string UnhandledGeneratedProperty => Hat != null ? Hat.HatType : null;

        public IList<Weapon> Weapons { get; protected set; }


        public void FixParentReferences()
        {
            foreach (var subscription in Subscriptions)
                subscription.Critter = this;
        }


        /// <summary>
        /// To check that property scanning works properly on entities having explicit prop implementations.
        /// </summary>
        int IHiddenInterface.Foo { get; set; }
    }
}