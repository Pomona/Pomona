#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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
using System.Linq;

using Pomona.Example.Models;

namespace Pomona.Example
{
    public class CritterDataSource : IPomonaDataSource
    {
        private readonly Dictionary<Type, object> entityLists = new Dictionary<Type, object>();

        private int idCounter;

        private bool notificationsEnabled = false;
        private object syncLock = new object();


        public CritterDataSource()
        {
            CreateObjectModel();
            this.notificationsEnabled = true;
        }

        #region IPomonaDataSource Members

        public T GetById<T>(object id)
        {
            lock (this.syncLock)
            {
                var idInt = Convert.ToInt32(id);
                return (T)((object)GetEntityList<T>().Cast<EntityBase>().First(x => x.Id == idInt));
            }
        }


        public ICollection<T> List<T>()
        {
            lock (this.syncLock)
            {
                return GetEntityList<T>();
            }
        }


        public T Post<T>(T newObject)
        {
            lock (this.syncLock)
            {
                return Save(newObject);
            }
        }

        #endregion

        public static IEnumerable<Type> GetEntityTypes()
        {
            return typeof(CritterModule).Assembly.GetTypes().Where(x => x.Namespace == "Pomona.Example.Models");
        }


        private void CreateObjectModel()
        {
            var rng = new Random(678343);

            for (var i = 0; i < 12; i++)
                Save(new WeaponModel() { Name = Words.GetSpecialWeapon(rng) });

            const int critterCount = 20;

            for (var i = 0; i < critterCount; i++)
                CreateRandomCritter(rng);

            CreateJunkWithNullables();

            var thingWithCustomIList = Save(new ThingWithCustomIList());
            foreach (var loner in thingWithCustomIList.Loners)
                Save(loner);
        }


        private void CreateJunkWithNullables()
        {
            Save(new JunkWithNullableInt() { Maybe = 1337, MentalState = "I'm happy, I have value!"});
            Save(new JunkWithNullableInt() { Maybe = null, MentalState = "I got nothing in life. So sad.."});
        }


        private void CreateRandomCritter(Random rng)
        {
            Critter critter;
            if (rng.NextDouble() > 0.8)
            {
                critter = new MusicalCritter();
                ((MusicalCritter)critter).Instrument = Words.GetCoolInstrument(rng);
            }
            else
                critter = new Critter();

            critter.Name = Words.GetAnimalWithPersonality(rng);

            critter.CrazyValue = new CrazyValueObject()
            { Sickness = Words.GetCritterHealthDiagnosis(rng, critter.Name) };

            CreateWeapons(rng, critter, 4);
            CreateSubscriptions(rng, critter, 3);

            Save(critter.Hat); // Random hat
            Save(critter);
        }


        private void CreateSubscriptions(Random rng, Critter critter, int maxSubscriptions)
        {
            var count = rng.Next(0, maxSubscriptions + 1);

            for (var i = 0; i < count; i++)
            {
                var weaponType = GetRandomEntity<WeaponModel>(rng);
                var subscription =
                    Save(
                        new Subscription(critter, weaponType)
                        { Sku = rng.Next(0, 9999).ToString(), StartsOn = DateTime.UtcNow.AddDays(rng.Next(0, 120)) });
                critter.Subscriptions.Add(subscription);
            }
        }


        private void CreateWeapons(Random rng, Critter critter, int maxWeapons)
        {
            var weaponCount = rng.Next(1, maxWeapons + 1);

            for (var i = 0; i < weaponCount; i++)
            {
                var weaponType = GetRandomEntity<WeaponModel>(rng);
                var weapon =
                    rng.NextDouble() > 0.5
                        ? Save(new Weapon(weaponType) { Dependability = rng.NextDouble() })
                        : Save(
                            new Gun(weaponType)
                            { Dependability = rng.NextDouble(), ExplosionFactor = rng.NextDouble() });
                critter.Weapons.Add(weapon);
            }
        }


        private IList<T> GetEntityList<T>()
        {
            var type = typeof(T);
            object list;
            if (!this.entityLists.TryGetValue(type, out list))
            {
                list = new List<T>();
                this.entityLists[type] = list;
            }
            return (IList<T>)list;
        }


        private T GetRandomEntity<T>(Random rng)
        {
            var entityList = GetEntityList<T>();

            if (entityList.Count == 0)
                throw new InvalidOperationException("No random entity to get. Count 0.");

            return entityList[rng.Next(0, entityList.Count)];
        }


        public T Save<T>(T entity)
        {
            var entityCast = (EntityBase)((object)entity);

            if (entityCast.Id != 0)
                throw new InvalidOperationException("Trying to save entity with id 0");
            entityCast.Id = this.idCounter++;
            if (this.notificationsEnabled)
                Console.WriteLine("Saving entity of type " + entity.GetType().Name + " with id " + entityCast.Id);

            GetEntityList<T>().Add(entity);
            return entity;
        }
    }
}