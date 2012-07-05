#region License

// --------------------------------------------------
// Copyright © OKB. All Rights Reserved.
// 
// This software is proprietary information of OKB.
// USE IS SUBJECT TO LICENSE TERMS.
// --------------------------------------------------

#endregion

using System;
using System.Collections.Generic;

using Pomona.TestModel;

namespace Pomona
{
    public class Repository
    {
        private readonly Dictionary<Type, object> entityLists = new Dictionary<Type, object>();

        private int idCounter;


        public Repository()
        {
            CreateObjectModel();
        }


        private void CreateObjectModel()
        {
            var rng = new Random(463345562);

            for (int i = 0; i < 12; i++)
            {
                Save(new BazookaModel() { Name = Words.GetSpecialWeapon(rng) });
            }

            const int critterCount = 20;
            
            for (int i = 0; i < critterCount; i++)
                CreateRandomCritter(rng);


        }

        public IList<T> GetAll<T>()
            where T : EntityBase
        {
            return GetEntityList<T>();
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

            for (int i = 0; i < count; i++)
            {
                var weaponType = GetRandomEntity<BazookaModel>(rng);
                var subscription =
                    Save(
                        new Subscription(critter, weaponType)
                        { Sku = rng.Next(0, 9999).ToString(), StartsOn = DateTime.UtcNow.AddDays(rng.Next(0, 120)) });
                critter.Subscriptions.Add(subscription);
            }
        }


        private void CreateWeapons(Random rng, Critter critter, int maxWeapons)
        {
            var weaponCount = rng.Next(0, maxWeapons + 1);

            for (int i = 0; i < weaponCount; i++)
            {
                var weaponType = GetRandomEntity<BazookaModel>(rng);
                var weapon =
                    Save(
                        new Bazooka(weaponType) { Dependability = rng.NextDouble(), ExplosionFactor = rng.NextDouble() });
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


        private T Save<T>(T entity)
            where T : EntityBase
        {
            if (entity.Id != 0)
                throw new InvalidOperationException("Trying to save entity with id 0");
            entity.Id = this.idCounter++;
            GetEntityList<T>().Add(entity);
            return entity;
        }
    }
}