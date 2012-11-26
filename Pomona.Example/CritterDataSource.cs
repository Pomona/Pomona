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
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Example.Models;
using Pomona.Internals;
using Pomona.Queries;

namespace Pomona.Example
{
    public class CritterDataSource : IPomonaDataSource
    {
        private Dictionary<Type, object> entityLists = new Dictionary<Type, object>();

        private int idCounter;

        private bool notificationsEnabled = false;
        private object syncLock = new object();


        public CritterDataSource()
        {
            ResetTestData();
            this.notificationsEnabled = true;
        }

        #region IPomonaDataSource Members

        public T GetById<T>(object id)
        {
            lock (this.syncLock)
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
                        string.Format("No entity of type {0} with id {1} found.", typeof(T).Name, id));
                }

                return (T)entity;
            }
        }


        public ICollection<T> List<T>()
        {
            lock (this.syncLock)
            {
                return GetEntityList<T>();
            }
        }


        public QueryResult<T> List<T>(IPomonaQuery query)
        {
            lock (this.syncLock)
            {
                var pq = (PomonaQuery)query;
                var expr = (Expression<Func<T, bool>>)pq.FilterExpression;

                var visitor = new MakeDictAccessesSafeVisitor();
                expr = (Expression<Func<T, bool>>)visitor.Visit(expr);

                var compiledExpr = expr.Compile();
                var count = GetEntityList<T>().Count(compiledExpr);
                var result = GetEntityList<T>().Where(compiledExpr);

                if (pq.OrderByExpression != null)
                    result = OrderByCompiledExpression(result, pq.OrderByExpression, pq.SortOrder);

                result = result.Skip(pq.Skip).Take(pq.Top);

                return new QueryResult<T>(result, pq.Skip, count, pq.Url);
            }
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


        private static string GetDictItemOrDefault(IDictionary<string, string> dict, string key)
        {
            string value;
            return dict.TryGetValue(key, out value) ? value : Guid.NewGuid().ToString();
        }


        private static IEnumerable<T> OrderByCompiledExpression<T>(
            IEnumerable<T> enumerable, LambdaExpression expression, SortOrder sortOrder)
        {
            var method = typeof(CritterDataSource).GetMethod(
                "OrderByCompiledExpressionGeneric", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(
                    typeof(T), expression.ReturnType);
            return (IEnumerable<T>)method.Invoke(null, new object[] { enumerable, expression, sortOrder });
        }


        private static IEnumerable<T> OrderByCompiledExpressionGeneric<T, TResult>(
            IEnumerable<T> enumerable, Expression<Func<T, TResult>> expression, SortOrder sortOrder)
        {
            var keySelector = expression.Compile();
            return sortOrder == SortOrder.Ascending
                       ? enumerable.OrderBy(keySelector)
                       : enumerable.OrderByDescending(keySelector);
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

        public static IEnumerable<Type> GetEntityTypes()
        {
            return typeof(CritterModule).Assembly.GetTypes().Where(x => x.Namespace == "Pomona.Example.Models");
        }


        public void ResetTestData()
        {
            this.idCounter = 1;
            this.entityLists = new Dictionary<Type, object>();
            this.notificationsEnabled = false;
            CreateObjectModel();
            this.notificationsEnabled = true;
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


        private void CreateFarms()
        {
            Save(new Farm("Insanity valley"));
            Save(new Farm("Broken boulevard"));
        }


        private void CreateJunkWithNullables()
        {
            Save(new JunkWithNullableInt() { Maybe = 1337, MentalState = "I'm happy, I have value!" });
            Save(new JunkWithNullableInt() { Maybe = null, MentalState = "I got nothing in life. So sad.." });
        }


        private void CreateObjectModel()
        {
            var rng = new Random(23576758);

            for (var i = 0; i < 70; i++)
                Save(new WeaponModel() { Name = Words.GetSpecialWeapon(rng) });

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
                    Instrument = Save(new Instrument() { Type = Words.GetCoolInstrument(rng) })
                };
                critter = musicalCritter;
            }
            else
                critter = new Critter();

            critter.CreatedOn = DateTime.UtcNow.AddDays(-rng.NextDouble() * 50.0);

            critter.Name = Words.GetAnimalWithPersonality(rng);

            critter.CrazyValue = new CrazyValueObject()
            { Sickness = Words.GetCritterHealthDiagnosis(rng, critter.Name) };

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
                        ? Save(new Weapon(critter, weaponType) { Strength = rng.NextDouble() })
                        : Save<Weapon>(
                            new Gun(critter, weaponType)
                            {
                                Strength = rng.NextDouble(),
                                ExplosionFactor = rng.NextDouble(),
                                Price = (decimal)(rng.NextDouble() * 10)
                            });
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
    }
}