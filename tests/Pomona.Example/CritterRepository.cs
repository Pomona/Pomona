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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.Example.Models;

namespace Pomona.Example
{
    public class CritterRepository
    {
        private static readonly MethodInfo deleteInternalMethod;
        private static readonly MethodInfo queryMethod;
        private static readonly MethodInfo saveCollectionMethod;
        private static readonly MethodInfo saveDictionaryMethod;
        private static readonly MethodInfo saveInternalMethod;

        private readonly List<PomonaQuery> queryLog = new List<PomonaQuery>();
        private readonly object syncLock = new object();
        private readonly TypeMapper typeMapper;
        private Dictionary<Type, object> entityLists = new Dictionary<Type, object>();

        private int idCounter;

        private bool notificationsEnabled;


        static CritterRepository()
        {
            queryMethod =
                ReflectionHelper.GetMethodDefinition<CritterRepository>(x => x.Query<object, object>());
            saveCollectionMethod =
                ReflectionHelper.GetMethodDefinition<CritterRepository>(
                    x => x.SaveCollection((ICollection<EntityBase>)null));
            saveDictionaryMethod =
                ReflectionHelper.GetMethodDefinition<CritterRepository>(
                    x => x.SaveDictionary((IDictionary<object, EntityBase>)null));
            saveInternalMethod =
                ReflectionHelper.GetMethodDefinition<CritterRepository>(x => x.SaveInternal<EntityBase>(null));
            deleteInternalMethod =
                ReflectionHelper.GetMethodDefinition<CritterRepository>(x => x.DeleteInternal<EntityBase>(null));
        }


        public CritterRepository(TypeMapper typeMapper)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            this.typeMapper = typeMapper;
            ResetTestData();
            this.notificationsEnabled = true;
        }

        #region IPomonaDataSource Members

        public PomonaResponse ApplyAndExecute(IQueryable queryable, PomonaQuery pq)
        {
            lock (this.syncLock)
            {
                this.queryLog.Add(pq);

                var visitor = new MakeDictAccessesSafeVisitor();
                pq.FilterExpression = (LambdaExpression)visitor.Visit(pq.FilterExpression);

                var throwOnCalculatedPropertyVisitor = new ThrowOnCalculatedPropertyVisitor();
                throwOnCalculatedPropertyVisitor.Visit(pq.FilterExpression);

                return pq.ApplyAndExecute(queryable);
            }
        }


        public ICollection<T> List<T>()
        {
            lock (this.syncLock)
            {
                return GetEntityList<T>();
            }
        }


        public object Patch<T>(T updatedObject)
        {
            var etagEntity = updatedObject as EtaggedEntity;
            if (etagEntity != null)
                etagEntity.SetEtag(Guid.NewGuid().ToString());

            Save(updatedObject);

            return updatedObject;
        }


        public object Post<T>(T newObject)
        {
            lock (this.syncLock)
            {
                newObject = Save(newObject);
                var order = newObject as Order;
                if (order != null)
                    return new OrderResponse(order);

                return newObject;
            }
        }


        public IQueryable<T> Query<T>()
            where T : class
        {
            lock (this.syncLock)
            {
                var entityType = typeof(T);
                var entityUriBaseType = ((ResourceType)this.typeMapper.GetClassMapping(typeof(T))).UriBaseType.Type;

                return
                    (IQueryable<T>)
                        queryMethod.MakeGenericMethod(entityUriBaseType, entityType).Invoke(this, null);
            }
        }


        private static string GetDictItemOrDefault(IDictionary<string, string> dict, string key)
        {
            string value;
            return dict.TryGetValue(key, out value) ? value : Guid.NewGuid().ToString();
        }


        private IQueryable<TEntity> Query<TEntityBase, TEntity>()
        {
            //var visitor = new MakeDictAccessesSafeVisitor();
            //pq.FilterExpression = (LambdaExpression)visitor.Visit(pq.FilterExpression);

            //var throwOnCalculatedPropertyVisitor = new ThrowOnCalculatedPropertyVisitor();
            //throwOnCalculatedPropertyVisitor.Visit(pq.FilterExpression);

            return GetEntityList<TEntityBase>().OfType<TEntity>().AsQueryable();
        }


