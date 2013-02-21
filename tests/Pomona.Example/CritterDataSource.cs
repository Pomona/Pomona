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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Example.Models;
using Pomona.Internals;

namespace Pomona.Example
{
    public class CritterDataSource : IPomonaDataSource
    {
        private static readonly MethodInfo saveCollectionMethod;
        private static readonly MethodInfo saveDictionaryMethod;
        private static readonly MethodInfo saveMethod;
        private static readonly MethodInfo queryMethod;

        private readonly List<PomonaQuery> queryLog = new List<PomonaQuery>();
        private readonly object syncLock = new object();
        private Dictionary<Type, object> entityLists = new Dictionary<Type, object>();

        private int idCounter;

        private bool notificationsEnabled;

        static CritterDataSource()
        {
            queryMethod =
                ReflectionHelper.GetGenericMethodDefinition<CritterDataSource>(x => x.Query<object, object>(null));
            saveCollectionMethod =
                ReflectionHelper.GetGenericMethodDefinition<CritterDataSource>(
                    x => x.SaveCollection((ICollection<EntityBase>) null));
            saveDictionaryMethod =
                ReflectionHelper.GetGenericMethodDefinition<CritterDataSource>(
                    x => x.SaveDictionary((IDictionary<object, EntityBase>) null));
            saveMethod = ReflectionHelper.GetGenericMethodDefinition<CritterDataSource>(x => x.Save<EntityBase>(null));
        }


        public CritterDataSource()
        {
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
                    var idInt = Convert.ToInt32(id);
                    entity = GetEntityList<T>().Cast<EntityBase>().FirstOrDefault(x => x.Id == idInt);
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

                return (T) entity;
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

        public QueryResult Query(IPomonaQuery query)
        {
            lock (syncLock)
            {
                var pq = (PomonaQuery) query;
                var entityType = pq.TargetType.MappedTypeInstance;
                var entityUriBaseType = pq.TargetType.UriBaseType.MappedTypeInstance;

                return
                    (QueryResult)
                    queryMethod.MakeGenericMethod(entityUriBaseType, entityType).Invoke(this, new object[] {pq});
            }
        }

        public ICollection<T> List<T>()
        {
            lock (syncLock)
            {
                return GetEntityList<T>();
            }
        }

        private QueryResult Query<TEntityBase, TEntity>(PomonaQuery pq)
        {
            queryLog.Add(pq);

            var visitor = new MakeDictAccessesSafeVisitor();
            pq.FilterExpression = (LambdaExpression) visitor.Visit(pq.FilterExpression);

            var throwOnCalculatedPropertyVisitor = new ThrowOnCalculatedPropertyVisitor();
            throwOnCalculatedPropertyVisitor.Visit(pq.FilterExpression);

            return pq.ApplyAndExecute(new EnumerableQuery<TEntity>(GetEntityList<TEntityBase>().OfType<TEntity>()));
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
                if (node.Method == OdataFunctionMapping.DictGetMethod)
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
            return typeof (CritterModule).Assembly.GetTypes().Where(x => x.Namespace == "Pomona.Example.Models");
        }


        public void ResetTestData()
        {
            lock (syncLock)
            {
                idCounter = 1;
                entityLists = new Dictionary<Type, object>();
                notificationsEnabled = false;
                CreateObjectModel();
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
            var entityCast = (EntityBase) ((object) entity);

            if (entityCast.Id != 0)
                return entity;
            entityCast.Id = idCounter++;
            if (notificationsEnabled)
                Console.WriteLine("Saving entity of type " + entity.GetType().Name + " with id " + entityCast.Id);

            foreach (var prop in typeof (T).GetProperties())
            {
                Type[] genericArguments;
                var propType = prop.PropertyType;
                if (typeof (EntityBase).IsAssignableFrom(propType))
                {
                    var value = prop.GetValue(entity, null);
                    if (value != null)
                        saveMethod.MakeGenericMethod(propType).Invoke(this, new[] {value});
                }
                else if (TypeUtils.TryGetTypeArguments(propType, typeof (ICollection<>), out genericArguments))
                {
                    if (typeof (EntityBase).IsAssignableFrom(genericArguments[0]))
                    {
                        var value = prop.GetValue(entity, null);
                        if (value != null)
                            saveCollectionMethod.MakeGenericMethod(genericArguments).Invoke(this, new[] {value});
                    }
                }
                else if (TypeUtils.TryGetTypeArguments(propType, typeof (IDictionary<,>), out genericArguments))
                {
                    if (typeof (EntityBase).IsAssignableFrom(genericArguments[1]))
                    {
                        var value = prop.GetValue(entity, null);
                        if (value != null)
                            saveDictionaryMethod.MakeGenericMethod(genericArguments).Invoke(this, new[] {value});
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
            Save(new JunkWithNullableInt {Maybe = 1337, MentalState = "I'm happy, I have value!"});
            Save(new JunkWithNullableInt {Maybe = null, MentalState = "I got nothing in life. So sad.."});
        }


        private void CreateObjectModel()
        {
            var rng = new Random(23576758);

            for (var i = 0; i < 70; i++)
                Save(new WeaponModel {Name = Words.GetSpecialWeapon(rng)});

            CreateFarms();

            const int critterCount = 180;

            for (var i = 0; i < critterCount; i++)
                CreateRandomCritter(rng);

            CreateJunkWithNullables();

            var thingWithCustomIList = Save(new ThingWithCustomIList());
            foreach (var loner in thingWithCustomIList.Loners)
                Save(loner);
        }


        private void CreateRandomCritter(Random rng)
        {
            Critter critter;
            if (rng.NextDouble() > 0.76)
            {
                var musicalCritter = new MusicalCritter
                    {
                        BandName = Words.GetBandName(rng),
                        Instrument = Save(new Instrument {Type = Words.GetCoolInstrument(rng)})
                    };
                critter = musicalCritter;
            }
            else
                critter = new Critter();

            critter.CreatedOn = DateTime.UtcNow.AddDays(-rng.NextDouble()*50.0);

            critter.Name = Words.GetAnimalWithPersonality(rng);

            critter.CrazyValue = new CrazyValueObject {Sickness = Words.GetCritterHealthDiagnosis(rng, critter.Name)};

            CreateWeapons(rng, critter, 24);
            CreateSubscriptions(rng, critter, 3);

            // Add to one of the farms
            var farms = GetEntityList<Farm>();
            var chosenFarm = farms[rng.Next(farms.Count)];

            chosenFarm.Critters.Add(critter);
            critter.Farm = chosenFarm;

            // Put on a random hat
            Save(critter.Hat);
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
            var weaponCount = rng.Next(1, maxWeapons + 1);

            for (var i = 0; i < weaponCount; i++)
            {
                var weaponType = GetRandomEntity<WeaponModel>(rng);
                var weapon =
                    rng.NextDouble() > 0.5
                        ? Save(new Weapon(critter, weaponType) {Strength = rng.NextDouble()})
                        : Save<Weapon>(
                            new Gun(critter, weaponType)
                                {
                                    Strength = rng.NextDouble(),
                                    ExplosionFactor = rng.NextDouble(),
                                    Price = (decimal) (rng.NextDouble()*10)
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
            return (IList<T>) list;
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