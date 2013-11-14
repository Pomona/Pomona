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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Pomona.Common.Internals;
using Pomona.Example.Models;
using Pomona.Example.Models.Existence;
using Pomona.Internals;

namespace Pomona.Example
{
    public class CritterRepository
    {
        private static readonly MethodInfo saveCollectionMethod;
        private static readonly MethodInfo saveDictionaryMethod;
        private static readonly MethodInfo saveInternalMethod;
        private static readonly MethodInfo queryMethod;

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
        }


        public CritterRepository(TypeMapper typeMapper)
        {
            if (typeMapper == null) throw new ArgumentNullException("typeMapper");
            this.typeMapper = typeMapper;
            ResetTestData();
            notificationsEnabled = true;
        }

        #region IPomonaDataSource Members

        public T GetById<T>(object id)
        {
            lock (syncLock)
            {
                object entity;
                try
                {
                    if (typeof(CelestialObject).IsAssignableFrom(typeof(T)))
                    {
                        entity = GetEntityList<T>().Cast<CelestialObject>().FirstOrDefault(x => x.Name == (string)id);
                    }
                    else
                    {
                        var idInt = Convert.ToInt32(id);
                        entity = GetEntityList<T>().Cast<EntityBase>().FirstOrDefault(x => x.Id == idInt);
                    }
                }
                catch (Exception)
                {
                    entity = null;
                }

                if (entity == null)
                {
                    throw new ResourceNotFoundException(
                        string.Format("No entity of type {0} with id {1} found.", typeof (T).Name, id));
                }

                return (T)entity;
            }
        }


        public object Post<T>(T newObject)
        {
            lock (syncLock)
            {
                newObject = Save(newObject);
                var order = newObject as Order;
                if (order != null)
                    return new OrderResponse(order);

                return newObject;
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

        public IQueryable<T> Query<T>()
            where T : class
        {
            lock (syncLock)
            {
                var entityType = typeof(T);
                var entityUriBaseType = ((ResourceType)typeMapper.GetClassMapping(typeof(T))).UriBaseType.MappedTypeInstance;

                return
                    (IQueryable<T>)
                        queryMethod.MakeGenericMethod(entityUriBaseType, entityType).Invoke(this, null);
            }
        }

        public ICollection<T> List<T>()
        {
            lock (syncLock)
            {
                return GetEntityList<T>();
            }
        }

        private IQueryable<TEntity> Query<TEntityBase, TEntity>()
        {
            //var visitor = new MakeDictAccessesSafeVisitor();
            //pq.FilterExpression = (LambdaExpression)visitor.Visit(pq.FilterExpression);

            //var throwOnCalculatedPropertyVisitor = new ThrowOnCalculatedPropertyVisitor();
            //throwOnCalculatedPropertyVisitor.Visit(pq.FilterExpression);

            return GetEntityList<TEntityBase>().OfType<TEntity>().AsQueryable();
        }


        private static string GetDictItemOrDefault(IDictionary<string, string> dict, string key)
        {
            string value;
            return dict.TryGetValue(key, out value) ? value : Guid.NewGuid().ToString();
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
            get { lock (syncLock) return queryLog; }
        }

        public static IEnumerable<Type> GetEntityTypes()
        {
            return
                typeof (CritterModule).Assembly.GetTypes()
                                      .Where(x => (x.Namespace == "Pomona.Example.Models" || (x.Namespace != null && x.Namespace.StartsWith("Pomona.Example.Models"))) && !x.IsGenericTypeDefinition);
        }


        public void ResetTestData()
        {
            lock (syncLock)
            {
                idCounter = 1;
                entityLists = new Dictionary<Type, object>();
                notificationsEnabled = false;
                CreateRandomData();
                notificationsEnabled = true;
                queryLog.Clear();
            }
        }

        private object SaveDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
            where TValue : EntityBase
        {
            SaveCollection(dictionary.Values);
            return dictionary;
        }

        private object SaveCollection<T>(ICollection<T> collection)
            where T : EntityBase
        {
            foreach (var item in collection)
            {
                Save(item);
            }
            return collection;
        }

        public T Save<T>(T entity)
        {
            var mappedTypeInstance = GetBaseUriType<T>();
            var saveMethodInstance = saveInternalMethod.MakeGenericMethod(mappedTypeInstance);
            return (T)saveMethodInstance.Invoke(this, new object[] { entity });
        }

        private Type GetBaseUriType<T>()
        {
            var transformedType = (TransformedType) typeMapper.GetClassMapping<T>();
            var mappedTypeInstance = (transformedType.UriBaseType ?? transformedType).MappedTypeInstance;
            return mappedTypeInstance;
        }

        public T SaveInternal<T>(T entity)
        {
            var entityCast = (EntityBase)((object)entity);

            if (entityCast.Id != 0)
                return entity;
            entityCast.Id = idCounter++;
            if (notificationsEnabled)
                Console.WriteLine("Saving entity of type " + entity.GetType().Name + " with id " + entityCast.Id);

            foreach (var prop in entity.GetType().GetProperties())
            {
                Type[] genericArguments;
                var propType = prop.PropertyType;
                if (typeof (EntityBase).IsAssignableFrom(propType))
                {
                    var value = prop.GetValue(entity, null);
                    if (value != null)
                        saveInternalMethod.MakeGenericMethod(propType).Invoke(this, new[] { value });
                }
                else if (TypeUtils.TryGetTypeArguments(propType, typeof (ICollection<>), out genericArguments))
                {
                    if (typeof (EntityBase).IsAssignableFrom(genericArguments[0]))
                    {
                        var value = prop.GetValue(entity, null);
                        if (value != null)
                            saveCollectionMethod.MakeGenericMethod(genericArguments).Invoke(this, new[] { value });
                    }
                }
                else if (TypeUtils.TryGetTypeArguments(propType, typeof (IDictionary<,>), out genericArguments))
                {
                    if (typeof (EntityBase).IsAssignableFrom(genericArguments[1]))
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

        public void AddToEntityList<T>(T entity)
        {
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

        public Critter CreateRandomCritter(Random rng = null, int? rngSeed = null, bool forceMusicalCritter = false)
        {
            if (rng == null)
                rng = new Random(rngSeed ?? 75648382 + idCounter);

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

            critter.CreatedOn = DateTime.UtcNow.AddDays(-rng.NextDouble()*50.0);

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
                                    Price = (decimal)(rng.NextDouble()*10)
                                });
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
            return (IList<T>)list;
        }


        private T GetRandomEntity<T>(Random rng)
        {
            var entityList = GetEntityList<T>();

            if (entityList.Count == 0)
                throw new InvalidOperationException("No random entity to get. Count 0.");

            return entityList[rng.Next(0, entityList.Count)];
        }
    }
}