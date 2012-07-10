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

using System;
using System.Collections.Generic;
using Pomona.Example.Models;

namespace Pomona.Example
{
    public class CritterRepository
    {
        private readonly Dictionary<Type, object> entityLists = new Dictionary<Type, object>();

        private int idCounter;


        public CritterRepository()
        {
            CreateObjectModel();
        }


        public IList<T> GetAll<T>()
        {
            return GetEntityList<T>();
        }


        private void CreateObjectModel()
        {
            var rng = new Random(463345562);

            for (var i = 0; i < 12; i++)
                Save(new WeaponModel() {Name = Words.GetSpecialWeapon(rng)});

            const int critterCount = 20;

            for (var i = 0; i < critterCount; i++)
                CreateRandomCritter(rng);
        }


        private void CreateRandomCritter(Random rng)
        {
            var critter = new Critter()
                              {
                                  Name = Words.GetAnimalWithPersonality(rng)
                              };

            CreateWeapons(rng, critter, 3);
            CreateSubscriptions(rng, critter, 2);

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
                            {Sku = rng.Next(0, 9999).ToString(), StartsOn = DateTime.UtcNow.AddDays(rng.Next(0, 120))});
                critter.Subscriptions.Add(subscription);
            }
        }


        private void CreateWeapons(Random rng, Critter critter, int maxWeapons)
        {
            var weaponCount = rng.Next(0, maxWeapons + 1);

            for (var i = 0; i < weaponCount; i++)
            {
                var weaponType = GetRandomEntity<WeaponModel>(rng);
                var weapon =
                    Save(
                        rng.NextDouble() > 0.5
                            ? new Weapon(weaponType) {Dependability = rng.NextDouble()}
                            : new Gun(weaponType)
                                  {Dependability = rng.NextDouble(), ExplosionFactor = rng.NextDouble()});
                critter.Weapons.Add(weapon);
            }
        }


        private IList<T> GetEntityList<T>()
        {
            var type = typeof (T);
            object list;
            if (!entityLists.TryGetValue(type, out list))
            {
                list = new List<T>();
                entityLists[type] = list;
            }
            return (IList<T>) list;
        }


        private T GetRandomEntity<T>(Random rng)
        {
            var entityList = GetEntityList<T>();

            if (entityList.Count == 0)
                throw new InvalidOperationException("No random entity to get. Count 0.");

            return entityList[rng.Next(0, entityList.Count)];
        }


        private T Save<T>(T entity)
            where T : EntityBase
        {
            if (entity.Id != 0)
                throw new InvalidOperationException("Trying to save entity with id 0");
            entity.Id = idCounter++;
            GetEntityList<T>().Add(entity);
            return entity;
        }
    }
}