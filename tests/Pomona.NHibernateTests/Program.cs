using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using PomonaNHibernateTest.Models;
using NHibernate.Linq;

namespace PomonaNHibernateTest
{
    internal class Program
    {
        private string DbFile = Path.GetTempFileName();
        private List<int> productIds = new List<int>();
        private Random rng;
        private ISession session;


        public void Run()
        {
            rng = new Random(376473672);
            var sessionFactory = CreateSessionFactory();
            // create properties
            var properties = new NameValueCollection();
            properties["level"] = "All";
            using (session = sessionFactory.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    CreateProducts();
                    trans.Commit();
                }
            }

            var customerCount = 30;
            for (var i = 0; i < customerCount; i++)
            {
                using (session = sessionFactory.OpenSession())
                {
                    using (var trans = session.BeginTransaction())
                    {
                        CreateCustomerWithOrders();
                        trans.Commit();
                    }
                }
            }

            var pth = new TestPomonaHost(new Uri("http://localhost:4321/"), sessionFactory);
            pth.Start();

            Console.ReadLine();
            pth.Stop();
        }


        private static void Main(string[] args)
        {
            var program = new Program();
            program.Run();
        }


        private void AddProduct(string name, string sku, decimal price)
        {
            var prod = new Product() {Name = name, Price = price, Sku = sku};
            session.Save(prod);
            productIds.Add(prod.Id);
        }


        private void AddRandomAttributesToOrder(PurchaseOrder order)
        {
            var attrKeys = new[] {"mega", "micro", "giga", "ultra"};
            var attrCount = rng.Next(0, 4);
            for (var i = 0; i < attrCount; i++)
            {
                var attrKey = attrKeys[rng.Next(0, attrKeys.Length)];
                if (order.Attributes.All(x => x.Key != attrKey))
                    order.Attributes.Add(
                        new EntityAttribute()
                            {
                                Key = attrKey,
                                Value = rng.Next(0, 4444).ToString(CultureInfo.InvariantCulture)
                            });
            }
        }


        private void BuildSchema(Configuration config)
        {
            // delete the existing db on each run
            if (File.Exists(DbFile))
                File.Delete(DbFile);

            // this NHibernate tool takes a configuration (with mapping info in)
            // and exports a database schema from it
            new SchemaExport(config)
                .Create(true, true);
        }


        private void CreateCustomerWithOrders()
        {
            var customer = new Customer() {Name = Words.GetAnimalWithPersonality(rng)};
            session.Save(customer);

            var orderCount = rng.Next(0, 9);

            for (var i = 0; i < orderCount; i++)
            {
                var itemCount = rng.Next(1, 5);
                var order = new PurchaseOrder() {Customer = customer, SomeGroup = rng.Next(0, 3)};
                Enumerable.Range(0, itemCount)
                          .Select(x => GetRandomProduct())
                          .Select(
                              x => new Item() {Price = x.Price, Product = x, Quantity = rng.Next(1, 6), Order = order})
                          .ToList()
                          .ForEach(item => order.Items.Add(item));

                AddRandomAttributesToOrder(order);

                session.Save(order);
            }
        }


        private void CreateProducts()
        {
            AddProduct("Coca Cola", "soda-coke", 10.5m);
            AddProduct("Solo", "soda-solo", 11.3m);
            AddProduct("Pizza Pepperoni", "pizza-pepperoni", 99.9m);
        }


        private ISessionFactory CreateSessionFactory()
        {
            var cfg = new TestAutomappingConfiguration();
            return Fluently.Configure()
                           .Database(
                               SQLiteConfiguration.Standard
                                                  .UsingFile(DbFile)
                                                  .ShowSql()
                )
                           .Mappings(
                               m =>
                               m.AutoMappings.Add(
                                   AutoMap.AssemblyOf<EntityBase>(cfg)
                                          .Conventions.AddFromAssemblyOf<CascadeAllConvention>()
                                          // The stuff below is just for testing the Any mapping, not related to Pomona
                                          .Override<PurchaseOrder>(o => o
                                              .ReferencesAny(x => x.RelatedEntity)
                                              .IdentityType(x => x.Id)
                                              .EntityTypeColumn("RelatedEntityType")
                                              .EntityIdentifierColumn("RelatedEntityId")
                                              .AddMetaValue<Customer>("Customer")
                                              )))
                           .ExposeConfiguration(BuildSchema)
                           .BuildSessionFactory();
        }


        private Product GetRandomProduct()
        {
            return session.Load<Product>(productIds[rng.Next(0, productIds.Count())]);
        }
    }
}