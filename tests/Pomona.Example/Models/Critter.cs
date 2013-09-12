#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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
using System.Collections.Generic;

namespace Pomona.Example.Models
{
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
                    new SimpleAttribute {Key = "MeaningOfLife", Value = "42"},
                    new SimpleAttribute {Key = "IsCat", Value = "maybe"}
                };

            Hat = new Hat();
            Protected = Guid.NewGuid().ToString();
        }


        public bool IsCaptured { get; internal set; }

        public CrazyValueObject CrazyValue { get; set; }

        public DateTime CreatedOn { get; set; }
        public IList<Critter> Enemies { get; set; }
        public Farm Farm { get; set; }
        public string Protected { get; protected set; }

        public Guid Guid { get; set; }
        public Hat Hat { get; set; }

        [NotKnownToDataSource]
        public string UnhandledGeneratedProperty
        {
            get { return Hat != null ? Hat.HatType : null; }
        }

        public string Password { get; set; }

        public int DecompiledGeneratedProperty
        {
            get { return Id + 100; }
        }

        public int HandledGeneratedProperty
        {
            get { return Id%6; }
        }

        public string Name { get; set; }

        public IList<SimpleAttribute> SimpleAttributes { get; set; }

        public IList<Subscription> Subscriptions { get; set; }
        public IList<Weapon> Weapons { get; set; }

        /// <summary>
        /// To check that property scanning works properly on entities having explicit prop implementations.
        /// </summary>
        int IHiddenInterface.Foo { get; set; }

        public void FixParentReferences()
        {
            foreach (var subscription in Subscriptions)
            {
                subscription.Critter = this;
            }
        }
    }
}