        public class MakeDictAccessesSafeVisitor : ExpressionVisitor
        {
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method == OdataFunctionMapping.DictStringStringGetMethod)
                {
                    return
                        Expression.Call(
                            ReflectionHelper.GetMethodInfo<string, string>(x => GetDictItemOrDefault(null, null)),
                            node.Object,
                            node.Arguments.First());
                }
                return base.VisitMethodCall(node);
            }
        }

        #endregion

        public List<PomonaQuery> QueryLog
        {
            get
            {
                lock (this.syncLock)
                    return this.queryLog;
            }
        }


        public static IEnumerable<Type> GetEntityTypes()
        {
            return
                typeof(CritterModule).Assembly.GetTypes()
                    .Where(
                        x =>
                            (x.Namespace == "Pomona.Example.Models"
                             || (x.Namespace != null && x.Namespace.StartsWith("Pomona.Example.Models")))
                            && !x.IsGenericTypeDefinition);
        }


        public void AddToEntityList<T>(T entity)
        {
        }


        public Critter CreateRandomCritter(Random rng = null, int? rngSeed = null, bool forceMusicalCritter = false)
        {
            if (rng == null)
                rng = new Random(rngSeed ?? 75648382 + this.idCounter);

            Critter critter;
            if (forceMusicalCritter || rng.NextDouble() > 0.76)
            {
                var musicalCritter = new MusicalCritter("written in the stars")
                {
                    BandName = Words.GetBandName(rng),
                    Instrument = Save(new Instrument { Type = Words.GetCoolInstrument(rng) })
                };
                critter = musicalCritter;
            }
            else
                critter = new Critter();

            critter.CreatedOn = DateTime.UtcNow.AddDays(-rng.NextDouble() * 50.0);

            critter.Name = Words.GetAnimalWithPersonality(rng);

            critter.CrazyValue = new CrazyValueObject { Sickness = Words.GetCritterHealthDiagnosis(rng, critter.Name) };

            CreateWeapons(rng, critter, 24);
            CreateSubscriptions(rng, critter, 3);

            // Add to one of the farms
            var farms = GetEntityList<Farm>();
            var chosenFarm = farms[rng.Next(farms.Count)];

            chosenFarm.Critters.Add(critter);
            critter.Farm = chosenFarm;

            // Patch on a random hat
            Save(critter.Hat);
            Save(critter);

            return critter;
        }


        public void CreateRandomData(int critterCount = 5, int weaponModelCount = 3)
        {
            var rng = new Random(23576758);

            for (var i = 0; i < 70; i++)
                Save(new WeaponModel { Name = Words.GetSpecialWeapon(rng) });

            CreateFarms();

            for (var i = 0; i < critterCount; i++)
                CreateRandomCritter(rng);

            CreateJunkWithNullables();

            var thingWithCustomIList = Save(new ThingWithCustomIList());
            foreach (var loner in thingWithCustomIList.Loners)
                Save(loner);
        }


        public void Delete<T>(T entity)
        {
            lock (this.syncLock)
            {
                var mappedTypeInstance = GetBaseUriType<T>();
                var deleteMethodInstance = deleteInternalMethod.MakeGenericMethod(mappedTypeInstance);
                deleteMethodInstance.Invoke(this, new object[] { entity });
            }
        }


        public void ResetTestData()
        {
            lock (this.syncLock)
            {
                this.idCounter = 1;
                this.entityLists = new Dictionary<Type, object>();
                this.notificationsEnabled = false;
                CreateRandomData();
                this.notificationsEnabled = true;
                this.queryLog.Clear();
            }
        }


        public T Save<T>(T entity)
        {
            var mappedTypeInstance = GetBaseUriType<T>();
            var saveMethodInstance = saveInternalMethod.MakeGenericMethod(mappedTypeInstance);
            return (T)saveMethodInstance.Invoke(this, new object[] { entity });
        }


        public T SaveInternal<T>(T entity)
        {
            var entityCast = (EntityBase)((object)entity);

            if (entityCast.Id != 0)
                return entity;
            entityCast.Id = this.idCounter++;
            if (this.notificationsEnabled)
                Console.WriteLine("Saving entity of type " + entity.GetType().Name + " with id " + entityCast.Id);

            foreach (var prop in entity.GetType().GetProperties())
            {
                Type[] genericArguments;
                var propType = prop.PropertyType;
                if (typeof(EntityBase).IsAssignableFrom(propType))
                {
                    var value = prop.GetValue(entity, null);
                    if (value != null)
                        saveInternalMethod.MakeGenericMethod(propType).Invoke(this, new[] { value });
                }
                else if (TypeUtils.TryGetTypeArguments(propType, typeof(ICollection<>), out genericArguments))
                {
                    if (typeof(EntityBase).IsAssignableFrom(genericArguments[0]))
                    {
                        var value = prop.GetValue(entity, null);
                        if (value != null)
                            saveCollectionMethod.MakeGenericMethod(genericArguments).Invoke(this, new[] { value });
                    }
                }
                else if (TypeUtils.TryGetTypeArguments(propType, typeof(IDictionary<,>), out genericArguments))
                {
                    if (typeof(EntityBase).IsAssignableFrom(genericArguments[1]))
                    {
                        var value = prop.GetValue(entity, null);
                        if (value != null)
                            saveDictionaryMethod.MakeGenericMethod(genericArguments).Invoke(this, new[] { value });
                    }
                }
            }

            GetEntityList<T>().Add(entity);
            return entity;
        }


        private void CreateFarms()
        {
            Save(new Farm("Insanity valley"));
            Save(new Farm("Broken boulevard"));
        }


        private void CreateJunkWithNullables()
        {
            Save(new JunkWithNullableInt { Maybe = 1337, MentalState = "I'm happy, I have value!" });
            Save(new JunkWithNullableInt { Maybe = null, MentalState = "I got nothing in life. So sad.." });
        }


        private void CreateSubscriptions(Random rng, Critter critter, int maxSubscriptions)
        {
            var count = rng.Next(0, maxSubscriptions + 1);

            for (var i = 0; i < count; i++)
            {
                var weaponType = GetRandomEntity<WeaponModel>(rng);
                var subscription =
                    Save(
                        new Subscription(weaponType)
                        {
                            Critter = critter,
                            Sku = rng.Next(0, 9999).ToString(),
                            StartsOn = DateTime.UtcNow.AddDays(rng.Next(0, 120))
                        });
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
                        ? Save(new Weapon(weaponType) { Strength = rng.NextDouble() })
                        : Save<Weapon>(
                            new Gun(weaponType)
                            {
                                Strength = rng.NextDouble(),
                                ExplosionFactor = rng.NextDouble(),
                                Price = (decimal)(rng.NextDouble() * 10)
                            });
                critter.Weapons.Add(weapon);
            }
        }


        private void DeleteInternal<T>(T entity)
        {
            GetEntityList<T>().Remove(entity);
        }


        private Type GetBaseUriType<T>()
        {
            var transformedType = (TransformedType)this.typeMapper.GetClassMapping<T>();
            var mappedTypeInstance =
                (transformedType.Maybe().OfType<ResourceType>().Select(x => (TransformedType)x.UriBaseType).OrDefault(
                    () => transformedType)).Type;
            return mappedTypeInstance;
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


        private object SaveCollection<T>(ICollection<T> collection)
            where T : EntityBase
        {
            foreach (var item in collection)
                Save(item);
            return collection;
        }


        private object SaveDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
            where TValue : EntityBase
        {
            SaveCollection(dictionary.Values);
            return dictionary;
        }
    }
}